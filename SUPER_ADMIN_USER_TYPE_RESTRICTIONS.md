# Super Admin User Type Restrictions Implementation

## Overview

This implementation adds restrictions to prevent **Super Admin** users from creating new user types beyond the existing two default user types: "Super Admin" and "Property Manager". This ensures system integrity and prevents unauthorized expansion of user type hierarchy.

## Problem Solved

Before this implementation, Super Admin users could create unlimited new user types, which could potentially:
- Complicate the system's user management
- Create security vulnerabilities
- Lead to inconsistent access control
- Allow creation of user types that bypass intended restrictions

## Solution Architecture

### 1. Core Validation Method: `IsSuperAdminOperationAllowedAsync`

**Location**: `Modules/UserTypeCRUD/Services/UserTypeService.cs`

```csharp
private async Task<bool> IsSuperAdminOperationAllowedAsync(int userId, string operation, int? targetUserTypeId = null)
```

**Purpose**: Validates whether a Super Admin user is allowed to perform specific operations on user types.

**Rules**:
- **CREATE**: Super Admin is **NOT allowed** to create new user types
- **UPDATE**: Super Admin can **only update** existing default user types (Super Admin, Property Manager)
- **DELETE**: Super Admin can **only delete** non-essential user types (cannot delete Super Admin or Property Manager)

### 2. Service Layer Restrictions

#### CreateUserTypeAsync Method
- **Protection**: Prevents Super Admin from creating any new user types
- **Error Message**: "Super Admin users are not allowed to create new user types. Only the existing 'Super Admin' and 'Property Manager' user types are permitted in the system."

#### UpdateUserTypeAsync Method
- **Protection**: Allows Super Admin to update only default user types (ID 1 and 2)
- **Error Message**: "Super Admin users can only modify the existing 'Super Admin' and 'Property Manager' user types."

#### DeleteUserTypeAsync Method
- **Protection**: Prevents Super Admin from deleting essential user types
- **Error Message**: "Super Admin users can only delete non-essential user types. The 'Super Admin' and 'Property Manager' user types cannot be deleted."

### 3. Controller Layer Integration

The existing `UserTypesController` already handles `UnauthorizedAccessException` appropriately:
- Returns HTTP 403 Forbidden status
- Includes the detailed error message
- Maintains proper API response structure

## API Behavior

### Restricted Operations (Super Admin)

| Operation | Endpoint | Allowed | Response |
|-----------|----------|---------|----------|
| Create User Type | `POST /api/usertypes` | ❌ No | 403 Forbidden |
| Update Non-Default User Type | `PUT /api/usertypes/{id}` (id > 2) | ❌ No | 403 Forbidden |
| Delete Default User Type | `DELETE /api/usertypes/1` or `/api/usertypes/2` | ❌ No | 403 Forbidden |

### Allowed Operations (Super Admin)

| Operation | Endpoint | Allowed | Response |
|-----------|----------|---------|----------|
| View User Types | `GET /api/usertypes` | ✅ Yes | 200 OK |
| View Specific User Type | `GET /api/usertypes/{id}` | ✅ Yes | 200 OK |
| Update Super Admin Type | `PUT /api/usertypes/1` | ✅ Yes | 200 OK |
| Update Property Manager Type | `PUT /api/usertypes/2` | ✅ Yes | 200 OK |

## User Type Constants

The system uses predefined constants for the two allowed user types:

```csharp
// In UserTypeConstants.cs
public const int SUPER_ADMIN_USER_TYPE_ID = 1;
public const int PROPERTY_MANAGER_USER_TYPE_ID = 2;
public const string SUPER_ADMIN_USER_TYPE_NAME = "Super Admin";
public const string PROPERTY_MANAGER_USER_TYPE_NAME = "Property Manager";
```

## Impact on Other User Types

### Property Manager Users
- **No Change**: Property Managers retain their existing restrictions
- Still cannot create user types with "Property Manager" related names
- Can create other user types (subject to their own access controls)

### Other User Types
- **No Change**: Other user types (if any exist) are not affected by these restrictions
- Their existing permissions and access controls remain intact

## Security Benefits

1. **System Integrity**: Prevents unauthorized expansion of user type hierarchy
2. **Access Control**: Maintains the intended two-tier user system
3. **Audit Trail**: Clear error messages for security monitoring
4. **Principle of Least Privilege**: Restricts even Super Admin from system-level changes

## Testing

Use the provided test file `super-admin-usertype-restriction-tests.http` to verify:

1. Super Admin cannot create new user types (403 Forbidden)
2. Super Admin can still update existing default user types (200 OK)
3. Super Admin cannot update non-default user types (403 Forbidden)
4. Super Admin cannot delete default user types (403 Forbidden)
5. Super Admin can still view user types (200 OK)

## Error Handling

All restrictions throw `UnauthorizedAccessException` with descriptive messages:

```csharp
// Creation restriction
throw new UnauthorizedAccessException(
    "Super Admin users are not allowed to create new user types. Only the existing 'Super Admin' and 'Property Manager' user types are permitted in the system."
);

// Update restriction
throw new UnauthorizedAccessException(
    "Super Admin users can only modify the existing 'Super Admin' and 'Property Manager' user types."
);

// Delete restriction
throw new UnauthorizedAccessException(
    "Super Admin users can only delete non-essential user types. The 'Super Admin' and 'Property Manager' user types cannot be deleted."
);
```

## Future Considerations

1. **Configuration**: These restrictions could be made configurable via app settings
2. **Logging**: Add audit logging for attempted unauthorized operations
3. **Monitoring**: Implement alerts for repeated unauthorized access attempts
4. **Documentation**: Update API documentation to reflect these restrictions

## Files Modified

1. `Modules/UserTypeCRUD/Services/UserTypeService.cs`
   - Added `IsSuperAdminOperationAllowedAsync` method
   - Modified `CreateUserTypeAsync` method
   - Modified `UpdateUserTypeAsync` method
   - Modified `DeleteUserTypeAsync` method

## Files Created

1. `super-admin-usertype-restriction-tests.http` - Test cases
2. `SUPER_ADMIN_USER_TYPE_RESTRICTIONS.md` - This documentation

## Validation

To validate the implementation works correctly:

1. Deploy the changes to your development environment
2. Obtain a JWT token for a Super Admin user
3. Run the test cases in `super-admin-usertype-restriction-tests.http`
4. Verify all creation attempts return 403 Forbidden
5. Verify existing user type management still works for default types

The implementation ensures that Super Admin users can only work with the two predefined user types while maintaining all other system functionality.