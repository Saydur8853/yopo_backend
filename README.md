# Yopo Backend API

A modular backend API built with ASP.NET Core 9.0 and Entity Framework Core, featuring a flexible module management system.

## Features

- **Modular Architecture**: Clean separation of concerns with dedicated modules
- **Module Management**: Built-in system for managing and tracking modules
- **UserType Management**: Complete CRUD operations for user types with module permissions
- **Invitation System**: Comprehensive invitation management system
- **RESTful API**: Well-structured REST endpoints with Swagger documentation
- **Entity Framework Core**: Code-first approach with MySQL database support
- **Docker Support**: Ready for containerization

## Project Structure

```
YopoBackend/
├── Constants/              # Application constants and module definitions
├── Controllers/            # Core API controllers
├── Data/                  # Database context and configurations
├── DTOs/                  # Data transfer objects
├── Models/                # Core entity models
├── Services/              # Core business logic services
├── Modules/               # Modular components
│   ├── UserTypeCRUD/      # Module ID: 1 - User type management
│   │   ├── Controllers/
│   │   ├── DTOs/
│   │   ├── Models/
│   │   └── Services/
│   └── InvitationCRUD/    # Module ID: 2 - Invitation management
│       ├── Controllers/
│       ├── DTOs/
│       ├── Models/
│       └── Services/
└── Properties/            # Launch settings and configurations
```

## Modules

### Module 1: UserTypeCRUD
- **Purpose**: Manage user types and their module permissions
- **Entities**: UserType, UserTypeModulePermission
- **Features**:
  - Create, read, update, delete user types
  - Assign module permissions to user types
  - Validate module access
  - Track user type status and metadata

### Module 2: InvitationCRUD
- **Purpose**: Manage invitation system
- **Entities**: Invitation
- **Features**:
  - Send and manage invitations
  - Track invitation status
  - Handle invitation expiration
  - User role assignment through invitations

## API Endpoints

### Modules
- `GET /api/modules` - Get all modules
- `GET /api/modules/{id}` - Get module by ID
- `POST /api/modules` - Create new module
- `PUT /api/modules/{id}` - Update module
- `DELETE /api/modules/{id}` - Delete module

### User Types
- `GET /api/usertypes` - Get all user types
- `GET /api/usertypes/active` - Get active user types
- `GET /api/usertypes/{id}` - Get user type by ID
- `POST /api/usertypes` - Create new user type
- `PUT /api/usertypes/{id}` - Update user type
- `DELETE /api/usertypes/{id}` - Delete user type
- `GET /api/usertypes/{id}/permissions` - Get user type permissions
- `PUT /api/usertypes/{id}/permissions` - Update user type permissions
- `GET /api/usertypes/check-name` - Check if user type name is available

### Invitations
- `GET /api/invitations` - Get all invitations
- `GET /api/invitations/{id}` - Get invitation by ID
- `POST /api/invitations` - Create new invitation
- `PUT /api/invitations/{id}` - Update invitation
- `DELETE /api/invitations/{id}` - Delete invitation

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- MySQL Server
- Git

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/Saydur8853/yopo_backend.git
   cd yopo_backend
   ```

2. **Configure Database**
   - Set up your MySQL database
   - Update the connection string in `appsettings.json` or set the `MYSQL_CONNECTION_STRING` environment variable

3. **Install Dependencies**
   ```bash
   dotnet restore
   ```

4. **Build the Project**
   ```bash
   dotnet build
   ```

5. **Run the Application**
   ```bash
   dotnet run --project YopoBackend
   ```

6. **Access Swagger UI**
   - Navigate to `https://localhost:7071` or `http://localhost:5265`
   - The Swagger UI will be available at the root URL in development mode

### Environment Variables

Create a `.env` file in the root directory with the following variables:

```env
MYSQL_CONNECTION_STRING=Server=localhost;Database=yopo_backend;User=root;Password=yourpassword;
```

## Database Schema

The application uses Entity Framework Core with a code-first approach. The database includes:

- **Modules**: Core module definitions and metadata
- **UserTypes**: User type definitions with status tracking
- **UserTypeModulePermissions**: Junction table for user type and module relationships
- **Invitations**: Invitation management with expiration and status tracking

## Development

### Adding New Modules

1. Create a new folder under `Modules/` with your module name
2. Implement the standard structure: Controllers, DTOs, Models, Services
3. Add the module definition to `Constants/ModuleConstants.cs`
4. Register services in `Program.cs`
5. Update the database context if new entities are added

### Code Standards

- Follow C# naming conventions
- Use XML documentation for public APIs
- Implement proper error handling
- Follow the repository pattern for data access
- Use DTOs for API contracts

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/YourFeature`)
3. Commit your changes (`git commit -m 'Add some feature'`)
4. Push to the branch (`git push origin feature/YourFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For support and questions, please open an issue in the GitHub repository.
