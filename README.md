# Diamond Forever Gallery

Diamond Forever Gallery is a simple web app built with **ASP.NET Core Razor Pages** (.NET 10). It lets you view a gallery of photos, see the details of each photo, and upload new photos.

## What You Need

- [Visual Studio 2026](https://visualstudio.microsoft.com/) (or Visual Studio 2022 with .NET 10 support)
- The **ASP.NET and web development** workload installed in Visual Studio
- Access to the Azure SQL database and Azure Blob Storage used by this project

## Getting Started

1. **Get the code.** Open a terminal and run:

   ```powershell
   git clone https://github.com/swethadebeers/DiamondForeverGallery
   ```

2. **Open the project.** In Visual Studio, open `DiamondForeverGallery.slnx`.

3. **Set up the local database.** Make sure **SQL Server Express LocalDB** is installed (in the Visual Studio Installer, under **Individual components**). Then confirm the local connection string in `DiamondForeverGallery\appsettings.Development.json`:

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=DiamondForeverGallery;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```

4. **Create the database.** In **Tools > NuGet Package Manager > Package Manager Console**, run both lines in the same session:

   ```powershell
   $env:ASPNETCORE_ENVIRONMENT = "Development"
   Update-Database
   ```

   The first line makes the command use the local connection string instead of Azure SQL. You only need to do this once per machine, or again after new migrations are added.

5. **Set up local blob storage.** Photos are stored in Azurite, a local emulator for Azure Storage, so you don't need Azure access to test uploads:
   - Install Node.js (https://nodejs.org), then run `npm install -g azurite`.
   - Start it in a separate terminal and keep it running:

     ```powershell
     azurite --blobHost 127.0.0.1
     ```

   The Development settings already point to Azurite (`BlobStorage:ConnectionString` is `UseDevelopmentStorage=true`). The `gallery` container is created automatically on the first upload.

6. **Run the app.** Press **F5** (or click the green **Run** button). Your web browser opens with the home page. The local database starts empty, so use the **Upload** page to add photos.

## Project Layout

| Folder / File | What it does |
| --- | --- |
| `Pages/` | The web pages (Home, Upload, Photo Details, Privacy, Error). |
| `Models/` | Describes the data, such as a gallery photo (`GalleryPhoto.cs`). |
| `Data/` | Connects the app to the database (`ApplicationDbContext.cs`). |
| `Migrations/` | Database setup scripts created by Entity Framework Core. |
| `Services/` | Helper code, such as saving photos (`PhotoStorageService.cs`). |
| `wwwroot/` | Static files: CSS, JavaScript, images, and libraries like Bootstrap and jQuery. |
| `Program.cs` | The starting point of the app. It sets up the services and pages. |
| `appsettings.json` | Settings, such as the database connection and blob storage details. |

## Configuration

Settings are stored in `appsettings.json`:

- **ConnectionStrings:DefaultConnection**: the Azure SQL database the app uses.
- **BlobStorage:AccountUri** and **BlobStorage:ContainerName**: where uploaded photos are stored.

Do not put passwords or secrets in this file.

## Common Tasks

- **Add a database change:** Use the Package Manager Console and run `Add-Migration <Name>`, then `Update-Database`.
- **Change the look:** Edit `wwwroot/css/site.css` or `Pages/Shared/_Layout.cshtml`.
- **Add a new page:** Right-click the `Pages` folder in Visual Studio and choose **Add > Razor Page**.

## Troubleshooting

- **Cannot connect to the database:** Check that you are signed in to Visual Studio with an account that has access to the Azure SQL server.
- **Photos do not upload:** Check the `BlobStorage` settings and your access to the storage account.
- **Page shows an error:** Look at the Output window and the Error page for details.

## Contributing

1. Create a new branch for your change.
2. Make your change and test it by running the app.
3. Commit your change and open a pull request on GitHub.
