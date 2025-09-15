# Testing Buildings API Integration with Users

## What was implemented:

1. **BuildingSummaryDTO** - A new DTO class for building information in user responses
2. **UserBuildingPermission** - A new model to manage user-building relationships  
3. **Updated UserResponseDTO** - Now includes a `Buildings` property
4. **Updated UserService** - MapToUserResponse method now includes building permissions
5. **Database Table** - UserBuildingPermissions table created with foreign keys

## Database Structure:

The UserBuildingPermissions table has been created with:
- Id (Primary Key)
- UserId (Foreign Key to Users)
- BuildingId (Foreign Key to Buildings)  
- IsActive (Boolean)
- CreatedAt, UpdatedAt (Timestamps)
- Unique constraint on (UserId, BuildingId)

## API Response Format:

The 04-Users API now returns building information in this format:

```json
{
  "id": 1,
  "emailAddress": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "fullName": "John Doe",
  "phoneNumber": "+1234567890",
  "profilePhoto": null,
  "userTypeId": 2,
  "userTypeName": "Building Manager",
  "isActive": true,
  "isEmailVerified": true,
  "lastLoginAt": "2025-01-15T10:30:00Z",
  "createdAt": "2025-01-10T09:00:00Z",
  "updatedAt": "2025-01-15T10:30:00Z",
  "buildings": [
    { "id": "1", "name": "Palm Tower" },
    { "id": "2", "name": "Marina Plaza" }
  ]
}
```

## To test:

1. **Create some building permissions:**
   ```sql
   INSERT INTO UserBuildingPermissions (UserId, BuildingId, IsActive) 
   VALUES (1, 1, 1), (1, 2, 1);
   ```

2. **Call the Users API:**
   - GET /api/users
   - GET /api/users/{id}
   - POST /api/auth/login
   
3. **Verify the response includes the buildings array**

## Usage Examples:

### Adding building permissions for a user:
```sql
-- Give user ID 1 permission to buildings 1 and 2
INSERT INTO UserBuildingPermissions (UserId, BuildingId, IsActive, CreatedAt) 
VALUES 
(1, 1, 1, NOW()),
(1, 2, 1, NOW());
```

### Removing building permissions:
```sql
-- Deactivate permission
UPDATE UserBuildingPermissions 
SET IsActive = 0, UpdatedAt = NOW() 
WHERE UserId = 1 AND BuildingId = 1;

-- Or delete completely
DELETE FROM UserBuildingPermissions 
WHERE UserId = 1 AND BuildingId = 1;
```

The buildings array will automatically populate based on the UserBuildingPermissions table when users are fetched via the API.