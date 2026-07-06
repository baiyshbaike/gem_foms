# Dialysis Proje Inceleme ve Yeniden Yazim Hazirlik Dokumani

Tarih: 2026-07-06

Kapsam: `Dialysis\` klasoru. Bu dokuman, projeyi bastan sona yeniden yazmaya hazirlanacak bir gelistirici veya baska bir yapay zeka icin hazirlandi. Amac, tum projeyi tekrar taramadan genel mimariyi, ana is akisini, korunacak artilari, kritik eksikleri, mantik hatalarini ve cozum onerilerini anlamayi saglamaktir.

Gizli bilgi notu: `Server/appsettings*.json` ve `Server/nlog.config` icinde canliya benzeyen connection string, JWT secret, verification/hash secret ve DB parolasi bulunuyor. Bu dokumanda gizli degerler bilerek kopyalanmadi. Yeniden yazima baslamadan once bu degerler rotate edilmeli ve repodan temizlenmelidir.

## 1. Kisa Yonetici Ozeti

`Dialysis` projesi hemodiyaliz surecini yoneten Blazor WebAssembly + ASP.NET Core API + EF Core/PostgreSQL tabanli bir uygulamadir. Hasta kaydi, medikal kart, hemodiyaliz seansi, merkez/makine/employee yonetimi, kalite incelemeleri, raporlar, Excel/Word ciktilari, QR/Tunduk dogrulama, HTTP/action loglama ve kullanici/rol yonetimi gibi genis bir is kapsamini tasir.

Proje is bilgisi acisindan degerli, fakat teknik borcu yuksek. En kritik durumlar sunlardir:

- Cozum su anda temiz build olmuyor.
- Hedef framework `net7.0`; build uyarisi .NET 7'nin destek disi oldugunu soyluyor.
- NuGet guvenlik uyarilari var: `Npgsql 7.0.4` ve `System.Text.Json 7.0.4` icin high severity vulnerability uyarisi alindi.
- `Server/Domain/AppDbContext.cs:91` satirindaki `OnModelCreating` metodu `override` degil; EF Core model konfigurasyonlari calismiyor olabilir.
- `HDSessionService` ve `MedCardService` icinde seans durum gecislerinde ciddi mantik hatalari var.
- Config dosyalarinda secret ve DB erisim bilgileri repoya girmis.
- HTTP logging middleware request/response body ve header'lari loglayabiliyor; bu saglik verisi ve token sizintisi riskidir.
- API authorization cogunlukla sadece `[Authorize]`; role/policy/ownership kontrolleri zayif.
- Domain entity'leri client ile paylasiliyor; `User.Password` gibi alanlar ayni shared modelde bulunuyor.
- Dosya yukleme/indirme mantigi `wwwroot` altina yaziyor ve dosya/PII guvenligi eksik.
- Test projesi bulunamadi.

Yeniden yazimda oncelik sirasi: guvenlik/secret temizligi, build'i ayaga kaldirma, domain state machine kurallari, veri modeli ayristirma, backend authorization, dosya saklama, testler, sonra UI sadelestirme olmalidir.

## 2. Proje Haritasi

Ana cozum:

- `Dialysis/Dialysis.sln`
- `Dialysis/global.json`: SDK `7.0.410`
- Projeler:
  - `Dialysis.Client`: Blazor WebAssembly UI
  - `Dialysis.Server`: ASP.NET Core backend/API
  - `Dialysis.Shared`: Client ve Server arasinda paylasilan modeller/DTO'lar
  - `Dialysis.MudBlazor`: vendored MudBlazor kaynagi

Ana klasorler:

- `Client/Pages`: UI ekranlari, formlar, rapor/print componentleri
- `Client/Domain`: client servisleri, auth state, helper katmani
- `Server/Controllers`: API endpointleri
- `Server/Domain/Services`: is mantigi servisleri
- `Server/Domain/Migrations`: EF Core migrationlari
- `Server/Doamin/Migrations`: typo klasor, eski/duplicate migration gibi duruyor
- `Server/Data`: DevExpress reporting icin SQLite veri dosyasi
- `Server/wwwroot`: template, upload ve public statik dosyalar
- `Shared/Models`: EF/domain model siniflari
- `Shared/Dto`: DTO siniflari
- `Shared/Params`: pagination/filter parametreleri
- `Shared/Responses`: response wrapper siniflari

Kaba metrikler:

- Kaynak kodu, `bin/obj/publish/.vs/.idea/MudBlazor` dislaninca yaklasik 290 `.cs`, 187 `.razor` dosyasi.
- `Shared` icinde yaklasik 63 model, 30 DTO, 11 param sinifi.
- Client tarafinda 177 Razor dosyasi var.
- En buyuk UI dosyalari cok buyuk: ornegin `McardPrintForm.razor` yaklasik 411 KB.
- En buyuk servisler: `MedCardService` yaklasik 132 KB, `PatientService` yaklasik 84 KB, `MedCenterService` yaklasik 28 KB, `StatusService` yaklasik 25 KB.

## 3. Teknoloji Yigini

Backend:

- ASP.NET Core `net7.0`
- EF Core + Npgsql/PostgreSQL
- JWT Bearer authentication
- BCrypt.Net-Next
- NLog + NLog.Database
- DevExpress Reporting
- ClosedXML/NPOI/ClosedXML.Report
- Hashids/QRCoder

Frontend:

- Blazor WebAssembly `net7.0`
- MudBlazor package `6.1.2`
- DevExpress Blazor/Reporting `22.2.4`
- Blazored.LocalStorage
- SignalR client
- Custom auth state provider

Dikkat:

- Repository icinde tam `MudBlazor` kaynak klasoru var, ancak `Client.csproj` ayni zamanda NuGet `MudBlazor` paketine referans veriyor. Bu build'i karmasiklastiriyor ve solution build'ini kiriyor.
- `Client/Program.cs` icinde `AddApiAuthorization()` kullaniliyor, fakat server tarafinda ASP.NET Identity/IdentityServer yapisi aktif degil; custom JWT kullaniliyor. Bu mimari karisiklik yaratir.

## 4. Build ve Calisma Durumu

Calistirilan komut:

```powershell
dotnet build Dialysis\Dialysis.sln --no-restore
```

Sonuc:

- Build temiz tamamlanmadi.
- `NETSDK1138`: `net7.0` destek disi.
- `NU1903`: `Npgsql 7.0.4` icin high severity vulnerability.
- `NU1903`: `System.Text.Json 7.0.4` icin high severity vulnerability.
- `Dialysis/MudBlazor/MudBlazor.csproj` build sirasinda `dotnet webcompiler ./Styles/MudBlazor.scss -c excubowebcompiler.json` komutu ile kirildi.
- Hata, `excubo.webcompiler 2.7.12` icin gereken `Microsoft.NETCore.App 6.0.0-preview.5.21301.5` runtime'inin kurulu olmamasindan kaynaklandi.
- Client/Server tarafinda cok sayida nullable, unused variable, always true/false ve null dereference uyarisi var.

Yeniden yazim icin sonuc:

- Ilk hedef, mevcut sistemi anlamak icin `MudBlazor` vendored projesini solution build disina almak veya NuGet paketine tamamen gecmek olmali.
- `bin`, `obj`, `publish`, `.vs`, `.idea`, upload dosyalari ve vendored library kaynaklari inceleme/build kapsamindan ayrilmali.
- Desteklenen bir .NET surumune gecis planlanmali. Kurumsal standarda gore .NET 8 LTS veya guncel destekli .NET surumu secilmeli.

## 5. Fonksiyonel Kapsam

Ana is alanlari:

| Alan | Kod kaniti | Aciklama |
| --- | --- | --- |
| Authentication/User | `AccountController`, `UserController`, `LoginService`, `AppStateProvider` | Login, JWT, rol, session, lockout, token version |
| Profile/Role | `ProfileController`, `UserProfile`, `UserProfileRole` | Kullanici profil ve rol iliskileri |
| Patient | `PatientController`, `PatientService`, `Patient`, `PatientHistory` | Hasta kaydi, dosya, protokol, external INN sorgusu |
| MedCard | `MedCardController`, `MedCardService`, `MedCard` ve first-* modeller | Medikal kart, ilk muayene, analizler, epicrisis |
| HD Session | `HDSessionController`, `HDSessionService`, `HDSessionEnum` | Hemodiyaliz seans durumlari ve saat/pause akislari |
| MedCenter | `MedCenterController`, `MedCenterService` | Merkez, makine, employee, center file islemleri |
| References/Status | `StatusController`, `StatusService` | Status, price, diagnosis, indicator, quality exam referanslari |
| Region | `RegionController`, `RegionService` | Region/district listeleri |
| Reporting | `ReportController`, `ReportingController`, `ReportDbContext` | DevExpress report designer/viewer |
| Excel/Word | `ExcellController`, `wwwroot/ExcellTemplates`, `wwwroot/Wordfiles` | Hesap/akt ciktilari |
| Logs | `HttpLogController`, `ActionLogController`, `HttpLoggingMiddleware` | HTTP log, action log, log settings |
| Tunduk/QR | `TundukController`, `VerifyController`, `IdentifyTunduk` | Dogrulama/verification akislari |

Controller endpoint yogunlugu:

- `MedCardController`: yaklasik 66 endpoint
- `PatientController`: yaklasik 40 endpoint
- `StatusController`: yaklasik 28 endpoint
- `MedCenterController`: yaklasik 19 endpoint
- `UserController`: yaklasik 8 endpoint
- Diger controllerlar daha kucuk kapsamli

Bu dagilim, ana domain agirliginin `MedCard`, `Patient`, `HDSession`, `MedCenter`, `Status/Reference` uzerinde oldugunu gosteriyor.

## 6. Ana Veri Modeli

`AppDbContext` DbSet listesine gore ana entity gruplari:

- Kullanici ve yetki: `User`, `Role`, `UserRole`, `UserProfile`, `UserProfileRole`, `UserSession`
- Hasta: `Patient`, `PatientHistory`, `PatientGroup`, `PatientGroupPerson`, `PatientGroupTitle`, `MedCenterPatientFile`
- Medikal kart: `MedCard`, `Analysis`, `AnalysisResult`, `AnalysisResultGroup`, `Epicrisis`, `FirstAnalysis`, `FirstInspection`, `FirstRespiratory`, `FirstCardiovascular`, `FirstConfectionery`, `FirstUrogenital`, `FirstEndocrine`, `FirstNeuro`
- Seans: `HDSession`, `HDSessionHour`, `HDSessionPause`
- Merkez: `MedCenter`, `MedCenterMachine`, `MedCenterUser`, `MedCenterFiles`, `MedCenterEmployee`
- Referans/veri sozlugu: `Status`, `GlobalStatus`, `PatientStatus`, `ActivePrice`, `CodeMKB`, `DialyzerType`, `MedicineType`, `Complaint`, `Indicator*`
- Kalite inceleme: `QualityExam1`, `QualityExam2`, `QualityExam2Patient`, `QualityExam3`, `QualityExam3Row`
- Bolge: `Region`, `District`
- Dosya/log/dogrulama: `SaveFile`, `ProtocolFile`, `IdentifyTunduk`, `HttpLog`, `ActionLog`, `LogSettings`

Kritik not:

- `Server/Domain/AppDbContext.cs:91` icinde `protected void OnModelCreating(ModelBuilder builder)` var. EF Core bunu override olarak gormez. Dogrusu `protected override void OnModelCreating(ModelBuilder builder)` olmalidir.
- Bu nedenle `HasDefaultValue`, `HasIndex`, `HasOne` gibi konfigurasyonlar runtime'da uygulanmiyor olabilir.
- Bu hata, migration uretimi ve runtime behavior acisindan en yuksek oncelikli teknik borctur.

## 7. Korunacak Artilar

Bu proje tamamen atilacak degil; icinde yeniden yazim icin degerli domain bilgisi var:

- Is kapsaminin buyuk bolumu koda yansimis: hasta, seans, medkart, merkez, makine, fiyat, kalite inceleme, rapor, belge.
- `HDSessionEnum` gibi status sozlugu olusturulmus; state machine'e donusturmek icin iyi bir baslangic.
- Yeni sayfalarda pagination/filter parametreleri ve DTO yaklasimi goruluyor.
- Login tarafinda BCrypt'e gecis, MD5 migration, lockout, active session, token version, `jti` ve action log gibi guvenlik iyilestirmeleri baslamis.
- Audit/HTTP loglama ve ayarlanabilir log settings gibi operasyonel ihtiyaclar dusunulmus.
- DevExpress Reporting ve Excel/Word template entegrasyonu gercek is ihtiyacini tasiyor.
- `Shared/Dto`, `Shared/Params`, `Shared/Responses` klasorleri gelecekte API contract ayristirmasi icin bir cekirdek sunuyor.
- UI tarafinda domain ekranlarinin buyuk cogunlugu mevcut; yeniden yazimda ekran envanteri olarak kullanilabilir.

## 8. Kritik Sorunlar ve Cozum Onerileri

| Oncelik | Bulgu | Kanit | Etki | Oneri |
| --- | --- | --- | --- | --- |
| P0 | EF model konfigurasyonu calismiyor olabilir | `Server/Domain/AppDbContext.cs:91` | Default value, index, relation ve session indexleri uygulanmayabilir | `override` ekle, migration diff uret, DB schema ile karsilastir |
| P0 | Cozum build olmuyor | `MudBlazor/MudBlazor.csproj` webcompiler hatasi | CI/CD ve guvenilir test yok | Vendored `MudBlazor`u cikart veya runtime/tooling'i guncelle |
| P0 | Secret'lar repoda | `Server/appsettings*.json`, `Server/nlog.config` | DB/JWT/verification ele gecirilebilir | Secret rotate, git history temizligi, env/secret manager |
| P0 | Saglik verisi ve token log riski | `Server/Domain/Services/HttpLoggingMiddleware.cs:65-100` | Authorization header, body, PII loglanabilir | Body/header redaction, default kapali, allowlist loglama |
| P0 | Backend authorization zayif | Controllerlar cogunlukla sadece `[Authorize]` | Her authenticated user fazla veri gorebilir | Policy/role/ownership kontrolu endpoint bazinda uygulanmali |
| P1 | Seans duplicate kontrolu hatali | `HDSessionService.cs:128` | Aktif seans mantigi yanlis calisir | `&&` veya `!excludedStatuses.Contains(StatusId)` kullan |
| P1 | HDSession iki kez eklenebilir | `HDSessionService.cs:155`, `:157` | Duplicate insert/tracking hatasi | Tek `AddAsync`, transaction, unique invariant |
| P1 | MedCard seans durum gecisleri belirsiz | `MedCardService.cs:686-760`, `1054`, `1258`, `1438` | Hasta/makine/seans akisi tutarsiz | Tek state machine servisi ve DB constraint |
| P1 | `EndSession` adlandirmasi yaniltici | `MedCardService.cs:1438` | Endpoint seansi bitirmiyor, sadece bazi alanlari guncelliyor | Komutlari `CompleteSession`, `UpdateEndVitals` gibi ayir |
| P1 | Raw SQL ve string interpolation | `StatusService.cs:119`, `MedCardService.cs:1017-1022` | SQL injection/format/localization riski | Parametreli SQL veya EF update kullan |
| P1 | DevExpress custom SQL acik | `Program.cs:166`, `ReportingController.cs:52` | Rapor designer SQL erisim riski | Sadece admin, readonly DB user, datasource allowlist |
| P1 | Dosyalar `wwwroot` altina yaziliyor | `PatientService`, `MedCenterService`, `MedCardService` | PII ve dosya path guvenligi riski | Private storage, signed download, MIME/size validation |
| P1 | Domain entity client'a gidiyor | `Shared/Models`, controller returnleri | Password/PII/siklikla fazla alan sizar | API DTO/command/query contract ayristir |
| P2 | Date/time stratejisi yok | cok sayida `DateTime.Now` | Timezone, rapor ve gunluk limit hatalari | UTC storage + local display + clock abstraction |
| P2 | Hata modeli standard degil | `Result.Succeeded=false` + HTTP 200 | Client null/hatali akislar | ProblemDetails, 4xx/5xx, typed errors |
| P2 | Test yok | test dosyasi/projesi bulunamadi | Rewrite riskli olur | Domain unit test + integration test + API contract test |
| P2 | Buyuk god service/componentler | `MedCardService`, `PatientService`, buyuk `.razor` | Bakim ve regresyon riski | Use-case servisleri, component split, query/command ayrimi |
| P2 | ExcellController unreachable code | `ExcellController.cs:171` | `docId == 2` iki kez kontrol ediliyor, 3. shart hic calismaz; muhtemelen `docId == 3` olmali | Shart duzelt veya switch/if-else pattern kullan |
| P2 | BgService N+1 query problemi | `BgService.cs:28-48` | Her seans icin ayri HDSessionHour sorgusu cekiliyor; buyuk veri setinde performans dususu | `Include()` ile eager loading veya tek sorguda join |
| P2 | BgService 60sn interval cok sik | `BgService.cs:94` | Her 60 saniyede tum aktif seanslar sorgulanıyor; gereksiz DB yukü | Interval 5-10 dk'ya cikar veya event-driven yaklasim |
| P2 | BgService 270 dakika hardcode | `BgService.cs:53` | 4.5 saat sabit kodlanmis; konfigurasyon olmali | `IConfiguration` veya `AppSettings` uzerinden ayarlanmali |
| P2 | BgService DateTime.Now kullanimi | `BgService.cs:53` | UTC karsilastirmasi yapilmiyor; timezone hatalari | `DateTime.UtcNow` kullanilmali |
| P2 | AllPatients2/3/4/5 tekerlekli kod | `PatientService.cs`, `PatientController.cs` | 5 ayri metot ayni isiyor; bakim yukü | Tek `GetPatients(PatientFilter)` metodu, Specification pattern |
| P2 | Base64 dosya transferi verimsiz | `PatientService.cs:184` | ~%33 fazla veri transferi; buyuk dosyalarda sorun | Streaming upload, multipart/form-data, chunked upload |
| P2 | ReadAllBytesAsync bellek kullanimi | `PatientController.cs:83` | Tum dosya bellege yukleniyor | `FileStreamResult` ile streaming |
| P2 | Eksik indeksleme | `AppDbContext.cs` | Patient.Inn, HDSession.Inn, MedCard.Inn, HDSession.StatusId indekslenmemis | Arama/filtreleme performansi icin indeks ekle |
| P2 | AddRespiratoty typo | `MedCardService.cs` | Metin adinda yazim hatasi | `AddRespiratory` olarak duzelt |
| P2 | ChekToPay typo | `MedCardService.cs` | Metin adinda yazim hatasi | `CheckToPay` olarak duzelt |
| P2 | Tutarssiz metin adlandirmasi | `PatientController.cs` | `bymedcard`, `activesessions` gibi kucuk harf metotlar | PascalCase standardi uygula |
| P2 | Tutarssiz hata mesajlari | tum servisler | Rusca/Ingilizce karisik hata mesajlari | Tek dil veya i18n resource dosyalari |
| P2 | Console.WriteLine kullanimi | `BgService.cs:86`, `MedCardController.cs:61`, `NavMenu.razor:254` | Production'da ILogger yerine Console | `ILogger<T>` veya `ILoggerManager` kullan |
| P2 | NullReferenceException riskleri | `PatientService.cs:43` | `userprofileid` null olabilir ama `.ProfileId` erisimi var | Null-oncedelik operatoru veya explicit kontrol |
| P2 | CORS fallback localhost:5000 | `Program.cs:227` | Production'da ApplicationUrl bossa localhost'a duser | Production'da zorunlu URL tanimi |
| P2 | Magic numbers | `PatientService.cs:350,656`, `HDSessionService.cs`, `BgService.cs:53` | GroupId=8, ActivePrice=6500, 270dk sabit kodlu | Constants sinifi veya konfigurasyon |
| P2 | Transaction tutarsizligi | `PatientService.cs` | Bazı islemlerde transaction var, bazilarinda yok | Tutarli transaction politikasi |
| P2 | Patient.AddPatient duplicate | `PatientService.cs:608-678` | AddEditMedCenterPatient ile ayni is yapiyor | Tek metot, duplicate kod kaldir |

## 9. Mantiksal Hatalar

### 9.1 EF Core `OnModelCreating` override degil

Dosya: `Server/Domain/AppDbContext.cs:91`

Mevcut durum:

```csharp
protected void OnModelCreating(ModelBuilder builder)
```

Bu metod `DbContext.OnModelCreating`'i override etmiyor. EF Core bu konfigurasyonu calistirmayabilir. Dosyanin devaminda cok sayida `HasDefaultValue(false)`, relation ve index konfigurasyonu var. Bu konfigurasyonlar uygulanmiyorsa database davranisi kodun varsaydigi gibi degildir.

Cozum:

- `protected override void OnModelCreating(ModelBuilder builder)` yap.
- Gerekirse `base.OnModelCreating(builder);` cagir.
- Migration snapshot ve mevcut DB schema karsilastir.
- Bu degisiklikten sonra test DB uzerinde migration diff kontrolu yap.

### 9.2 HDSession duplicate session predicate'i her zaman true

Dosya: `Server/Domain/Services/HDSessionService.cs:128`

Kod mantigi:

```csharp
p.StatusId != Finished || p.StatusId != Payed || p.StatusId != SendToPay
```

Bir status ayni anda uc farkli deger olamayacagi icin bu ifade pratikte her zaman true olur. Ornek: status `Finished` ise `StatusId != Payed` true olur. Bu nedenle aktif/kapali seans ayrimi bozulur.

Cozum:

```csharp
var excluded = new[] {
    (long)HDSessionEnum.Finished,
    (long)HDSessionEnum.Payed,
    (long)HDSessionEnum.SendToPay
};

