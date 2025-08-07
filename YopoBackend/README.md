# Yopo Backend API

A .NET 9 Web API project with MySQL database integration and Swagger documentation.

## Features

- **ASP.NET Core Web API** with .NET 9
- **MySQL** database with Entity Framework Core
- **Environment-based configuration** using .env files
- **Swagger UI** for API documentation
- **CRUD operations** for User management
- **CORS** enabled for cross-origin requests

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [MySQL Server](https://dev.mysql.com/downloads/mysql/) (running on localhost:3306)
- [Git](https://git-scm.com/)

## Setup Instructions

### 1. Clone the Repository
```bash
git clone <your-repository-url>
cd YopoBackend
```

### 2. Database Setup
Make sure MySQL is running on your local machine with these settings:
- Host: localhost
- Port: 3306
- Username: root
- Password: admin

Create a database named `yopo_backend`:
```sql
CREATE DATABASE yopo_backend;
```

### 3. Environment Configuration
The `.env` file is already configured with your local database settings:
```
DB_SERVER=localhost
DB_PORT=3306
DB_DATABASE=yopo_backend
DB_USER=root
DB_PASSWORD=admin
MYSQL_CONNECTION_STRING=Server=localhost;Port=3306;Database=yopo_backend;Uid=root;Pwd=admin;
ASPNETCORE_ENVIRONMENT=Development
```

### 4. Install Dependencies
```bash
dotnet restore
```

### 5. Run the Application
```bash
dotnet run
```

The API will start on:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

## API Documentation

Once the application is running, you can access:
- **Swagger UI**: `https://localhost:5001/` (root URL)
- **API Endpoints**: `https://localhost:5001/api/users`

## Available Endpoints

### Users API (`/api/users`)
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create a new user
- `PUT /api/users/{id}` - Update an existing user
- `DELETE /api/users/{id}` - Delete a user

### Sample User JSON
```json
{
  "name": "John Doe",
  "email": "john.doe@example.com"
}
```

## Project Structure

```
YopoBackend/
├── Controllers/
│   └── UsersController.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Models/
│   └── User.cs
├── .env
├── Program.cs
├── YopoBackend.csproj
└── README.md
```

## Key Technologies

- **ASP.NET Core 9.0** - Web API framework
- **Entity Framework Core** - ORM for database operations
- **Pomelo.EntityFrameworkCore.MySql** - MySQL provider for EF Core
- **Swashbuckle.AspNetCore** - Swagger/OpenAPI documentation
- **DotNetEnv** - Environment variable management

## Development Commands

```bash
# Run the application
dotnet run

# Build the application
dotnet build

# Run tests (when added)
dotnet test

# Update database (when using migrations)
dotnet ef database update

# Create new migration (when needed)
dotnet ef migrations add <MigrationName>
```

## Environment Variables

The application uses the following environment variables from the `.env` file:
- `MYSQL_CONNECTION_STRING` - Complete MySQL connection string
- `DB_SERVER` - Database server hostname
- `DB_PORT` - Database port
- `DB_DATABASE` - Database name
- `DB_USER` - Database username
- `DB_PASSWORD` - Database password

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License

This project is licensed under the MIT License.
