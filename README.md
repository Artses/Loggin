# Loggin

An observability orchestrator composed of an ASP.NET Core API and a modern Windows desktop client.

> The Go collector is available in the [Artses/Loggin_Collector](https://github.com/Artses/Loggin_Collector) project.

## Features

* JWT-based authentication and user registration.
* CRUD operations for collectors and log sources.
* Log collection and search.
* Dashboard with activity metrics and a polling-based activity chart.
* Automatic reconnection with exponential backoff, configurable timeout, and polling interval.
* Light, dark, or Windows system theme with custom color palettes applied without restarting the application.
* Settings persisted in `%LOCALAPPDATA%\Artses\Loggin`.
* JWT securely stored in the Windows Credential Manager.
* Minimize, close-to-tray, and system tray context menu support.
* Optional startup with Windows through the current user's registry.
* Portable and `win-x64` installer releases.

## Technologies

| Layer     | Technology                                                                   |
| --------- | ---------------------------------------------------------------------------- |
| API       | ASP.NET Core 8, Entity Framework Core, PostgreSQL, JWT, BCrypt               |
| Desktop   | WPF on .NET 8, lightweight MVVM, `HttpClient`, Windows Credential Manager    |
| Chart     | Custom vector-based WPF control (`DrawingContext`), no external dependencies |
| Tests     | xUnit and mocked HTTP handler                                                |
| Installer | Inno Setup 6                                                                 |
| CI/CD     | GitHub Actions and GitHub CLI                                                |

## Project Structure

```text
Controllers/                 API REST endpoints
DTOs/, Models/               API contracts and entities
Services/, Repositories/     Business logic and persistence
Loggin.Desktop/
  Controls/                  Reusable WPF chart
  Models/                    Client contracts
  Services/                  API, themes, credentials, settings, and Windows services
  ViewModels/                State, commands, and asynchronous workflows
Loggin.Desktop.Tests/        Contract and HTTP failure tests
installer/Loggin.iss         Installer definition
build/                       Icon, release, and release notes scripts
.github/workflows/           Release pipeline
```

## Backend

### Requirements

* .NET 8 SDK
* PostgreSQL (or Docker Desktop)
* A secure `JWT_KEY` stored outside version control

Copy `env-example` to `.env`, update the environment variables, and start the database:

```powershell
docker compose up -d
dotnet run --project Api_Loggin.csproj
```

During development, Swagger is available at:

`http(s)://localhost:<port>/swagger`

### API Endpoints

| Method              | Route                 | Purpose                              |
| ------------------- | --------------------- | ------------------------------------ |
| POST                | `/api/Auth/register`  | Create an account                    |
| POST                | `/api/Auth/login`     | Authenticate                         |
| GET                 | `/api/Auth/hi`        | Validate session                     |
| GET/POST/PUT/DELETE | `/api/Collector`      | Collector CRUD                       |
| GET                 | `/api/Collector/{id}` | Retrieve collector                   |
| GET/POST/PUT/DELETE | `/api/Log`            | Log source CRUD                      |
| GET                 | `/api/Log/{id}`       | Retrieve log source                  |
| POST                | `/api/Log/GetLogs`    | Fetch log entries from the collector |

The backend contracts remain unchanged. The API does not currently provide WebSockets or Server-Sent Events, so the desktop client implements real-time updates through asynchronous polling. The default polling interval is 5 seconds and can be changed in **Settings**.

The desktop client's structured logs are stored in:

`%LOCALAPPDATA%\Artses\Loggin\logs`

## Running the Desktop Application

```powershell
dotnet run --project Loggin.Desktop/Loggin.Desktop.csproj
```

On the first launch:

1. Enter the API base URL on the login screen, including `http://` or `https://`.
2. Sign in or create an account.
3. Open **Settings** to configure the timeout, polling frequency, theme, and system tray behavior.
4. Save the changes. No restart is required.

## Tests

```powershell
dotnet test Loggin.Desktop.Tests/Loggin.Desktop.Tests.csproj --configuration Release
```

The tests verify URL and theme validation, login contracts, Bearer authentication, JSON deserialization, and backend error propagation.

## Manual Build

Publish directly:

```powershell
dotnet publish Loggin.Desktop/Loggin.Desktop.csproj `
  -c Release -r win-x64 --self-contained true `
  -p:Version=0.0.1 -o artifacts/publish
```

Complete build (API, tests, portable package, and installer):

```powershell
powershell -ExecutionPolicy Bypass -File build/Build-Release.ps1 -Version 0.0.1
```

The script requires Inno Setup 6 to generate the installer. Use `-SkipInstaller` if you only want the portable version.

Generated artifacts:

* `artifacts/Loggin-v0.0.1-win-x64-portable.zip`
* `artifacts/Loggin-Setup-v0.0.1-win-x64.exe`

The installer uses a fixed `AppId`, allowing future versions to upgrade existing installations. It registers an uninstaller, installs the application under `%LOCALAPPDATA%\Programs` (or the appropriate per-user directory), and optionally creates a desktop shortcut.

## Versioning and Releases

The `release.yml` workflow runs on every push to the `main` branch:

1. Checks out the repository with full history and tags.
2. Finds the latest `v0.0.N` tag and calculates `N + 1`.
3. Restores dependencies, builds the API, runs tests, and stops if any step fails.
4. Injects the new version into the desktop application's metadata.
5. Publishes the self-contained `win-x64` build, packages the portable version, and builds the installer.
6. Generates release notes, creates an annotated tag, and publishes a GitHub Release containing both artifacts.

The workflow only monitors pushes to the `main` branch, so pushing the generated tag does not trigger another execution. Concurrency is serialized to prevent two releases from generating the same version number. The workflow is granted only the permissions required to read pull requests and write repository contents for tags and releases.

### Release Notes and Contributors

`build/Generate-ReleaseNotes.ps1` lists all commits since the previous tag, preserving links to issues and pull requests. Contributor credits combine commit authors and `Co-authored-by` trailers, normalize identities by email when possible, and filter out known bot and automation accounts. The primary author is always **Artses**.

## System Tray and Windows Startup

* Minimizing or closing the application can hide the window in the system tray, depending on the saved preference.
* Double-clicking the tray icon restores the window.
* The tray menu allows opening the application, opening **Settings**, or exiting the process.
* The **Start with Windows** option only manages `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` and does not require administrator privileges.
* The portable version can also enable startup, but moving or deleting its folder afterward will invalidate the registered path. Disable the option before relocating the executable.

## Icon and Metadata

The vector source is located at:

`Loggin.Desktop/Assets/loggin-mark.svg`

To regenerate the multi-resolution `.ico` file:

```powershell
powershell -ExecutionPolicy Bypass -File build/Create-Icon.ps1
```

The icon is used by the executable, application window, taskbar, system tray, installer, and shortcuts. The application name, version, description, and author are defined in `Loggin.Desktop.csproj` and automatically updated with the pipeline version.

## Known Limitations (Will be fixed)

* Real-time updates rely on polling because the backend does not expose a push-based transport.
* Log source availability also depends on the Go collector and the configured log path.
* Local HTTPS certificates must be trusted by Windows; the application does not disable TLS certificate validation.
* The **System** theme is evaluated when applying or saving settings. If the Windows theme changes while the application is running, the preview must be reapplied.

[![License: CC BY-NC 4.0](https://shields.io)](https://creativecommons.org/licenses/by-nc/4.0/legalcode)
