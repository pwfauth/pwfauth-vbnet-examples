# PwfAuthCSharpClient

A **C# / .NET Framework 4.8.1** console example that runs through **every client
feature** of the [PWF Auth](https://pwfauth.com) API. It is the same program as
[`PwfAuthConsoleClient`](../PwfAuthConsoleClient) (VB.NET on .NET 8), written in
C# for **.NET Framework** apps.

![A full run — every feature green](../docs/csharp-client.png)

## Built on the PWFAuth package

The official [PWFAuth NuGet package](https://www.nuget.org/packages/PWFAuth)
targets **netstandard2.0**, so the same client works here and on modern .NET.
`dotnet build` restores it and its dependencies (`System.Text.Json` and friends)
from nuget.org.

The program also turns on **TLS 1.2** through `ServicePointManager.SecurityProtocol`,
because older .NET Framework defaults are rejected by the server's edge.

## Features demonstrated

| # | Feature | Package call |
| --- | --- | --- |
| 0 | App info | `GetAppInfoAsync` |
| 1 | Check a license key | `CheckKeyAsync` |
| 2 | Login · move a license to this PC when it is bound elsewhere | `LoginAsync` · `ResetHardwareIdAsync` |
| 3 | Heartbeat | `HeartbeatAsync` (a real app: `StartHeartbeat` + `SessionEnded`) |
| 4 | Logout | `LogoutAsync` |
| 5 | Free trial | `CreateTrialAsync` |
| 6 | Register · account login | `RegisterAccountAsync` (or `RegisterAccountWithKeyAsync`) · `AccountLoginAsync` |
| 7 | Redeem a key onto the account | `RedeemKeyAsync` |
| 8 | Change the account password | `ChangeAccountPasswordAsync` |

## Run

```powershell
setx PWF_APP_SECRET "your_app_secret"            # run once (get it from the dashboard)
dotnet run -- PWF-XXXX-XXXX-XXXX                  # or run with no argument to be prompted
dotnet run -- PWF-XXXX-XXXX-XXXX PWF-SPARE-KEY    # also redeem an UNUSED key onto the demo account
```

Exit codes: `0` = ran · `1` = missing secret/key · `3` = the server could not be reached
or refused the App Secret.

Building targets `net481`, so you need the **.NET Framework 4.8.1**
developer/targeting pack. It is bundled with Visual Studio; otherwise
`dotnet build` restores it through the build-only
`Microsoft.NETFramework.ReferenceAssemblies` package.

## Files

| File | Role |
| --- | --- |
| `Program.cs` | The guided demo of all features |
| `PwfAuthCSharpClient.csproj` | `net481` + the `PWFAuth` package |

## Notes

- An app secret shipped in a client binary can be extracted. Treat license
  checks as a deterrent, not DRM. Keep the secret out of source control: use the
  `PWF_APP_SECRET` environment variable. The in-code default is blank.
- This demo calls the real API, so it leaves test data on the server:
  - The account section creates a throwaway `demo_<timestamp>` account.
  - Section 5 issues a trial, once per app and device.
  - Section 7 uses up the spare key.