!excluded.Contains(p.StatusId)
```

veya tum `!=` kosullari `&&` ile baglanmali.

### 9.3 HDSession iki kez Add ediliyor

Dosya: `Server/Domain/Services/HDSessionService.cs:155` ve `:157`

`ImageStart` doluysa once `if` icinde `AddAsync(hdSession)`, sonra `if` disinda tekrar `AddAsync(hdSession)` calisiyor. EF tracking/insert hatasi veya beklenmeyen davranis uretir.

Cozum:

- Sadece bir kez add et.
- Resim kaydetme ve DB insert tek transaction akisi olarak tasarlanmasi gereken iki asamadir.
- Dosya yazimi basarisiz olursa DB insert olmamali, DB insert basarisiz olursa dosya orphan kalmamali.

### 9.4 MedCard AddIdentify sureci status/date acisindan riskli

Dosya: `Server/Domain/Services/MedCardService.cs:686-760`

Bulgu:

- Aktif seans kontrolunde `p.StatusId != Identification || (...)` gibi genis bir ifade var.
- `IdentifyOn` nullable olabilir, fakat `(DateTime)p.IdentifyOn` cast ediliyor.
- `SessionEnd >= DateTime.Today` kullanimi status ve timezone ile birlikte yanlis sonuc verebilir.
- Hasta, medcard, aktif seans ve makine kontrolu transaction icinde degil.

Cozum:

- `HDSession` icin resmi state transition tablosu cikar.
- `CanIdentify`, `CanStart`, `CanFinish`, `CanSendToPay`, `CanPay` gibi domain methodlari yaz.
- Tek aktif seans/patient/machine invariant'i DB unique/partial index ile desteklenmeli.
- Nullable alanlar acikca validate edilmeli.

### 9.5 `EndSession` endpoint'i seansi bitirmiyor

Dosya: `Server/Domain/Services/MedCardService.cs:1438`

Bu metod end vitals/complication/image gibi bilgileri guncelliyor, fakat `SessionEnd`, `StatusId = Finished`, duration veya price hesaplamasini set etmiyor. Gercek kapanis davranisi `SendToEnd` metodunda (`MedCardService.cs:1054`) gibi duruyor.

Etki:

- API ismi ve davranis uyumsuz.
- UI/entegrasyon hangi endpoint'in gercek seans bitirme oldugunu karistirabilir.

Cozum:

- Komutlari ayir:
  - `UpdateSessionEndMeasurements`
  - `CompleteSession`
  - `SendSessionToPayment`
- Her komutun allowed from/to status listesi olmali.

### 9.6 Toplu odeme SQL'i string liste ile uretiliyor

Dosya: `Server/Domain/Services/MedCardService.cs:1017-1022`

`idsString` string olarak SQL icine giriyor. ID'ler modelden long olarak gelse bile guvenlik ve bakim acisindan parametreli liste/EF update daha dogru.

Cozum:

- EF Core destekli surumde `ExecuteUpdateAsync` kullan.
- Alternatif: PostgreSQL array parametresi ile `WHERE "Id" = ANY(@ids)`.
- Guncellenen row sayisi ile istenen ID sayisini karsilastir.

### 9.7 ActivePrice guncellemesi parametresiz raw SQL

Dosya: `Server/Domain/Services/StatusService.cs:119`

`DateTime.Now` string interpolation ile SQL'e yaziliyor. Culture/date format ve injection riskleri olusur. Ayrica eski fiyatlari kapatma ve yeni fiyati ekleme transaction icinde olmali.

Cozum:

- Parametreli SQL veya EF update.
- Tek aktif fiyat garantisi icin partial unique index.
- Eskiyi kapat + yeniyi ekle transaction.

### 9.8 QualityExam Akt2/Akt3 metodlari stub

Dosya: `Server/Domain/Services/StatusService.cs:455-488`

`AddAkt2`, `AddAkt3` success donuyor ama veri kaydetmiyor. `AllAkt2`, `AllAkt3` null donuyor. UI bu endpointleri kullaniyorsa kullanici basarili sandigi halde veri kaybolur.

Cozum:

- Ya implement et.
- Ya endpointleri 501/NotImplemented dondurecek sekilde acikca kapat.
- UI'dan kullanilmiyorsa kaldir.

### 9.9 Nullable cast ve tarih parsing riskleri

Ornek:

- `Server/Controllers/MedCardController.cs:293-298`: nullable `hour` cast ediliyor.
- Bircok controller string tarih parse/substr kullanimi yapiyor.

Cozum:

- Query parametrelerini `DateOnly`, `DateTimeOffset`, nullable typed parametre olarak model bind et.
- Manuel substring parse yerine `TryParseExact` veya model binder kullan.
- Gecersiz tarih 400 donmeli.

### 9.10 ExcellController unreachable code

Dosya: `Server/Controllers/ExcellController.cs:167-179`

```csharp
if(docId == 1)
{
    return File(..."Акт_медицинской_экспертизы_форма11.docx"...)
}
else if (docId == 2)
{
    return File(..."Акт_медицинской_экспертизы_форма11_1.docx"...)
}
else if (docId == 2)  // ← HATA: Bu sart asla calismaz
{
    return File(..."Акт_медицинской_экспертизы_форма11_2.docx"...)
}
```

Sorun: `docId == 2` condition'i iki kez kontrol ediliyor. Ikinci kontrol asla calismayacaktir. Muhtemelen `docId == 3` olmalidir.

Cozum: Uclu condition duzelt veya switch pattern kullan.

### 9.11 BgService N+1 query problemi

Dosya: `Server/BgService.cs:28-48`

```csharp
var allVal = await (from u in dbContext.HDSession ...).ToListAsync();
foreach (var session in allVal)
{
    item.HDSessionHours = await dbContext.HDSessionHour
        .Where(p => p.HDSessionId.Equals(item.HDSession.Id)).ToListAsync();
}
```

Sorun: Ilk sorgu tum seanslari cekiyor, sonra her seans icin ayri ayri saat verileri cekiliyor (N+1 problem). Buyuk veri setinde ciddi performans dususune neden olur.

Cozum: `Include()` ile eager loading veya tek sorguda join kullanarak tum veriler cekilmeli.

### 9.12 BgService 60 saniyelik interval cok sik

Dosya: `BgService.cs:94`

```csharp
await Task.Delay(60000); // Her 60 saniyede bir
```

Sorun: 60 saniyede bir tum aktif seanslar sorgulaniyor. Bu, veritabaninda gereksiz yuk olusturuyor.

Cozum: Interval 5-10 dakikaya cikarilabilir. Veya event-driven yaklasim (SignalR bildirim) kullanilabilir.

### 9.13 BgService 270 dakika hardcode

Dosya: `BgService.cs:53`

```csharp
if ((int)(DateTime.Now - (DateTime)session.HDSession.SessionStart).TotalMinutes > 270)
```

Sorun: 270 dakika (4.5 saat) sabit kodlanmis. Bu deger konfigurasyon olmali.

Cozum: `IConfiguration` veya `AppSettings` uzerinden ayarlanmali.

### 9.14 BgService DateTime.Now kullanimi

Dosya: `BgService.cs:53`

Sorun: `DateTime.Now` kullaniliyor, `DateTime.UtcNow` degil. Timezone hatalarina neden olabilir.

Cozum: UTC storage + local display pattern'i uygulanmali.

### 9.15 AllPatients2/3/4/5 tekerlekli kod

Dosyalar: `PatientService.cs`, `PatientController.cs`

Tekerlekli kod ornegin:
- `AllPatients()` - tum hastalar
- `AllPatients2()` - tarih araligina gore
- `AllPatients3()` - medcenter bazli
- `AllPatients4()` - medcenter + toplam gun
- `AllPatients5()` - medcenter + tarih araligi

Her biri farkli parametreler ve filtreleme mantigina sahip, ancak temelde ayni is yapiyor.

Cozum: Tek bir `GetPatients(PatientFilter filter)` metodu olusturulmali. Filter pattern veya Specification pattern kullanilmali.

### 9.16 Base64 dosya transferi verimsiz

Dosya: `PatientService.cs:184`

```csharp
var fileBytes = Convert.FromBase64String(cleanBase64);
await File.WriteAllBytesAsync(fullPath, fileBytes);
```

Sorun: Buyuk dosyalar base64 olarak API'ye gonderiliyor. Base64, veri boyutunu ~%33 artirir. 100MB'lik dosya 133MB olarak iletilir.

Cozum: Streaming upload kullanilmali. `Multipart/form-data` formu tercih edilmeli. Chunked upload dusunulebilir.

### 9.17 ReadAllBytesAsync bellek kullanimi

Dosya: `PatientController.cs:83-84`

```csharp
var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
var base64String = Convert.ToBase64String(fileBytes);
return Content(base64String);
```

Sorun: Tum dosya bellege yukleniyor ve base64 olarak donduruluyor.

Cozum: `FileResult` ile streaming yapilmali. `FileStreamResult` kullanilmali.

### 9.18 Eksik indeksleme

Dosya: `Server/Domain/AppDbContext.cs`

Cogu tabloda indeks tanimlanmamis. Ozellikle su alanlar indekslenmeli:
- `Patient.Inn` (arama icin)
- `HDSession.Inn` (arama icin)
- `MedCard.Inn` (arama icin)
- `HDSession.StatusId` (filtreleme icin)
- `HDSession.MedCenterId` (filtreleme icin)

### 9.19 Tekerlekli kod: Hasta listesi metodlari

Dosya: `PatientService.cs` ve `PatientController.cs`

- `AllPatients()`
- `AllPatients2()`
- `AllPatients3()`
- `AllPatients4()`
- `AllPatients5()`

Her biri farkli parametreler ve filtreleme mantigina sahip, ancak temelde ayni is yapiyor. Bu, bakim yukunu artiriyor.

Cozum: Tek bir `GetPatients(PatientFilter filter)` metodu olusturulmali. Filter pattern veya Specification pattern kullanilmali.

### 9.20 Tutarssiz hata mesajlari

Ornek:
- `PatientService.cs:139` → `"Transaction Error"` (Ingilizce)
- `PatientService.cs:145` → `"Model not found"` (Ingilizce)
- `UserService.cs:84` → `"Пароль должен содержать минимум 8 символов..."` (Rusca)
- `PatientService.cs:332` → `"Пациент с таким ИНН существует"` (Rusca)

Sorun: Hata mesajlari tutarsiz bir dille yazilmis. Bazilari Rusca, bazilari Ingilizce.

Cozum: Tek bir dil secilmeli. Resource dosyalari ile internationalization (i18n) desteklenmeli. Hata kodlari tanimlanmali.

### 9.21 Console.WriteLine kullanimi

Ornek:
- `BgService.cs:86` → `Console.WriteLine(activeSession.Inn);`
- `MedCardController.cs:61` → `Console.WriteLine(ex);`
- `NavMenu.razor:254,258` → `Console.WriteLine("no only");` / `Console.WriteLine("isonly");`

Sorun: Production'da hata yonetimi icin `Console.WriteLine` kullaniliyor. Bu, loglama altyapisini zayiflatiyor.

Cozum: `ILogger<T>` veya `ILoggerManager` kullanilmali. Yapilandirilmis loglama (structured logging) uygulanmali.

### 9.22 NullReferenceException riskleri

Dosya: `PatientService.cs:42-44`

```csharp
var userprofileid = await _dbContext.User.Where(c=>c.Id == id).FirstOrDefaultAsync();
var profilename = await _dbContext.UserProfile.Where(c => c.Id == userprofileid.ProfileId).FirstOrDefaultAsync();
var name = profilename?.Title?.ToString();
```

Sorun: `userprofileid` null olabilir, ancak `.ProfileId` erisimi NullReferenceException firlatabilir.

Cozum: Explicit null kontrolu veya null-oncedelik operatoru (`?.`) kullanilmali.

### 9.23 CORS fallback localhost:5000

Dosya: `Server/Program.cs:227`

```csharp
.WithOrigins(builder.Configuration["AppConfiguration:ApplicationUrl"] ?? "http://localhost:5000")
```

Sorun: Production'da `ApplicationUrl` bossa `http://localhost:5000` kullanilacak. Bu, guvenlik acigi olusturabilir.

