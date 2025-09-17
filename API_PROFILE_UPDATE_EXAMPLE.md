# Profile Update API - PUT /api/users/me

## Updated Request Structure

The profile update endpoint now accepts the following user-editable fields:

### Request Body Example:
```json
{
  "email": "user@example.com",
  "password": "newPassword123",
  "name": "John Doe",
  "address": "123 Main St, City, Country",
  "phoneNumber": "+1234567890",
  "profilePhotoBase64": "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD..."
}
```

### Field Descriptions:

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `email` | string | ✅ Yes | User's email address. Must be unique across all users |
| `password` | string | ❌ No | New password. Leave empty to keep current password |
| `name` | string | ✅ Yes | User's full name |
| `address` | string | ❌ No | User's address |
| `phoneNumber` | string | ❌ No | User's phone number |
| `profilePhotoBase64` | string | ❌ No | Base64 encoded profile photo |

### Key Features:

1. **Email Uniqueness Validation**: The system checks that the new email isn't already used by another user
2. **Optional Password Update**: Users can change their password by providing a new one, or leave it empty to keep current
3. **Profile Photo Integration**: Can update profile photo as part of the same request
4. **Removed Admin Fields**: No longer requires `userTypeId`, `isActive`, or `isEmailVerified`

### Frontend Changes:

The profile form now includes:
- ✅ Editable email field
- ✅ Optional password field with hint "Leave empty to keep current password"
- ✅ All other existing fields (name, address, phone, photo)

### Security:
- JWT authentication required
- Users can only update their own profile
- Email uniqueness enforced
- Password hashing handled automatically
- Profile photo validation (file type, size limits)

## Migration from Previous Version:

**Before** (required admin fields):
```json
{
  "name": "John Doe",
  "phoneNumber": "+1234567890", 
  "address": "123 Main St",
  "userTypeId": 2147483647,    // ❌ Removed
  "isActive": true,            // ❌ Removed  
  "isEmailVerified": true      // ❌ Removed
}
```

**After** (user-friendly fields):
```json
{
  "email": "john@example.com", // ✅ Added
  "password": "newPass123",    // ✅ Added (optional)
  "name": "John Doe",
  "phoneNumber": "+1234567890",
  "address": "123 Main St"
}
```

This change makes the API more intuitive for end users while maintaining proper access control for administrative functions.