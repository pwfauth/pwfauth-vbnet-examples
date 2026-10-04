# PwfAuthConsoleClient

A VB.NET **console** example that runs through **every client feature** of the
[PWF Auth](https://pwfauth.com) API, built on the official
[PWFAuth NuGet package](https://www.nuget.org/packages/PWFAuth).

![A full run — every feature green](../docs/console-client.png)

## Features demonstrated

| # | Feature | Package call | Endpoint |
| --- | --- | --- | --- |
| 0 | App info | `GetAppInfoAsync` | `GET /api/app/info.php` |
| 1 | Check a license key | `CheckKeyAsync` | `POST /api/auth/check-key.php` |
| 2 | Login (bind this PC, open a session) | `LoginAsync` | `POST /api/auth/login.php` |
| 2 | Move a license to this PC, when it is bound to another one | `ResetHardwareIdAsync` | `POST /api/customer/reset-hwid.php` |
| 3 | Heartbeat | `HeartbeatAsync` | `POST /api/auth/heartbeat.php` |
| 4 | Logout | `LogoutAsync` | `POST /api/auth/logout.php` |
| 5 | Free trial | `CreateTrialAsync` | `POST /api/auth/trial.php` |
| 6 | Register an end-user account | `RegisterAccountAsync` / `RegisterAccountWithKeyAsync` | `POST /api/auth/account-register.php` |
| 6 | Account login | `AccountLoginAsync` | `POST /api/auth/account-login.php` |
| 7 | Redeem a key onto the account | `RedeemKeyAsync` | `POST /api/auth/account-redeem.php` |
| 8 | Change the account password | `ChangeAccountPasswordAsync` | `POST /api/auth/change-password.php` |

`Program.vb` runs them in order and prints a one-line result for each.

- **Section 2** asks before moving a license. A move unbinds every device of the
  key and starts your app's cooldown, 12 hours by default.
- **Section 3** sends one heartbeat by hand. A real app calls
  `client.StartHeartbeat()` once after login instead, and handles `SessionEnded`.
  The WinForms example shows how.

## Run

```powershell
setx PWF_APP_SECRET "your_app_secret"            # run once (get it from the dashboard)
dotnet run -- PWF-XXXX-XXXX-XXXX                  # or run with no argument to be prompted
dotnet run -- PWF-XXXX-XXXX-XXXX PWF-SPARE-KEY    # also redeem an UNUSED key onto the demo account
```

Exit codes: `0` = ran · `1` = missing secret/key · `3` = the server could not be reached
or refused the App Secret.

## Configuration

| Variable | Purpose | Default |
| --- | --- | --- |
| `PWF_APP_SECRET` | Your app secret (Applications → your app → App Settings) | — |
| `PWF_BASE_URL` | API base URL | `https://pwfauth.com` |
| `PWF_SPARE_KEY` | An unused key for section 7 (same as the 2nd argument) | — |

## Notes

- An app secret shipped in a client binary can be extracted. Treat license
  checks as a deterrent, not DRM, and keep the secret out of source control.
- This demo calls the real API, so it leaves test data on the server:
  - Section 6 creates a **throwaway end-user account** (`demo_<timestamp>`) on
    every run.
  - Section 5 issues a trial, once per app and device.
  - Section 7 uses up the spare key.
- If your app turned on **"Sign-up needs a license key"**, section 6 signs up
  with the spare key instead (`RegisterAccountWithKeyAsync`).