Cozum: Production'da zorunlu URL tanimi yapilmali. Fallback kullanilmamali.

### 9.24 Magic numbers

Ornek:
- `PatientService.cs:350` → `patient.GroupId = 8;` ("Yeni hasta" grubu)
- `PatientService.cs:765` → `if (item.GroupId == 2)` ("Arşiv" grubu)
- `HDSessionService.cs:174` → `if (retData.GroupId == 8)` ("Yeni hasta" kontrolu)
- `BgService.cs:53` → `> 270` (4.5 saat)
- `MedCardService.cs:126` → `model.HdSession.ActivePrice = 6500;`

Sorun: Magic number'lar kodun anlasilmasini zorlastiriyor ve bakimi güçlestiriyor.

Cozum: `PatientGroup` enum'i olusturulmali. `Constants` sinifinda sabitler tanimlanmali. Konfigurasyon dosyasina tasinalmali.

## 10. Guvenlik ve Gizlilik

### 10.1 Secrets

Riskli dosyalar:

- `Server/appsettings.json`
- `Server/appsettings.Development.json`
- `Server/appsettings.Production.json`
- `Server/nlog.config`

Bu dosyalarda canliya benzeyen DB connection string, password, JWT secret, verification/hash secret gorunuyor. Degerler bu dokumana alinmadi.

