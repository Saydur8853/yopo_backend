# Customer CRUD Module

**Module ID**: 6  
**Module Name**: CustomerCRUD  
**Version**: 1.0.0

## Overview

The Customer CRUD module provides comprehensive customer management functionality for the YOPO backend system. It allows users to create, read, update, and delete customer records, with each customer being assigned to a specific user for management.

## Features

- **Full CRUD Operations**: Create, Read, Update, and Delete customer records
- **User-Customer Relationship**: Each customer is managed by a specific user (One-to-Many relationship)
- **Pagination Support**: Efficiently handle large datasets with built-in pagination
- **Advanced Filtering**: Filter customers by name, address, user, type, and status
- **Status Management**: Activate/deactivate customers and manage payment status
- **Trial Support**: Track customers in trial periods
- **Comprehensive Customer Data**: Store detailed customer information including business metrics

## Database Schema

### Customer Entity Attributes

- `CustomerId` (Primary Key): Unique identifier
- `Name`: Customer name (required, max 200 characters)
- `Address`: Customer address (required, max 500 characters)
- `CompanyLicense`: Company license number (optional, max 100 characters)
- `NumberOfUnits`: Number of units managed by customer
- `Active`: Whether customer account is active
- `IsTrial`: Whether customer is on trial period
- `Paid`: Payment status
- `Type`: Customer type (e.g., Individual, Corporate, Enterprise)
- `NumberOfTowers`: Number of towers/buildings managed
- `UserId`: Foreign key to User table (required)
- `CreatedBy`: ID of user who created the record
- `CreatedAt`: Creation timestamp
- `UpdatedAt`: Last update timestamp

### Relationships

- **User-Customer**: One-to-Many relationship
  - One User can manage many Customers
  - Each Customer belongs to exactly one User
  - Foreign Key: `UserId` references `Users.Id`
  - Delete Behavior: Restrict (prevents deleting a User with customers)

## API Endpoints

### GET /api/customers
Get all customers with pagination and filtering
- Query parameters: `page`, `pageSize`, `searchTerm`, `userId`, `type`, `activeOnly`
- Returns: Paginated list of customers

### GET /api/customers/{id}
Get a specific customer by ID
- Returns: Customer details or 404 if not found

### POST /api/customers
Create a new customer
- Body: `CreateCustomerDTO`
- Returns: Created customer details

### PUT /api/customers/{id}
Update an existing customer
- Body: `UpdateCustomerDTO`
- Returns: Updated customer details or 404 if not found

### DELETE /api/customers/{id}
Delete a customer
- Returns: 204 No Content or 404 if not found

### GET /api/customers/user/{userId}
Get all customers managed by a specific user
- Query parameters: `page`, `pageSize`
- Returns: Paginated list of user's customers

### PATCH /api/customers/{id}/active
Update customer active status
- Body: boolean value
- Returns: 204 No Content or 404 if not found

### PATCH /api/customers/{id}/payment
Update customer payment status
- Body: boolean value
- Returns: 204 No Content or 404 if not found

## DTOs

### CreateCustomerDTO
Used for creating new customers
- All customer fields except ID, timestamps, and calculated fields
- Validation attributes for data integrity

### UpdateCustomerDTO
Used for updating existing customers
- Same fields as CreateCustomerDTO
- All validations applied

### CustomerResponseDTO
Used for API responses
- All customer fields plus computed fields like UserName
- Used in both single and list responses

### CustomerListResponseDTO
Used for paginated responses
- Contains list of customers and pagination metadata

## Usage Examples

### Creating a Customer
```json
POST /api/customers
{
  "name": "ABC Corporation",
  "address": "123 Business Street, City, State",
  "companyLicense": "BL123456",
  "numberOfUnits": 50,
  "active": true,
  "isTrial": false,
  "paid": true,
  "type": "Corporate",
  "numberOfTowers": 2,
  "userId": 1
}
```

### Filtering Customers
```
GET /api/customers?type=Corporate&activeOnly=true&page=1&pageSize=20
```

### Getting User's Customers
```
GET /api/customers/user/1?page=1&pageSize=10
```

## Security

- **Authentication Required**: All endpoints require valid JWT token
- **Module Permission**: Users must have access to Module ID 6 (CustomerCRUD)
- **User Validation**: System validates that assigned users exist before creating/updating customers

## Business Rules

1. **User Assignment**: Each customer must be assigned to a valid user
2. **Data Validation**: All required fields must be provided and within length limits
3. **Active Status**: Customers can be activated/deactivated independently
4. **Payment Tracking**: Payment status is tracked separately from active status
5. **Trial Management**: Customers can be marked as trial accounts
6. **Metrics Tracking**: Number of units and towers are tracked for business analytics

## Error Handling

- **404 Not Found**: When customer or referenced user doesn't exist
- **400 Bad Request**: When validation fails or invalid data provided
- **401 Unauthorized**: When user lacks proper authentication
- **403 Forbidden**: When user lacks module permissions
- **500 Internal Server Error**: For unexpected server errors

## Future Enhancements

- Customer document management
- Contract tracking and expiration alerts
- Integration with billing systems
- Customer analytics and reporting
- Bulk operations for customer management
- Customer hierarchy support (parent-child relationships)
