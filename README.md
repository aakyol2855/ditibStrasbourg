# DitibStasbourg Project

This is a web application for managing DITIB personnel, mosques, and associations in Strasbourg.

## Features
- **Personnel Management**: Add, edit, delete personnel.
- **Institution Management**: Manage Mosques and Associations.
- **Assignment Management**: Assign personnel to institutions with dates.
- **Filtering**: Filter assignments by year, institution type, and personnel.
- **Excel Export**: Export filtered assignments to Excel.

## Setup Instructions

### Prerequisites
- .NET 10 SDK
- Docker / Podman (for MSSQL container)

### Database Setup
1. Run MSSQL in a container:
   ```bash
   podman run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong(!)Password" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
   ```

2. Update `appsettings.json` connection string if necessary:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost,1433;Database=DitibStasbourgDB;User Id=sa;Password=YourStrong(!)Password;TrustServerCertificate=True;"
   }
   ```

3. Update the database (initial migration is already created):
   ```bash
   dotnet ef database update
   ```

### Running the Application
```bash
dotnet run
```

## Technologies Used
- ASP.NET Core MVC (.NET 10)
- Entity Framework Core (MSSQL)
- ClosedXML (Excel Export)
