<p align="center"><a href="README.md">English</a> · <a href="README.zh-CN.md">简体中文</a> · <b>Türkçe</b> · <a href="README.ar.md">العربية</a></p>
<p align="center">Video eğitimleri: <a href="https://www.youtube.com/@pwfauth">youtube.com/@pwfauth</a></p>

# PWF Auth için VB.NET ve C# örnek istemcileri

Bu sürüm **PWFAuth 1.7.0** kullanır. Depodaki NuGet.Config, doğrulanmış resmi SDK'yı vendor/ klasöründen geri yükler; aynı paket nuget.org'da da yayımlanmıştır. PWFAuth 1.4.0 ve daha eski sürümler mevcut sunucuyla iletişim kuramaz (sunucu yanıtlarını artık yeni bir anahtarla imzalıyor), bu nedenle eski kopyaları güncelleyin

<!-- CI badge hidden until the GitHub account's Actions billing is resolved. To restore, delete these comment markers:
[![build](https://github.com/pwfauth/pwfauth-vbnet-examples/actions/workflows/build.yml/badge.svg)](https://github.com/pwfauth/pwfauth-vbnet-examples/actions/workflows/build.yml)
-->

**[PWF Auth](https://pwfauth.com)** için **VB.NET** ve **C#** örnek istemcileri. PWF Auth; lisans anahtarları, son kullanıcı hesapları ve güncelleme kontrolleri için tek bir REST API üzerinden çalışan ücretsiz bir arka uçtur. Üç örneğin hepsi resmi **[PWFAuth NuGet paketi](https://www.nuget.org/packages/PWFAuth)** üzerine kuruludur, yani her örnek kendi uygulamanızda yazacağınız çağrılardan ibarettir

![PwfAuthWinFormsClient "API Explorer": oturum açık, heartbeat çalışıyor](docs/api-explorer.png)

> `PwfAuthWinFormsClient`, sekmeli **API Explorer** uygulamasıdır. Her özellik tek tıkla çalışır, oturum açıldıktan sonra düzenli kontrol (heartbeat) arka planda çalışır ve her yanıt etkinlik günlüğüne yazılır

![PwfAuthConsoleClient: tüm özelliklerden geçen tek bir çalıştırma](docs/console-client.png)

> `PwfAuthConsoleClient`, tüm özellikleri tek bir rehberli çalıştırmada gösterir

![PwfAuthCSharpClient: aynı akış .NET Framework 4.8.1 üzerinde](docs/csharp-client.png)

> `PwfAuthCSharpClient`, aynı akışın **.NET Framework 4.8.1 üzerinde C#** sürümüdür. Paket netstandard2.0'ı hedeflediği için eski uygulamalarda da çalışır

| Proje | Ne gösterir |
| --- | --- |
| [`PwfAuthConsoleClient`](PwfAuthConsoleClient) | .NET 8 üzerinde VB.NET. Rehberli bir komut satırı akışı: uygulama bilgisi, anahtar kontrolü, giriş (ve lisansı bu bilgisayara taşıma), heartbeat, kullanıcı değişkenleri, çıkış, ücretsiz deneme, hesaplar, bir anahtarı hesaba tanımlama ve şifre değiştirme |
| [`PwfAuthWinFormsClient`](PwfAuthWinFormsClient) | .NET 8 üzerinde VB.NET. Aynı özellikler sekmeli bir **"API Explorer"** penceresinde; ayrıca arka planda çalışan heartbeat (`StartHeartbeat` + `SessionEnded`), güncelleme kontrolü ve bir **Variables** sekmesi |
| [`PwfAuthCSharpClient`](PwfAuthCSharpClient) | Komut satırı akışının **.NET Framework 4.8.1 üzerinde C#** sürümü |

## Paketin sizin yerinize yaptıkları

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

- **Şifreleme.** Şifrelenmesi gereken istekleri (giriş, heartbeat, çıkış, anahtar kontrolü, uygulama içeriği) şifreler ve şifrelenmeden başarı bildiren bir yanıtı reddeder
- **Donanım kimliği (HWID).** Lisansları bu bilgisayara bağlar
- **Yanlış saat.** Bilgisayarın saati yanlışsa bunu kendiliğinden telafi eder
- **Uzaktan durdurma (kill switch).** Uzaktan durdurma komutunuza uyan heartbeat'i çalıştırır
- **Lisans taşıma.** `ResetHardwareIdAsync`, uygulamanızın bekleme süresine uyarak bir lisansı yeni bir bilgisayara taşır
- **Anahtara bağlı hesaplar.** `RegisterAccountWithKeyAsync` ve `RedeemKeyAsync`, lisans anahtarlarıyla çalışan hesapları kapsar
- **Kullanıcı değişkenleri.** `SetVariableAsync`, `GetVariableAsync`, `GetVariablesAsync` ve `DeleteVariableAsync`, oturum açmış müşteri için sunucuda küçük değerler saklar: ayarlar, ilerleme, işaretler. Bu değerler müşteriyle birlikte yeni bilgisayara taşınır; bunları panelinizde görüp düzenleyebilirsiniz (**Licences** veya **Users** → yan panel → **Variables**)

Bu örneklerin önceki sürümleri elle yazılmış kendi istemcilerini içeriyordu. Artık paketi kullandıkları için sunucudaki her değişikliği ve düzeltmeyi bir paket güncellemesiyle alırlar

## Gereksinimler

- .NET SDK 8.0 veya daha yenisi (`dotnet --version`)
- `PwfAuthCSharpClient` için: **.NET Framework 4.8.1** hedefleme paketi. Visual Studio ile birlikte gelir ve derleme sırasında NuGet üzerinden de geri yüklenir
- Ücretsiz bir PWF Auth hesabı ve bir uygulama. <https://pwfauth.com> adresinden oluşturabilirsiniz

`dotnet build`, `PWFAuth` paketini nuget.org'dan otomatik olarak geri yükler

## Yapılandırma

Dağıtmadan önce seçtiğiniz örnekte uygulama gizli anahtarını (app secret), yani `DefaultAppSecret` değerini ayarlayın. Gerçek gizli anahtarı asla depoya göndermeyin. Sunucu `https://pwfauth.com` olarak sabittir; ayar alanları salt okunurdur ve ortam değişkenleriyle yapılan geçersiz kılmalar yok sayılır

## Çalıştırma

```powershell
# Console — pass the key as an argument, or run with none to be prompted.
# An optional 2nd key (UNUSED — it gets used up) shows redeeming a key onto an account.
dotnet run --project PwfAuthConsoleClient -- PWF-XXXX-XXXX-XXXX [PWF-SPARE-KEY]

# Windows Forms
dotnet run --project PwfAuthWinFormsClient

# C# on .NET Framework 4.8.1
dotnet run --project PwfAuthCSharpClient -- PWF-XXXX-XXXX-XXXX [PWF-SPARE-KEY]
```

## Güvenlik notu

App Secret her istemci uygulamasının içinde dağıtılır ve ikili dosyadan çıkarılabilir; bu yüzden istemci tarafındaki lisans kontrollerini **DRM değil, caydırıcı bir önlem** olarak görün. Gizli anahtarı kaynak kontrolünün dışında tutun: uygulamayı kendi özel derleme sürecinizde yapılandırın. Koddaki varsayılan değer bilerek boş bırakılmıştır

## Lisans

MIT lisansı, ayrıntılar için [LICENSE](LICENSE) dosyasına bakın
