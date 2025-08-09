# YopoBackend - Data Access Control System

## Overview

YopoBackend includes a sophisticated Data Access Control system that allows you to control whether users of the same user type can access each other's data or only their own data. This is particularly useful when you have multiple users with the same role but need to segregate their data access.

## Data Access Control Types

The system supports two types of data access control:

1. **"ALL"** - Users can access all data for their user type (default behavior)
2. **"OWN"** - Users can only access data they created themselves

## Your Use Case Example

Let's walk through your specific scenario:

### Scenario Setup
- **User Type**: "Security Admin"  
- **Module Access**: Only "BuildingCRUD" module
- **Users**: 
  - saydur@gmail.com (creates some building data)
  - rahim@gmail.com (should/shouldn't see saydur's data based on access control)

## How to Use the Data Access Control Feature

### 1. Creating User Types with Data Access Control

#### Option A: ALL Access (Users can see each other's data)
```json
POST /api/usertype
{
  "name": "Security Admin",
  "description": "Security administrators with building access",
  "moduleIds": [4], // BuildingCRUD module ID
  "dataAccessControl": "ALL"
}
```

#### Option B: OWN Access (Users can only see their own data)
```json
POST /api/usertype
{
  "name": "Security Admin Private",
  "description": "Security administrators with private building access",
  "moduleIds": [4], // BuildingCRUD module ID
  "dataAccessControl": "OWN"
}
```

### 2. Creating Users for the User Type

Create users and assign them to the user type:

```json
POST /api/auth/register
{
  "firstName": "Saydur",
  "lastName": "Rahman",
  "emailAddress": "saydur@gmail.com",
  "password": "SecurePassword123!",
  "userTypeId": 1 // The Security Admin user type ID
}
```

```json
POST /api/auth/register
{
  "firstName": "Rahim",
  "lastName": "Ahmed", 
  "emailAddress": "rahim@gmail.com",
  "password": "SecurePassword123!",
  "userTypeId": 1 // Same Security Admin user type ID
}
```

### 3. Understanding Data Access Behavior