Cozum:

- Tumu rotate edilmeli.
- Git history'den temizlenmeli.
- Local development icin `dotnet user-secrets` veya local env dosyasi, production icin secret manager/env var kullanilmali.
- `appsettings*.json` icinde sadece placeholder ve config key kalmali.

### 10.2 JWT ayarlari

Dosya: `Server/Program.cs:79-86`

Bulgu:

- `RequireHttpsMetadata = false`
- `ValidateIssuer = false`
- `ValidateAudience = false`

Cozum:

- Production'da HTTPS zorunlu.
- Issuer/audience validate edilmeli.
- Token lifetime ve refresh/revocation stratejisi netlesmeli.
- Existing `UserSession`, `TokenVersion`, `jti` iyi bir temel; policy ile tamamlanmali.

### 10.3 Public MD5 endpoint

Dosya: `Server/Controllers/AccountController.cs:45`

`api/account/getmd5` public utility endpoint olarak duruyor. MD5 eski migration icin kullanilmis olabilir, fakat public endpoint olarak kalmamali.

Cozum:

- Endpoint kaldir.
- MD5 sadece login migration icinde internal legacy check olarak kalabilir.

### 10.4 HTTP body/header loglama

Dosya: `Server/Domain/Services/HttpLoggingMiddleware.cs:65-100`

