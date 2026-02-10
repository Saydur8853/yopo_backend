using Microsoft.EntityFrameworkCore;
using YopoBackend.Constants;
using YopoBackend.Data;
using YopoBackend.Modules.IntercomAccess.DTOs;
using YopoBackend.Modules.IntercomAccess.Models;
using YopoBackend.Modules.TenantCRUD.Models;

namespace YopoBackend.Modules.IntercomAccess.Services
{
    public class IntercomSyncService : IIntercomSyncService
    {
        private readonly ApplicationDbContext _context;

        public IntercomSyncService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PendingTenantFaceDTO>> GetPendingTenantsAsync(int buildingId)
        {
            return await _context.Set<TempIntercom>()
                .AsNoTracking()
                .Where(t => t.BuildingId == buildingId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new PendingTenantFaceDTO
                {
                    TenantId = t.TenantId,
                    TenantName = t.TenantName,
                    UnitId = t.UnitId,
                    BuildingId = t.BuildingId,
                    FrontImage = t.FrontImageUrl,
                    LeftImage = t.LeftImageUrl,
                    RightImage = t.RightImageUrl
                })
                .ToListAsync();
        }

        public async Task<int> ConfirmSyncAsync(int buildingId)
        {
            var rows = await _context.Set<TempIntercom>()
                .Where(t => t.BuildingId == buildingId)
                .ToListAsync();

            if (rows.Count == 0)
            {
                return 0;
            }

            _context.RemoveRange(rows);
            await _context.SaveChangesAsync();
            return rows.Count;
        }

        public async Task<(int inserted, int updated, int skipped)> BackfillPendingAsync(int? buildingId)
        {
            var faces = await _context.Set<IntercomFaceBiometric>()
                .AsNoTracking()
                .Where(f => f.IsActive)
                .ToListAsync();

            if (faces.Count == 0)
            {
                return (0, 0, 0);
            }

            var userIds = faces.Select(f => f.UserId).Distinct().ToList();
            var tenantUserIds = await _context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id) && u.IsActive && u.UserTypeId == UserTypeConstants.TENANT_USER_TYPE_ID)
                .Select(u => u.Id)
                .ToListAsync();

            if (tenantUserIds.Count == 0)
            {
                return (0, 0, faces.Count);
            }

            var tenantUserSet = tenantUserIds.ToHashSet();
            var filteredFaces = faces.Where(f => tenantUserSet.Contains(f.UserId)).ToList();

            var units = await _context.Units
                .AsNoTracking()
                .Where(u => u.TenantId.HasValue && tenantUserSet.Contains(u.TenantId.Value))
                .ToListAsync();

            var unitByTenantUser = units
                .Where(u => u.TenantId.HasValue)
                .GroupBy(u => u.TenantId!.Value)
                .ToDictionary(g => g.Key, g => g.First());

            var unitIds = units.Select(u => u.UnitId).Distinct().ToList();

            var tenantsByTenantId = await _context.Tenants
                .AsNoTracking()
                .Where(t => tenantUserSet.Contains(t.TenantId))
                .GroupBy(t => t.TenantId)
                .Select(g => g.First())
                .ToDictionaryAsync(t => t.TenantId);

            var tenantsByUnitId = await _context.Tenants
                .AsNoTracking()
                .Where(t => t.UnitId.HasValue && unitIds.Contains(t.UnitId.Value))
                .GroupBy(t => t.UnitId!.Value)
                .Select(g => g.First())
                .ToDictionaryAsync(t => t.UnitId!.Value);

            IQueryable<TempIntercom> tempQuery = _context.Set<TempIntercom>();
            if (buildingId.HasValue)
            {
                tempQuery = tempQuery.Where(t => t.BuildingId == buildingId.Value);
            }
            var existingRows = await tempQuery.ToListAsync();
            var existingMap = existingRows.ToDictionary(t => (t.TenantId, t.BuildingId));

            var inserted = 0;
            var updated = 0;
            var skipped = 0;

            foreach (var face in filteredFaces)
            {
                if (!tenantUserSet.Contains(face.UserId))
                {
                    skipped++;
                    continue;
                }

                int? resolvedBuildingId = null;
                int? resolvedUnitId = null;
                Tenant? tenant = null;

                if (unitByTenantUser.TryGetValue(face.UserId, out var unit))
                {
                    resolvedBuildingId = unit.BuildingId;
                    resolvedUnitId = unit.UnitId;
                    if (!tenantsByUnitId.TryGetValue(unit.UnitId, out tenant))
                    {
                        tenantsByTenantId.TryGetValue(face.UserId, out tenant);
                    }
                }
                else
                {
                    tenantsByTenantId.TryGetValue(face.UserId, out tenant);
                    if (tenant != null)
                    {
                        resolvedBuildingId = tenant.BuildingId;
                        resolvedUnitId = tenant.UnitId;
                    }
                }

                if (tenant == null || !resolvedBuildingId.HasValue)
                {
                    skipped++;
                    continue;
                }

                if (buildingId.HasValue && resolvedBuildingId.Value != buildingId.Value)
                {
                    skipped++;
                    continue;
                }

                var key = (tenant.TenantId, resolvedBuildingId.Value);
                if (!existingMap.TryGetValue(key, out var temp))
                {
                    temp = new TempIntercom
                    {
                        TenantId = tenant.TenantId,
                        TenantName = tenant.TenantName,
                        UnitId = resolvedUnitId ?? tenant.UnitId,
                        BuildingId = resolvedBuildingId.Value,
                        FrontImageUrl = face.FrontImageUrl,
                        LeftImageUrl = face.LeftImageUrl,
                        RightImageUrl = face.RightImageUrl,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Add(temp);
                    existingMap[key] = temp;
                    inserted++;
                }
                else
                {
                    temp.TenantName = tenant.TenantName;
                    temp.UnitId = resolvedUnitId ?? tenant.UnitId;
                    temp.FrontImageUrl = face.FrontImageUrl;
                    temp.LeftImageUrl = face.LeftImageUrl;
                    temp.RightImageUrl = face.RightImageUrl;
                    temp.UpdatedAt = DateTime.UtcNow;
                    updated++;
                }
            }

            if (inserted > 0 || updated > 0)
            {
                await _context.SaveChangesAsync();
            }

            return (inserted, updated, skipped);
        }
    }
}
