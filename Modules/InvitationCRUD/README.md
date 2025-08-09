# Invitation CRUD Module

**Module Name:** invitationCRUD  
**Module ID:** 2  

## Overview

This module provides complete CRUD operations for managing invitations with email addresses, user types, and configurable expiry times (1-7 days).

## Features

- ✅ Create invitations with email validation
- ✅ Read invitations (all, by ID, active, expired)
- ✅ Update invitation details
- ✅ Delete invitations
- ✅ Email duplicate prevention for active invitations
- ✅ Automatic expiry calculation and tracking
- ✅ Configurable expiry duration (1-7 days)

## Database Schema

### Invitation Table
| Field | Type | Description |
|-------|------|-------------|
| Id | int (PK) | Auto-increment primary key |
| EmailAddress | varchar(255) | Email address (indexed, required) |
| UserRoll | varchar(50) | Roll of user (required) |
| ExpiryTime | datetime | When the invitation expires (required) |
| CreatedAt | datetime | When invitation was created |
| UpdatedAt | datetime | When invitation was last updated |

### Computed Properties
- `IsExpired`: Boolean indicating if invitation has expired
- `DaysUntilExpiry`: Number of days until expiration

## API Endpoints

### Base URL: `/api/invitations`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/` | Get all invitations |
| GET | `/{id}` | Get invitation by ID |
| POST | `/` | Create new invitation |
| PUT | `/{id}` | Update existing invitation |
| DELETE | `/{id}` | Delete invitation |
| GET | `/active` | Get active (non-expired) invitations |
| GET | `/expired` | Get expired invitations |
| GET | `/check-email/{email}` | Check if email has active invitation |

## Request/Response DTOs

### CreateInvitationDTO
```json
{
  "emailAddress": "user@example.com",
  "userRoll": "Admin",
  "expiryDays": 7
}
```

### UpdateInvitationDTO
```json
{
  "emailAddress": "newuser@example.com",
  "userRoll": "User", 
  "expiryDays": 5
}
```

### InvitationResponseDTO
```json
{
  "id": 1,
  "emailAddress": "user@example.com",
  "userRoll": "Admin",
  "expiryTime": "2025-08-14T10:11:00Z",
  "createdAt": "2025-08-07T10:11:00Z",
  "updatedAt": null,
  "isExpired": false,
  "daysUntilExpiry": 7
}
```

## Validation Rules

### Email Address
- Must be a valid email format
- Required field
- Maximum 255 characters
- Automatically converted to lowercase
- Must be unique among active invitations

### User Roll
- Required field
- Maximum 50 characters
- Examples: "Admin", "User", "SuperAdmin", etc.

### Expiry Days
- Must be between 1 and 7 days
- Default: 7 days if not specified

## Business Rules

1. **No Duplicate Active Invitations**: Cannot create a new invitation for an email that already has an active invitation
2. **Automatic Expiry**: Invitations automatically expire based on the configured expiry time
3. **Email Normalization**: Email addresses are stored in lowercase
4. **Audit Trail**: CreatedAt and UpdatedAt timestamps are automatically managed

## Error Handling

| Status Code | Scenario |
|-------------|----------|
| 200 | Success |
| 201 | Created successfully |
| 204 | Deleted successfully |
| 400 | Invalid request data |
| 404 | Invitation not found |
| 409 | Email already has active invitation |

## Usage Examples

See `invitation-api-tests.http` for comprehensive API testing examples.

## Module Structure

```
Modules/InvitationCRUD/
├── Controllers/
│   └── InvitationsController.cs
├── Models/
│   └── Invitation.cs
├── DTOs/
│   └── InvitationDTOs.cs
├── Services/
│   ├── IInvitationService.cs
│   └── InvitationService.cs
├── invitation-api-tests.http
└── README.md
```

## Integration

The module is automatically integrated into the main application:
- Service registration in `Program.cs`
- Database context configuration in `ApplicationDbContext.cs`
- Controller auto-discovery by ASP.NET Core
