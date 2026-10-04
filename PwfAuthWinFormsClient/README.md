# PwfAuthWinFormsClient

A VB.NET **Windows Forms** "API Explorer" for the [PWF Auth](https://pwfauth.com)
API, built on the official [PWFAuth NuGet package](https://www.nuget.org/packages/PWFAuth).
Every client feature is one button in a tabbed window, and each result goes to a
live **Activity log**.

![The API Explorer: signed in, heartbeat running](../docs/api-explorer.png)

> The console sibling ([PwfAuthConsoleClient](../PwfAuthConsoleClient)) runs the
> same features from the command line.

## Features demonstrated

| Tab | Feature | Package call |
| --- | --- | --- |
| Activation | Check a license key (no device seat used) | `CheckKeyAsync` |
| Activation | Login, then the background heartbeat | `LoginAsync` + `StartHeartbeat` |
| Activation | The kill switch: ban, pause, expiry or reset signs the window out | `SessionEnded` event |
| Activation | One heartbeat by hand · Logout | `HeartbeatAsync` · `LogoutAsync` |
| Trial & Reset | Free trial | `CreateTrialAsync` |
| Trial & Reset | Move a license to this PC | `ResetHardwareIdAsync` |
| Accounts | Register · Register with a license key | `RegisterAccountAsync` · `RegisterAccountWithKeyAsync` |
| Accounts | Account login (with heartbeat) | `AccountLoginAsync` |
| Accounts | Redeem a key onto the account | `RedeemKeyAsync` |
| Accounts | Change password (signs the account out on every device) | `ChangeAccountPasswordAsync` |
| App info | Name, version, download URL, login message, maintenance | `GetAppInfoAsync` |
| App info | Update check | `CheckUpdateAsync` |

## Run

```powershell
setx PWF_APP_SECRET "your_app_secret"   # run once (get it from the dashboard)
dotnet run
```

Requires Windows (targets `net8.0-windows`). The **App Secret** and **Base URL**
fields at the top of the window are pre-filled from `PWF_APP_SECRET` and
`PWF_BASE_URL`, or from the fallbacks in `Form1.vb`.

## How it works

- **The layout** lives in `Form1.Designer.vb`, so you can edit it in the Visual
  Studio Windows Forms Designer.
- **The code-behind** in `Form1.vb` keeps **one `PwfClient`** for the whole app,
  because the client holds the session and runs the heartbeat. Each button calls
  one method and logs the reply.
- **The session.** The login handler calls `client.StartHeartbeat()` on the UI
  thread. The package then raises `SessionEnded` on the UI thread too, so the
  handler can update labels directly, with no `Invoke`.
- **Try the kill switch.** Ban the key in your dashboard and the window signs out
  within one beat.
- **Closing the window** logs out, so the session ends at once instead of timing out on the server.

## Files

| File | Role |
| --- | --- |
| `Form1.vb` | One event handler per feature, plus the `SessionEnded` handler |
| `Form1.Designer.vb` | The tabbed layout (Windows Forms Designer) |
| `Program.vb` | Starts the app |

## Configuration

| Variable | Purpose | Default |
| --- | --- | --- |
| `PWF_APP_SECRET` | Your app secret (Applications → your app → App Settings) | — |
| `PWF_BASE_URL` | API base URL | `https://pwfauth.com` |

## Notes

- An app secret shipped in a desktop binary can be extracted. Treat client-side
  license checks as a deterrent, not DRM, and keep the secret out of source
  control.
- **Test data.** This demo calls the real API, so it leaves test data on the
  server:
  - When you leave the fields blank, the **Accounts** tab fills in a throwaway
    `demo_<timestamp>` account.
  - **Start free trial** issues a trial, once per app and device.
  - **Register with the key** and **Redeem the key** use the key up.
