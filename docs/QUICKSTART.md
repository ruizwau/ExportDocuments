# Quick Start Guide - DataExport Solution

## 🚀 Quick Setup

### 1. Prerequisites Check
```bash
# Verify .NET 8 installation
dotnet --version

# Check SQL Server connection
sqlcmd -S "LAPTOP-2GHN6DAS\LOCAL" -d "DummyExportDB" -E -Q "SELECT COUNT(*) FROM Project"

# Verify AWS credentials
aws s3 ls s3://deltabonafide
```

### 2. Configuration
Edit `DataExport.Worker/appsettings.json`:
```json
{
  "Export": {
    "PageSize": 500,           // ⚡ Adjust based on data size
    "BatchCount": 3,           // 🔢 Number of parallel batches
    "BucketName": "your-bucket", // 🪣 Your S3 bucket
    "Region": "us-east-2"      // 🌍 Your AWS region
  }
}
```

### 3. Run Export
```bash
# Navigate to solution folder
cd DataExportSolution

# Run the export
dotnet run --project DataExport.Worker
```

## 📊 Monitoring Progress

### Console Output
```
Looking for SQL file: export_projects_count ✅
Looking for SQL file: export_projects_paged ✅
✅ Successfully exported batch 1, page 0 to S3
✅ Successfully exported batch 2, page 1 to S3
✅ Successfully exported batch 3, page 2 to S3
```

### Check Results in Database
```sql
-- View export log
SELECT 
    BatchNumber,
    PageNumber, 
    S3Key,
    Success,
    RowsExported,
    LoggedAt,
    ErrorMessage
FROM ExportManifest 
ORDER BY LoggedAt DESC;
```

### Check S3 Objects
```bash
# List exported files
aws s3 ls s3://deltabonafide/exports/ --recursive

# Download a file to check content
aws s3 cp s3://deltabonafide/exports/batch_1/projects_page_1.json ./sample.json
```

## 🔧 Common Commands

### Development
```bash
# Build solution
dotnet build

# Run tests (if any)
dotnet test

# Clean build artifacts
dotnet clean
```

### Debug Mode
```bash
# Run with detailed logging
dotnet run --project DataExport.Worker --configuration Debug --verbosity detailed

# Or use VS Code F5 with configured launch.json
```

### Configuration Variants
```bash
# Use different appsettings file
dotnet run --project DataExport.Worker --configuration Production

# Override with environment variables
export Export__BucketName="production-bucket"
dotnet run --project DataExport.Worker
```

## 🚨 Troubleshooting

| Error | Solution |
|-------|----------|
| `Connection failed` | Check SQL Server is running and connection string |
| `Access Denied S3` | Verify AWS credentials: `aws configure list` |
| `File not found: .sql` | Check `Sql.BasePath` in appsettings.json |
| `Cannot insert NULL` | Run `create_export_manifest_table.sql` script |

## 📈 Performance Tuning

### For Large Datasets (>100K records)
```json
{
  "Export": {
    "PageSize": 1000,    // ⬆️ Increase page size
    "BatchCount": 5      // ⬆️ More parallel batches
  }
}
```

### For Small Datasets (<10K records)
```json
{
  "Export": {
    "PageSize": 200,     // ⬇️ Decrease page size  
    "BatchCount": 1      // ⬇️ Single batch
  }
}
```

---
*For detailed documentation, see [README.md](README.md)*