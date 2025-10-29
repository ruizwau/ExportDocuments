# Data Export Solution - Process Flow Diagram

```mermaid
graph TD
    A[🚀 Start DataExport.Worker] --> B[📖 Load appsettings.json]
    B --> C[🔧 Configure Services<br/>- ExportOptions<br/>- S3 Client<br/>- FileSqlProvider<br/>- ExportService]
    C --> D[🎯 Start Export Process]
    
    D --> E[🔍 Load SQL Files<br/>- count_export_projects.sql<br/>- export_projects_paged.sql]
    E --> F[🗄️ Connect to SQL Server<br/>DummyExportDB]
    F --> G[📊 Get Total Project Count<br/>SELECT COUNT(*) FROM Project]
    G --> H[📐 Calculate Pagination<br/>- Total Pages<br/>- Pages per Batch]
    
    H --> I[🔄 For Each Batch]
    I --> J[📄 For Each Page in Batch]
    
    J --> K[🔍 Execute Paged Query<br/>WITH @Offset, @Limit]
    K --> L[📋 Fetch Project Data<br/>+ Tasks + Documents + Comments<br/>AS JSON]
    L --> M[☁️ Upload to S3<br/>exports/batch_X/projects_page_Y.json]
    M --> N[📝 Log to ExportManifest Table<br/>- BatchNumber<br/>- PageNumber<br/>- S3Key<br/>- Success/Error]
    
    N --> O{More Pages?}
    O -->|Yes| J
    O -->|No| P{More Batches?}
    P -->|Yes| I
    P -->|No| Q[✅ Export Complete]
    
    subgraph "🗄️ Database Tables"
        DB1[Project]
        DB2[Task]
        DB3[Document]
        DB4[DocumentComment]
        DB5[ExportManifest]
    end
    
    subgraph "☁️ AWS S3 Bucket"
        S3A[exports/batch_1/projects_page_1.json]
        S3B[exports/batch_1/projects_page_2.json]
        S3C[exports/batch_2/projects_page_3.json]
        S3D[...]
    end
    
    subgraph "📁 Configuration Files"
        CFG1[appsettings.json]
        CFG2[count_export_projects.sql]
        CFG3[export_projects_paged.sql]
        CFG4[create_export_manifest_table.sql]
    end
    
    K --> DB1
    K --> DB2
    K --> DB3
    K --> DB4
    N --> DB5
    M --> S3A
    M --> S3B
    M --> S3C
    
    style A fill:#e1f5fe
    style Q fill:#c8e6c9
    style M fill:#fff3e0
    style N fill:#f3e5f5
```

## Architecture Overview

```mermaid
graph LR
    subgraph "🖥️ Application Layer"
        A1[DataExport.Worker<br/>Console App]
        A2[DataExport.Core<br/>Business Logic]
    end
    
    subgraph "💾 Data Layer"
        D1[SQL Server<br/>DummyExportDB]
        D2[FileSqlProvider<br/>SQL Files]
    end
    
    subgraph "☁️ Cloud Layer"
        C1[AWS S3<br/>deltabonafide bucket]
    end
    
    subgraph "⚙️ Configuration"
        CF1[appsettings.json<br/>Connection Strings<br/>S3 Settings<br/>Batch Configuration]
    end
    
    A1 --> A2
    A2 --> D1
    A2 --> D2
    A2 --> C1
    A1 --> CF1
    
    style A1 fill:#e3f2fd
    style A2 fill:#f1f8e9
    style D1 fill:#fff8e1
    style C1 fill:#fce4ec
```