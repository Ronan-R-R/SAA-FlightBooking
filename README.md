# SAA Flight Booking System

Database design and manipulation project for the South African Airways Flight
Booking System. Built for MDB622 (Design and Manipulate Databases, NQF 6).

The solution contains a Microsoft SQL Server database, a C# console application
that verifies connectivity and lists passengers, and a WPF desktop application
for browsing the data. Data access uses ADO.NET (Microsoft.Data.SqlClient).

## Project structure

```
SAA-FlightBooking/
  sql/                       T-SQL scripts, run in numeric order
    01_create_database.sql
    02_create_tables.sql
    03_relationships.sql
    04_insert_data.sql
    05_retrieve_data.sql
    06_update_data.sql
    07_delete_data.sql
    08_join_reports.sql
  src/
    SAA.Data/                ADO.NET data-access library (shared)
    SAA.ConsoleApp/          console app: connect + list passengers
    SAA.DesktopApp/          WPF desktop client
  installer/
    SAA-Setup.iss            Inno Setup script -> setup.exe
  docs/                      ERD and supporting documentation
  SAA.sln
```

## Prerequisites

- Microsoft SQL Server (SQL Server Express is sufficient) with SSMS.
- .NET 8 SDK to build from source. The published desktop app is
  self-contained, so an installed runtime is not required to run it.

## Database setup

Run the scripts in order against your SQL Server instance. Using `sqlcmd`:

```
sqlcmd -S .\SQLEXPRESS -i sql\01_create_database.sql
sqlcmd -S .\SQLEXPRESS -i sql\02_create_tables.sql
sqlcmd -S .\SQLEXPRESS -i sql\03_relationships.sql
sqlcmd -S .\SQLEXPRESS -i sql\04_insert_data.sql
```

Scripts 05 to 08 contain the retrieve, update, delete and join queries and can
be run in SSMS to view results.

## Connection string

Both applications resolve the connection string from `SAA.Data/DbConfig.cs`.
The default targets `.\SQLEXPRESS`. To point at another instance without
rebuilding, set the `SAA_CONNECTION` environment variable, for example:

```
set SAA_CONNECTION=Server=(localdb)\MSSQLLocalDB;Database=SAA_FlightBooking;Trusted_Connection=True;TrustServerCertificate=True;
```

## Running

```
dotnet run --project src/SAA.ConsoleApp
dotnet run --project src/SAA.DesktopApp
```

## Building the installer

Publish a self-contained build, then compile the Inno Setup script:

```
dotnet publish src/SAA.DesktopApp -c Release -r win-x64 --self-contained true ^
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish/DesktopApp
"%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe" installer/SAA-Setup.iss
```

The installer is written to `installer/Output/setup.exe`.
