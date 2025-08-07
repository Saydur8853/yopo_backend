# Module Management System

This document describes the module management system implemented in Yopo Backend API.

## Overview

The module management system provides a structured way to manage modules in the application with static module IDs defined in code, a database table for module metadata, and APIs to retrieve module information.

## Components

### 1. Module Constants (`YopoBackend/Constants/ModuleConstants.cs`)

Contains static constants for all modules in the system:

- `INVITATION_MODULE_ID = 2` - Module ID for InvitationCRUD module
- `INVITATION_MODULE_NAME = "InvitationCRUD"` - Module name
- `INVITATION_MODULE_DESCRIPTION` - Module description
- `INVITATION_MODULE_VERSION = "1.0.0"` - Module version

### 2. Module Model (`YopoBackend/Models/Module.cs`)

Database entity representing modules with properties:
- `Id` - Unique module identifier (manually set, not auto-generated)
- `Name` - Module display name
- `Description` - Module description
- `IsActive` - Whether the module is enabled
- `Version` - Module version
- `CreatedAt` - Creation timestamp
- `UpdatedAt` - Last update timestamp

### 3. Module APIs (`YopoBackend/Controllers/ModulesController.cs`)

Provides REST endpoints for module management:

#### Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/modules` | Get all modules |
| GET | `/api/modules/{id}` | Get specific module by ID |
| GET | `/api/modules/active` | Get all active modules |
| POST | `/api/modules/initialize` | Initialize/sync modules from constants |

#### Example Responses

**GET /api/modules**
```json
{
  "modules": [
    {
      "id": 2,
      "name": "InvitationCRUD",
      "description": "Module for managing invitations with CRUD operations",
      "isActive": true,
      "version": "1.0.0",
      "createdAt": "2024-01-07T10:30:00Z",
      "updatedAt": null
    }
  ],
  "totalCount": 1
}
```

**GET /api/modules/2**
```json
{
  "id": 2,
  "name": "InvitationCRUD",
  "description": "Module for managing invitations with CRUD operations",
  "isActive": true,
  "version": "1.0.0",
  "createdAt": "2024-01-07T10:30:00Z",
  "updatedAt": null
}
```

### 4. Database Integration

- **Modules Table**: Stores module metadata in the database
- **Auto-initialization**: Modules are automatically created/updated from constants during app startup
- **Manual sync**: Use `POST /api/modules/initialize` to force sync

## How Module IDs Are Set

1. **Static Definition**: Module IDs are defined as constants in `ModuleConstants.cs`
2. **Database Sync**: The `ModuleService.InitializeModulesAsync()` method syncs constants to database
3. **Usage in Code**: Controllers and services reference `ModuleConstants.INVITATION_MODULE_ID` instead of hardcoded values

## Adding New Modules

To add a new module:

1. **Add constants** in `ModuleConstants.cs`:
   ```csharp
   public const int NEW_MODULE_ID = 3;
   public const string NEW_MODULE_NAME = "NewModule";
   // ... other constants
   ```

2. **Update Modules dictionary** in `ModuleConstants.cs`:
   ```csharp
   public static readonly Dictionary<int, ModuleInfo> Modules = new()
   {
       { INVITATION_MODULE_ID, new ModuleInfo { ... } },
       { NEW_MODULE_ID, new ModuleInfo { ... } }  // Add this
   };
   ```

3. **Register services** in `Program.cs`:
   ```csharp
   // Module: NewModule (Module ID: 3 - defined in ModuleConstants.NEW_MODULE_ID)
   builder.Services.AddScoped<INewModuleService, NewModuleService>();
   ```

4. **Reference constants** in your module's controller:
   ```csharp
   /// <summary>
   /// Controller for NewModule (Module ID: {ModuleConstants.NEW_MODULE_ID})
   /// </summary>
   ```

## Testing

Use the provided HTTP test file `module-api-tests.http` to test the module APIs:

```http
### Get all modules
GET http://localhost:5000/api/modules

### Get InvitationCRUD module (ID: 2)
GET http://localhost:5000/api/modules/2

### Get active modules only
GET http://localhost:5000/api/modules/active
```

## Benefits

1. **Centralized Configuration**: All module IDs in one place
2. **Type Safety**: Compile-time checking of module IDs
3. **Database Persistence**: Module metadata stored in database
4. **API Access**: RESTful APIs to query module information
5. **Auto-sync**: Modules automatically initialized on startup
6. **Version Tracking**: Track module versions and updates
