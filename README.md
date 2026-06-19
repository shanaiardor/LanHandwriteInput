<p align="center">
  <img src="LanHandwriteInput/assets/app-logo.png" alt="logo" width="120" />
</p>

<h1 align="center">局域网手写输入</h1>

<p align="center">
  阅读古籍遇到生僻繁体字？掏出手机写一个，直接送进 <a href="https://github.com/goldendict/goldendict">GoldenDict</a> 查释义。
</p>

<p align="center">
  <a href="#-下载安装"><strong>📥 下载安装包</strong></a> &nbsp;·&nbsp;
  <a href="#-使用场景">使用场景</a> &nbsp;·&nbsp;
  <a href="#-快速开始">快速开始</a> &nbsp;·&nbsp;
  <a href="#-本地开发">本地开发</a>
</p>

---

## 💡 使用场景

用 [GoldenDict](https://github.com/goldendict/goldendict) 阅读古籍、文言文时，经常碰到**不认识、也不会打**的繁体字或异体字（比如「𠮷」「𪚥」「𨰻」）。

传统做法：打开手机手写输入 → 搜字 → 再手动敲进 GoldenDict → 来回切换很麻烦。

**本工具的做法：**

```
📖 读实体书
     │
     ▼
  遇到生僻字
     │
     ▼
  拿起手机 → 扫码 → 手写这个字
     │
     ▼
  字自动出现在 GoldenDict 搜索框
     │
     ▼
GoldenDict 弹出释义 ✅
```

手机变身**无线手写板**，写完即查，不用切换窗口，不用学仓颉/五笔。

## 🚀 快速开始

1. **[下载安装包](https://github.com/shanaiardor/LanHandwriteInput/releases/download/v0.01/LanHandwriteInput-Setup-0.0.1.exe)**，双击安装。
2. 启动程序，桌面右下角出现托盘图标。
3. 手机和电脑连接**同一个 Wi-Fi**。
4. 打开 GoldenDict，**把光标点到搜索框里**。
5. 鼠标悬停在托盘图标上 → 弹出二维码 → 手机扫码。
6. 在手机上**手写那个生僻字**，写完即自动出现在 GoldenDict 搜索框，释义瞬间弹出。
7. 继续阅读下一个字，重复即可。

> 💡 勾选「粘贴后自动回车」，写完字后自动触发 GoldenDict 搜索。

## ✨ 为什么用它配合 GoldenDict？

| 问题 | 解决 |
|---|---|
| 繁体字不会拼音、不会拆字 | 手机上直接手写，识别率极高 |
| 切换窗口打断阅读节奏 | 写完字自动进 GoldenDict，无需碰键盘 |
| 一个字查完要清空再查下一个 | 每次粘贴自动覆盖旧内容 |
| 异体字、生僻字键盘打不出 | 手写不受字符集限制，只要能写就能送 |

## 📸 效果示意

| 桌面端主窗口 | 手机手写页 |
|:---:|:---:|
| 左侧二维码，右侧接收日志 | 大格手写区，写完即发 |
| 「收到：𪚥」→「已发送输入事件」 | 支持空格、退格、清屏 |

## 📥 下载安装

### 安装包（推荐）

**[📦 LanHandwriteInput-Setup-0.0.1.exe](https://github.com/shanaiardor/LanHandwriteInput/releases/download/v0.01/LanHandwriteInput-Setup-0.0.1.exe)**

- Windows 10 / 11 x64
- 安装后开始菜单 / 桌面快捷方式
- 首次运行需允许防火墙访问

### 便携版

前往 [Releases](https://github.com/shanaiardor/LanHandwriteInput/releases) 下载 `publish.zip`，解压即用。

## 🛠 技术栈

| 层 | 技术 |
|---|---|
| 桌面端 | C# / .NET 8 / Windows Forms |
| Web 服务 | ASP.NET Core (Kestrel) 内嵌服务器 |
| 手机前端 | Vue 3 + Vite（移动端手写优化） |
| 二维码 | [QRCoder](https://github.com/codebude/QRCoder) |
| 文本输入 | Win32 `SendInput`（剪贴板 + 模拟按键） |
| 打包 | Inno Setup 6 |

## 📁 项目结构

```
LanHandwriteInput/
├── LanHandwriteInput/
│   ├── Program.cs              # 入口
│   ├── Form1.cs                # 主窗口 / 托盘 / 二维码
│   ├── LocalWebServer.cs       # 内嵌 HTTP 服务器
│   ├── WindowsTextInput.cs     # Win32 模拟输入封装
│   ├── assets/                 # 图标
│   ├── web/                    # Vue 前端源码
│   │   ├── src/App.vue         # 逐字手写组件
│   │   └── src/style.css       # 移动端深色适配
│   └── dist/                   # 前端构建产物
├── installer.iss               # Inno Setup 打包脚本
├── build-web.ps1               # 前端构建
├── build-installer.ps1         # 一键构建+打包
└── LanHandwriteInput.sln
```

## 🔧 本地开发

**环境：** .NET 8 SDK + Node.js ≥ 18

```bash
# 桌面端
cd LanHandwriteInput
dotnet run

# 前端热更新开发
cd LanHandwriteInput/web
npm install
npm run dev

# 构建安装包（需 Inno Setup 6）
.\build-installer.ps1
```

## 🔒 安全说明

- 仅监听局域网，不暴露公网
- 手写文字只做本地粘贴，不上传任何服务器
- 源码完全开放，可审计

## 🐛 常见问题

<details>
<summary><b>手机扫码后打不开？</b></summary>

1. 确保手机电脑同一 Wi-Fi
2. 允许 Windows 防火墙访问本程序
3. 重启程序，重新扫码
</details>

<details>
<summary><b>字没进 GoldenDict？</b></summary>

先在 GoldenDict 搜索框里点一下，确保光标在那里，然后再在手机上写字。
</details>

<details>
<summary><b>能用手机自带手写输入法吗？</b></summary>

可以。手机上系统自带的手写键盘就是最好的选择。写一个候选字上屏一个，体验最佳。
</details>



---

<p align="center">
  <sub>读圣贤书，写方块字 📖✍️</sub>
</p>
