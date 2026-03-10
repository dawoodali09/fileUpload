# File Manager - ASP.NET MVC File Upload Application

A powerful web-based file upload and management application built with ASP.NET MVC 5. Supports large file uploads up to 500MB with chunked upload technology that bypasses CDN limits.

## Features

- **Large File Uploads**: Upload files up to 500MB with chunked transfer technology
- **CDN Compatible**: Works seamlessly with Cloudflare and other CDNs using 5MB chunk uploads
- **Drag & Drop**: Modern drag-and-drop interface for easy file uploads
- **Real-time Progress**: Visual progress bar with percentage tracking
- **Multiple File Types**: Support for various file formats:
  - **Videos**: MP4, AVI, MOV, WMV, MKV, WebM, FLV
  - **Documents**: PDF, DOC, DOCX, XLS, XLSX, PPT, PPTX
  - **Images**: JPG, JPEG, PNG, GIF, BMP, WebP
  - **Archives**: ZIP, RAR, 7Z, TAR, GZ
- **File Preview**: In-browser preview for videos and images
- **File Management**: Download and delete uploaded files
- **Responsive Design**: Works on desktop and mobile devices

## Technology Stack

- ASP.NET MVC 5
- .NET Framework 4.7.2
- Entity Framework 6
- SQL Server
- Bootstrap 3.4
- jQuery 3.4

## Installation

### Prerequisites

- Visual Studio 2019 or later
- .NET Framework 4.7.2
- SQL Server (any edition)
- IIS or IIS Express

### Setup Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/file-upload-mvc.git
   cd file-upload-mvc
   ```

2. **Configure the database connection**

   Update the connection string in `UploadPlayVideoFile/Web.config`:
   ```xml
   <connectionStrings>
     <add name="TestingDBEntities"
          connectionString="metadata=res://*/Models.MediaModel.csdl|res://*/Models.MediaModel.ssdl|res://*/Models.MediaModel.msl;provider=System.Data.SqlClient;provider connection string=&quot;Data Source=YOUR_SERVER;Initial Catalog=FileUploadDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=True;TrustServerCertificate=True;App=EntityFramework&quot;"
          providerName="System.Data.EntityClient" />
   </connectionStrings>
   ```

3. **Create the database**

   Run the SQL script in `UploadPlayVideoFile/App_Data/SetupDatabase.sql` on your SQL Server:
   ```bash
   sqlcmd -S YOUR_SERVER -U YOUR_USER -P YOUR_PASSWORD -i "UploadPlayVideoFile/App_Data/SetupDatabase.sql"
   ```

4. **Build and run**
   ```bash
   msbuild UploadPlayVideoFile.sln /p:Configuration=Release
   ```

   Or open in Visual Studio and press F5.

## Configuration

### File Size Limits

The application is configured for 500MB uploads. To modify, update `Web.config`:

```xml
<!-- maxRequestLength is in KB -->
<httpRuntime maxRequestLength="512000" executionTimeout="7200" />

<!-- maxAllowedContentLength is in bytes -->
<requestLimits maxAllowedContentLength="524288000" />
```

### Cloudflare Configuration

The application uses chunked uploads (5MB chunks) to bypass Cloudflare's upload limits. This works automatically for files over 50MB.

**Alternative**: Set your upload subdomain to "DNS only" (grey cloud) in Cloudflare to bypass the proxy entirely.

## Project Structure

```
UploadPlayVideoFile/
├── App_Data/
│   ├── SetupDatabase.sql    # Database setup script
│   └── Temp/                # Temporary chunk storage
├── Content/
│   └── Site.css             # Application styles
├── Controllers/
│   └── HomeController.cs    # Main controller with upload logic
├── Models/
│   ├── MediaFile.cs         # File entity model
│   └── MediaModel.edmx      # Entity Framework model
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml     # Home page
│   │   ├── MediaFiles.cshtml# Upload page
│   │   ├── About.cshtml     # About page
│   │   └── Contact.cshtml   # Contact page
│   └── Shared/
│       └── _Layout.cshtml   # Master layout
├── UploadMediaFiles/        # Uploaded files storage
└── Web.config               # Application configuration
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/media-files` | Display uploaded files |
| POST | `/Home/MediaFiles` | Upload a file (standard) |
| POST | `/Home/InitializeChunkedUpload` | Start chunked upload |
| POST | `/Home/UploadChunk` | Upload a file chunk |
| POST | `/Home/CompleteChunkedUpload` | Finalize chunked upload |
| POST | `/Home/DeleteFile` | Delete a file |

## Usage

1. Navigate to the application URL
2. Click "Get Started" or go to the Files page
3. Drag and drop files onto the upload area, or click to browse
4. Monitor upload progress in real-time
5. View, download, or delete uploaded files

## Security Features

- CSRF protection with anti-forgery tokens
- File type validation (whitelist approach)
- Sanitized file names to prevent path traversal
- Unique file names with timestamps

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request