#### When DataAccessControl = "ALL"
- ✅ Saydur can see all buildings in the system
- ✅ Rahim can see all buildings in the system (including Saydur's buildings)
- ✅ Both users see the same data set

#### When DataAccessControl = "OWN"  
- ✅ Saydur can only see buildings he created
- ✅ Rahim can only see buildings he created
- ❌ Rahim cannot see Saydur's buildings
- ❌ Saydur cannot see Rahim's buildings

## API Endpoints

### User Type Management

#### Create User Type
```http
POST /api/usertype
Content-Type: application/json

{
  "name": "Security Admin",
  "description": "Security administrators",
  "moduleIds": [4],
  "dataAccessControl": "OWN" // or "ALL"
}
```

#### Update User Type Data Access Control
```http
PUT /api/usertype/{id}
Content-Type: application/json

{
  "name": "Security Admin",
  "description": "Updated description",
  "moduleIds": [4],
  "dataAccessControl": "ALL" // Changed from OWN to ALL
}
```

#### Get User Type Details
```http
GET /api/usertype/{id}
```

Response:
```json
{
  "id": 1,
  "name": "Security Admin",
  "description": "Security administrators",
  "isActive": true,
  "dataAccessControl": "OWN",
  "moduleIds": [4],
  "moduleNames": ["BuildingCRUD"],
  "createdAt": "2025-01-09T10:00:00Z"
}
```

### Building Data Access (Example)

When users make requests to building endpoints, the system automatically filters data based on their user type's `DataAccessControl` setting:

#### Get Buildings (Filtered by Access Control)
```http
GET /api/building
Authorization: Bearer {jwt_token}
```

**If DataAccessControl = "ALL":**
```json
[
  {
    "id": 1,
    "name": "Building A",
    "createdBy": 1, // Saydur's user ID
    "address": "123 Main St"
  },
  {
    "id": 2, 
    "name": "Building B",
    "createdBy": 2, // Rahim's user ID
    "address": "456 Oak Ave"
  }
]
```

**If DataAccessControl = "OWN" and request from Rahim:**
```json
[
  {
    "id": 2,
    "name": "Building B", 
    "createdBy": 2, // Only Rahim's data
    "address": "456 Oak Ave"
  }
]
```

## Implementation Details

### Database Schema

The `UserTypes` table includes a `DataAccessControl` column:

```sql
CREATE TABLE UserTypes (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(100) NOT NULL UNIQUE,
    Description VARCHAR(500),
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    DataAccessControl VARCHAR(10) NOT NULL DEFAULT 'ALL',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME
);
```

### How It Works

1. **User Authentication**: When a user logs in, their JWT token includes their user type information
2. **Data Filtering**: API endpoints check the user's `DataAccessControl` setting
3. **Query Modification**: 
   - If `DataAccessControl = "ALL"`: No additional filtering
   - If `DataAccessControl = "OWN"`: Add `WHERE CreatedBy = {current_user_id}` to queries

### Code Example (Service Layer)

```csharp
public async Task<List<Building>> GetBuildingsAsync(int userId, int userTypeId)
{
    var userType = await _context.UserTypes.FindAsync(userTypeId);
    var query = _context.Buildings.AsQueryable();
    
    if (userType.DataAccessControl == "OWN")
    {
        query = query.Where(b => b.CreatedBy == userId);
    }
    
    return await query.ToListAsync();
}
```

## Best Practices

### 1. Choose the Right Access Control
- Use **"ALL"** when users should collaborate and share data
- Use **"OWN"** when data should be private to each user

### 2. Consider User Experience
- Clearly communicate to users what data they can access
- Provide appropriate error messages when access is denied

### 3. Audit Trail
- Always track who created data using `CreatedBy` fields
- Log access attempts for security auditing

### 4. Testing Different Scenarios
```bash
# Test with ALL access
curl -X GET "http://localhost:5206/api/building" \
  -H "Authorization: Bearer {saydur_token}"

curl -X GET "http://localhost:5206/api/building" \
  -H "Authorization: Bearer {rahim_token}"

# Compare results to ensure both users see the same data

# Test with OWN access  
# Update user type to use "OWN" access control
curl -X PUT "http://localhost:5206/api/usertype/1" \
  -H "Content-Type: application/json" \
  -d '{"dataAccessControl": "OWN", ...}'

# Test again - users should now see different data sets
```

## Migration and Updates

### Changing Access Control for Existing User Types

You can update existing user types to change their access control:

```http
PUT /api/usertype/1
{
  "name": "Security Admin",
  "description": "Updated access control",
  "moduleIds": [4],
  "dataAccessControl": "OWN" // Changed from ALL to OWN
}
```

⚠️ **Important**: Changing from "ALL" to "OWN" will immediately restrict data access for all users of that type.

## Troubleshooting

### Common Issues

1. **Users can't see any data after changing to "OWN"**
   - Ensure the `CreatedBy` field is properly populated in your data
   - Check that the user ID matches the `CreatedBy` values

2. **Access control not working**
   - Verify the user type's `DataAccessControl` field is set correctly
   - Ensure your API endpoints are checking and applying the access control logic

3. **Database errors after schema changes**
   - Run `dotnet ef database update` to apply pending migrations
   - Check that the `DataAccessControl` column exists in the `UserTypes` table

### Verification Commands

```bash
# Check user type settings
dotnet run -- check-usertype {userTypeId}

# Verify migration status  
dotnet ef migrations list

# Test database connection
dotnet run -- test-connection
```

## Security Considerations

1. **Data Isolation**: The "OWN" access control provides data isolation but is not a replacement for proper authentication and authorization
2. **Audit Logging**: Consider implementing audit logs to track data access patterns  
3. **Regular Reviews**: Periodically review user type access controls to ensure they align with business requirements

This Data Access Control feature gives you fine-grained control over data visibility while maintaining the flexibility to use the same user type for multiple users with different data access needs.

## Quick Testing Guide

Here's a step-by-step guide to test the Data Access Control feature using your specific example:

### Step 1: Start the Application
```bash
dotnet run
```
The API will be available at `http://localhost:5206`

### Step 2: Create a Security Admin User Type
```bash
# Create user type with ALL access (users can see each other's data)
curl -X POST "http://localhost:5206/api/usertype" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Security Admin",
    "description": "Security administrators with building access",
    "moduleIds": [4],
    "dataAccessControl": "ALL"
  }'
```

### Step 3: Register Saydur
```bash
curl -X POST "http://localhost:5206/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Saydur",
    "lastName": "Rahman",
    "emailAddress": "saydur@gmail.com",
    "password": "SecurePassword123!",
    "userTypeId": 1
  }'
```

### Step 4: Register Rahim
```bash
curl -X POST "http://localhost:5206/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Rahim",
    "lastName": "Ahmed",
    "emailAddress": "rahim@gmail.com",
    "password": "SecurePassword123!",
    "userTypeId": 1
  }'
```

### Step 5: Login as Saydur
```bash
curl -X POST "http://localhost:5206/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "emailAddress": "saydur@gmail.com",
    "password": "SecurePassword123!"
  }'

# Save the returned JWT token as SAYDUR_TOKEN
```

### Step 6: Login as Rahim
```bash
curl -X POST "http://localhost:5206/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "emailAddress": "rahim@gmail.com",
    "password": "SecurePassword123!"
  }'

# Save the returned JWT token as RAHIM_TOKEN
```

### Step 7: Create Building as Saydur
```bash
curl -X POST "http://localhost:5206/api/buildings" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {SAYDUR_TOKEN}" \
  -d '{
    "name": "Saydur Building",
    "address": "123 Saydur Street, Dhaka",
    "photo": "https://example.com/photo1.jpg"
  }'
```

### Step 8: Create Building as Rahim
```bash
curl -X POST "http://localhost:5206/api/buildings" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {RAHIM_TOKEN}" \
  -d '{
    "name": "Rahim Complex",
    "address": "456 Rahim Avenue, Chittagong",
    "photo": "https://example.com/photo2.jpg"
  }'
```

### Step 9: Test "ALL" Access Control
```bash
# Get buildings as Saydur (should see both buildings)
curl -X GET "http://localhost:5206/api/buildings" \
  -H "Authorization: Bearer {SAYDUR_TOKEN}"

# Get buildings as Rahim (should see both buildings)
curl -X GET "http://localhost:5206/api/buildings" \
  -H "Authorization: Bearer {RAHIM_TOKEN}"

# Both should return the same list with both buildings
```

### Step 10: Change to "OWN" Access Control
```bash
curl -X PUT "http://localhost:5206/api/usertype/1" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Security Admin",
    "description": "Security administrators with private building access",
    "moduleIds": [4],
    "dataAccessControl": "OWN"
  }'
```

### Step 11: Test "OWN" Access Control
```bash
# Get buildings as Saydur (should see only Saydur's building)
curl -X GET "http://localhost:5206/api/buildings" \
  -H "Authorization: Bearer {SAYDUR_TOKEN}"

# Get buildings as Rahim (should see only Rahim's building)
curl -X GET "http://localhost:5206/api/buildings" \
  -H "Authorization: Bearer {RAHIM_TOKEN}"

# Now each user should see only their own buildings
```

### Expected Results:
- **"ALL" access**: Both users see all buildings in the system
- **"OWN" access**: Each user sees only buildings they created
- **Update/Delete**: Users with "OWN" access can only modify their own buildings
- **Building creation**: Always works regardless of access control type

### Using Postman or Similar API Tools:
1. Import the API endpoints into Postman
2. Set up environment variables for tokens
3. Follow the same steps but use the GUI interface
4. Watch how the responses change when you switch between "ALL" and "OWN" access control
