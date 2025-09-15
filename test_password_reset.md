# Password Reset Functionality for Super Admin

## Overview
The YopoBackend now includes a password reset functionality that allows Super Admin users to reset any user's password without requiring the current password.

## New API Endpoint

### POST `/api/users/{id}/reset-password`
- **Description**: Resets a user's password (Super Admin only)
- **Authorization**: Bearer token required
- **Permissions**: Only users with User Type ID = 1 (Super Admin) can use this endpoint
- **Content-Type**: application/json

#### Request Body
```json
{
  "newPassword": "Imran12345#"
}
```

#### Response Examples

**Success (200)**:
```json
{
  "message": "Password reset successfully."
}
```

**Forbidden (403) - Non-Super Admin**:
```json
{
  "message": "Only Super Admin can reset user passwords."
}
```

**Not Found (404)**:
```json
{
  "message": "User not found."
}
```

**Bad Request (400) - Invalid Password**:
```json
{
  "newPassword": [
    "New password must contain at least one uppercase letter, one lowercase letter, one number, and one special character"
  ]
}
```

## Password Requirements
The new password must:
- Be at least 8 characters long
- Contain at least one uppercase letter
- Contain at least one lowercase letter  
- Contain at least one number
- Contain at least one special character

## Example Usage with curl

### 1. Login as Super Admin
```bash
curl -X POST "http://localhost:5206/api/users/login" \
  -H "Content-Type: application/json" \
  -d "{\"emailAddress\":\"saydurrahman440@gmail.com\",\"password\":\"Imran12345#\"}"
```

### 2. Reset User Password (using the token from login)
```bash
curl -X POST "http://localhost:5206/api/users/{userId}/reset-password" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE" \
  -d "{\"newPassword\":\"NewPassword123#\"}"
```

## Implementation Details

### Access Control
- Only users with `UserTypeId = 1` (Super Admin) can reset passwords
- Access control is enforced at both the service layer and controller layer
- Non-Super Admin users will receive a 403 Forbidden response

### Security Features
- Password is hashed using BCrypt before storing
- All password reset operations are logged with details of who performed the reset
- JWT token authentication required
- Input validation ensures password complexity requirements

### Database Changes
- No database schema changes required
- Uses existing User table and password hashing mechanism
- Updates the `UpdatedAt` timestamp when password is reset

## Frontend Integration
The frontend "Manage Users" interface now shows different password management options based on user permissions:

### For Current User (Any User Type):
- **"Change Password" button** appears on their own user profile
- Opens a modal requiring:
  1. Current password (for verification)
  2. New password (meeting requirements)
- Sends POST request to `/api/users/change-password`
- Available to all authenticated users for their own account

### For Super Admin Users:
- **"Reset Password" button** appears for OTHER users (not their own)
- Opens a modal requiring only:
  1. New password (no current password needed)
- Sends POST request to `/api/users/{userId}/reset-password`
- Only available to users with `userTypeId = 1` (Super Admin)

### User Interface Logic:
```javascript
if (user.id === currentUser.id) {
    // Show "Change Password" button for current user
    showChangePasswordButton();
} else if (currentUser.userTypeId === 1) {
    // Show "Reset Password" button for Super Admin on other users
    showResetPasswordButton();
}
```

## Testing the Implementation
You can test this functionality by:

1. Starting the application: `dotnet run`
2. The application will be available at `http://localhost:5206`
3. Use the example curl commands above or test through Swagger UI at `http://localhost:5206/swagger`
4. Check the console logs to see password reset confirmations