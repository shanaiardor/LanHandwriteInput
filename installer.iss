#define MyAppName "局域网手写输入"
#define MyAppVersion "0.1.0"
#define MyAppExeName "LanHandwriteInput.exe"

[Setup]
AppId={{B8B7A937-DAF0-4C74-8FEF-BC31F4C17322}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher=LanHandwriteInput
DefaultDirName={localappdata}\Programs\LanHandwriteInput
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=artifacts\installer
OutputBaseFilename=LanHandwriteInput-Setup-{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "附加图标："; Flags: unchecked

[Files]
Source: "artifacts\publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "启动 {#MyAppName}"; Flags: nowait postinstall skipifsilent
