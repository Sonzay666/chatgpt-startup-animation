# ChatGPT 开场动画

一个 Windows 启动器：打开已安装的 ChatGPT 桌面应用，在它的窗口上播放 550C 风格动画，然后关闭覆盖层，让 ChatGPT 继续使用。

> 这是个人制作的非官方项目，不隶属于 OpenAI。启动器不会改写 ChatGPT 安装文件，也不会打开 ChatGPT 网页。

## 给使用者

### AI 一句话安装

把下面这句话发给你正在使用的电脑 AI 助手：

> 请按 https://github.com/Sonzay666/chatgpt-startup-animation 的 README 中“AI 一句话安装”步骤，阅读并检查 `install.ps1`，为我在这台 Windows 64 位电脑安装最新版本、校验 SHA-256 并创建桌面快捷方式；只在已安装 ChatGPT Windows 桌面版和 WebView2 Runtime 时继续，不要修改 ChatGPT 安装文件，完成后启动一次确认。

AI 助手需要能访问网络和本机文件，并能运行 PowerShell。也可以直接下载仓库 Releases 中的 ZIP，完整解压后运行“启动 ChatGPT 550C.cmd”。

### 运行条件

- Windows 10/11 x64。
- 已安装官方 ChatGPT Windows 桌面应用。浏览器网页版不适用。
- Microsoft Edge WebView2 Runtime。许多 Windows 设备已经安装；若启动器提示缺少，请从[微软官方下载页](https://developer.microsoft.com/microsoft-edge/webview2/)安装 Evergreen Runtime。
- .NET 运行时已随启动器打包，无需单独安装。

### 安装脚本做什么

`install.ps1` 从 GitHub 最新 Release 下载启动器 ZIP 和 SHA-256 校验文件，验证压缩包后将它安装到 `%LOCALAPPDATA%\Programs\ChatGPT-550C-Animation` 下的版本目录，并创建桌面快捷方式。脚本不需要管理员权限，不会改动 ChatGPT 文件。再次运行可安装新版本。

## 从源码构建

需要 Windows x64 和 .NET 10 SDK：

```powershell
dotnet publish src/Codex550COverlay/Codex550COverlay.csproj -c Release -r win-x64 --self-contained true -o publish
Copy-Item packaging/* publish/
Compress-Archive -Path publish/* -DestinationPath ChatGPT-550C-Launcher-win-x64.zip
```

推送 `v*` 格式的 Git tag 会触发 GitHub Actions 构建并发布 Windows x64 Release，包含 ZIP 和校验文件。

## 项目文件

- `assets/`：开场动画 HTML。公开版不含原文件中嵌入的音轨；动画默认静音。
- `src/Codex550COverlay/`：原生 WPF 窗口覆盖层和 ChatGPT 桌面应用启动代码。
- `install.ps1`：下载、校验、安装并创建快捷方式。
- `packaging/`：便携版启动命令和说明文件。

## 许可

项目按 MIT License 发布。动画是视觉模拟内容，不代表真实的硬件检测或 AI 推理过程。
