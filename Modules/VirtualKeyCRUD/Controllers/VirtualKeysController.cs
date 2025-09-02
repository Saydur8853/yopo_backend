using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using YopoBackend.Modules.VirtualKeyCRUD.DTOs;
using YopoBackend.Modules.VirtualKeyCRUD.Services;
using YopoBackend.Attributes;
using YopoBackend.Constants;
using System.Security.Claims;

namespace YopoBackend.Modules.VirtualKeyCRUD.Controllers
{
    /// <summary>
    /// Controller for Virtual Key CRUD operations.
    /// Module ID: 10 (VirtualKeyCRUD)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [RequireModule(ModuleConstants.VIRTUAL_KEY_MODULE_ID)]
    public class VirtualKeysController : ControllerBase
    {
        private readonly IVirtualKeyService _virtualKeyService;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualKeysController"/> class.
        /// </summary>
        /// <param name="virtualKeyService">The virtual key service.</param>
        public VirtualKeysController(IVirtualKeyService virtualKeyService)
        {
            _virtualKeyService = virtualKeyService;
        }

        /// <summary>
        /// Gets all virtual keys based on the current user's access control settings.
        /// </summary>
        /// <returns>A list of all virtual keys the current user has access to.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VirtualKeyDto>>> GetAllVirtualKeys()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var virtualKeys = await _virtualKeyService.GetAllVirtualKeysAsync(userId);
                return Ok(virtualKeys);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all active virtual keys based on the current user's access control settings.
        /// </summary>
        /// <returns>A list of all active virtual keys the current user has access to.</returns>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<VirtualKeyDto>>> GetActiveVirtualKeys()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var virtualKeys = await _virtualKeyService.GetActiveVirtualKeysAsync(userId);
                return Ok(virtualKeys);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all expired virtual keys based on the current user's access control settings.
        /// </summary>
        /// <returns>A list of all expired virtual keys the current user has access to.</returns>
        [HttpGet("expired")]
        public async Task<ActionResult<IEnumerable<VirtualKeyDto>>> GetExpiredVirtualKeys()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var virtualKeys = await _virtualKeyService.GetExpiredVirtualKeysAsync(userId);
                return Ok(virtualKeys);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a virtual key by ID, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the virtual key.</param>
        /// <returns>The virtual key with the specified ID if the user has access to it.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<VirtualKeyDto>> GetVirtualKey(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var virtualKey = await _virtualKeyService.GetVirtualKeyByIdAsync(id, userId);
                if (virtualKey == null)
                {
                    return NotFound($"Virtual key with ID {id} not found or you don't have access to it.");
                }

