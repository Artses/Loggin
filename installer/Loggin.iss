#ifndef MyAppVersion
  #define MyAppVersion "0.0.1"
#endif
#ifndef PublishDir
  #define PublishDir "..\artifacts\publish"
#endif
#ifndef OutputDir
  #define OutputDir "..\artifacts"
#endif

#define MyAppName "Loggin Desktop"
#define MyAppPublisher "Artses"
#define MyAppExeName "Loggin.Desktop.exe"
#define MyAppURL "https://github.com/lucasfhs/Loggin"

[Setup]
AppId={{A99E76E0-B608-4D33-AB53-D13DD0CF9E37}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={autopf}\Artses\Loggin Desktop
DefaultGroupName=Loggin Desktop
DisableProgramGroupPage=yes
OutputDir={#OutputDir}
OutputBaseFilename=Loggin-Setup-v{#MyAppVersion}-win-x64
SetupIconFile=..\Loggin.Desktop\Assets\loggin.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
VersionInfoVersion={#MyAppVersion}.0
VersionInfoCompany=Artses
VersionInfoDescription=Instalador do Loggin Desktop
VersionInfoProductName=Loggin Desktop
VersionInfoProductVersion={#MyAppVersion}

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "Criar um atalho na área de trabalho"; GroupDescription: "Atalhos adicionais:"

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\Loggin Desktop"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\Assets\loggin.ico"
Name: "{autodesktop}\Loggin Desktop"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\Assets\loggin.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Abrir o Loggin Desktop"; Flags: nowait postinstall skipifsilent
