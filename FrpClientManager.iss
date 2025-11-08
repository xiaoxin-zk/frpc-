; Inno Setup 脚本文件 - 中文注释
; 用于创建 FRP客户端管理器的安装程序

#define MyAppName "Frp自定义启动器"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "小小渔"
#define MyAppURL "https://www.yourcompany.com/"
#define MyAppExeName "FrpClientManager.exe"
#define MyAppOutputDir "安装包输出"

[Setup]
; 注意: AppId 的值唯一标识此应用程序
AppId={{3cb834e2-a05b-4189-b757-34bda75602f7}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
; 修复：确保允许选择安装目录
DisableDirPage=no
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
; 取消注释下一行可以为所有用户安装（需要管理员权限）
; PrivilegesRequired=admin
OutputDir={#MyAppOutputDir}
OutputBaseFilename=FRP客户端管理器安装程序
SetupIconFile=logo.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
; 中文设置
LanguageDetectionMethod=uilanguage
ShowLanguageDialog=auto
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
; 保留图片设置
WizardSmallImageFile=setup-small.bmp
WizardImageFile=setup-large.bmp
; 安装程序窗口标题
AppCopyright=版权所有 (c) 2024 {#MyAppPublisher}
; 修复：确保创建卸载注册表键
CreateUninstallRegKey=yes
; 允许用户选择开始菜单文件夹
DisableProgramGroupPage=no

[Languages]
Name: "chinesesimp"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"

[CustomMessages]
chinesesimp.CreateDesktopIcon=创建桌面快捷方式(&D)
chinesesimp.CreateQuickLaunchIcon=创建快速启动栏快捷方式(&Q)
chinesesimp.LaunchProgram=启动 FRP客户端管理器(&L)

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1

[Types]
Name: "full"; Description: "完全安装"
Name: "compact"; Description: "精简安装"
Name: "custom"; Description: "自定义安装"; Flags: iscustom

[Components]
Name: "main"; Description: "主程序文件"; Types: full compact custom; Flags: fixed
Name: "docs"; Description: "说明文档"; Types: full

[Files]
Source: "PublishOutput\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs; Components: main
; 修复：确保图标文件被正确安装
Source: "logo.ico"; DestDir: "{app}"; Flags: ignoreversion; Components: main
; 注意: 不要在任何共享系统文件上使用 "Flags: ignoreversion"

[Icons]
; 修复：简化图标路径，直接使用可执行文件自带的图标
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\卸载 {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; 修复：在卸载前确保程序已关闭
Filename: "taskkill.exe"; Parameters: "/f /im {#MyAppExeName}"; Flags: runhidden waituntilterminated skipifdoesntexist
; 如果您的程序有卸载参数，可以保留下面这行，否则可以删除
; Filename: "{app}\{#MyAppExeName}"; Parameters: "/uninstall"; Flags: runhidden waituntilterminated skipifdoesntexist

[Registry]
; 修复：使用 AppId 作为注册表键名，避免中文和空格问题
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{{3cb834e2-a05b-4189-b757-34bda75602f7}"; ValueType: string; ValueName: "DisplayName"; ValueData: "{#MyAppName}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{{3cb834e2-a05b-4189-b757-34bda75602f7}"; ValueType: string; ValueName: "UninstallString"; ValueData: """{uninstallexe}"""; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{{3cb834e2-a05b-4189-b757-34bda75602f7}"; ValueType: string; ValueName: "DisplayIcon"; ValueData: "{app}\{#MyAppExeName}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{{3cb834e2-a05b-4189-b757-34bda75602f7}"; ValueType: string; ValueName: "DisplayVersion"; ValueData: "{#MyAppVersion}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{{3cb834e2-a05b-4189-b757-34bda75602f7}"; ValueType: string; ValueName: "Publisher"; ValueData: "{#MyAppPublisher}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{{3cb834e2-a05b-4189-b757-34bda75602f7}"; ValueType: string; ValueName: "URLInfoAbout"; ValueData: "{#MyAppURL}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{{3cb834e2-a05b-4189-b757-34bda75602f7}"; ValueType: string; ValueName: "InstallLocation"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{{3cb834e2-a05b-4189-b757-34bda75602f7}"; ValueType: dword; ValueName: "NoModify"; ValueData: "1"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{{3cb834e2-a05b-4189-b757-34bda75602f7}"; ValueType: dword; ValueName: "NoRepair"; ValueData: "1"; Flags: uninsdeletekey

[Code]
// 自定义代码：中文提示信息
function InitializeSetup(): Boolean;
begin
  // 由于我们发布的是自包含应用，不需要检查 .NET 运行时
  Result := True;
end;

// 在安装前显示欢迎信息
procedure InitializeWizard();
begin
  WizardForm.WelcomeLabel1.Caption := '欢迎使用 FRP客户端管理器 安装向导';
  WizardForm.WelcomeLabel2.Caption := '这个向导将引导您完成 {#MyAppName} 的安装。' + #13#10 + #13#10 + '建议您在继续安装前关闭所有其他应用程序。';
end;

// 在安装完成后显示完成信息
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    MsgBox('安装完成！' + #13#10 + #13#10 + '{#MyAppName} 已成功安装到您的计算机。' + #13#10 + '您可以通过桌面或开始菜单快捷方式启动程序。', mbInformation, MB_OK);
  end;
end;

// 修复：增强的卸载前处理
function InitializeUninstall(): Boolean;
var
  ResultCode: Integer;
begin
  // 卸载前确认
  if MsgBox('您确定要完全卸载 {#MyAppName} 以及所有的组件吗？', mbConfirmation, MB_YESNO) = IDYES then
  begin
    // 确保程序已关闭
    Exec('taskkill.exe', '/f /im {#MyAppExeName}', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    
    // 额外等待一段时间确保进程完全结束
    Sleep(1000);
    
    Result := True;
  end
  else
  begin
    Result := False;
  end;
end;

// 修复：卸载过程中处理文件占用问题
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  ResultCode: Integer;
begin
  case CurUninstallStep of
    usUninstall:
      begin
        // 再次确保程序已关闭
        Exec('taskkill.exe', '/f /im {#MyAppExeName}', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
      end;
    usPostUninstall:
      begin
        // 卸载完成后提示
        MsgBox('卸载完成！{#MyAppName} 已从您的计算机中移除。', mbInformation, MB_OK);
      end;
  end;
end;