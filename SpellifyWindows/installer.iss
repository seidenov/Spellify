[Setup]
AppName=Spellify
AppVersion=1.0.0
AppPublisher=Spellify
AppPublisherURL=https://github.com/yourusername/spellify
DefaultDirName={autopf}\Spellify
DefaultGroupName=Spellify
UninstallDisplayIcon={app}\Spellify.exe
OutputDir=installer
OutputBaseFilename=Spellify-Setup
Compression=lzma2
SolidCompression=yes
PrivilegesRequired=lowest
SetupIconFile=icon_256x256.ico
WizardStyle=modern

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "startup"; Description: "Запускать при старте Windows"; GroupDescription: "Дополнительно:"

[Files]
Source: "dist\Spellify.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Spellify"; Filename: "{app}\Spellify.exe"
Name: "{group}\Удалить Spellify"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Spellify"; Filename: "{app}\Spellify.exe"; Tasks: desktopicon
Name: "{userstartup}\Spellify"; Filename: "{app}\Spellify.exe"; Tasks: startup

[Run]
Filename: "{app}\Spellify.exe"; Description: "Запустить Spellify"; Flags: nowait postinstall skipifsilent