Request body, request headers, response body ve response headers loglanabiliyor. Bu projede hasta INN, isim, medikal bilgiler, dosya base64'leri, Authorization header ve password gibi hassas veriler bulunabilir.

Cozum:

- Default olarak body log kapali.
- Header allowlist kullan, `Authorization`, `Cookie`, `Set-Cookie` asla loglama.
- Body redaction: password, token, INN, passport, file content, image/base64 alanlari maskele.
- Max body length tek basina yeterli degil.
- Log saklama suresi ve erisim yetkisi net olmali.

### 10.5 Authorization ve ownership

Controllerlarin cogu sadece `[Authorize]` ile korunmus. Role bazli veya policy bazli backend kontrol az gorunuyor. UI menu gizleme guvenlik degildir.

Cozum:

- Backend endpoint bazinda policy:
  - Admin operations
  - MedCenter staff operations
  - FOMS/report operations
  - Read-only report access
- Her patient/session/file download icin medcenter ownership kontrolu.
- Dosya indirme endpointleri ozellikle ownership kontrolu yapmali.

### 10.6 Dosya yukleme/indirme

Riskler:

- Base64 body olarak dosya aliniyor.
- Bazi dosyalar `wwwroot` altina yaziliyor.
- Bazi pathler user/model/file name ile uretiliyor.
- Upload ve download ownership/validation eksik gorunuyor.

