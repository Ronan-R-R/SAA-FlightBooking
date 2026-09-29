; =====================================================================
; SAA Flight Booking System - Inno Setup script
; Builds setup.exe that installs the self-contained WPF application.
; The published app is self-contained: the .NET 8 runtime is bundled,
; so no separate runtime install is required on the target machine.
; SQL Server (e.g. .\SQLEXPRESS) and the SAA_FlightBooking database
; must exist on the target - see README.md.
; =====================================================================

#define AppName "SAA Flight Booking System"
#define AppVersion "1.0.0"
#define AppPublisher "CTU Training Solutions"
#define AppExeName "SAA.FlightBooking.exe"
#define PublishDir "..\publish\DesktopApp"

[Setup]
AppId={{8F4C2A91-2D7B-4E36-9C1A-5B2E7A6F1D30}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\SAA Flight Booking
DefaultGroupName=SAA Flight Booking
DisableProgramGroupPage=yes
UninstallDisplayName={#AppName}
UninstallDisplayIcon={app}\{#AppExeName}
OutputDir=Output
OutputBaseFilename=setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
ArchitecturesAllowed=x64compatible
PrivilegesRequired=admin

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"

[Files]
Source: "{#PublishDir}\{#AppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion isreadme

[Icons]
Name: "{group}\SAA Flight Booking"; Filename: "{app}\{#AppExeName}"
Name: "{group}\Uninstall SAA Flight Booking"; Filename: "{uninstallexe}"
Name: "{autodesktop}\SAA Flight Booking"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Launch SAA Flight Booking"; Flags: nowait postinstall skipifsilent
