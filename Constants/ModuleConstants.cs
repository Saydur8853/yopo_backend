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
        /// Module ID for the BuildingCRUD module.
        /// </summary>
        public const int BUILDING_MODULE_ID = 4;

        /// <summary>
        /// Module name for the BuildingCRUD module.
        /// </summary>
        public const string BUILDING_MODULE_NAME = "BuildingCRUD";

        /// <summary>
        /// Module description for the BuildingCRUD module.
        /// </summary>
        public const string BUILDING_MODULE_DESCRIPTION = "Module for managing buildings with CRUD operations";

        /// <summary>
        /// Current version of the BuildingCRUD module.
        /// </summary>
        public const string BUILDING_MODULE_VERSION = "1.0.0";

        /// <summary>
        /// Module ID for the TenantCRUD module.
        /// </summary>
        public const int TENANT_MODULE_ID = 5;

        /// <summary>
        /// Module name for the TenantCRUD module.
        /// </summary>
        public const string TENANT_MODULE_NAME = "TenantCRUD";

        /// <summary>
        /// Module description for the TenantCRUD module.
        /// </summary>
        public const string TENANT_MODULE_DESCRIPTION = "Module for managing tenants with CRUD operations";

        /// <summary>
        /// Current version of the TenantCRUD module.
        /// </summary>
        public const string TENANT_MODULE_VERSION = "1.0.0";

        /// <summary>
        /// Module ID for the CustomerCRUD module.
        /// </summary>
        public const int CUSTOMER_MODULE_ID = 6;

        /// <summary>
        /// Module name for the CustomerCRUD module.
        /// </summary>
        public const string CUSTOMER_MODULE_NAME = "CustomerCRUD";

        /// <summary>
        /// Module description for the CustomerCRUD module.
        /// </summary>
        public const string CUSTOMER_MODULE_DESCRIPTION = "Module for managing customers with CRUD operations";

        /// <summary>
        /// Current version of the CustomerCRUD module.
        /// </summary>
        public const string CUSTOMER_MODULE_VERSION = "1.0.0";

        /// <summary>
        /// Module ID for the InvoiceCRUD module.
        /// </summary>
        public const int INVOICE_MODULE_ID = 7;

        /// <summary>
        /// Module name for the InvoiceCRUD module.
        /// </summary>
        public const string INVOICE_MODULE_NAME = "InvoiceCRUD";

        /// <summary>
        /// Module description for the InvoiceCRUD module.
        /// </summary>
        public const string INVOICE_MODULE_DESCRIPTION = "Module for managing invoices with CRUD operations";

        /// <summary>
        /// Current version of the InvoiceCRUD module.
        /// </summary>
        public const string INVOICE_MODULE_VERSION = "1.0.0";

        /// <summary>
        /// Module ID for the CCTVcrud module.
        /// </summary>
        public const int CCTVcrudModuleId = 8;

        /// <summary>
        /// Module name for the CCTVcrud module.
        /// </summary>
        public const string CCTV_MODULE_NAME = "CCTVcrud";

        /// <summary>
        /// Module description for the CCTVcrud module.
        /// </summary>
        public const string CCTV_MODULE_DESCRIPTION = "Module for managing CCTV cameras with CRUD operations and monitoring capabilities";

        /// <summary>
        /// Current version of the CCTVcrud module.
        /// </summary>
        public const string CCTV_MODULE_VERSION = "1.0.0";

        /// <summary>
        /// Module ID for the IntercomCRUD module.
        /// </summary>
        public const int INTERCOM_MODULE_ID = 9;

        /// <summary>
        /// Module name for the IntercomCRUD module.
        /// </summary>
        public const string INTERCOM_MODULE_NAME = "IntercomCRUD";

        /// <summary>
        /// Module description for the IntercomCRUD module.
        /// </summary>
        public const string INTERCOM_MODULE_DESCRIPTION = "Module for managing intercom systems with CRUD operations and maintenance tracking";

        /// <summary>
        /// Current version of the IntercomCRUD module.
        /// </summary>
        public const string INTERCOM_MODULE_VERSION = "1.0.0";

        /// <summary>
        /// Module ID for the VirtualKeyCRUD module.
        /// </summary>
        public const int VIRTUAL_KEY_MODULE_ID = 10;

        /// <summary>
        /// Module name for the VirtualKeyCRUD module.
        /// </summary>
        public const string VIRTUAL_KEY_MODULE_NAME = "VirtualKeyCRUD";

        /// <summary>
        /// Module description for the VirtualKeyCRUD module.
        /// </summary>
        public const string VIRTUAL_KEY_MODULE_DESCRIPTION = "Module for managing virtual keys with CRUD operations and access control";

        /// <summary>
        /// Current version of the VirtualKeyCRUD module.
        /// </summary>
        public const string VIRTUAL_KEY_MODULE_VERSION = "1.0.0";

        /// <summary>
        /// Gets all module information as a dictionary for easy access.
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
                TENANT_MODULE_ID,
                new ModuleInfo
                {
                    Id = TENANT_MODULE_ID,
                    Name = TENANT_MODULE_NAME,
                    Description = TENANT_MODULE_DESCRIPTION,
                    Version = TENANT_MODULE_VERSION,
                    IsActive = true
                }
            },
            {
                CUSTOMER_MODULE_ID,
                new ModuleInfo
                {
                    Id = CUSTOMER_MODULE_ID,
                    Name = CUSTOMER_MODULE_NAME,
                    Description = CUSTOMER_MODULE_DESCRIPTION,
                    Version = CUSTOMER_MODULE_VERSION,
                    IsActive = true
                }
            },
            {
                INVOICE_MODULE_ID,
                new ModuleInfo
                {
                    Id = INVOICE_MODULE_ID,
                    Name = INVOICE_MODULE_NAME,
                    Description = INVOICE_MODULE_DESCRIPTION,
                    Version = INVOICE_MODULE_VERSION,
                    IsActive = true
                }
            },
            {
                CCTVcrudModuleId,
                new ModuleInfo
                {
                    Id = CCTVcrudModuleId,
                    Name = CCTV_MODULE_NAME,
                    Description = CCTV_MODULE_DESCRIPTION,
                    Version = CCTV_MODULE_VERSION,
                    IsActive = true
                }
            },
            {
                INTERCOM_MODULE_ID,
                new ModuleInfo
                {
                    Id = INTERCOM_MODULE_ID,
                    Name = INTERCOM_MODULE_NAME,
                    Description = INTERCOM_MODULE_DESCRIPTION,
                    Version = INTERCOM_MODULE_VERSION,
                    IsActive = true
                }
            },
            {
                VIRTUAL_KEY_MODULE_ID,
                new ModuleInfo
                {
                    Id = VIRTUAL_KEY_MODULE_ID,
                    Name = VIRTUAL_KEY_MODULE_NAME,
                    Description = VIRTUAL_KEY_MODULE_DESCRIPTION,
                    Version = VIRTUAL_KEY_MODULE_VERSION,
                    IsActive = true
                }
            }
        };

        /// <summary>
        /// Represents module information.
        /// </summary>
        public class ModuleInfo
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Version { get; set; } = string.Empty;
            public bool IsActive { get; set; }
        }
    }
}