Cozum:

- Dosyalari public `wwwroot` disinda sakla.
- DB'de metadata, storage'da random object key kullan.
- Original filename sadece display metadata olsun.
- MIME, extension, magic bytes, size limit ve virus scan ekle.
- Download her zaman authorized endpointten signed/streamed donsun.

### 10.7 Reporting SQL

Dosyalar:

- `Server/Program.cs:166`
- `Server/Controllers/ReportingController.cs:52`

DevExpress custom SQL / SQL data source acik. Rapor designer yetkili olmayan kullaniciya acilirsa veri tabani geneline erisim riski dogar.

Cozum:

- Designer sadece admin/sinirli role acik olmali.
- Rapor data source allowlist.
- Read-only DB user.
- Custom SQL disable veya sadece guvenli stored view/procedure listesi.

## 11. Domain State Machine Onerisi

Mevcut `HDSessionEnum` yeniden yazimda merkez alinmali, fakat string/long status daginikligi yerine tek state machine kurulmalidir.

Onerilen seans durumlari:

- `Draft` veya `Created`
- `Identification`
- `Identified`
- `Started`
- `Paused`
- `EndMeasurementsEntered`
- `Finished`
- `SendToPay`
- `Payed`
- `Cancelled`

Onerilen transitionlar:

| From | Command | To | Ana validasyon |
| --- | --- | --- | --- |
| Created | IdentifyPatient | Identification/Identified | Hasta aktif, medcard var, bugun uygun seans yok |
| Identified | StartSession | Started | Makine aktif, makine bugun limitte degil, tek aktif seans |
| Started | PauseSession | Paused | Aktif pause yok |
| Paused | ResumeSession | Started | Aktif pause var |
| Started | EnterEndMeasurements | EndMeasurementsEntered | End vitals ve complication valid |
| EndMeasurementsEntered | CompleteSession | Finished | Duration, price, SessionEnd set |
| Finished | SendToPayment | SendToPay | Billing kurallari |
| SendToPay | MarkPaid | Payed | Payment/batch kontrolu |

DB invariant onerileri:

- `Patient.Inn` unique olmasi gerekiyorsa unique index.
- Aktif seans icin partial unique index: patient basina aktif statuslarda tek seans.
- Makine basina ayni anda tek aktif seans.
- `ActivePrice` icin `IsDeleted=false` durumda tek aktif fiyat.
- `User.Username` unique.
- `Role.Name` unique.
- Dosya kayitlari icin owner ve object key unique/immutable.

## 12. API Tasarimi Onerisi

Mevcut API'lerde endpoint sayisi fazla, is mantigi controller/servis icinde daginik, response modeli standard degil.

Yeniden yazim icin:

- API domain bazli versionlanmali: `/api/v1/patients`, `/api/v1/med-cards`, `/api/v1/hd-sessions`, `/api/v1/med-centers`, `/api/v1/reports`.
- Entity yerine DTO don:
  - Query DTO: ekranda gereken alanlar
  - Command DTO: create/update icin gereken alanlar
  - Detail DTO: detay sayfalari
- HTTP status dogru kullan:
  - 200/201 success
  - 400 validation
  - 401 unauthenticated
  - 403 forbidden
  - 404 not found
  - 409 conflict/domain invariant
  - 422 domain validation
- Hata modeli `ProblemDetails` veya tek standard error contract olmali.
- Pagination, sorting, filtering ortak bir pattern kullanmali.
- OpenAPI/Swagger contract yeniden yazimda zorunlu olmali.

## 13. Frontend Incelemesi

Pozitif:

- Blazor WASM ile tum domain ekranlari buyuk oranda mevcut.
- MudBlazor ve DevExpress componentleri kullanilmis.
- Local draft/cache ihtiyaclari dusunulmus.
- Rapor ve print ekranlari is surecini dogrudan destekliyor.

Sorunlar:

- Cok buyuk Razor dosyalari var; UI, API call, formatlama, print template ve state ayni dosyada karismis.
- `Client/Domain/Account/AppStateProvider.cs:78-80`: `givenname` claim'i `ClaimTypes.NameIdentifier` olarak ekleniyor; bu claim parsing bugidir.
- Token `localStorage` icinde tutuluyor; Blazor WASM icin yaygin ama XSS riskine karsi CSP/input sanitization gerekir.
- `Client/Program.cs` icinde `AddApiAuthorization()` ile custom JWT birlikte duruyor; gereksiz/kafa karistirici.
- `Client/Shared/NavMenu.razor` icinde stray attribute `d` ve hardcoded versiyon bilgisi goruldu.
- `Client/Domain/Extensions/RsultExtensions.cs` hata durumunda exception swallow edip null donduruyor; client tarafinda sessiz hata ve NRE uretir.

