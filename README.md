# Quizzler Backend

A RESTful API backend service for an educational platform that enables students to create and manage interactive learning content including quizzes, flashcards, and lessons.

![Quizzler](media/quizzler.png)

## Key Features

- **User Authentication & Authorization**: Secure JWT-based authentication system with Argon2 password hashing
- **User Management**: Complete CRUD operations for user profiles with email and username validation
- **Educational Content Models**: Support for quizzes, flashcards, lessons, and media management
- **RESTful API Design**: Clean, well-structured endpoints following REST principles
- **Swagger Documentation**: Interactive API documentation with Bearer token authentication support
- **Database Integration**: Entity Framework Core with MySQL for robust data persistence
- **Security Best Practices**: Password hashing with Argon2, secure token generation, and input validation

## Architecture & Tech Stack

### Backend Framework
- **.NET 6.0**: Modern, cross-platform web framework
- **ASP.NET Core Web API**: RESTful API framework with dependency injection

### Database & ORM
- **MySQL 8.0**: Relational database management system
- **Entity Framework Core 7.0**: Object-relational mapping with Pomelo MySQL provider
- **Code-First Approach**: Database schema defined through C# models

### Authentication & Security
- **JWT (JSON Web Tokens)**: Stateless authentication with configurable expiration
- **Argon2**: Memory-hard password hashing algorithm for enhanced security
- **Microsoft Identity Model**: Token validation and claims-based authorization

### API Documentation
- **Swashbuckle (Swagger)**: Interactive API documentation with Bearer token support

### Key Libraries
- `Isopoh.Cryptography.Argon2`: Advanced password hashing
- `MlkPwgen`: Secure salt generation
- `Pomelo.EntityFrameworkCore.MySql`: MySQL database provider for EF Core

## Technical Challenges

### 1. Secure Password Storage with Argon2
Implementing Argon2 password hashing required careful configuration of memory cost, thread count, and hash length to balance security and performance. The solution uses Argon2's Data-Independent Addressing mode with configurable parameters that adapt to the server's processor count, ensuring optimal security without compromising system resources.

### 2. Owned Entity Configuration for LoginInfo
The `LoginInfo` entity is configured as an owned entity of `User` using EF Core's `OwnsOne` relationship. This design ensures that login credentials are tightly coupled with user data while maintaining a clean separation of concerns. The challenge was ensuring proper configuration in `OnModelCreating` to correctly map the owned entity to the database schema.

## Setup & Installation

### Prerequisites

- **.NET 6.0 SDK** or later
- **MySQL 8.0** server (or compatible version)
- **Visual Studio 2022**, **VS Code**, or **JetBrains Rider** (optional, for IDE support)

### Step 1: Clone the Repository

```bash
git clone https://github.com/yourusername/Quizzler-Backend.git
cd Quizzler-Backend/Quizzler-Backend
```

### Step 2: Database Setup

1. Create a MySQL database:
   ```sql
   CREATE DATABASE quizzler-db;
   ```

2. Execute the database schema script:
   ```bash
   mysql -u your_username -p quizzler-db < ../DB.sql
   ```

   Alternatively, you can use the MySQL Workbench model file (`DB.mwb`) to generate the schema.

### Step 3: Configure Application Settings

1. Set up your database connection string as an environment variable:
   ```bash
   export DbConnection="Server=localhost;Database=quizzler-db;User=your_username;Password=your_password;"
   ```

   Or add it to your `appsettings.Development.json`:
   ```json
   {
     "DbConnection": "Server=localhost;Database=quizzler-db;User=your_username;Password=your_password;",
     "JwtKey": "your-secret-key-minimum-32-characters-long-for-security",
     "JwtIssuer": "Quizzler-API"
   }
   ```

2. Configure JWT settings:
   - `JwtKey`: A secure secret key (minimum 32 characters recommended)
   - `JwtIssuer`: The issuer name for JWT tokens

### Step 4: Restore Dependencies

```bash
dotnet restore
```

### Step 5: Build the Project

```bash
dotnet build
```

### Step 6: Run the Application

```bash
dotnet run
```

The API will be available at `http://localhost:4200` (or the port specified in `Program.cs`).

### Step 7: Access Swagger Documentation

Once the application is running, navigate to:
```
http://localhost:4200/swagger
```

The Swagger UI provides interactive API documentation where you can test endpoints. To test protected endpoints:
1. Register a new user via `/api/user/register`
2. Login via `/api/user/login` to receive a JWT token
3. Click "Authorize" in Swagger UI and enter: `Bearer {your-token}`

## API Endpoints

### User Management

- `POST /api/user/register` - Register a new user
- `POST /api/user/login` - Authenticate and receive JWT token
- `GET /api/user/profile` - Get current user's profile (requires authentication)
- `GET /api/user/{id}/profile` - Get user profile by ID (requires authentication)
- `PATCH /api/user/update` - Update user profile (requires authentication)
- `DELETE /api/user/delete` - Delete user account (requires authentication)
- `GET /api/user/check` - Validate JWT token and update last seen timestamp (requires authentication)

## Project Structure

```
Quizzler-Backend/
├── Controllers/
│   └── UserController.cs          # User management endpoints
├── Services/
│   └── UserService.cs             # Business logic for user operations
├── Data/
│   └── QuizzlerDbContext.cs       # Entity Framework database context
├── Models/
│   ├── User.cs                    # User entity model
│   ├── LoginInfo.cs               # Login credentials (owned entity)
│   ├── Lesson.cs                  # Lesson entity model
│   ├── Quiz.cs                    # Quiz entity model
│   └── ...                        # Additional domain models
├── Dtos/
│   ├── LoginDto.cs                # Login request DTO
│   ├── UserRegisterDto.cs         # User registration DTO
│   └── UserUpdateDto.cs          # User update DTO
├── Program.cs                     # Application entry point and configuration
└── Quizzler-Backend.csproj        # Project file with dependencies
```

## Database Schema

![Database Schema](media/database.png)

The database includes entities for:
- **Users**: User accounts with authentication information
- **Lessons**: Educational lesson content
- **Quizzes**: Interactive quiz assessments
- **Flashcards**: Study flashcard sets
- **Media**: Media files associated with content
- **Questions & Answers**: Quiz question and answer pairs

## Development Notes

- The application uses `ReferenceHandler.Preserve` for JSON serialization to handle circular references in entity relationships
- Kestrel server is configured to allow synchronous I/O for compatibility with certain operations
- The application listens on port 4200 by default (configurable in `Program.cs`)
- Environment-specific settings can be configured in `appsettings.Development.json`

## License

This project is part of a portfolio showcase. Please refer to the repository license for usage terms.
