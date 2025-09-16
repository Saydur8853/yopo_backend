namespace YopoBackend.Constants
{
    /// <summary>
    /// Contains static constants for default user types and related information.
    /// </summary>
    public static class UserTypeConstants
    {
        /// <summary>
        /// User Type ID for the default Super Admin user type.
        /// </summary>
        public const int SUPER_ADMIN_USER_TYPE_ID = 1;

        /// <summary>
        /// User Type name for the default Super Admin user type.
        /// </summary>
        public const string SUPER_ADMIN_USER_TYPE_NAME = "Super Admin";

        /// <summary>
        /// User Type description for the default Super Admin user type.
        /// </summary>
        public const string SUPER_ADMIN_USER_TYPE_DESCRIPTION = "Default super administrator with access to all modules and system functions";

        /// <summary>
        /// User Type ID for the Property Manager user type.
        /// </summary>
        public const int PROPERTY_MANAGER_USER_TYPE_ID = 2;

        /// <summary>
        /// User Type name for the Property Manager user type.
        /// </summary>
        public const string PROPERTY_MANAGER_USER_TYPE_NAME = "Property Manager";

        /// <summary>
        /// User Type description for the Property Manager user type.
        /// </summary>
        public const string PROPERTY_MANAGER_USER_TYPE_DESCRIPTION = "Property manager with access to all modules but limited to managing only their own created data";

        /// <summary>
        /// Gets all default user type information as a dictionary for easy access.
        /// </summary>
        public static readonly Dictionary<int, UserTypeInfo> DefaultUserTypes = new()
        {
            {
                SUPER_ADMIN_USER_TYPE_ID,
                new UserTypeInfo
                {
                    Id = SUPER_ADMIN_USER_TYPE_ID,
                    Name = SUPER_ADMIN_USER_TYPE_NAME,
                    Description = SUPER_ADMIN_USER_TYPE_DESCRIPTION,
                    IsActive = true,
                    HasAllModuleAccess = true
                }
            },
            {
                PROPERTY_MANAGER_USER_TYPE_ID,
                new UserTypeInfo
                {
                    Id = PROPERTY_MANAGER_USER_TYPE_ID,
                    Name = PROPERTY_MANAGER_USER_TYPE_NAME,
                    Description = PROPERTY_MANAGER_USER_TYPE_DESCRIPTION,
                    IsActive = true,
                    HasAllModuleAccess = true
                }
            }
        };

        /// <summary>
        /// Represents default user type information.
        /// </summary>
        public class UserTypeInfo
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public bool IsActive { get; set; }
            public bool HasAllModuleAccess { get; set; }
        }
    }
}
