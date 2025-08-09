# UserCRUD Module Documentation

**Module ID:** 3  
**Module Name:** UserCRUD  
**Version:** 1.0.0  

## Overview

The UserCRUD module provides complete user management functionality with JWT-based authentication. This module handles user registration, login, profile management, and all CRUD operations for user accounts.

## Features

### 🔐 Authentication Features
- **User Registration**: Create new user accounts with validation
- **User Login**: Authenticate users and generate JWT tokens
- **JWT Token Management**: Secure token generation, validation, and claims extraction
- **Password Management**: Secure password hashing and change functionality
- **Email Availability Check**: Verify if email is already registered

### 👤 User Management Features  
- **Profile Management**: View and update user profiles
- **User CRUD Operations**: Create, read, update, and delete users
- **User Status Management**: Activate/deactivate user accounts
- **Pagination & Filtering**: Search and filter users with pagination
- **Role-based Access**: Integration with UserType system

### 🔒 Security Features
- **Bcrypt Password Hashing**: Secure password storage
- **JWT Authentication**: Stateless token-based authentication  
- **Token Validation**: Comprehensive token verification
- **Protected Endpoints**: Authorization-required operations
- **Account Status Checks**: Inactive account protection

## API Endpoints

### Authentication Endpoints

#### POST `/api/users/register`
Register a new user account.

**Request Body:**
```json
{
  "emailAddress": "user@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890",
  "userTypeId": 1
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-02T12:00:00Z",
  "user": {
    "id": 1,
    "emailAddress": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "fullName": "John Doe",
    "phoneNumber": "+1234567890",
    "userTypeId": 1,
    "userTypeName": "Admin",
    "isActive": true,
    "isEmailVerified": false,
    "createdAt": "2024-01-01T10:00:00Z"
  },
  "message": "Registration successful"
}
```

#### POST `/api/users/login`
Authenticate a user and receive JWT token.

**Request Body:**
```json
{
  "emailAddress": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-02T12:00:00Z",
  "user": {
    "id": 1,
    "emailAddress": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "fullName": "John Doe",
    "lastLoginAt": "2024-01-01T12:00:00Z"
  },
  "message": "Login successful"
}
```

#### GET `/api/users/check-email`
Check if an email address is available for registration.

**Query Parameters:**
- `email` (string, required): Email address to check

**Response (200 OK):**
```json
{
  "email": "user@example.com",
  "isAvailable": false,
  "message": "Email is already registered."
}
```

### User Management Endpoints

#### GET `/api/users/me`
Get the current authenticated user's profile.

**Headers:**
- `Authorization: Bearer <jwt_token>`

**Response (200 OK):**
```json
{
  "id": 1,
  "emailAddress": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "fullName": "John Doe",
  "phoneNumber": "+1234567890",
  "userTypeId": 1,
  "userTypeName": "Admin",
  "isActive": true,
  "isEmailVerified": true,
  "lastLoginAt": "2024-01-01T12:00:00Z",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-01-01T11:00:00Z"
}
```

#### GET `/api/users`
Get paginated list of users with optional filtering.

**Headers:**
- `Authorization: Bearer <jwt_token>`

**Query Parameters:**
- `page` (int, optional): Page number (default: 1)
- `pageSize` (int, optional): Items per page (default: 10)
- `searchTerm` (string, optional): Search in name or email
- `userTypeId` (int, optional): Filter by user type
- `isActive` (bool, optional): Filter by active status

