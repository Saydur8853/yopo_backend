using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YopoBackend.Modules.CustomerCRUD.DTOs;
using YopoBackend.Modules.CustomerCRUD.Services;
using YopoBackend.Services;
using System.Security.Claims;
using YopoBackend.Attributes;

namespace YopoBackend.Modules.CustomerCRUD.Controllers
{
    /// <summary>
    /// Controller for managing customer operations.
    /// Module: CustomerCRUD (Module ID: 6)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [RequireModule(6)] // CustomerCRUD module
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IJwtService _jwtService;

        /// <summary>
        /// Initializes a new instance of the CustomersController class.
        /// </summary>
        /// <param name="customerService">The customer service.</param>
        /// <param name="jwtService">The JWT service for user authentication.</param>
        public CustomersController(ICustomerService customerService, IJwtService jwtService)
        {
            _customerService = customerService;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Gets all customers with pagination and filtering support.
        /// </summary>
        /// <param name="page">The page number (default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10).</param>
        /// <param name="searchTerm">Optional search term to filter by name or address.</param>
        /// <param name="userId">Optional user ID to filter customers by user.</param>
        /// <param name="type">Optional customer type to filter by.</param>
        /// <param name="activeOnly">Whether to include only active customers.</param>
        /// <returns>A paginated list of customers.</returns>
        [HttpGet]
        public async Task<ActionResult<CustomerListResponseDTO>> GetAllCustomers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? userId = null,
            [FromQuery] string? type = null,
            [FromQuery] bool activeOnly = false)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var result = await _customerService.GetAllCustomersAsync(page, pageSize, searchTerm, userId, type, activeOnly);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving customers.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets a customer by ID.
        /// </summary>
        /// <param name="id">The customer ID.</param>
        /// <returns>The customer if found.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerResponseDTO>> GetCustomerById(int id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                {
                    return NotFound(new { message = $"Customer with ID {id} not found." });
                }

                return Ok(customer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the customer.", details = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new customer.
        /// </summary>
        /// <param name="createCustomerDto">The customer creation data.</param>
        /// <returns>The created customer.</returns>
        [HttpPost]
        public async Task<ActionResult<CustomerResponseDTO>> CreateCustomer([FromBody] CreateCustomerDTO createCustomerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentUserId = GetCurrentUserId();
                var customer = await _customerService.CreateCustomerAsync(createCustomerDto, currentUserId);

                return CreatedAtAction(nameof(GetCustomerById), new { id = customer.CustomerId }, customer);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the customer.", details = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing customer.
        /// </summary>
        /// <param name="id">The customer ID.</param>
        /// <param name="updateCustomerDto">The customer update data.</param>
        /// <returns>The updated customer.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<CustomerResponseDTO>> UpdateCustomer(int id, [FromBody] UpdateCustomerDTO updateCustomerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var customer = await _customerService.UpdateCustomerAsync(id, updateCustomerDto);
                if (customer == null)
                {
                    return NotFound(new { message = $"Customer with ID {id} not found." });
                }

                return Ok(customer);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the customer.", details = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a customer by ID.
        /// </summary>
        /// <param name="id">The customer ID.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCustomer(int id)
        {
            try
            {
                var success = await _customerService.DeleteCustomerAsync(id);
                if (!success)
                {
                    return NotFound(new { message = $"Customer with ID {id} not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the customer.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets all customers managed by a specific user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="page">The page number (default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10).</param>
        /// <returns>A paginated list of customers managed by the user.</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<CustomerListResponseDTO>> GetCustomersByUserId(
            int userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var result = await _customerService.GetCustomersByUserIdAsync(userId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving customers for the user.", details = ex.Message });
            }
        }

        /// <summary>
        /// Activates or deactivates a customer.
        /// </summary>
        /// <param name="id">The customer ID.</param>
        /// <param name="isActive">Whether to activate or deactivate the customer.</param>
        /// <returns>No content if successful.</returns>
        [HttpPatch("{id}/active")]
        public async Task<ActionResult> SetCustomerActiveStatus(int id, [FromBody] bool isActive)
        {
            try
            {
                var success = await _customerService.SetCustomerActiveStatusAsync(id, isActive);
                if (!success)
                {
                    return NotFound(new { message = $"Customer with ID {id} not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the customer status.", details = ex.Message });
            }
        }

        /// <summary>
        /// Updates the payment status of a customer.
        /// </summary>
        /// <param name="id">The customer ID.</param>
        /// <param name="paid">Whether the customer has paid.</param>
        /// <returns>No content if successful.</returns>
        [HttpPatch("{id}/payment")]
        public async Task<ActionResult> UpdatePaymentStatus(int id, [FromBody] bool paid)
        {
            try
            {
                var success = await _customerService.UpdatePaymentStatusAsync(id, paid);
                if (!success)
                {
                    return NotFound(new { message = $"Customer with ID {id} not found." });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the payment status.", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets the current user's ID from the JWT token.
        /// </summary>
        /// <returns>The current user's ID.</returns>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Invalid user token.");
        }
    }
}
