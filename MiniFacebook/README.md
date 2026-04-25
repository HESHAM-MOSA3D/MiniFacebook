# MiniFacebook

A Razor Pages web application built with .NET 10. Intended as a minimal social platform for learning and demonstration purposes.

---

## 🚀 Features
- Razor Pages frontend
- Authentication scaffold (if enabled)
- Basic post, profile, and feed functionality (project-specific features may vary)
- Designed for local development and easy customization

---

## 🛠 Tech Stack
- .NET 10
-  ASP.NET Core Razor Pages
-  Entity Framework Core for data access
-  SQL Server / SQLite for database

---

## 📦 Prerequisites
- .NET 10 SDK
- Visual Studio 2026 or Visual Studio Code (with C# extensions)
- Git
- (If using SQL Server) LocalDB or SQL Server instance

---

## ⚡ Getting Started (PowerShell)

### 1. Clone the repository
```bash
git clone https://github.com/HESHAM-MOSA3D/MiniFacebook.git
cd MiniFacebook


2. Restore dependencies
dotnet restore

3. Build the project
dotnet build

4. Run the application
dotnet run --project src/YourRazorProjectName


Replace src/YourRazorProjectName with the actual path to your Razor Pages project if different.

5. Open in browser
https://localhost:5001
or
http://localhost:5000

⚙️ Configuration

The appsettings.json file contains environment-specific settings:

ConnectionStrings → Database connection
Logging → Logging configuration
Authentication → External authentication settings (if used)
Example connection string (LocalDB):
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MiniFacebookDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}

🗄 Database (Entity Framework Core)
Add migrations
dotnet ef migrations add InitialCreate --project src/YourRazorProjectName --startup-project src/YourRazorProjectName

Apply migrations
dotnet ef database update --project src/YourRazorProjectName --startup-project src/YourRazorProjectName

🧪 Running Tests

If test projects exist in the solution:

dotnet test

🖥 Running in Visual Studio
Open the solution (.sln) in Visual Studio 2026
Set the Razor Pages project as the Startup Project
Press F5 to run using IIS Express or project profile
📌 Development Notes
Follow the existing coding style in the repository
Keep changes small and focused per feature or bug fix
Update this README when adding major features or configuration changes
🤝 Contributing
Fork the repository
Create a feature branch
Commit changes with clear messages
Open a Pull Request
📄 License

This project is licensed under the MIT License.