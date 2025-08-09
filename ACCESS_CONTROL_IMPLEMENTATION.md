# Access Control Implementation Summary

This document summarizes the comprehensive access control implementation across all modules in the YOPO Backend system.

## Overview

The system now implements two levels of access control:

1. **Module-Level Access Control**: Users must have permission to access specific modules
2. **Data-Level Access Control**: Users can see either their own data ("OWN") or all data ("ALL") based on their user type configuration

## Module Access Control

### RequireModule Attribute
The `RequireModuleAttribute` class provides module-level access control by checking if the current user's user type has permission to access a specific module.

**Location**: `Attributes/RequireModuleAttribute.cs`

**How it works**:
- Checks if user is authenticated
- Retrieves user ID from JWT token claims
- Queries database to verify user has access to the required module through their user type permissions
- Returns `403 Forbidden` if no access, allows request to continue if access is granted

### Implementation Status by Module

#### ✅ 1. UserTypeCRUD Module (ID: 1)
- **Controller**: `Modules/UserTypeCRUD/Controllers/UserTypesController.cs`
- **Status**: ✅ COMPLETE
- **Implementation**: 
  - `[RequireModule(ModuleConstants.USER_TYPE_MODULE_ID)]` applied at controller level
  - All endpoints protected with module access control
  - No data-level access control needed (manages system-wide user types)

#### ✅ 2. InvitationCRUD Module (ID: 2) 
- **Controller**: `Modules/InvitationCRUD/Controllers/InvitationsController.cs`
- **Status**: ✅ COMPLETE
- **Implementation**:
  - `[RequireModule(ModuleConstants.INVITATION_MODULE_ID)]` applied at controller level
  - `[Authorize]` added to ensure authentication
  - All endpoints protected with module access control
  - **Data-level access control**: Can be implemented in service layer if needed

#### ✅ 3. UserCRUD Module (ID: 3)
- **Controller**: `Modules/UserCRUD/Controllers/UsersController.cs` 
- **Status**: ✅ COMPLETE
- **Implementation**:
  - `[RequireModule(ModuleConstants.USER_MODULE_ID)]` applied to CRUD endpoints only
  - Authentication endpoints (login, register, logout) remain unrestricted
  - Public endpoints (check-email, registration-eligibility) remain unrestricted  
  - **Data-level access control**: Can be implemented in service layer if needed

#### ✅ 4. BuildingCRUD Module (ID: 4)
- **Controller**: `Modules/BuildingCRUD/Controllers/BuildingsController.cs`
- **Status**: ✅ COMPLETE (Reference Implementation)
- **Implementation**:
  - `[RequireModule(ModuleConstants.BUILDING_MODULE_ID)]` applied at controller level
  - **Data-level access control**: ✅ FULLY IMPLEMENTED in service layer
    - Users with "OWN" access can only see/modify buildings they created
    - Users with "ALL" access can see/modify all buildings
    - Implemented in all service methods: GetAll, GetById, Update, Delete

## Data Access Control Implementation

### UserType.DataAccessControl Property
Each user type has a `DataAccessControl` property that determines data visibility:
- `"OWN"`: User can only access data they created (CreatedBy = UserId)
- `"ALL"`: User can access all data for their user type

### Building Module - Reference Implementation
The BuildingCRUD module serves as the reference implementation for data-level access control:

**Location**: `Modules/BuildingCRUD/Services/BuildingService.cs`

**Key Methods**:
- `GetBuildingsBasedOnUserAccess()`: Filters buildings based on user's access level
- `GetBuildingByIdAsync()`: Respects access control when retrieving single building
- `UpdateBuildingAsync()`: Only allows updates to accessible buildings
- `DeleteBuildingAsync()`: Only allows deletion of accessible buildings

**Implementation Pattern**:
```csharp
// Get user with their user type information
var user = await _context.Users
    .Include(u => u.UserType)
    .FirstOrDefaultAsync(u => u.Id == userId);

if (user?.UserType == null)
{
    return null; // No access if user or user type not found
}

var query = _context.Buildings.AsQueryable();

// Apply access control based on user type's DataAccessControl setting
if (user.UserType.DataAccessControl == "OWN")
{
    // User can only see buildings they created
    query = query.Where(b => b.CreatedBy == userId);
}
// If DataAccessControl is "ALL", user can see all buildings (no additional filtering needed)
```

## Next Steps for Complete Implementation

### For Other Modules Needing Data-Level Access Control:

1. **UserCRUD Module**: 
   - Consider if users should be able to see only users they created or all users
   - Implement similar pattern in `UserService.cs` if needed

2. **InvitationCRUD Module**:
   - Consider if users should see only invitations they created or all invitations  
   - Implement similar pattern in `InvitationService.cs` if needed

3. **UserTypeCRUD Module**:
   - Generally should remain "ALL" access as it manages system-wide configuration
   - No changes needed unless specific requirements exist

## Security Features

### Authentication & Authorization
- JWT-based authentication
- Role-based authorization through user types
- Module permissions system
- Token revocation support

### Access Control Layers
1. **Authentication**: User must be logged in (JWT token required)
2. **Module Access**: User's user type must have permission to access the module
3. **Data Access**: User can only see data according to their access control level ("OWN" vs "ALL")

### Error Handling
- `401 Unauthorized`: Not authenticated
- `403 Forbidden`: Authenticated but no module access
- `404 Not Found`: Data doesn't exist or user doesn't have access to it

## Module Constants Reference

```csharp
ModuleConstants.USER_TYPE_MODULE_ID = 1    // UserTypeCRUD
ModuleConstants.INVITATION_MODULE_ID = 2   // InvitationCRUD  
ModuleConstants.USER_MODULE_ID = 3         // UserCRUD
ModuleConstants.BUILDING_MODULE_ID = 4     // BuildingCRUD
```

## Testing Access Control

To test the access control implementation:

1. Create different user types with different module permissions
2. Create users with different user types  
3. Set different `DataAccessControl` values ("OWN" vs "ALL")
4. Test that users can only access modules and data according to their permissions

## Conclusion

✅ **Module-level access control** is now implemented for all modules
✅ **Data-level access control** is fully implemented for BuildingCRUD (reference implementation)
⚡ **Data-level access control** can be easily added to other modules following the BuildingCRUD pattern

The system now provides comprehensive, multi-layered security that can be easily extended and customized based on business requirements.
