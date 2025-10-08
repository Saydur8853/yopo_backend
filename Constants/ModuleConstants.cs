namespace YopoBackend.Constants
{
    /// <summary>
    /// Contains static constants for module IDs and related information.
    /// </summary>
    public static class ModuleConstants
    {
        /// <summary>
        /// Module ID for the UserTypeCRUD module.
        /// </summary>
        public const int USER_TYPE_MODULE_ID = 1;

        /// <summary>
        /// Module name for the UserTypeCRUD module.
        /// </summary>
        public const string USER_TYPE_MODULE_NAME = "UserTypeCRUD";

        /// <summary>
        /// Module description for the UserTypeCRUD module.
        /// </summary>
        public const string USER_TYPE_MODULE_DESCRIPTION = "Module for managing user types and their module permissions";

        /// <summary>
        /// Current version of the UserTypeCRUD module.
        /// </summary>
        public const string USER_TYPE_MODULE_VERSION = "1.0.0";

        /// <summary>
        /// Current version of the PropertyManagerCRUD module.
        /// </summary>
        public const string PROPERTY_MANAGER_MODULE_VERSION = "1.0.0";

        /// <summary>
        /// Module ID for the BuildingCRUD module.
        /// </summary>
        public const int BUILDING_MODULE_ID = 4;

        /// <summary>
        /// Module ID for the FloorCRUD module.
        /// </summary>
        public const int FLOOR_MODULE_ID = 5;

        /// <summary>
        /// Module ID for the UnitCRUD module.
        /// </summary>
        public const int UNIT_MODULE_ID = 6;

        /// <summary>
        /// Module ID for the AmenityCRUD module.
        /// </summary>
        public const int AMENITY_MODULE_ID = 7;

        /// <summary>
        /// Module ID for the TenantCRUD module.
        /// </summary>
        public const int TENANT_MODULE_ID = 8;

        /// <summary>
        /// Module name for the BuildingCRUD module.
        /// </summary>
        public const string BUILDING_MODULE_NAME = "BuildingCRUD";

        /// <summary>
        /// Module description for the BuildingCRUD module.
        /// </summary>
        public const string BUILDING_MODULE_DESCRIPTION = "Module for managing buildings with PM data access control";

        /// <summary>
        /// Current version of the BuildingCRUD module.
        /// </summary>
        public const string BUILDING_MODULE_VERSION = "1.0.0";

        /// <summary>
        /// Module ID for the InvitationCRUD module.
        /// </summary>
        public const int INVITATION_MODULE_ID = 2;

        /// <summary>
        /// Module name for the InvitationCRUD module.
        /// </summary>
        public const string INVITATION_MODULE_NAME = "InvitationCRUD";

        /// <summary>
        /// Module description for the InvitationCRUD module.
        /// </summary>
        public const string INVITATION_MODULE_DESCRIPTION = "Module for managing invitations with CRUD operations";

        /// <summary>
        /// Current version of the InvitationCRUD module.
        /// </summary>
        public const string INVITATION_MODULE_VERSION = "1.0.0";

        /// <summary>
        /// Module ID for the UserCRUD module.
        /// </summary>
        public const int USER_MODULE_ID = 3;

        /// <summary>
        /// Module name for the UserCRUD module.
        /// </summary>
        public const string USER_MODULE_NAME = "UserCRUD";

        /// <summary>
        /// Module description for the UserCRUD module.
        /// </summary>
        public const string USER_MODULE_DESCRIPTION = "Module for managing users with authentication and CRUD operations";

        /// <summary>
        /// Current version of the UserCRUD module.
        /// </summary>
        public const string USER_MODULE_VERSION = "1.0.0";


        /// <summary>
        /// Gets all module information as a dictionary for easy access.
        /// Only includes modules that actually exist in the codebase.
        /// </summary>
        public static readonly Dictionary<int, ModuleInfo> Modules = new()
        {
            {
                USER_TYPE_MODULE_ID,
                new ModuleInfo
                {
                    Id = USER_TYPE_MODULE_ID,
                    Name = USER_TYPE_MODULE_NAME,
                    Description = USER_TYPE_MODULE_DESCRIPTION,
                    Version = USER_TYPE_MODULE_VERSION,
                    IsActive = true
                }
            },
            {
                INVITATION_MODULE_ID,
                new ModuleInfo
                {
                    Id = INVITATION_MODULE_ID,
                    Name = INVITATION_MODULE_NAME,
                    Description = INVITATION_MODULE_DESCRIPTION,
                    Version = INVITATION_MODULE_VERSION,
                    IsActive = true
                }
            },
            {
                USER_MODULE_ID,
                new ModuleInfo
                {
                    Id = USER_MODULE_ID,
                    Name = USER_MODULE_NAME,
                    Description = USER_MODULE_DESCRIPTION,
                    Version = USER_MODULE_VERSION,
                    IsActive = true
                }
            },
            {
                BUILDING_MODULE_ID,
                new ModuleInfo
                {
                    Id = BUILDING_MODULE_ID,
                    Name = BUILDING_MODULE_NAME,
                    Description = BUILDING_MODULE_DESCRIPTION,
                    Version = BUILDING_MODULE_VERSION,
                    IsActive = true
                }
            },
            {
                FLOOR_MODULE_ID,
                new ModuleInfo
                {
                    Id = FLOOR_MODULE_ID,
                    Name = "FloorCRUD",
                    Description = "Module for managing floors under buildings",
                    Version = "1.0.0",
                    IsActive = true
                }
            },
            {
                UNIT_MODULE_ID,
                new ModuleInfo
                {
                    Id = UNIT_MODULE_ID,
                    Name = "UnitCRUD",
                    Description = "Module for managing units under floors and buildings",
                    Version = "1.0.0",
                    IsActive = true
                }
            },
            {
                AMENITY_MODULE_ID,
                new ModuleInfo
                {
                    Id = AMENITY_MODULE_ID,
                    Name = "AmenityCRUD",
                    Description = "Module for managing building amenities and facilities",
                    Version = "1.0.0",
                    IsActive = true
                }
            },
            {
                TENANT_MODULE_ID,
                new ModuleInfo
                {
                    Id = TENANT_MODULE_ID,
                    Name = "TenantCRUD",
                    Description = "Module for managing tenants and invitations with PM access control",
                    Version = "1.0.0",
                    IsActive = true
                }
            }
        };

        /// <summary>
        /// Represents module information.
        /// </summary>
        public class ModuleInfo
        {
            /// <summary>
            /// Gets or sets the ID of the module.
            /// </summary>
            public int Id { get; set; }
            /// <summary>
            /// Gets or sets the name of the module.
            /// </summary>
            public string Name { get; set; } = string.Empty;
            /// <summary>
            /// Gets or sets the description of the module.
            /// </summary>
            public string Description { get; set; } = string.Empty;
            /// <summary>
            /// Gets or sets the version of the module.
            /// </summary>
            public string Version { get; set; } = string.Empty;
            /// <summary>
            /// Gets or sets a value indicating whether the module is active.
            /// </summary>
            public bool IsActive { get; set; }
        }
    }
}
