<p align="center"><a href="README.md">English</a> · <a href="README.zh-CN.md">简体中文</a> · <a href="README.tr.md">Türkçe</a> · <b>العربية</b></p>
<p align="center">شروحات بالفيديو: <a href="https://www.youtube.com/@pwfauth">youtube.com/@pwfauth</a></p>

# أمثلة عملاء VB.NET و C# لمنصة PWF Auth

يستخدم هذا الإصدار من الأمثلة **PWFAuth 1.7.0**. يستعيد ملف NuGet.Config المرفق حزمة SDK الرسمية الموثّقة من المجلد vendor/، والحزمة نفسها منشورة على nuget.org. لا تستطيع الإصدارات PWFAuth 1.4.0 وما قبلها التواصل مع الخادم الحالي (فهو يوقّع ردوده الآن بمفتاح جديد)، لذلك حدّث أي نسخة قديمة لديك

<!-- CI badge hidden until the GitHub account's Actions billing is resolved. To restore, delete these comment markers:
[![build](https://github.com/pwfauth/pwfauth-vbnet-examples/actions/workflows/build.yml/badge.svg)](https://github.com/pwfauth/pwfauth-vbnet-examples/actions/workflows/build.yml)
-->

أمثلة عملاء بلغتَي **VB.NET** و **C#** لمنصة **[PWF Auth](https://pwfauth.com)**، وهي خدمة خلفية مجانية لمفاتيح الترخيص وحسابات المستخدمين والتحقق من التحديثات، كلها عبر واجهة REST API واحدة. الأمثلة الثلاثة مبنية على **[حزمة PWFAuth الرسمية على NuGet](https://www.nuget.org/packages/PWFAuth)**، لذا فكل مثال هو الاستدعاءات نفسها التي ستكتبها في تطبيقك

![واجهة "API Explorer" في PwfAuthWinFormsClient بعد تسجيل الدخول، والتحقق الدوري يعمل](docs/api-explorer.png)

> المشروع `PwfAuthWinFormsClient` هو **API Explorer** بواجهة تبويبات. كل ميزة بنقرة واحدة، ويعمل التحقق الدوري (heartbeat) في الخلفية بعد تسجيل الدخول، وتظهر كل استجابة في سجل النشاط

![PwfAuthConsoleClient: تشغيل واحد يمر بكل الميزات](docs/console-client.png)

> المشروع `PwfAuthConsoleClient` تشغيل واحد موجّه يمر بكل الميزات

![PwfAuthCSharpClient: التشغيل نفسه على .NET Framework 4.8.1](docs/csharp-client.png)

> المشروع `PwfAuthCSharpClient` هو التشغيل نفسه بلغة **C# على .NET Framework 4.8.1**. تستهدف الحزمة netstandard2.0، لذا تعمل في التطبيقات الأقدم أيضاً

| المشروع | ما يعرضه |
| --- | --- |
| [`PwfAuthConsoleClient`](PwfAuthConsoleClient) | مكتوب بلغة VB.NET على .NET 8. تشغيل موجّه من سطر الأوامر: معلومات التطبيق، والتحقق من المفتاح، وتسجيل الدخول (مع نقل الترخيص إلى هذا الجهاز)، والتحقق الدوري، ومتغيرات المستخدم، وتسجيل الخروج، والنسخة التجريبية المجانية، والحسابات، وتفعيل مفتاح على حساب، وتغيير كلمة المرور |
| [`PwfAuthWinFormsClient`](PwfAuthWinFormsClient) | مكتوب بلغة VB.NET على .NET 8. الميزات نفسها في نافذة **"API Explorer"** بتبويبات، إضافةً إلى التحقق الدوري في الخلفية (`StartHeartbeat` + `SessionEnded`) والتحقق من التحديثات وتبويب **Variables** |
| [`PwfAuthCSharpClient`](PwfAuthCSharpClient) | تشغيل سطر الأوامر نفسه بلغة **C# على .NET Framework 4.8.1** |

## ما تفعله الحزمة نيابةً عنك

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

- **التشفير.** تشفّر الحزمة الطلبات التي يجب تشفيرها (تسجيل الدخول، والتحقق الدوري، وتسجيل الخروج، والتحقق من المفتاح، ومحتوى التطبيق)، وترفض أي رد يدّعي النجاح دون تشفير
- **معرّف الجهاز (HWID).** تربط الحزمة التراخيص بهذا الجهاز
- **الساعة الخاطئة.** تعالج الحزمة بنفسها خطأ ساعة الجهاز
- **الإيقاف عن بُعد.** تشغّل الحزمة التحقق الدوري الذي يلتزم بالإيقاف عن بُعد من لوحة التحكم
- **نقل الترخيص.** تنقل الدالة `ResetHardwareIdAsync` الترخيص إلى جهاز جديد، مع مراعاة مدة الانتظار التي حددتها لتطبيقك
- **حسابات مرتبطة بالمفاتيح.** تغطي الدالتان `RegisterAccountWithKeyAsync` و `RedeemKeyAsync` الحسابات التي تعمل بمفاتيح الترخيص
- **متغيرات المستخدم.** تحفظ الدوال `SetVariableAsync` و `GetVariableAsync` و `GetVariablesAsync` و `DeleteVariableAsync` قيماً صغيرة على الخادم للعميل الذي سجّل دخوله: إعدادات وتقدّم وعلامات. تنتقل هذه القيم مع العميل إلى أي جهاز جديد، ويمكنك رؤيتها وتعديلها من لوحة التحكم (**Licences** أو **Users** ← اللوحة الجانبية ← **Variables**)

كانت النسخ السابقة من هذه الأمثلة تحمل عميلاً مكتوباً يدوياً. أما الآن فهي تستخدم الحزمة، لذا تحصل على كل تغيير وإصلاح في الخادم بمجرد تحديث الحزمة

## المتطلبات

- حزمة .NET SDK 8.0 أو أحدث (`dotnet --version`)
- لمشروع `PwfAuthCSharpClient`: حزمة الاستهداف **.NET Framework 4.8.1**. تأتي مع Visual Studio، كما يستعيدها البناء عبر NuGet
- حساب مجاني في PWF Auth وتطبيق. أنشئ واحداً على <https://pwfauth.com>

يستعيد الأمر `dotnet build` الحزمة `PWFAuth` من nuget.org تلقائياً

## الإعداد

اضبط سر التطبيق `DefaultAppSecret` في المثال الذي اخترته قبل التوزيع. لا ترفع السر الحقيقي إلى المستودع أبداً. عنوان الخادم ثابت على `https://pwfauth.com`، وحقول الإعدادات للقراءة فقط، وأي قيمة بديلة في متغيرات البيئة تُتجاهل

## التشغيل

```powershell
# Console — pass the key as an argument, or run with none to be prompted.
# An optional 2nd key (UNUSED — it gets used up) shows redeeming a key onto an account.
dotnet run --project PwfAuthConsoleClient -- PWF-XXXX-XXXX-XXXX [PWF-SPARE-KEY]

# Windows Forms
dotnet run --project PwfAuthWinFormsClient

# C# on .NET Framework 4.8.1
dotnet run --project PwfAuthCSharpClient -- PWF-XXXX-XXXX-XXXX [PWF-SPARE-KEY]
```

## ملاحظة أمنية

يُوزَّع سر التطبيق (App Secret) داخل كل تطبيق عميل، ويمكن استخراجه من الملف التنفيذي، لذا تعامل مع التحقق من الترخيص في جهة العميل على أنه **وسيلة ردع، وليس DRM**. أبقِ السر خارج نظام إدارة الشيفرة المصدرية: اضبط التطبيق أثناء عملية بناء خاصة لا يطّلع عليها غيرك. القيمة الافتراضية في الشيفرة فارغة عمداً

## الرخصة

مرخّص بموجب MIT، راجع [LICENSE](LICENSE)