                return Ok(virtualKey);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all virtual keys for a specific building.
        /// </summary>
        /// <param name="buildingId">The ID of the building.</param>
        /// <returns>A list of virtual keys for the specified building.</returns>
        [HttpGet("building/{buildingId}")]
        public async Task<ActionResult<IEnumerable<VirtualKeyDto>>> GetVirtualKeysByBuildingId(int buildingId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var virtualKeys = await _virtualKeyService.GetVirtualKeysByBuildingIdAsync(buildingId, userId);
                return Ok(virtualKeys);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all virtual keys assigned to a specific tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>A list of virtual keys assigned to the specified tenant.</returns>
        [HttpGet("tenant/{tenantId}")]
        public async Task<ActionResult<IEnumerable<VirtualKeyDto>>> GetVirtualKeysByTenantId(int tenantId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var virtualKeys = await _virtualKeyService.GetVirtualKeysByTenantIdAsync(tenantId, userId);
                return Ok(virtualKeys);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all virtual keys associated with a specific intercom.
        /// </summary>
        /// <param name="intercomId">The ID of the intercom.</param>
        /// <returns>A list of virtual keys associated with the specified intercom.</returns>
        [HttpGet("intercom/{intercomId}")]
        public async Task<ActionResult<IEnumerable<VirtualKeyDto>>> GetVirtualKeysByIntercomId(int intercomId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var virtualKeys = await _virtualKeyService.GetVirtualKeysByIntercomIdAsync(intercomId, userId);
                return Ok(virtualKeys);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets virtual keys by status.
        /// </summary>
        /// <param name="status">The status to filter by.</param>
        /// <returns>A list of virtual keys with the specified status.</returns>
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<VirtualKeyDto>>> GetVirtualKeysByStatus(string status)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var virtualKeys = await _virtualKeyService.GetVirtualKeysByStatusAsync(status, userId);
                return Ok(virtualKeys);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets virtual keys by type.
        /// </summary>
        /// <param name="type">The type to filter by.</param>
        /// <returns>A list of virtual keys with the specified type.</returns>
        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<VirtualKeyDto>>> GetVirtualKeysByType(string type)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var virtualKeys = await _virtualKeyService.GetVirtualKeysByTypeAsync(type, userId);
                return Ok(virtualKeys);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets virtual keys that are about to expire within the specified number of days.
        /// </summary>
        /// <param name="days">Number of days to look ahead for expiring keys.</param>
        /// <returns>A list of virtual keys that will expire soon.</returns>
        [HttpGet("expiring/{days}")]
        public async Task<ActionResult<IEnumerable<VirtualKeyDto>>> GetVirtualKeysExpiringInDays(int days)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var virtualKeys = await _virtualKeyService.GetVirtualKeysExpiringInDaysAsync(days, userId);
                return Ok(virtualKeys);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new virtual key.
        /// </summary>
        /// <param name="createVirtualKeyDto">The data for creating the virtual key.</param>
        /// <returns>The created virtual key.</returns>
        [HttpPost]
        public async Task<ActionResult<VirtualKeyDto>> CreateVirtualKey(CreateVirtualKeyDto createVirtualKeyDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var createdVirtualKey = await _virtualKeyService.CreateVirtualKeyAsync(createVirtualKeyDto, userId);
                return CreatedAtAction(nameof(GetVirtualKey), new { id = createdVirtualKey.KeyId }, createdVirtualKey);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing virtual key, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the virtual key to update.</param>
        /// <param name="updateVirtualKeyDto">The data for updating the virtual key.</param>
        /// <returns>The updated virtual key.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<VirtualKeyDto>> UpdateVirtualKey(int id, UpdateVirtualKeyDto updateVirtualKeyDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var updatedVirtualKey = await _virtualKeyService.UpdateVirtualKeyAsync(id, updateVirtualKeyDto, userId);
                if (updatedVirtualKey == null)
                {
                    return NotFound($"Virtual key with ID {id} not found or you don't have access to modify it.");
                }

                return Ok(updatedVirtualKey);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a virtual key, respecting the current user's access control settings.
        /// </summary>
        /// <param name="id">The ID of the virtual key to delete.</param>
        /// <returns>A confirmation of deletion.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteVirtualKey(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid token." });
                }

                var result = await _virtualKeyService.DeleteVirtualKeyAsync(id, userId);
                if (!result)
                {
                    return NotFound($"Virtual key with ID {id} not found or you don't have access to delete it.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Records usage of a virtual key.
        /// </summary>
        /// <param name="keyId">The ID of the virtual key being used.</param>
        /// <param name="usageDto">The usage data.</param>
        /// <returns>Success confirmation.</returns>
        [HttpPost("{keyId}/usage")]
        public async Task<ActionResult> RecordVirtualKeyUsage(int keyId, VirtualKeyUsageDto usageDto)
        {
            try
            {
                var result = await _virtualKeyService.RecordVirtualKeyUsageAsync(keyId, usageDto.UsageLocation, usageDto.UsageDetails);
                if (!result)
                {
                    return BadRequest("Unable to record usage. Virtual key may be inactive or expired.");
                }

                return Ok(new { message = "Usage recorded successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates a unique PIN code for virtual keys.
        /// </summary>
        /// <returns>A unique PIN code.</returns>
        [HttpPost("generate-pin")]
        public async Task<ActionResult<string>> GenerateUniquePinCode()
        {
            try
            {
                var pinCode = await _virtualKeyService.GenerateUniquePinCodeAsync();
                return Ok(new { pinCode });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates a QR code for a virtual key.
        /// </summary>
        /// <param name="keyId">The ID of the virtual key.</param>
        /// <param name="additionalData">Additional data to include in the QR code.</param>
        /// <returns>The QR code data.</returns>
        [HttpPost("{keyId}/generate-qr")]
        public async Task<ActionResult<string>> GenerateQrCode(int keyId, [FromBody] string? additionalData = null)
        {
            try
            {
                var qrCode = await _virtualKeyService.GenerateQrCodeAsync(keyId, additionalData);
                return Ok(new { qrCode });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if a virtual key has expired.
        /// </summary>
        /// <param name="keyId">The ID of the virtual key to check.</param>
        /// <returns>True if the virtual key has expired, false otherwise.</returns>
        [HttpGet("{keyId}/is-expired")]
        public async Task<ActionResult<bool>> IsVirtualKeyExpired(int keyId)
        {
            try
            {
                var isExpired = await _virtualKeyService.IsVirtualKeyExpiredAsync(keyId);
                return Ok(new { keyId, isExpired });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the status of expired virtual keys.
        /// </summary>
        /// <returns>The number of virtual keys updated.</returns>
        [HttpPost("update-expired-status")]
        public async Task<ActionResult> UpdateExpiredVirtualKeysStatus()
        {
            try
            {
                var updatedCount = await _virtualKeyService.UpdateExpiredVirtualKeysStatusAsync();
                return Ok(new { message = $"Updated {updatedCount} expired virtual keys." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Helper method to return a Forbidden result.
        /// </summary>
        /// <param name="value">The value to return.</param>
        /// <returns>A Forbidden ActionResult.</returns>
        private ActionResult Forbidden(object value)
        {
            return StatusCode(403, value);
        }
    }
}
