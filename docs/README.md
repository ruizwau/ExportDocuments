# DataExport Solution Documentation

## 🎯 Overview

The DataExport Solution is a .NET 8 console application designed to export project data from a SQL Server database to AWS S3 in JSON format. The solution implements batch processing with pagination, robust error handling, and comprehensive logging.

## 🏗️ Architecture

### Projects Structure
```
DataExportSolution/
├── DataExport.Core/           # Business logic and services
│   ├── Entities/             # Data models and options
│   ├── Sql/                  # SQL provider interfaces
│   ├── ExportService.cs      # Main export logic
│   └── ExportManifestLogger.cs
├── DataExport.Worker/        # Console application entry point
│   ├── Program.cs            # Application configuration and startup
│   └── appsettings.json      # Configuration settings
├── Sql/                      # SQL query files
│   ├── count_export_projects.sql
│   ├── export_projects_paged.sql
│   └── create_export_manifest_table.sql
└── docs/                     # Documentation
```

## ⚙️ Configuration

### appsettings.json
```json
{
  "Export": {
    "PageSize": 500,                    // Records per page
    "BatchCount": 3,                    // Number of batches
    "BucketName": "deltabonafide",      // S3 bucket name
    "Region": "us-east-2",              // AWS region
    "S3Prefix": "exports"               // S3 key prefix
  },
  "Sql": {
    "BasePath": "C:\\...\\Sql",         // Path to SQL files
    "Queries": {
      "export_projects_paged": "export_projects_paged.sql",
      "export_projects_count": "count_export_projects.sql"
    }
  },
  "ConnectionStrings": {
    "Default": "server=...;Database=DummyExportDB;..."
  }
}
```

## 🗄️ Database Schema

### Core Tables
- **Project**: Main project information
- **Task**: Tasks associated with projects
- **Document**: Documents related to projects
- **DocumentComment**: Comments on documents
- **ExportManifest**: Logging table for export operations

### ExportManifest Table Structure
```sql
CREATE TABLE dbo.ExportManifest (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    BatchNumber INT NOT NULL,
    PageNumber INT NOT NULL,
    PageIndex INT NOT NULL,
    S3Key NVARCHAR(500) NOT NULL,
    Success BIT NOT NULL DEFAULT 1,
    RowsExported INT NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    LoggedAt DATETIME2 DEFAULT GETUTCDATE()
);
```

## 🔄 Process Flow

### 1. Initialization
- Load configuration from `appsettings.json`
- Configure dependency injection container
- Initialize AWS S3 client and SQL providers

### 2. Data Discovery
- Execute `count_export_projects.sql` to get total record count
- Calculate pagination parameters (total pages, pages per batch)

### 3. Batch Processing
For each batch:
- Calculate page range for the batch
- Process each page within the batch

### 4. Page Processing
For each page:
- Execute `export_projects_paged.sql` with `@Offset` and `@Limit` parameters
- Retrieve hierarchical JSON data (Projects → Tasks, Documents → Comments)
- Upload JSON to S3 with key format: `exports/batch_{N}/projects_page_{N}.json`
- Log operation result to `ExportManifest` table

### 5. Error Handling
- Individual page failures don't stop the entire process
- Errors are logged to both console and database
- Detailed error messages are stored in `ExportManifest.ErrorMessage`

## 📊 Data Format

### Export Structure
The exported JSON contains hierarchical project data:

```json
[
  {
    "ProjectId": 1,
    "Name": "Sample Project",
    "Tasks": [
      {
        "TaskId": 1,
        "Title": "Sample Task",
        "Status": "In Progress",
        "AssignedTo": "John Doe"
      }
    ],
    "Documents": [
      {
        "DocumentId": 1,
        "FileName": "requirements.doc",
        "Comments": [
          {
            "CommentId": 1,
            "Author": "Jane Smith",
            "Message": "Looks good"
          }
        ]
      }
    ]
  }
]
```

## 🚀 Usage

### Running the Application
```bash
# Standard execution
dotnet run --project DataExport.Worker

# Debug mode
dotnet run --project DataExport.Worker --configuration Debug

# From compiled executable
dotnet DataExport.Worker.dll
```

### Prerequisites
1. .NET 8 Runtime
2. SQL Server with `DummyExportDB` database
3. AWS credentials configured (AWS CLI, environment variables, or IAM roles)
4. Required NuGet packages:
   - Microsoft.Data.SqlClient
   - Dapper
   - AWSSDK.S3
   - Microsoft.Extensions.* (Configuration, Hosting, Options)

## 🔧 Key Features

### ✅ Implemented Features
- **Batch Processing**: Configurable batch sizes for large datasets
- **Pagination**: Memory-efficient processing with configurable page sizes
- **Error Resilience**: Individual page failures don't stop the process
- **Comprehensive Logging**: Database and console logging
- **JSON Export**: Hierarchical data structure with nested relationships
- **AWS S3 Integration**: Secure upload with proper content types
- **Configuration Management**: Strongly-typed configuration with IOptions
- **Dependency Injection**: Clean architecture with DI container

### 🔍 Monitoring and Debugging
- Real-time console output showing progress
- Database logging in `ExportManifest` table
- Detailed error messages and stack traces
- File existence validation for SQL queries
- Connection status reporting

## 📈 Performance Considerations

### Scalability
- **Page Size**: Default 500 records per page (configurable)
- **Batch Size**: Default 3 batches (configurable)
- **Memory Usage**: Streaming approach for large datasets
- **Database Impact**: Parameterized queries with proper indexing

### Optimization Tips
1. Adjust `PageSize` based on record size and memory constraints
2. Monitor S3 upload performance and adjust batch sizes accordingly
3. Use database indexes on `ProjectId` for optimal join performance
4. Consider parallel processing for independent batches (future enhancement)

## 🛠️ Troubleshooting

### Common Issues
1. **Connection Failures**: Verify connection strings and network connectivity
2. **S3 Access Denied**: Check AWS credentials and bucket permissions
3. **SQL Parameter Errors**: Ensure SQL files use correct parameter names (`@Offset`, `@Limit`)
4. **File Not Found**: Verify `Sql.BasePath` configuration and file locations

### Debug Mode
Use the configured `launch.json` for step-by-step debugging in VS Code:
- Set breakpoints in `ExportService.cs`
- Monitor variable values during execution
- Inspect SQL query results and S3 upload responses

## 📋 Maintenance

### Regular Tasks
- Monitor `ExportManifest` table for failed exports
- Clean up old S3 objects if needed
- Review and optimize SQL queries performance
- Update AWS SDK and other dependencies regularly

### Configuration Updates
- Modify `appsettings.json` for different environments
- Update SQL queries in the `Sql/` folder as needed
- Adjust batch and page sizes based on data growth

---

*Last Updated: October 29, 2025*
*Version: 1.0*