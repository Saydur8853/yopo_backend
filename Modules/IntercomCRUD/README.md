# Intercom CRUD Module

**Module ID:** 9  
**Module Name:** IntercomCRUD  
**Version:** 1.0.0  

## Overview

The Intercom CRUD module provides comprehensive management capabilities for intercom systems within buildings. This module allows users to track, monitor, and maintain intercom devices with detailed information about their installation, configuration, and operational status.

## Features

### Core Functionality
- **Create, Read, Update, Delete** operations for intercom systems
- **Building-based organization** - each intercom belongs to a specific building
- **Installation tracking** - monitor installation status and dates
- **Service and maintenance tracking** - track service dates and warranty information
- **Advanced filtering** - find intercoms by various criteria

### Key Attributes Tracked
- **Basic Information**: Name, Model, Type, Size, Color
- **Financial**: Price information
- **Installation**: Installation status, date, and location within building
- **Technical**: Operating System, IP Address, MAC Address, Firmware Version
- **Features**: CCTV integration, PIN pad functionality, touch screen, remote access
- **Maintenance**: Service dates, warranty expiry dates
- **Operational**: Active/inactive status, description/notes

### Specialized Endpoints
- `/api/intercoms` - Get all intercoms (with access control)
- `/api/intercoms/active` - Get only active intercoms
- `/api/intercoms/installed` - Get only installed intercoms
- `/api/intercoms/with-cctv` - Get intercoms with CCTV integration
- `/api/intercoms/with-pin-pad` - Get intercoms with PIN pad functionality
- `/api/intercoms/maintenance-required` - Get intercoms needing maintenance
- `/api/intercoms/warranty-expiring` - Get intercoms with expiring warranties
- `/api/intercoms/building/{buildingId}` - Get intercoms for specific building
- `/api/intercoms/{id}/toggle-status` - Toggle active/inactive status
- `/api/intercoms/{id}/service-date` - Update service date

## Relationships

### Building Relationship
- **One-to-Many**: One building can have multiple intercom systems
- **Foreign Key**: `BuildingId` (required)
- **Constraint**: Intercom names must be unique within each building
- **Access Control**: Users can only access intercoms in buildings they have permission to view

## Access Control

The module implements comprehensive access control through the base `BaseAccessControlService`:

- **Data Access Control**: Users only see intercoms in buildings they have access to
- **Super Admin**: Has access to all intercom systems across all buildings
- **Regular Users**: Access limited based on their building permissions
- **Module Permissions**: Requires `INTERCOM_MODULE_ID` permission to access any endpoints

## Security Features

- **JWT Authentication**: All endpoints require valid JWT tokens
- **Role-based Access**: Different user types have different levels of access
- **Data Isolation**: Users cannot access intercoms outside their permitted buildings
- **Audit Trail**: Tracks who created each intercom record with `CreatedBy` field

## API Response Types

### IntercomDto (Full Details)
Complete intercom information including building name and all technical details.

### IntercomListDto (List View)
Simplified intercom information optimized for listing views and performance.

## Validation Rules

- **Name**: Required, max 200 characters, must be unique per building
- **Model**: Required, max 100 characters
- **Location**: Required, max 500 characters (describes where in building)
- **BuildingId**: Required, must reference existing building
- **Various String Fields**: Length limitations as defined in model attributes
- **Dates**: Optional datetime fields for installation, service, warranty tracking

## Business Rules

1. **Unique Names**: Intercom names must be unique within each building
2. **Building Access**: Users can only create intercoms in buildings they have access to
3. **Maintenance Tracking**: System can identify intercoms requiring maintenance based on service dates
4. **Warranty Monitoring**: System can track and alert on expiring warranties
5. **Status Management**: Intercoms can be toggled between active and inactive states

## Database Schema

The module creates the `Intercoms` table with:
- Primary key: `IntercomId`
- Foreign key: `BuildingId` → `Buildings.Id`
- Unique constraint: `(BuildingId, Name)` to ensure unique names per building
- Indexes on `BuildingId` for query performance

## Usage Examples

```bash
# Get all intercoms for current user
GET /api/intercoms

# Get intercoms in a specific building
GET /api/intercoms/building/1

# Get intercoms with CCTV integration
GET /api/intercoms/with-cctv

# Get intercoms needing maintenance (default: 12 months threshold)
GET /api/intercoms/maintenance-required

# Create a new intercom
POST /api/intercoms
{
    "name": "Main Entrance Intercom",
    "model": "Aiphone IX-MV",
    "type": "Video",
    "buildingId": 1,
    "location": "Main entrance lobby",
    "hasCCTV": true,
    "isPinPad": true,
    "isInstalled": true,
    "dateInstalled": "2024-09-02T00:00:00Z"
}
```

## Future Enhancements

- Integration with CCTV module for unified security management
- Mobile app connectivity for remote intercom management
- Integration with tenant management for direct communication
- Advanced reporting and analytics
- IoT device status monitoring
- Maintenance scheduling and automated alerts
