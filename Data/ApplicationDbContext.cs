using Microsoft.EntityFrameworkCore;
using YopoBackend.Modules.InvitationCRUD.Models;
using YopoBackend.Modules.UserTypeCRUD.Models;
using YopoBackend.Modules.UserCRUD.Models;
using YopoBackend.Modules.BuildingCRUD.Models;
using YopoBackend.Modules.TenantCRUD.Models;
using YopoBackend.Modules.CustomerCRUD.Models;
using YopoBackend.Modules.InvoiceCRUD.Models;
using YopoBackend.Modules.CCTVcrud.Models;
using YopoBackend.Modules.IntercomCRUD.Models;
using YopoBackend.Modules.VirtualKeyCRUD.Models;
using YopoBackend.Modules.DoorCRUD.Models;
using YopoBackend.Modules.NotificationCRUD.Models;
using YopoBackend.Models;

namespace YopoBackend.Data
{
    /// <summary>
    /// The main application database context that provides access to the database entities.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="options">The options for this context.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the Modules DbSet for managing module entities.
        /// </summary>
        public DbSet<Module> Modules { get; set; }

        // Module: UserTypeCRUD (Module ID: 1)
        /// <summary>
        /// Gets or sets the UserTypes DbSet for managing user type entities.
        /// </summary>
        public DbSet<UserType> UserTypes { get; set; }

        /// <summary>
        /// Gets or sets the UserTypeModulePermissions DbSet for managing user type module permissions.
        /// </summary>
        public DbSet<UserTypeModulePermission> UserTypeModulePermissions { get; set; }

        // Module: InvitationCRUD (Module ID: 2)
        /// <summary>
        /// Gets or sets the Invitations DbSet for managing invitation entities.
        /// </summary>
        public DbSet<Invitation> Invitations { get; set; }

        // Module: UserCRUD (Module ID: 3)
        /// <summary>
        /// Gets or sets the Users DbSet for managing user entities.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the UserTokens DbSet for managing user authentication tokens.
        /// </summary>
        public DbSet<UserToken> UserTokens { get; set; }

        // Module: BuildingCRUD (Module ID: 4)
        /// <summary>
        /// Gets or sets the Buildings DbSet for managing building entities.
        /// </summary>
        public DbSet<Building> Buildings { get; set; }

        // Module: TenantCRUD (Module ID: 5)
        /// <summary>
        /// Gets or sets the Tenants DbSet for managing tenant entities.
        /// </summary>
        public DbSet<Tenant> Tenants { get; set; }

        // Module: CustomerCRUD (Module ID: 6)
        /// <summary>
        /// Gets or sets the Customers DbSet for managing customer entities.
        /// </summary>
        public DbSet<Customer> Customers { get; set; }

        // Module: InvoiceCRUD (Module ID: 7)
        /// <summary>
        /// Gets or sets the Invoices DbSet for managing invoice entities.
        /// </summary>
        public DbSet<Invoice> Invoices { get; set; }

        // Module: CCTVcrud (Module ID: 8)
        /// <summary>
        /// Gets or sets the CCTVs DbSet for managing CCTV camera entities.
        /// </summary>
        public DbSet<CCTV> CCTVs { get; set; }

        // Module: IntercomCRUD (Module ID: 9)
        /// <summary>
        /// Gets or sets the Intercoms DbSet for managing intercom system entities.
        /// </summary>
        public DbSet<Intercom> Intercoms { get; set; }

        // Module: VirtualKeyCRUD (Module ID: 10)
        /// <summary>
        /// Gets or sets the VirtualKeys DbSet for managing virtual key entities.
        /// </summary>
        public DbSet<VirtualKey> VirtualKeys { get; set; }

// Module: DoorCRUD (Module ID: 12)
        /// <summary>
        /// Gets or sets the Doors DbSet for managing door entities.
        /// </summary>
        public DbSet<Door> Doors { get; set; }

        // Module: NotificationCRUD (Module ID: 14)
        /// <summary>
        /// Gets or sets the Notifications DbSet for managing notification entities.
        /// </summary>
        public DbSet<Notification> Notifications { get; set; }

