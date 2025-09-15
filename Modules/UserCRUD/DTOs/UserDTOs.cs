using System.ComponentModel.DataAnnotations;

namespace YopoBackend.Modules.UserCRUD.DTOs
{
    // Request DTOs

    /// <summary>
    /// DTO for user registration request.
    /// </summary>
    public class RegisterUserRequestDTO
    {
        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        [MaxLength(255, ErrorMessage = "Email address cannot exceed 255 characters")]
        public string EmailAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password of the user.
        /// Must contain at least 8 characters with uppercase, lowercase, number and special character.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{8,}$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the phone number of the user.
        /// </summary>
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

    }


    /// <summary>
    /// DTO for user login request.
    /// </summary>
    public class LoginRequestDTO
    {
        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string EmailAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password of the user.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for creating a new user.
    /// </summary>
    public class CreateUserRequestDTO
    {
        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        [MaxLength(255, ErrorMessage = "Email address cannot exceed 255 characters")]
        public string EmailAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password of the user.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the phone number of the user.
        /// </summary>
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the profile photo URL/path of the user.
        /// </summary>
        [MaxLength(1000, ErrorMessage = "Profile photo path cannot exceed 1000 characters")]
        public string? ProfilePhoto { get; set; }

        /// <summary>
        /// Gets or sets the user type ID that determines the user's role and permissions.
        /// </summary>
        [Required(ErrorMessage = "User type is required")]
        [Range(1, int.MaxValue, ErrorMessage = "User type ID must be a positive number")]
        public int UserTypeId { get; set; }

        /// <summary>
        /// Gets or sets whether the user account is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the user's email has been verified.
        /// </summary>
        public bool IsEmailVerified { get; set; } = false;
    }

    /// <summary>
    /// DTO for updating an existing user.
    /// </summary>
    public class UpdateUserRequestDTO
    {
        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the phone number of the user.
        /// </summary>
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the profile photo URL/path of the user.
        /// </summary>
        [MaxLength(1000, ErrorMessage = "Profile photo path cannot exceed 1000 characters")]
        public string? ProfilePhoto { get; set; }

        /// <summary>
        /// Gets or sets the user type ID that determines the user's role and permissions.
        /// </summary>
        [Required(ErrorMessage = "User type is required")]
        [Range(1, int.MaxValue, ErrorMessage = "User type ID must be a positive number")]
        public int UserTypeId { get; set; }

        /// <summary>
        /// Gets or sets whether the user account is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets whether the user's email has been verified.
        /// </summary>
        public bool IsEmailVerified { get; set; }
    }

    /// <summary>
    /// DTO for changing user password.
    /// </summary>
    public class ChangePasswordRequestDTO
    {
        /// <summary>
        /// Gets or sets the current password of the user.
        /// </summary>
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the new password of the user.
        /// Must contain at least 8 characters with uppercase, lowercase, number and special character.
        /// </summary>
        [Required(ErrorMessage = "New password is required")]
        [MinLength(8, ErrorMessage = "New password must be at least 8 characters long")]
        [MaxLength(100, ErrorMessage = "New password cannot exceed 100 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{8,}$", 
            ErrorMessage = "New password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
        public string NewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for resetting user password (Super Admin only).
    /// </summary>
    public class ResetPasswordRequestDTO
    {
        /// <summary>
        /// Gets or sets the new password of the user.
        /// Must contain at least 8 characters with uppercase, lowercase, number and special character.
        /// </summary>
        [Required(ErrorMessage = "New password is required")]
        [MinLength(8, ErrorMessage = "New password must be at least 8 characters long")]
        [MaxLength(100, ErrorMessage = "New password cannot exceed 100 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{8,}$", 
            ErrorMessage = "New password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
        public string NewPassword { get; set; } = string.Empty;
    }

    // Response DTOs

    /// <summary>
    /// DTO for authentication response with JWT token.
    /// </summary>
    public class AuthenticationResponseDTO
    {
        /// <summary>
        /// Gets or sets the JWT authentication token.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets when the token expires.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets the user information.
        /// </summary>
        public UserResponseDTO User { get; set; } = new();

        /// <summary>
        /// Gets or sets the success message.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for user response.
    /// </summary>
    public class UserResponseDTO
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string EmailAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the full name of the user.
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the phone number of the user.
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the profile photo URL/path of the user.
        /// </summary>
        public string? ProfilePhoto { get; set; }

        /// <summary>
        /// Gets or sets the user type ID that determines the user's role and permissions.
        /// </summary>
        public int UserTypeId { get; set; }

        /// <summary>
        /// Gets or sets the user type name.
        /// </summary>
        public string UserTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the user account is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets whether the user's email has been verified.
        /// </summary>
        public bool IsEmailVerified { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user last logged in.
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the list of modules that the user has access to based on their user type.
        /// </summary>
        public List<PermittedModuleDto> PermittedModules { get; set; } = new();
    }

    /// <summary>
    /// DTO for representing a module permission.
    /// </summary>
    public class PermittedModuleDto
    {
        /// <summary>
        /// Gets or sets the module ID.
        /// </summary>
        public string ModuleId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the module name.
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for paginated user list response.
    /// </summary>
    public class UserListResponseDTO
    {
        /// <summary>
        /// Gets or sets the list of users.
        /// </summary>
        public List<UserResponseDTO> Users { get; set; } = new();

        /// <summary>
        /// Gets or sets the total number of users.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets or sets whether there is a next page.
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Gets or sets whether there is a previous page.
        /// </summary>
        public bool HasPreviousPage { get; set; }
    }
}
