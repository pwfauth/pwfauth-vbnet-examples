# PwfAuthWinFormsClient

A VB.NET **Windows Forms** desktop UI — an "API Explorer" that exercises **every
client-facing feature** of the [PWF Auth](https://pwfauth.com) API from a tabbed
window, writing each request/response to a live **Activity log**.

> The console sibling ([PwfAuthConsoleClient](../PwfAuthConsoleClient)) runs the
> same features from the command line. Both share the identical `PwfAuthClient`,
> `CryptoEnvelope`, and `Hwid` files.

## Features demonstrated

| Tab | Feature | Endpoint | Wire format |
| --- | --- | --- | --- |
| Activation | Check a license key | `POST /api/auth/check-key.php` | plain |
| Activation | Login (bind HWID, open session) | `POST /api/auth/login.php` | **encrypted** |
| Activation | Heartbeat (keep alive / kill code) | `POST /api/auth/heartbeat.php` | **encrypted** |
| Activation | Logout | `POST /api/auth/logout.php` | **encrypted** |
| Trial & Reset | Start a free trial | `POST /api/auth/trial.php` | plain |
| Trial & Reset | Request an HWID reset | `POST /api/auth/request-hwid-reset.php` | plain |
| Accounts | Register an end-user account | `POST /api/auth/account-register.php` | plain |
| Accounts | End-user account login | `POST /api/auth/account-login.php` | plain |
| Accounts | Change account password | `POST /api/auth/change-password.php` | plain |
| App info | App info + update check | `GET /api/app/info.php` | encrypted reply |

## Run

```powershell
setx PWF_APP_SECRET "your_app_secret"   # run once (get it from the dashboard)
dotnet run
```

Requires Windows (targets `net8.0-windows`). The **App Secret** and **Base URL**
are editable at the top of the window; the fields are pre-filled from
`PWF_APP_SECRET` / `PWF_BASE_URL` (or the fallbacks in `Form1.vb`).

## How it works

`Form1.vb` builds a dark-themed, tabbed UI in code (no designer needed to read
it). Every button calls one method on the shared `PwfAuthClient` and logs the
outcome. The session tab shows the real lifecycle: **Check → Login → Heartbeat →
Logout**, where login/heartbeat/logout travel as the AES-256 + HMAC envelope
(`CryptoEnvelope.vb`) while check-key stays plain JSON.

No external NuGet packages — just `System.Net.Http`, `System.Text.Json`, and
`System.Security.Cryptography`.

## Files

| File | Role |
| --- | --- |
| `Form1.vb` | The tabbed UI + one event handler per feature |
| `PwfAuthClient.vb` | The client — one method per feature |
| `CryptoEnvelope.vb` | AES-256-CBC + HMAC-SHA256 envelope — byte-for-byte compatible with the server's `PayloadCrypto` |
| `Hwid.vb` | A stable per-machine hardware id sent at login |

## Configuration

| Variable | Purpose | Default |
| --- | --- | --- |
| `PWF_APP_SECRET` | Your app secret (Applications → your app → API key) | — |
| `PWF_BASE_URL` | API base URL | `https://pwfauth.com` |

## Notes

- An app secret shipped in a desktop binary can be extracted — treat client-side
  license checks as a deterrent, not DRM. Keep the secret out of source control.
- The **Accounts** tab auto-fills a throwaway `demo_<timestamp>` account when you
  leave the fields blank, and **Start free trial** issues a trial (once per
  app/device) — this is a demo that hits the live API, so it leaves that test
  data on the server.
