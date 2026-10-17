# ChatGPT / Codex 550C 开场动画

Windows 桌面启动器：启动 OpenAI 官方桌面应用，在应用窗口上播放 550C 风格动画，然后关闭覆盖层。提供 ChatGPT 和 Codex 两个独立版本与快捷方式。

> 这是个人制作的非官方项目，不隶属于 OpenAI。启动器不会改写官方应用安装文件，也不会打开网页版本。

## 选择版本

### Codex 版：AI 一句话安装

把这句话发给能操作本机文件和 PowerShell 的 AI 助手：

> 请阅读 https://github.com/Sonzay666/chatgpt-startup-animation 的 README 中“Codex 版：AI 一句话安装”，检查并运行 `install-codex.ps1`，从最新 GitHub Release 下载 Codex 版启动器、校验 SHA-256 并创建桌面快捷方式；确认安装了 OpenAI 官方 Windows 桌面应用。如果 Codex 还不是应用的默认启动视图，请在应用设置中将 Codex 设为默认，并确认左上角当前选择的是 Codex；不要修改应用安装文件。完成后启动“Codex 550C 开场动画”快捷方式确认效果。

也可以在 Releases 下载 `Codex-550C-Launcher-win-x64.zip`，完整解压后运行“启动 Codex 550C.cmd”。

Codex 版使用 OpenAI 当前统一的 Windows 桌面应用。官方说明，旧版 Codex 应用更新后会成为包含 ChatGPT、Work 和 Codex 的新桌面应用，并可把 Codex 设为默认打开视图。启动器启动这个官方应用，再覆盖其窗口播放动画。

### ChatGPT 版：AI 一句话安装

> 请按 https://github.com/Sonzay666/chatgpt-startup-animation 的 README 中“ChatGPT 版：AI 一句话安装”步骤，阅读并检查 `install.ps1`，为我在这台 Windows 64 位电脑安装最新版本、校验 SHA-256 并创建桌面快捷方式；只在已安装 OpenAI 官方 Windows 桌面应用和 WebView2 Runtime 时继续，不要修改应用安装文件，完成后启动一次确认。

也可以在 Releases 下载 `ChatGPT-550C-Launcher-win-x64.zip`，完整解压后运行“启动 ChatGPT 550C.cmd”。

## 运行条件

- Windows 10/11 x64。
- 已安装 OpenAI 官方 Windows 桌面应用。网页版本不适用。
- Codex 版需要在应用中选择 Codex 视图，并将其设为默认启动视图；应用更新或账号状态可能影响该设置，请首次运行时确认。
- Microsoft Edge WebView2 Runtime。若启动器提示缺少，请安装[微软官方 Evergreen Runtime](https://developer.microsoft.com/microsoft-edge/webview2/)。
- .NET 运行时已随启动器打包，无需单独安装。

## 安装脚本

`install.ps1` 和 `install-codex.ps1` 分别下载对应版本的 ZIP 与 `SHA256SUMS.txt`，校验后安装到 `%LOCALAPPDATA%\Programs` 下各自的目录，并创建独立桌面快捷方式。无需管理员权限，不会修改官方应用文件。再次运行会安装最新版本。

## 从源码构建

需要 Windows x64 和 .NET 10 SDK：

```powershell
dotnet publish src/Codex550COverlay/Codex550COverlay.csproj -c Release -r win-x64 --self-contained true -o publish-chatgpt
Copy-Item packaging/chatgpt/* publish-chatgpt/
Compress-Archive -Path publish-chatgpt/* -DestinationPath ChatGPT-550C-Launcher-win-x64.zip

dotnet publish src/Codex550COverlay/Codex550COverlay.csproj -c Release -r win-x64 --self-contained true -p:LauncherEdition=Codex -o publish-codex
Copy-Item packaging/codex/* publish-codex/
Compress-Archive -Path publish-codex/* -DestinationPath Codex-550C-Launcher-win-x64.zip
```

推送 `v*` 格式的 Git tag 会触发 GitHub Actions，为两个版本构建 Windows x64 Release，并附上 SHA-256 校验文件。

## 项目文件

- `assets/`：开场动画 HTML。公开版不含原文件中嵌入的音轨；动画默认静音。
- `src/Codex550COverlay/`：原生 WPF 窗口覆盖层。编译时选择 ChatGPT 或 Codex 版本标识。
- `install.ps1`、`install-codex.ps1`：下载、校验、安装并创建对应版本的桌面快捷方式。
- `packaging/chatgpt/`、`packaging/codex/`：各自的便携启动命令和说明文件。

## 许可

项目按 MIT License 发布。动画中的系统数据是视觉模拟内容，不代表真实的硬件检测或 AI 推理过程。