        /// <summary>
        /// Configures the model and entity relationships using the model builder.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure Module entity
            modelBuilder.Entity<Module>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever(); // We manually set module IDs
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Version).HasMaxLength(20);
            });
            
            // Configure UserType entity (Module ID: 1)
            modelBuilder.Entity<UserType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd(); // Allow both auto-generated and manual IDs
                // Removed unique index on Name to allow multiple user types with the same name
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
            });
            
            // Configure UserTypeModulePermission entity (Module ID: 1)
            modelBuilder.Entity<UserTypeModulePermission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Configure relationships
                entity.HasOne(e => e.UserType)
                    .WithMany(ut => ut.ModulePermissions)
                    .HasForeignKey(e => e.UserTypeId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Module)
                    .WithMany()
                    .HasForeignKey(e => e.ModuleId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Create composite index for UserTypeId and ModuleId to prevent duplicates
                entity.HasIndex(e => new { e.UserTypeId, e.ModuleId }).IsUnique();
            });
            
            // Configure Invitation entity (Module ID: 2)
            modelBuilder.Entity<Invitation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.EmailAddress);
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.ExpiryTime)
                    .HasColumnType("datetime");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.EmailAddress).HasMaxLength(255);
                
                // Configure foreign key relationship with UserType
                entity.HasOne(e => e.UserType)
                    .WithMany()
                    .HasForeignKey(e => e.UserTypeId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent deleting UserType if it has invitations
            });
            
            // Configure User entity (Module ID: 3)
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.EmailAddress).IsUnique();
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.LastLoginAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.EmailAddress).HasMaxLength(255);
                entity.Property(e => e.PasswordHash).HasMaxLength(255);
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.ProfilePhoto).HasMaxLength(1000);
                
                // Configure foreign key relationship with UserType
                entity.HasOne(e => e.UserType)
                    .WithMany()
                    .HasForeignKey(e => e.UserTypeId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent deleting UserType if it has users
            });
            
            // Configure UserToken entity (Module ID: 3)
            modelBuilder.Entity<UserToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                // Removed TokenValue index due to MySQL key length limitations (2000 chars * 4 bytes > 3072 bytes max)
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.UserId, e.TokenType });
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.ExpiresAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.LastUsedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.RevokedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.TokenValue).HasMaxLength(2000);
                entity.Property(e => e.TokenType).HasMaxLength(50);
                entity.Property(e => e.DeviceInfo).HasMaxLength(500);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                
                // Configure foreign key relationship with User
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade); // Delete tokens when user is deleted
            });
            
            // Configure Building entity (Module ID: 4)
            modelBuilder.Entity<Building>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.DateStartOperation)
                    .HasColumnType("datetime(6)");
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.Photo).HasMaxLength(1000);
                entity.Property(e => e.Type).HasMaxLength(100);
                entity.Property(e => e.Developer).HasMaxLength(200);
                entity.Property(e => e.Color).HasMaxLength(50);
            });
            
            // Configure Tenant entity (Module ID: 5)
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.TenantId);
                entity.HasIndex(e => e.BuildingId);
                entity.HasIndex(e => new { e.BuildingId, e.UnitNo }).IsUnique(); // Ensure unique unit per building
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.ContractStartDate)
                    .HasColumnType("datetime");
                entity.Property(e => e.ContractEndDate)
                    .HasColumnType("datetime");
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.UnitNo).HasMaxLength(20);
                entity.Property(e => e.Contact).HasMaxLength(500);
                entity.Property(e => e.MemberType).HasMaxLength(50);
                entity.Property(e => e.Files).HasColumnType("json");
                
                // Configure foreign key relationship with Building
                entity.HasOne(e => e.Building)
                    .WithMany()
                    .HasForeignKey(e => e.BuildingId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Building if it has tenants
            });
            
            // Configure Customer entity (Module ID: 6)
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Name);
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.CompanyLicense).HasMaxLength(100);
                entity.Property(e => e.Type).HasMaxLength(50);
                
                // Configure foreign key relationship with User
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Customers)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent deleting User if it has customers
            });
            
            // Configure Invoice entity (Module ID: 7)
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(e => e.InvoiceId);
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.BuildingId);
                entity.HasIndex(e => new { e.CustomerId, e.Month, e.Year, e.BuildingId }).IsUnique(); // Ensure unique invoice per customer/building/month/year
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.DueDate)
                    .HasColumnType("datetime");
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            });
            
            // Configure CCTV entity (Module ID: 8)
            modelBuilder.Entity<CCTV>(entity =>
            {
                entity.HasKey(e => e.CctvId);
                entity.HasIndex(e => e.BuildingId);
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => new { e.BuildingId, e.Name }).IsUnique(); // Ensure unique name per building
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.InstallationDate)
                    .HasColumnType("datetime");
                entity.Property(e => e.LastMaintenanceDate)
                    .HasColumnType("datetime");
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.Model).HasMaxLength(100);
                entity.Property(e => e.Size).HasMaxLength(50);
                entity.Property(e => e.Location).HasMaxLength(500);
                entity.Property(e => e.Stream).HasMaxLength(1000);
                entity.Property(e => e.Resolution).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(1000);
                
                // Configure foreign key relationship with Building
                entity.HasOne(e => e.Building)
                    .WithMany()
                    .HasForeignKey(e => e.BuildingId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Building if it has CCTVs
                
                // Configure foreign key relationship with Tenant (optional)
                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.SetNull); // Set null when tenant is deleted
            });
            
            // Configure Intercom entity (Module ID: 9)
            modelBuilder.Entity<Intercom>(entity =>
            {
                entity.HasKey(e => e.IntercomId);
                entity.HasIndex(e => e.BuildingId);
                entity.HasIndex(e => new { e.BuildingId, e.Name }).IsUnique(); // Ensure unique name per building
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.DateInstalled)
                    .HasColumnType("datetime");
                entity.Property(e => e.ServiceDate)
                    .HasColumnType("datetime");
                entity.Property(e => e.WarrantyExpiryDate)
                    .HasColumnType("datetime");
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.Model).HasMaxLength(100);
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.Size).HasMaxLength(50);
                entity.Property(e => e.Color).HasMaxLength(50);
                entity.Property(e => e.OperatingSystem).HasMaxLength(100);
                entity.Property(e => e.Location).HasMaxLength(500);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.MacAddress).HasMaxLength(50);
                entity.Property(e => e.FirmwareVersion).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                
                // Configure foreign key relationship with Building
                entity.HasOne(e => e.Building)
                    .WithMany()
                    .HasForeignKey(e => e.BuildingId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Building if it has intercoms
            });
            
            // Configure VirtualKey entity (Module ID: 10)
            modelBuilder.Entity<VirtualKey>(entity =>
            {
                entity.HasKey(e => e.KeyId);
                entity.HasIndex(e => e.BuildingId);
                entity.HasIndex(e => e.IntercomId);
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => e.PinCode); // For faster lookups
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.DateCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.DateExpired)
                    .HasColumnType("datetime");
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.AccessLocation).HasMaxLength(500);
                entity.Property(e => e.PinCode).HasMaxLength(20);
                entity.Property(e => e.QrCode).HasMaxLength(1000);
                
                // Configure foreign key relationship with Building
                entity.HasOne(e => e.Building)
                    .WithMany()
                    .HasForeignKey(e => e.BuildingId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Building if it has virtual keys
                
                // Configure foreign key relationship with Tenant (optional)
                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.SetNull); // Set null when tenant is deleted
                
                // Configure foreign key relationship with Intercom (optional)
                entity.HasOne(e => e.Intercom)
                    .WithMany()
                    .HasForeignKey(e => e.IntercomId)
                    .OnDelete(DeleteBehavior.SetNull); // Set null when intercom is deleted
            });
            
            // Configure Door entity (Module ID: 12)
            modelBuilder.Entity<Door>(entity =>
            {
                entity.HasKey(e => e.DoorId);
                entity.HasIndex(e => e.BuildingId);
                entity.HasIndex(e => e.IntercomId);
                entity.HasIndex(e => e.CCTVId);
                entity.HasIndex(e => new { e.BuildingId, e.Type, e.Location }); // For efficient filtering
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.DateCreated)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.Type).HasMaxLength(100);
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.Location).HasMaxLength(500);
                entity.Property(e => e.AccessLevel).HasMaxLength(50);
                entity.Property(e => e.OperatingHours).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(1000);
                
                // Configure foreign key relationship with Building
                entity.HasOne(e => e.Building)
                    .WithMany()
                    .HasForeignKey(e => e.BuildingId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Building if it has doors
                
                // Configure foreign key relationship with Intercom (optional)
                entity.HasOne(e => e.Intercom)
                    .WithMany()
                    .HasForeignKey(e => e.IntercomId)
                    .OnDelete(DeleteBehavior.SetNull); // Set null when intercom is deleted
                
                // Configure foreign key relationship with CCTV (optional)
                entity.HasOne(e => e.CCTV)
                    .WithMany()
                    .HasForeignKey(e => e.CCTVId)
                    .OnDelete(DeleteBehavior.SetNull); // Set null when CCTV is deleted
            });
            
            // Configure Notification entity (Module ID: 14)
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.NotificationId);
                entity.HasIndex(e => e.BuildingId);
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => e.CreatedBy);
                entity.HasIndex(e => new { e.Type, e.Status }); // For efficient filtering
                entity.HasIndex(e => e.Priority); // For priority-based queries
                entity.HasIndex(e => e.IsUrgent); // For urgent notification queries
                entity.HasIndex(e => e.ScheduledAt); // For scheduled notification queries
                entity.HasIndex(e => e.IsActive); // For active/inactive filtering
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.ScheduledAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.SentAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.DeliveredAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.ReadAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.ExpiresAt)
                    .HasColumnType("datetime");
                entity.Property(e => e.Title).HasMaxLength(300);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.Type).HasMaxLength(50);
                entity.Property(e => e.SentTo).HasMaxLength(100);
                entity.Property(e => e.SendFrom).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.WarningLevel).HasMaxLength(20);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.File).HasColumnType("json");
                entity.Property(e => e.Metadata).HasColumnType("json");
                
                // Configure foreign key relationship with Building (optional)
                entity.HasOne(e => e.Building)
                    .WithMany()
                    .HasForeignKey(e => e.BuildingId)
                    .OnDelete(DeleteBehavior.SetNull); // Set null when building is deleted
                
                // Configure foreign key relationship with Customer (optional)
                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull); // Set null when customer is deleted
                
                // Configure foreign key relationship with Tenant (optional)
                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.SetNull); // Set null when tenant is deleted
                
                // Configure foreign key relationship with Creator (required)
                entity.HasOne(e => e.Creator)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent deleting user if they have created notifications
            });
        }
    }
}
