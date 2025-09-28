using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YopoBackend.Attributes;
using YopoBackend.Constants;
using YopoBackend.Modules.UserCRUD.DTOs;
using YopoBackend.Modules.UserCRUD.Services;

namespace YopoBackend.Modules.UserCRUD.Controllers
{
    /// <summary>
    /// Controller for customer management (Super Admin dashboard).
    /// Allows Super Admin to view and manage customer records for property managers.
    /// Module: UserCRUD (Module ID: 3)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    [RequireModule(ModuleConstants.USER_MODULE_ID)]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        /// <summary>
        /// Initializes a new instance of the CustomersController class.
        /// </summary>
        /// <param name="customerService">The customer service.</param>
        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        /// <summary>
        /// Gets all customers with pagination and filtering (Super Admin only).
        /// </summary>
        /// <param name="page">Page number for listing customers (starting from 1)</param>
        /// <param name="pageSize">Page size for listing customers</param>
        /// <param name="searchTerm">Search term for filtering customers by name, company, or email</param>
        /// <param name="customerId">Optional filter by specific customer ID</param>
        /// <param name="active">Filter by active status</param>
        /// <param name="isTrial">Filter by trial status</param>
        /// <param name="paid">Filter by paid status</param>
        /// <returns>Paginated list of customers.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(CustomerListResponseDTO), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetCustomers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? customerId = null,
            [FromQuery] bool? active = null,
            [FromQuery] bool? isTrial = null,
            [FromQuery] bool? paid = null)
        {
            // Verify user is Super Admin
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int currentUserId))
                return Unauthorized(new { message = "Invalid token." });

            // Check if user is Super Admin (only Super Admin can view all customers)
            var userTypeClaim = User.FindFirst("UserTypeId")?.Value;
            if (!int.TryParse(userTypeClaim, out int userTypeId) || userTypeId != UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID)
            {
                return StatusCode(403, new { message = "Only Super Admin can access customer management." });
            }

            try
            {
                var result = await _customerService.GetAllCustomersAsync(page, pageSize, searchTerm, customerId, active, isTrial, paid);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve customers: " + ex.Message });
            }
        }


        /// <summary>
        /// Updates a customer record (Super Admin only).
        /// </summary>
        /// <param name="id">The customer ID to update.</param>
        /// <param name="updateRequest">The update request data.</param>
        /// <returns>The updated customer if successful.</returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(CustomerResponseDTO), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerRequestDTO updateRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verify user is Super Admin
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int currentUserId))
                return Unauthorized(new { message = "Invalid token." });

            var userTypeClaim = User.FindFirst("UserTypeId")?.Value;
            if (!int.TryParse(userTypeClaim, out int userTypeId) || userTypeId != UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID)
            {
                return StatusCode(403, new { message = "Only Super Admin can update customer records." });
            }

            try
            {
                var result = await _customerService.UpdateCustomerAsync(id, updateRequest);
                if (result == null)
                    return NotFound(new { message = "Customer not found or update failed." });

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update customer: " + ex.Message });
            }
        }

        /// <summary>
        /// Deletes a customer record (Super Admin only).
        /// This will also delete the associated user record due to foreign key constraints.
        /// </summary>
        /// <param name="id">The customer ID to delete.</param>
        /// <returns>Success status if deletion is successful.</returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            // Verify user is Super Admin
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int currentUserId))
                return Unauthorized(new { message = "Invalid token." });

            var userTypeClaim = User.FindFirst("UserTypeId")?.Value;
            if (!int.TryParse(userTypeClaim, out int userTypeId) || userTypeId != UserTypeConstants.SUPER_ADMIN_USER_TYPE_ID)
            {
                return StatusCode(403, new { message = "Only Super Admin can delete customer records." });
            }

            try
            {
                var result = await _customerService.DeleteCustomerAsync(id);
                if (!result)
                    return NotFound(new { message = "Customer not found." });

                return Ok(new { message = "Customer deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete customer: " + ex.Message });
            }
        }

    }
}