Ek sorunlar:

- `MudBlazor/MudBlazor.csproj` iceriginde vendored kaynak kodu kullaniliyor; bu build surecini kiriyor ve guncelleme yukunu artiriyor.
- `Client/Program.cs:14-15` icinde `CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("ru-RU")` hardcode edilmis; lokalizasyon icin esneklik yok.
- `Client/Dialysis.Client.csproj:11` icinde `NoWarn` ile cok sayida uyari bastirilmis: `CS8618, CS8603, CA1416, CS8602, CS8601, CS8625, CS1998, CS8604, CS0618`. Bu uyari seviyesini dusuruyor ve potansiyel sorunlari gormezden geliyor.
- `Client/Shared/NavMenu.razor:233` icinde hardcoded versiyon numarasi `<p>2.5.6</p>` bulunuyor; bu bilgi konfigurasyondan gelmeli.
- `Client/Shared/MainLayout.razor:98` icinde `CurrentUserId = "1"` hardcoded; bu dinamik olmali.

Yeniden yazim frontend onerisi:

- Domain ekranlarini route/page + feature component + API client + view model olarak ayir.
- Form state, validation ve save command'larini componentten ayir.
- Print/report componentlerini normal ekran componentlerinden ayir.
- Merkezi API client:
  - Auth token ekleme
  - Error handling
  - ProblemDetails mapping
  - 401/403 redirect
- UI'da rol gizleme kalsin ama backend policy asil kaynak olsun.

## 14. Veri, Migration ve Repository Temizligi

Bulgu:

- `Server/Domain/Migrations` ve typo `Server/Doamin/Migrations` birlikte var.
- `bin`, `obj`, `publish`, `.vs`, `.idea` klasorleri repository/workspace icinde mevcut.
- `Server/wwwroot/UploadFiles`, `UploadMachine`, `UploadImages`, Word/Excel template ve potansiyel runtime dosyalari repo icinde.
- `obj` icindeki dosyalarda eski pathler (`D:\Projects\hemodialysis`) goruluyor.

Cozum:

- `.gitignore` kesinlestir:
  - `bin/`
  - `obj/`
  - `.vs/`
  - `.idea/`
  - `publish/`
  - runtime upload klasorleri
  - local DB/report cache dosyalari
- Migration klasoru teklesmeli.
- Typo `Doamin` temizlenmeli.
- Runtime upload ve hassas ornek dosyalar repodan ayrilmali.
- Template dosyalari gercekten urun asset'i ise `Templates/` gibi kontrollu klasorde tutulmali; runtime output ile karismamali.

## 15. Yeniden Yazim Icin Onerilen Mimari

Backend katmanlari:

- `Dialysis.Api`: controllers, auth, middleware, OpenAPI
- `Dialysis.Application`: use-case command/query handlers, validation, authorization services
- `Dialysis.Domain`: aggregate, value object, domain services, state machine
- `Dialysis.Infrastructure`: EF Core, file storage, external clients, reporting adapters
- `Dialysis.Contracts`: API DTO/request/response contractleri

Frontend katmanlari:

- `Dialysis.Web`: Blazor app
- `Features/Patients`, `Features/Sessions`, `Features/MedCards`, `Features/MedCenters`, `Features/Reports`
- `Shared/ApiClient`, `Shared/Auth`, `Shared/Components`

Domain odaklari:

- Patient aggregate
- MedCard aggregate
- HDSession aggregate
- MedCenter/Machine aggregate
- Billing/Payment aggregate
- Reference data
- Audit/logging
- File/document storage

Persistence:

- EF Core PostgreSQL devam edebilir.
- Domain entity ve API DTO ayrilmali.
- `DateTimeOffset`/UTC stratejisi belirlenmeli.
- Soft delete ortak interface/filter ile uygulanmali.
- DB migrationlari CI'da test edilmeli.

Integration:

- Tunduk/INN external sorgulari typed HttpClient ile config tabanli olmali.
- Retry/timeout/circuit breaker eklenmeli.
- Hard-coded IP/URL koddan cikmali.

## 16. Test Stratejisi

Minimum rewrite testleri:

- HDSession state machine unit testleri:
  - duplicate active session engellenir
  - finished/payed/send-to-pay statuslar aktif sayilmaz
  - start/finish/pause/resume valid transitionlari
  - invalid transition 409/422 doner
- MedCard/Patient invariant testleri:
  - pasif hasta seans baslatamaz
  - medcard yoksa session akisi durur
  - makine disabled ise session baslatilmaz
- Authorization integration testleri:
  - medcenter A kullanicisi medcenter B dosyasini indiremez
  - admin-only endpointleri normal kullanici cagiramaz
- File storage testleri:
  - extension/MIME/size validation
  - path traversal engellenir
  - unauthorized download 403
- API contract testleri:
  - validation 400
  - not found 404
  - conflict 409
- Migration testleri:
  - fresh DB migrate
  - seeded reference data

Mevcut projede gorulen test projesi olmadigi icin, rewrite'in ilk gununden itibaren test altyapisi kurulmalidir.

## 17. Oncelikli Is Listesi

### Hemen, mevcut proje ustunde

1. Secret'lari rotate et ve repodan temizle.
2. `AppDbContext.OnModelCreating` icin `override` hatasini duzelt, migration/schema etkisini kontrol et.
3. `MudBlazor` vendored proje build sorununu cozumle veya solution'dan cikar.
4. `Npgsql`, `System.Text.Json`, .NET target ve diger paket guvenlik guncellemelerini planla.
5. HTTP logging redaction ekle veya production'da body/header loglamayi kapat.
6. Public `getmd5` endpointini kaldir.
7. Critical session logic buglarini fixle:
   - `HDSessionService.cs:128`
   - `HDSessionService.cs:155-157`
   - `MedCardService` state transitionlari

### Rewrite baslamadan once

1. Ekran envanteri cikar: hangi ekran aktif, hangisi legacy?
2. Endpoint envanteri cikar: kullaniliyor/kullanilmiyor ayrimi.
3. DB schema ve production data invariantlari dokumante et.
4. `HDSessionEnum` ve status table gercek anlamlarini is ekibiyle dogrula.
5. Rapor/template listesini is kritikligine gore siniflandir.
6. PII ve dosya saklama politikasi belirle.
7. Role/policy matrisi hazirla.

### Rewrite sirasinda

1. Domain state machine once yazilsin.
2. API contractler ve authorization policy'ler sabitlensin.
3. Patient/MedCard/HDSession modulleri onceliklendirilsin.
4. Existing UI ekranlari referans alinip feature feature yeniden kurulsun.
5. Raporlar en son degil, domain verisi stabilize olduktan sonra paralel tasinsin.
6. Eski sistemden veri migrasyonu icin script ve dry-run raporlari hazirlansin.

## 18. Baska Bir AI Icin Devam Notlari

Bu dokumandan devam edecek AI icin kisa yonlendirme:

- Tum projeyi bastan taramak yerine once bu dosyadaki P0/P1 maddelerini kontrol et.
- Ilk bakilacak dosyalar:
  - `Dialysis/Server/Domain/AppDbContext.cs`
  - `Dialysis/Server/Domain/Services/HDSessionService.cs`
  - `Dialysis/Server/Domain/Services/MedCardService.cs`
  - `Dialysis/Server/Domain/Services/PatientService.cs`
  - `Dialysis/Server/Domain/Services/StatusService.cs`
  - `Dialysis/Server/Domain/Services/LoginService.cs`
  - `Dialysis/Server/Domain/Services/HttpLoggingMiddleware.cs`
  - `Dialysis/Server/Program.cs`
  - `Dialysis/Client/Domain/Account/AppStateProvider.cs`
  - `Dialysis/Client/Program.cs`
- `bin`, `obj`, `publish`, `.vs`, `.idea`, `MudBlazor` vendored source ve upload klasorlerini ilk incelemede disla.
- Secret degerlerini dokumana veya loga kopyalama.
- Build'i tekrar denersen once `MudBlazor` vendored proje sorununu dikkate al.
- Mantik incelemesinde ana odak `HDSession` state transitionlari olsun.
- API guvenligi incelenirken UI menu/rol kontrolunu yeterli sayma; backend policy aransin.
- Dosya islemleri incelenirken `wwwroot` altina yazilan her dosya potansiyel public exposure kabul edilmeli.

## 19. Acik Sorular

Rewrite'e baslamadan is ekibine sorulmasi gerekenler:

1. `HDSessionEnum` statuslarinin resmi is anlamlari nelerdir?
2. Bir hasta ayni gun kac seans alabilir? Istisnalar var mi?
3. Bir makine ayni gun kac seans alabilir? Kapasite statusa gore mi saate gore mi hesaplanir?
4. `Identification`, `EndIdentification`, `Finished`, `SendToPay`, `Payed` arasindaki tam is akisi nedir?
5. Hangi roller hangi merkez/hasta/seans verisini gorebilir?
6. Medcenter kullanicisi baska merkezin hastasini veya dosyasini gorebilir mi?
7. Rapor designer kimlere acik olmali?
8. Upload edilen hasta/employee/medical dosyalari ne kadar sure saklanmali?
9. Tunduk/INN external servisleri icin SLA, timeout ve fallback beklentisi nedir?
10. Mevcut production DB'deki en kritik tablolar ve veri hacimleri nedir?
11. Hangi eski ekranlar aktif kullaniliyor, hangileri legacy?
12. Fiyat/odeme surecinde resmi hesaplama kurallari nelerdir?

## 20. Sonuc

`Dialysis` is bilgisi cok yuksek ama teknik olarak riskli bir uygulama. Yeniden yazim mantikli; ancak once mevcut domain kurallari ve guvenlik riskleri netlestirilmezse yeni sistem eski hatalari daha temiz kodla tekrar uretebilir.

Bu dokumanin pratik kullanimi:

- P0/P1 sorunlari guvenlik ve domain dogrulama icin ilk sprint backlog'u olsun.
- `HDSession` state machine rewrite'in cekirdegi olsun.
- Entity/DTO ayrimi, authorization ve file storage yeni mimarinin temel kural seti olsun.
- Eski proje ekran ve rapor envanteri olarak kullanilsin, mimari ornek olarak kopyalanmasin.

## 21. Ek Bulgular (AI Analizi)

Bu bolum, mevcut inceleme tamamlandiktan sonra AI tarafindan yapilan ek analiz ile tespit edilen ek bulgulari icermektedir. Mevcut bulgularin hicbiri silinmemis, sadece ek olarak asagidaki maddeler contributes edilmistir.

### 21.1 Mantiksal Hatalar (Ek)

| # | Bulgu | Kanit | Etki |
| --- | --- | --- | --- |
| 1 | ExcellController unreachable code | `ExcellController.cs:171` | `docId == 2` iki kez kontrol, 3. shart calismaz |
| 2 | BgService N+1 query | `BgService.cs:28-48` | Her seans icin ayri sorgu, performans dususu |
| 3 | BgService 60sn interval | `BgService.cs:94` | Gereksiz DB yukü |
| 4 | BgService 270dk hardcode | `BgService.cs:53` | Konfigurasyon eksik |
| 5 | BgService DateTime.Now | `BgService.cs:53` | Timezone riski |
| 6 | AllPatients2/3/4/5 tekerlekli kod | `PatientService.cs` | 5 metot ayni is |
| 7 | Base64 verimsiz | `PatientService.cs:184` | ~%33 fazla veri |
| 8 | ReadAllBytesAsync | `PatientController.cs:83` | Bellek kullanimi |
| 9 | Eksik indeksleme | `AppDbContext.cs` | Arama performansi |
| 10 | AddRespiratoty typo | `MedCardService.cs` | Yanlis metin adi |
| 11 | ChekToPay typo | `MedCardService.cs` | Yanlis metin adi |
| 12 | Tutarssiz metin adlandirmasi | `PatientController.cs` | PascalCase eksik |
| 13 | Tutarssiz hata mesajlari | tum servisler | Rusca/Ingilizce karisik |
| 14 | Console.WriteLine | `BgService.cs:86` | Production'da loglama |
| 15 | NullReferenceException risk | `PatientService.cs:43` | Null check eksik |
| 16 | CORS fallback | `Program.cs:227` | localhost:5000 risk |
| 17 | Magic numbers | `PatientService.cs` | GroupId=8, ActivePrice=6500 |
| 18 | Transaction tutarsizligi | `PatientService.cs` | Tutarli degil |
| 19 | Patient.AddPatient duplicate | `PatientService.cs:608-678` | Ayni is iki metot |

### 21.2 Frontend Bulgulari (Ek)

| # | Bulgu | Kanit | Etki |
| --- | --- | --- | --- |
| 1 | MudBlazor vendored source build sorunu | `MudBlazor/MudBlazor.csproj` | Build kirilmasi |
| 2 | Culture hardcode | `Client/Program.cs:14-15` | Lokalizasyon eksik |
| 3 | NoWarn ile uyari bastirma | `Client/Dialysis.Client.csproj:11` | 9 uyari bastirilmis |
| 4 | Hardcoded versiyon | `Client/Shared/NavMenu.razor:233` | Versiyon sabit |
| 5 | Hardcoded CurrentUserId | `Client/Shared/MainLayout.razor:98` | Dinamik olmali |

### 21.3 Kod Kalitesi Bulgulari (Ek)

| # | Bulgu | Kanit | Etki |
| --- | --- | --- | --- |
| 1 | MedCardService God Class | `MedCardService.cs` (~2387 satir) | 50+ metot, bakim zor |
| 2 | Date parsing tekerlekli kod | 6+ dosya | Ayni kod tekrar ediyor |
| 3 | Doamin/ typo klasor | `Server/Doamin/` | Yanlis yazim |
| 4 | Tekerlekli kod: AllPatients | `PatientService.cs` | 5 benzer metot |
| 5 | Magic numbers | `PatientService.cs:350,656` | GroupId=8, ActivePrice=6500 |