**Response (200 OK):**
```json
{
  "users": [
    {
      "id": 1,
      "emailAddress": "user@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "fullName": "John Doe",
      "userTypeId": 1,
      "userTypeName": "Admin",
      "isActive": true,
      "createdAt": "2024-01-01T10:00:00Z"
    }
  ],
  "totalCount": 25,
  "page": 1,
  "pageSize": 10,
  "totalPages": 3,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

#### GET `/api/users/{id}`
Get a specific user by ID.

**Headers:**
- `Authorization: Bearer <jwt_token>`

**Path Parameters:**
- `id` (int): User ID

#### POST `/api/users`
Create a new user (admin function).

**Headers:**
- `Authorization: Bearer <jwt_token>`
- `Content-Type: application/json`

**Request Body:**
```json
{
  "emailAddress": "newuser@example.com",
  "password": "SecurePassword123!",
  "firstName": "New",
  "lastName": "User",
  "phoneNumber": "+1987654321",
  "userTypeId": 2,
  "isActive": true,
  "isEmailVerified": true
}
```

#### PUT `/api/users/{id}`
Update an existing user.

**Headers:**
- `Authorization: Bearer <jwt_token>`
- `Content-Type: application/json`

**Path Parameters:**
- `id` (int): User ID

**Request Body:**
```json
{
  "firstName": "Updated",
  "lastName": "Name",
  "phoneNumber": "+1234567891",
  "userTypeId": 1,
  "isActive": true,
  "isEmailVerified": true
}
```

#### DELETE `/api/users/{id}`
Delete a user by ID.

**Headers:**
- `Authorization: Bearer <jwt_token>`

**Path Parameters:**
- `id` (int): User ID

### Password Management

#### POST `/api/users/change-password`
Change the current user's password.

**Headers:**
- `Authorization: Bearer <jwt_token>`
- `Content-Type: application/json`

**Request Body:**
```json
{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewPassword456!"
}
```

#### POST `/api/users/{id}/change-password`
Change another user's password (admin function).

**Headers:**
- `Authorization: Bearer <jwt_token>`
- `Content-Type: application/json`

**Path Parameters:**
- `id` (int): User ID

### User Status Management

#### PATCH `/api/users/{id}/status`
Activate or deactivate a user account.

**Headers:**
- `Authorization: Bearer <jwt_token>`

**Path Parameters:**
- `id` (int): User ID

**Query Parameters:**
- `isActive` (bool): New status (true = active, false = inactive)

## JWT Token Structure

The JWT tokens include the following claims:

```json
{
  "nameid": "1",              // User ID
  "email": "user@example.com", // Email address
  "name": "John Doe",         // Full name
  "given_name": "John",       // First name
  "family_name": "Doe",       // Last name
  "UserTypeId": "1",          // User type ID
  "IsActive": "true",         // Account status
  "IsEmailVerified": "false", // Email verification status
  "iss": "YopoBackend",       // Issuer
  "aud": "YopoBackend",       // Audience
  "exp": 1641123456,          // Expiration timestamp
  "iat": 1641037056           // Issued at timestamp
}
```

## Password Requirements

- Minimum 8 characters
- Maximum 100 characters
- Should include mix of letters, numbers, and special characters (recommended)

## Error Responses

### 400 Bad Request
```json
{
  "message": "Registration failed. Email may already be registered or user type is invalid.",
  "errors": {
    "EmailAddress": ["Email address is required"],
    "Password": ["Password must be at least 8 characters long"]
  }
}
```

### 401 Unauthorized
```json
{
  "message": "Invalid email or password, or account is inactive."
}
```

### 404 Not Found
```json
{
  "message": "User not found."
}
```

## Database Schema

### Users Table
```sql
CREATE TABLE Users (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    EmailAddress VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    PhoneNumber VARCHAR(20),
    ProfilePhoto VARCHAR(1000),
    UserTypeId INT NOT NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    IsEmailVerified BOOLEAN DEFAULT FALSE,
    LastLoginAt DATETIME,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME,
    FOREIGN KEY (UserTypeId) REFERENCES UserTypes(Id)
);
```

## Configuration

### JWT Settings (appsettings.json)
```json
{
  "Jwt": {
    "SecretKey": "YourSecretKeyHere",
    "Issuer": "YopoBackend",
    "Audience": "YopoBackend",
    "ExpirationHours": 24
  }
}
```

### Environment Variables
- `JWT_SECRET_KEY`: JWT secret key (overrides appsettings)
- `MYSQL_CONNECTION_STRING`: Database connection string

## Security Considerations

1. **Password Storage**: Passwords are hashed using bcrypt with salt
2. **JWT Security**: Tokens are signed with HMAC SHA256
3. **Token Validation**: Comprehensive validation including expiration
4. **Account Security**: Inactive accounts cannot login
5. **HTTPS**: Use HTTPS in production for token transmission

## Dependencies

- `BCrypt.Net-Next`: For password hashing
- `Microsoft.AspNetCore.Authentication.JwtBearer`: JWT authentication
- `System.IdentityModel.Tokens.Jwt`: JWT token handling
- `Microsoft.EntityFrameworkCore`: Database operations

## Testing

Use the provided `user-auth-tests.http` file to test all authentication and user management endpoints. The file includes:

- Registration and login flows
- Token-based API calls
- Error case testing
- Complete CRUD operations

## Module Integration

The UserCRUD module integrates with:

1. **UserTypeCRUD Module (ID: 1)**: For role-based permissions
2. **Database Context**: Shared ApplicationDbContext
3. **JWT Service**: Centralized token management
4. **Module Service**: Module registration and management

## Example Usage Flow

1. **Register a new user**
   ```bash
   POST /api/users/register
   ```

2. **Login to get JWT token**
   ```bash
   POST /api/users/login
   ```

3. **Use token for protected operations**
   ```bash
   GET /api/users/me
   Authorization: Bearer <your-jwt-token>
   ```

4. **Manage users (admin functions)**
   ```bash
   GET /api/users?page=1&pageSize=10
   Authorization: Bearer <admin-jwt-token>
   ```

The UserCRUD module provides a complete, secure, and scalable authentication system ready for production use!
