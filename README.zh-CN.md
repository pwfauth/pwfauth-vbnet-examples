<p align="center"><a href="README.md">English</a> · <b>简体中文</b> · <a href="README.tr.md">Türkçe</a> · <a href="README.ar.md">العربية</a></p>
<p align="center">视频教程：<a href="https://www.youtube.com/@pwfauth">youtube.com/@pwfauth</a></p>

# PWF Auth 的 VB.NET 与 C# 示例客户端

本版本使用 **PWFAuth 1.7.0**。仓库自带的 NuGet.Config 会从 vendor/ 还原经过验证的官方 SDK，nuget.org 上发布的也是同一个包。PWFAuth 1.4.0 及更早版本无法与当前服务器通信（服务器现在用新密钥对响应签名），请升级旧的副本

<!-- CI badge hidden until the GitHub account's Actions billing is resolved. To restore, delete these comment markers:
[![build](https://github.com/pwfauth/pwfauth-vbnet-examples/actions/workflows/build.yml/badge.svg)](https://github.com/pwfauth/pwfauth-vbnet-examples/actions/workflows/build.yml)
-->

这是 **[PWF Auth](https://pwfauth.com)** 的 **VB.NET** 和 **C#** 示例客户端。PWF Auth 是一个免费的后端，通过同一套 REST API 提供许可证密钥、终端用户账户和更新检查。三个示例都基于官方的 **[PWFAuth NuGet 包](https://www.nuget.org/packages/PWFAuth)**，所以每个示例里的代码就是你在自己应用中会写的那些调用

![PwfAuthWinFormsClient 的“API Explorer”：已登录，心跳运行中](docs/api-explorer.png)

> `PwfAuthWinFormsClient` 是带标签页的 **API Explorer**。每个功能点一下即可调用，登录后心跳在后台运行，每条响应都会写入活动日志

![PwfAuthConsoleClient：一次运行走完所有功能](docs/console-client.png)

> `PwfAuthConsoleClient` 通过一次引导式运行演示全部功能

![PwfAuthCSharpClient：在 .NET Framework 4.8.1 上运行同一流程](docs/csharp-client.png)

> `PwfAuthCSharpClient` 是同一流程的 **C# / .NET Framework 4.8.1** 版本。该包面向 netstandard2.0，因此较旧的应用也能使用

| 项目 | 演示内容 |
| --- | --- |
| [`PwfAuthConsoleClient`](PwfAuthConsoleClient) | VB.NET，基于 .NET 8。引导式命令行流程：应用信息、检查密钥、登录（以及把许可证迁移到本机）、心跳、用户变量、注销、免费试用、账户、把密钥兑换到账户上，以及修改密码 |
| [`PwfAuthWinFormsClient`](PwfAuthWinFormsClient) | VB.NET，基于 .NET 8。在带标签页的 **"API Explorer"** 窗口中提供相同功能，另外还有后台心跳（`StartHeartbeat` + `SessionEnded`）、更新检查和 **Variables** 标签页 |
| [`PwfAuthCSharpClient`](PwfAuthCSharpClient) | 命令行流程的 **C# / .NET Framework 4.8.1** 版本 |

## 这个包帮你做了什么

```vb
' One client for the whole app: it holds the session and runs the heartbeat.
Private ReadOnly _client As New PwfClient("YOUR_APP_SECRET")

Private Async Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
    Dim login = Await _client.LoginAsync(txtKey.Text)
    If Not login.Success Then MessageBox.Show(login.Message) : Return
    _client.StartHeartbeat()    ' raises SessionEnded on a ban, pause, expiry or reset

    ' a value kept on the server for this key, editable in your panel
    Await _client.SetVariableAsync("theme", "dark")
End Sub
```

- **加密。** 对必须加密的请求（登录、心跳、注销、检查密钥、应用内容）进行加密，并拒绝任何未加密却声称成功的响应
- **硬件 ID（HWID）。** 把许可证绑定到当前电脑
- **时钟错误。** 电脑时钟不准时会自动修正
- **远程停用（kill switch）。** 运行心跳，遵从你在后台发出的远程停用
- **迁移许可证。** `ResetHardwareIdAsync` 把许可证迁移到新电脑，并遵守应用设置的冷却时间
- **基于密钥的账户。** `RegisterAccountWithKeyAsync` 和 `RedeemKeyAsync` 用于以许可证密钥为基础的账户
- **用户变量。** `SetVariableAsync`、`GetVariableAsync`、`GetVariablesAsync` 和 `DeleteVariableAsync` 为已登录的客户在服务器上保存少量数据，比如设置、进度、标记。这些数据会跟随客户迁移到新电脑，你也可以在后台查看和编辑（**Licences** 或 **Users** → 侧边面板 → **Variables**）

这些示例的早期版本自带一个手写的客户端。现在它们改用这个包，只需更新包就能获得服务器端的所有变更和修复

## 环境要求

- .NET SDK 8.0 或更高版本（`dotnet --version`）
- 对于 `PwfAuthCSharpClient`：需要 **.NET Framework 4.8.1** 目标包。它随 Visual Studio 一起安装，构建时也会通过 NuGet 还原
- 一个免费的 PWF Auth 账户和一个应用，可在 <https://pwfauth.com> 创建

`dotnet build` 会自动从 nuget.org 还原 `PWFAuth` 包

## 配置

分发前，请在所选示例中配置应用密钥（app secret）`DefaultAppSecret`。切勿把真实的密钥提交到仓库。服务器固定为 `https://pwfauth.com`，设置字段为只读，环境变量覆盖会被忽略

## 运行

```powershell
# Console — pass the key as an argument, or run with none to be prompted.
# An optional 2nd key (UNUSED — it gets used up) shows redeeming a key onto an account.
dotnet run --project PwfAuthConsoleClient -- PWF-XXXX-XXXX-XXXX [PWF-SPARE-KEY]

# Windows Forms
dotnet run --project PwfAuthWinFormsClient

# C# on .NET Framework 4.8.1
dotnet run --project PwfAuthCSharpClient -- PWF-XXXX-XXXX-XXXX [PWF-SPARE-KEY]
```

## 安全提示

应用密钥（App Secret）会随客户端应用一起分发，可以从二进制文件中提取出来，因此客户端的许可证校验只能当作**威慑手段，而不是 DRM**。不要把密钥放进源码管理：请在你自己的私有构建流程中配置应用。代码中的默认值特意留空

## 许可

MIT 许可，详见 [LICENSE](LICENSE)
