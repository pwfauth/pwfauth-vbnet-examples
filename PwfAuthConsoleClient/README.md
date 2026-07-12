# PwfAuthConsoleClient

A VB.NET **console** example that exercises **every client-facing feature** of the
[PWF Auth](https://pwfauth.com) API — a complete, dependency-free reference client.

![A full run — every endpoint green](../docs/console-client.png)

## Features demonstrated

| # | Feature | Endpoint | Wire format |
| --- | --- | --- | --- |
| 0 | App info + update check | `GET /api/app/info.php` | encrypted reply |
| 1 | Check a license key | `POST /api/auth/check-key.php` | plain |
| 2 | Login (bind HWID, open session) | `POST /api/auth/login.php` | **encrypted** |
| 3 | Heartbeat (keep alive / kill code) | `POST /api/auth/heartbeat.php` | **encrypted** |
| 4 | Logout | `POST /api/auth/logout.php` | **encrypted** |
| 5 | Start a free trial | `POST /api/auth/trial.php` | plain |
| 6 | Request an HWID reset | `POST /api/auth/request-hwid-reset.php` | plain |
| 7 | Register an end-user account | `POST /api/auth/account-register.php` | plain |
| 8 | End-user account login | `POST /api/auth/account-login.php` | plain |
| 9 | Change account password | `POST /api/auth/change-password.php` | plain |

`Program.vb` runs all of them in order, printing a one-line explanation + the
result for each.

## Run

```powershell
setx PWF_APP_SECRET "your_app_secret"   # run once (get it from the dashboard)
dotnet run -- PWF-XXXX-XXXX-XXXX          # or run with no argument to be prompted
```

Exit codes: `0` = ran · `2` = key invalid · `3` = network error.

## Files

| File | Role |
| --- | --- |
| `Program.vb` | The guided demo of all features |
| `PwfAuthClient.vb` | The client — one method per feature (`CheckKeyAsync`, `LoginAsync`, `HeartbeatAsync`, `LogoutAsync`, `StartTrialAsync`, `RequestHwidResetAsync`, `AccountRegisterAsync`, `AccountLoginAsync`, `ChangePasswordAsync`, `GetAppInfoAsync`) |
| `CryptoEnvelope.vb` | AES-256-CBC + HMAC-SHA256 envelope — byte-for-byte compatible with the server's `PayloadCrypto` |
| `Hwid.vb` | A stable per-machine hardware id sent at login |

`check-key` and the plain endpoints send/receive normal JSON; **login / heartbeat
/ logout require the encrypted envelope** (and `app/info` returns one). No external
NuGet packages — just `System.Net.Http`, `System.Text.Json`, and
`System.Security.Cryptography`.

## Configuration

| Variable | Purpose | Default |
| --- | --- | --- |
| `PWF_APP_SECRET` | Your app secret (Applications → your app → API key) | — |
| `PWF_BASE_URL` | API base URL | `https://pwfauth.com` |

## Notes

- An app secret shipped in a client binary can be extracted — treat license
  checks as a deterrent, not DRM. Keep the secret out of source control.
- Section 7 creates a **throwaway end-user account** (`demo_<timestamp>`) on the
  server each run, and section 5 issues a trial (once per app/device) — this is
  a demo that actually calls the live API, so it leaves that test data behind.
