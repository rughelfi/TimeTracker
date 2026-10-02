; Inno Setup Script for TimeTracker
; Compatible with Inno Setup 6.x

#ifndef MyAppVersion
#define MyAppVersion "1.0.0"
#endif

#define MyAppName "TimeTracker"
#define MyAppPublisher "TimeTracker"
#define MyAppURL "https://github.com/rughelfi/TimeTracker"
#define MyAppExeName "TimeTracker.exe"

#ifndef SourceDir
#define SourceDir "temp_publish"
#endif

#ifndef OutputDir
#define OutputDir "Output"
#endif

[Setup]
; AppId is a unique GUID for TimeTracker. Do not change it across versions!
AppId={{B37C7146-59A6-4B81-995C-62425FE4CD30}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
; Per-user install without admin rights or all-users install with elevation
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog commandline
OutputDir={#OutputDir}
OutputBaseFilename=TimeTracker-Setup-v{#MyAppVersion}
SetupIconFile=..\TimeTracker\Resources\app_icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "it"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "en"; MessagesFile: "compiler:Default.isl"

[CustomMessages]
it.CreateDesktopIcon=Crea un'icona sul &Desktop
it.AutoStartGroup=Opzioni di avvio:
it.AutoStartProgram=Avvia &TimeTracker automaticamente all'avvio di Windows
it.LaunchProgram=Avvia TimeTracker adesso
en.CreateDesktopIcon=Create a &desktop shortcut
en.AutoStartGroup=Startup options:
en.AutoStartProgram=Start &TimeTracker automatically when Windows starts
en.LaunchProgram=Launch TimeTracker now

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "autostart"; Description: "{cm:AutoStartProgram}"; GroupDescription: "{cm:AutoStartGroup}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
; Set Windows Startup Run entry if autostart task was chosen
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#MyAppName}"; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletevalue; Tasks: autostart
; Ensure Run key is deleted on uninstall even if configured from within app settings
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: none; ValueName: "{#MyAppName}"; Flags: uninsdeletevalue

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram}"; Flags: nowait postinstall skipifsilent
