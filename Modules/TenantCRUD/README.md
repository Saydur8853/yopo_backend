# TenantCRUD Module

**Module ID**: 5  
**Module Name**: TenantCRUD  
**Version**: 1.0.0  
**Description**: Module for managing tenants with CRUD operations

## Overview

The TenantCRUD module provides comprehensive tenant management functionality, including the ability to create, read, update, and delete tenant records. Each tenant is associated with a building and contains detailed information about their tenancy.

## Features

- ✅ **Create Tenant**: Add new tenant records with full details
- ✅ **Read Tenant**: Retrieve tenant information by ID or list all tenants
- ✅ **Update Tenant**: Modify existing tenant information
- ✅ **Delete Tenant**: Soft delete (deactivate) or hard delete tenant records
- ✅ **Building Association**: Link tenants to specific buildings
- ✅ **Contract Management**: Track contract start and end dates
- ✅ **Payment Tracking**: Monitor payment status
- ✅ **File Attachments**: Store document references in JSON format
- ✅ **Advanced Filtering**: Filter by building, payment status, member type, etc.
- ✅ **Pagination Support**: Handle large datasets efficiently

## Model Structure

### Tenant Entity

| Attribute | Type | Description | Required |
|-----------|------|-------------|----------|
| `TenantId` | int | Primary key, auto-generated | ✓ |
| `Name` | string(200) | Tenant name | ✓ |
| `BuildingId` | int | Foreign key to Building entity | ✓ |
| `Type` | string(50) | Tenant type (Residential, Commercial, etc.) | ✓ |
| `Floor` | int | Floor number | ✓ |
| `UnitNo` | string(20) | Unit number | ✓ |
| `ParkingSpace` | int | Number of parking spaces allocated | |
| `Contact` | string(500) | Contact information | |
| `ContractStartDate` | DateTime | Contract start date | ✓ |
| `ContractEndDate` | DateTime | Contract end date | ✓ |
| `Paid` | bool | Payment status | |
| `MemberType` | string(50) | Member type (Owner, Renter, etc.) | ✓ |
| `Files` | string(JSON) | File paths/document references | |
| `Active` | bool | Status flag (default: true) | |
| `CreatedAt` | DateTime | Creation timestamp | ✓ |
| `UpdatedAt` | DateTime? | Last update timestamp | |

### Relationships

- **Building**: Many-to-One relationship with Building entity
- **Unique Constraint**: BuildingId + UnitNo (ensures unique units per building)

## API Endpoints

### Base URL
```
/api/tenants
```

### Endpoints Summary

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|---------------|
| `GET` | `/api/tenants` | Get all tenants with filtering | Required |
| `GET` | `/api/tenants/{id}` | Get tenant by ID | Required |
| `POST` | `/api/tenants` | Create new tenant | Required |
| `PUT` | `/api/tenants/{id}` | Update existing tenant | Required |
| `DELETE` | `/api/tenants/{id}` | Delete tenant | Required |

### Query Parameters (GET /api/tenants)

- `buildingId` (int): Filter by building ID
- `paid` (bool): Filter by payment status
- `memberType` (string): Filter by member type
- `active` (bool): Filter by active status
- `page` (int): Page number for pagination (default: 1)
- `pageSize` (int): Items per page (default: 10, max: 100)
- `search` (string): Search in tenant name

## Usage Examples

### Create a New Tenant
```json
POST /api/tenants
{
  "name": "John Doe",
  "buildingId": 1,
  "type": "Residential",
  "floor": 5,
  "unitNo": "5A",
  "parkingSpace": 1,
  "contact": "john.doe@example.com, +1234567890",
  "contractStartDate": "2024-01-01T00:00:00Z",
  "contractEndDate": "2024-12-31T23:59:59Z",
  "paid": true,
  "memberType": "Owner",
  "files": "[\"contract.pdf\", \"id_copy.jpg\"]",
  "active": true
}
```

### Update Tenant Information
```json
PUT /api/tenants/1
{
  "name": "John Smith",
  "paid": false,
  "contact": "john.smith@example.com, +1234567891"
}
```

### Get Tenants with Filtering
```http
GET /api/tenants?buildingId=1&paid=true&page=1&pageSize=20&search=john
```

## File Structure

```
Modules/TenantCRUD/
├── Controllers/
│   └── TenantsController.cs      # API endpoints
├── DTOs/
│   └── TenantDTOs.cs            # Data transfer objects
├── Models/
│   └── Tenant.cs                # Entity model
├── Services/
│   ├── ITenantService.cs        # Service interface
│   └── TenantService.cs         # Service implementation
└── README.md                    # This documentation
```

## Dependencies

- **EntityFramework Core**: For database operations
- **Microsoft.AspNetCore.Mvc**: For API controllers
- **Building Module**: For building associations

## Database Considerations

- **Indexes**: 
  - Primary key on `TenantId`
  - Index on `BuildingId` for efficient joins
  - Unique index on `BuildingId + UnitNo` combination
- **Foreign Keys**: 
  - `BuildingId` references `Buildings.Id` with RESTRICT delete behavior
- **JSON Storage**: `Files` field uses MySQL JSON data type

## Security & Authorization

- **Module-based Authorization**: Requires TenantCRUD module permission
- **JWT Authentication**: All endpoints require valid JWT token
- **User Context**: Tracks creation and modification by user

## Error Handling

The module provides comprehensive error handling for:
- Invalid building references
- Duplicate unit numbers within buildings
- Contract date validation
- Required field validation
- Database constraint violations

## Future Enhancements

- [ ] Tenant communication system
- [ ] Lease renewal notifications
- [ ] Payment integration
- [ ] Document management system
- [ ] Tenant portal access
- [ ] Maintenance request integration

---

**Module Registration**: This module is automatically registered in the system with ID 5 and will be available to users with appropriate permissions.
