# gem.foms — Kapsamlı Mimari ve Güvenlik Raporu

**Tarih:** 17 Temmuz 2026  
**Kapsam:** Backend (.NET 10) + Frontend (Vue 3) — tam sistem incelemesi  
**Yöntem:** Statik kod analizi (kalıp düzeyinde), dosya düzeyinde okuma  
**Not:** Bu rapor yalnızca bir denetim raporudur — herhangi bir dosya değiştirilmemiştir.

---

## İçindekiler

1. [Yönetici Özeti](#1-yönetici-özeti)
2. [Sistem Mimarisi](#2-sistem-mimarisi)
3. [Kritik Güvenlik Bulguları](#3-kritik-güvenlik-bulguları)
4. [Backend Detaylı İnceleme](#4-backend-detaylı-i̇nceleme)
5. [Frontend Detaylı İnceleme](#5-frontend-detaylı-i̇nceleme)
6. [İş Mantığı ve Doğruluk](#6-i̇ş-mantığı-ve-doğruluk)
7. [Performans](#7-performans)
8. [Gözlemlenebilirlik ve Hata Takibi](#8-gözlemlenebilirlik-ve-hata-takibi)
9. [Test Kapsamı](#9-test-kapsamı)
10. [Bakım ve Kod Kalitesi](#10-bakım-ve-kod-kalitesi)
11. [Sonuç ve Önceliklendirme](#11-sonuç-ve-önceliklendirme)

---

## 1. Yönetici Özeti

### Genel Değerlendirme

| Alan | Not | Yorum |
|------|-----|-------|
| **Güvenlik** | **D−** | Kritik bir kimlik doğrulama atlatma hatası ve düz metin parolalar/kayıtlı gizli anahtarlar mevcut |
| **Mimari Tasarım** | **B** | Temiz katmanlı mimari — iyi Domain/Application/Infrastructure ayrımı, ama taban sınıf patlaması var |
| **İş Mantığı** | **C+** | Durum makinesi doğru, ama ölçüm değerleri dize olarak saklanıyor ve kilitleme eksik |
| **Performans** | **C** | Her yetkilendirilmiş istekte veritabanı çağrısı, yinelenen denetim günlüğü yazmaları, önbellek yok |
| **Gözlemlenebilirlik** | **F** | Serilog paketleri atanmış ama yapılandırılmamış; monitör yok; ilişki ID'si yok |
| **Test Kapsamı** | **F** | Birim testleri kısmen dolu; entegrasyon testleri boş; CI'da test zorunluluğu yok |
| **Frontend Güvenlik** | **C** | Yenileme belirteci JS'ten okunabilir alanda; CSP yok; açık yönlendirme kısmen engellenmiş |
| **Frontend Bakımı** | **C** | Şablon kalıntıları (marketing/billing/ai-talk) kullanılmıyor; i18n büyük ölçüde kullanılmıyor |

### En Kritik 5 Bulgu (Düzeltme Gerektiren)

| # | Bulgu | Etki | Yer |
|---|-------|------|-----|
| 1 | **Auth atlatma** — yanlış parola girişinde `return null` yok; kod başarılı giriş akışına düşer | Herkes herhangi bir şifreyle oturum açabilir | `backend/src/Infrastructure/Auth/AuthService.cs` |
| 2 | **Gizli anahtarlar repoya kaydedilmiş** — `.env`, `appsettings.Development.json` içinde düz metin parola/jwt | Geçmiş sızıntısı kalıcı | `gem.foms/.env`, `Api/appsettings.Development.json` |
| 3 | **Patient tenant izolasyonu yok** — `Patient` entity'si `ITenantEntity` uygulamıyor | Tüm tenant'lar tüm hastaları görür | `Domain/Patients/Patient.cs` |
| 4 | **Yenileme belirteci sessionStorage'da açık metin** — XSS → tam hesap ele geçirme | Her XSS tek Noktalı Saldırı | `frontend/src/services/auth-session.ts` |
| 5 | ** Permissions.cs içinde fazladan boşluk** — `Permissions.All` içinde `"SessionRead "` (boşluklu) → denetleyici ile veritabanı arasında ayrışma | Doktor rolü seans işlemlerini yapamaz | `backend/src/Application/Authorization/Permissions.cs` |

---

## 2. Sistem Mimarisi

### 2.1 Genel Yapı

```
gem.foms/
├── backend/                         # .NET 10 çözümü
│   ├── Api/                         # ASP.NET Core Web API
│   ├── Application/                 # İş mantığı arayüzleri
│   ├── Domain/                      # Etki alanı varlıkları
│   ├── Infrastructure/              # EF Core + Servis uygulamaları
│   ├── Contracts/                   # DTO'lar
│   ├── tests/UnitTests/             # Birim testleri (kısmen dolu)
│   ├── tests/IntegrationTests/      # Boş (yalnızca yer tutucu)
│   ├── Backend.slnx                 # Yeni format çözüm dosyası
│   ├── Dockerfile                   # Çok aşamalı yapı
│   ├── docker-compose.yml           # PostgreSQL + API
│   ├── global.json                  # .NET 10 SDK
│   └── .env                         # ← KRİTİK: Düz metin gizli anahtarlar
│
├── Frontend/                        # Vue 3 SPA (shadcn-vue-admin çatallanması)
│   ├── src/
│   │   ├── services/                # HTTP katmanı + API modülleri
│   │   ├── stores/                  # Pinia mağazaları
│   │   ├── router/                  # Vue Router + guards
│   │   ├── pages/                   # Sayfa bileşenleri
│   │   ├── composables/             # Vue composable'ları
│   │   ├── components/ui/            # shadcn-vue primitifleri
│   │   ├── components/server-data-grid/  # Özel veri tablosu
│   │   ├── lib/                     # Yardımcı kod
│   │   ├── plugins/                 # i18n, Pinia, dayjs, NProgress
│   │   └── types/                   # TS tip bildirimleri
│   ├── vite.config.ts
│   ├── package.json
│   └── .env.development
│
├── Dialysis/                        # Eski proje (inceleme amaçlı korundu)
├── DIALYSIS_PROJECT_REVIEW.md       # Eski proje denetim raporu
└── ARCHITECTURE.md                  # Mimari öneri dokümanı
```

### 2.2 Backend Katmanlı Mimari

```
Domain ────────────► (harici bağımlılık yok, saf)
   ▲
Contracts ─────────► (DTO'lar, bağımlılık yok, saf)
   ▲
Application ───────► Domain + Contracts (arayüz tanımları)
   ▲
Infrastructure ────► Application + Domain (EF Core, servis uygulamaları)
   ▲
Api ───────────────► Application + Infrastructure + Contracts (denetleyiciler)
```

**Architectural koku:** `Application.csproj` `Microsoft.AspNetCore.Identity 2.3.11` paketini referans alıyor — parola hash'leme iş mantığı katmanında değil altyapıda olmalı. Bu, Temiz Mimari prensibini ihlal eder.

### 2.3 Teknoloji Yığını

| Katman | Teknoloji | Sürüm | Not |
|--------|-----------|-------|-----|
| Backend | .NET | 10.0.103 | En son SDK |
| Backend | EF Core | 10.0.9 | Npgsql 10.0.2 |
| Backend | JWT Bearer | 10.0.9 | Doğrulayıcı yapılandırılmış |
| Backend | Serilog | 10.0.0 | Atanmış ama yapılandırılmamış |
| Frontend | Vue | 3.5 | Composition API |
| Frontend | TypeScript | 6.0 | Katı mod |
| Frontend | Vite | 8.0 | Hızlı dev/build |
| Frontend | Pinia | 3.0 | Durum yönetimi |
| Frontend | Axios | - | Token interceptor'larla HTTP istemcisi |
| Frontend | shadcn-vue | - | 60+ UI bileşeni |
| Frontend | Tailwind CSS | 4.0 | Stil çözümü |

---

## 3. Kritik Güvenlik Bulguları

### 3.1 KİMLİK DOĞRULAMA ATLATMA — CRITICAL

**Dosya:** `Infrastructure/Auth/AuthService.cs`

Yanlış parola doğrulaması sonrası kod akışı:

```csharp
if (passwordResult == PasswordVerificationResult.Failed)
{
    user.FailedLoginCount++;
    await _auditLogService.AddAsync(...);
    await _db.SaveChangesAsync(cancellationToken);
}   // ← return null; YOK! Kod normal şekilde devam eder
    
// Başarılı giriş akışı yürütülür:
var permissions = await LoadPermissionsAsync(user.Id, ...);
// JWT verilir...
user.FailedLoginCount = 0;   // ← sayaç sıfırlanır
```

**Etki:** Herhangi bir kullanıcı, bilinen bir kullanıcı adı için **herhangi bir parola** deneyerek geçerli bir JWT alabilir. Bu, kimlik doğrulamanın tamamen baypas edilmesidir.

**Düzeltme:** `SaveChangesAsync` çağrısından sonra `return null;` eklenmeli.

---

### 3.2 GİZLİ ANAHTARLARIN KAYDEDİLMESİ — CRITICAL

**Dosya:** `.env` (kök dizin, repo içerisine işlenmiş olası)

```
POSTGRES_PASSWORD=123qwe
JWT_SECRET=change-me-development-secret-at-least-32-characters
SEED_ADMIN_PASSWORD=menadmin
```

**Dosya:** `Api/appsettings.Development.json` — bağlantı dizesi düz metin parola içerir.

**Etki:** Tarih boyunca işlenmiş gizli anahtarlar kalıcıdır. `git log -p` tüm parolaları gösterir.

**Düzeltme:** Tüm gizli anahtarları `dotnet user-secrets`'a taşı, `.env` dosyasının takip edilmediğini `git ls-files` ile doğrula, işlenmiş parolaları döndür.

---

### 3.3 HASTA TENANT İZOLASYONU YOK — CRITICAL

**Dosya:** `Domain/Patients/Patient.cs`

`Patient` entity'si `ActiveSoftDeletableAuditableEntityBase`'den miras alır ancak `ITenantEntity` uygulamaz. `TenantId` alanı yok. `PatientService` tenant filtresi uygulamaz.

**Etki:** Tüm tenant'lar (MedCenter'lar) aynı hasta havuzunu görür. MedCenter A'nın kullanıcısı MedCenter B'nin hastalarını görebilir/düzenleyebilir.

**Düzeltme:** `Patient`'ı `TenantActiveSoftDeletableAuditableEntityBase`'den türet, `AppDbContext`'e genel sorgu filtresi ekle.

---

### 3.4 YENİLEME BELİRTECİ sessionStorage'DA — HIGH

**Dosya:** `Frontend/src/services/auth-session.ts:20`

```ts
sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session))
// session = { accessToken, refreshToken, user, permissions, activeTenant }
```

**Etki:** XSS saldırısı → `sessionStorage.getItem('dialysis.auth')` ile hem erişim hem yenileme belirteci alınır. Token çalınması durumunda HTTPOnly çözümü gerekir.

**Düzeltme:** Yenileme belirteci `HttpOnly; Secure; SameSite=Strict` çerezine taşı. Erişim belirteci yalnızca bellekte.

---

### 3.5 İZİN SİSTEMİNDE YAZIM HATASI — HIGH

**Dosya:** `Application/Authorization/Permissions.cs`

`Permissions.All` dizisinde bazı sabitlerin sonunda fazladan boşluk var:

```csharp
public static readonly string[] All = new[] {
    "patient.read",
    // ...
    "SessionRead ",   // ← BOŞLUK VAR!
    "SessionCreate ", // ← BOŞLUK VAR!
    // ...
};
```

**Etki:** `Permissions.All` üzerinden veritabanına eklenen değer `"session.read "` (boşluklu), ancak denetleyiciler `Permissions.SessionRead` (boşluksuz) kullanır. İki ayrı izin kaydı oluşur. Doktor rolüne `session.read` atanmaya çalışılınca bulunamaz.

**Düzeltme:** Fazladan boşlukları temizle, `StringComparer.OrdinalIgnoreCase` ile dizi tanımla.

---

### 3.6 DAHA FAZLA GÜVENLİK BULGULARı

| Önem | Bulgu | Dosya |
|------|-------|-------|
| High | Serilog paketleri atanmış ama `Program.cs`'te yapılandırılmamış `src/Api/Program.cs` |
| High | Genel istisna işleyici yok — işlenmemiş hatalar 500 döner, hiçbir ilişki kimliği yok `Program.cs` |
| High | İki kiracı değiştirme çağrısı eşzamanlıursa curl'ler yarışır `Frontend/src/services/http.ts:50-83` |
| Medium | `ManagementRegionsController` izin admin.tenants ile aynı, admin yönetim bölgelerini kullanamaz |
| Medium | Admin parola minimum 6 karakter `[MinLength(6)]` → endüstri standardı 8+ |
| Medium | Hesap kilitleme yok `FailedLoginCount++` ama `LockoutEndAt` asla ayarlanmıyor |
| Medium | Çift depolama: hem `.env` hem `appsettings.Development.json` içinde düz metin gizli anahtarlar |
| Medium | `HttpContext.TraceIdentifier` ilişki için kullanılıyor ama istemciye gösterilmiyor |
| Medium | Sağlık kontrolü yalnızca DB'yi kontrol eder, RabbitMQ/Redis'i kontrol etmez |
| Low | Docker konteyner kök kullanıcı olarak yürütülür — güvenlik açığı |
| Low | PostgreSQL TLS yapılandırılmamış (bağlantı şifrelenmemiş olabilir) |
| Low | Brute-force engelleme: `FailedLoginCount` artırılır ama hiçbir eylem yapılmaz |
| Info | MHI hizmet URL'si şifrelenmemiş HTTP: `http://192.168.0.49:8000/api/v1/mhi/` |
| Info | IRI/IP doğrulama: MHI çağrısında kimlik doğrulama/yetkilendirme yok |

---

## 4. Backend Detaylı İnceleme

### 4.1 Domain Taban Sınıfı Patlaması

`Domain/Common/` altında 8 adet taban sınıf üretilmiş:

```
EntityBase
├── AuditableEntityBase
│   ├── ActiveAuditableEntityBase
│   ├── SoftDeletableAuditableEntityBase
│   │   └── ActiveSoftDeletableAuditableEntityBase
│   └── TenantAuditableEntityBase
│       ├── TenantSoftDeletableAuditableEntityBase
│       │   └── TenantActiveSoftDeletableAuditableEntityBase
```

**Sorun:** Her özellik kombinasyonu için yeni sınıf. Bu, Kartez ürün büyümesidir. **Önerilen yaklaşım:** Arayüzler + tek bir `AuditableEntity` taban sınıfı + EF Core interceptor'ları.

### 4.2 İki Aynı İsimli `Region` Sınıfı

- `Domain.Tenants.Region` — yönetim bölgesi, string PK
- `Domain.Regions.Region` — coğrafi bölge, long PK

`Tenant.cs` ve `ManagerRegionAccess.cs` her ikisini de kullanır ve tam ad-belirtme yapar. **Kafa karıştırıcı.** Biri yeniden adlandırılmalı (ör. `ManagementRegion` vs `GeoRegion`).

### 4.3 AppDbContext — 635 Satır, Tek Dosya

- **24 DbSet**, tüm yapılandırma `OnModelCreating` içinde satır içi.
- `IEntityTypeConfiguration<T>` sınıfları kullanılmıyor — tek sorumluluk ihlali.
- **Genel sorgu filtresi yok** (`HasQueryFilter`). Tenant filtreleme her servis metodu içinde manuel.
- `ApplyAuditFields()` geçersiz kılma, `IRequestContext.UserId`'den yararlanır ama `null` olduğunda `SystemUserId = 0` kullanır.
- `SaveChangesAsync'de TenantId` otomatik atanmıyor — servis kodu manuel atar.
- Tüm FK'lar `DeleteBehavior.Restrict` — basamaklı silme yok. Bu soft-delete ile uyuşuyor ama `MedCardClinicalInfo` gibi alt varlıklar soft-delete edilmediğinden yetim kayıtlar oluşur.

### 4.4 Migration Geçmişi

14 migration (9 Temmuz → 16 Temmuz 2026, 7 gün). Bazıları birbirini iptal ediyor:
- `AddTenantTimeZoneId` → sonra `RemoveTenantLocalizationFields` ile geri alındı.

**Öneri:** Etiketlemeden önce squashing migration'lar uygulanmalı.

### 4.5 Denetleyici Yapısı ve Koku

12 denetleyici mevcut. Hepsi `[Authorize]` ile korunmuş. Admin için `[Authorize(Policy = "Permission:" + Permissions.AdminAudit)]` kullanılır.

**Koku:** Her denetleyici kendi `CurrentUserId()` yardımcı metodunu tekrarlar — DRY ihlali. Temel denetleyici sınıfına taşı.

**Koku:** `[ProducesResponseType]` attribute'ları yok — Swagger açık 4xx cevap gövdeleri göstermez.

**Koku:** Hata yanıtları tutarsız:
- `TenantsController` özel `ConflictProblem`, `BadRequestProblem` yardımcıları tanımlar.
- `PatientsController`, `AdminUsersController` vb. yalnızca `BadRequest()` döner, gövdesiz.

### 4.6 Arka Plan İşçi (Background Worker)

`HdSessionWorkflowWorker` her 60 saniyede çalışır. 4 süpürme yapıp tek `SaveChangesAsync` ile kaydeder. Yaklaşım doğru.

**Sorun 1:** Tüm seansları tenant sınırı olmadan `Where(x => x.Status == ...)` ile yükler — sayfa yok. Büyük sistemde bellek sorunu.

**Sorun 2:** Dağıtılmış kilitleme yok. İki API replikası varsa her ikisi de satırları yükler ve değiştirir — son işlem kazanır, çift denetim günlüğü.

**Sorun 3:** Jitter yok — her iki replika aynı anda çalışır → büyük gürültü kıralımı.

**Sorun 4:** İşçi hata işleme: `catch (Exception ex)` ve `ILogger.Log.LogError` ile işlemeye devam eder. İyi.

### 4.7 Kimlik Doğrulama Servisi Detayları

Zaten §3.1'de ele alındı.

Ek Sorunlar:
- `LoginAsync` izlenen varlık yükler (`AsNoTracking()` yok) — hafif performans kaybı.
- `LoadPermissionsAsync` her giriş/yenile/me çağrısında DB'ye gider — önbellek yok. Kısa TTL'li `IMemoryCache` ekle.
- `RefreshAsync` yarış koşulu: İki eşzamanlı çağrı aynı belirteci bulur, ikisi de yeni 结果 üretir. Atomik `UPDATE ... WHERE RevokedAt IS NULL` ile cozulmeli.
- Kullanıcı girişinde `FailedLoginCount'ı sıfırlar` ama `LockoutEndAt'ı` asla ayarlamaz → Hesap kilitleme atacagi cansız alan (dead field).

### 4.8 Tenant Erişim Servisi — Veritabanı Yapay İştablosu

Her `ActiveTenantPolicy` kontrolü, `CanAccessTenantAsync` çağırır — bu tüm erişilebilir kiracı kimliklerini yükler. Birden çok LINQ çapraz birleşimi yapar ve `.Contains` ile kontrol eder.

**Etki:** Her yetkilendirilmiş istekte birden çok DB çağrısı. Kısa istek ömrü için `IMemoryCache` ile önbelleğe alın.

---

## 5. Frontend Detaylı İnceleme

### 5.1 Çift Oturum Kaynağı (Enbuyuk Mimari Koku)

Oturum üç yerde saklanır:

1. `sessionStorage['dialysis.auth']` — gerçek yazma konumu
2. Pinia mağazası `useAuthStore` — başlangıçta `sessionStorage'dan` yüklenir
3. Axios interceptor — her istekte doğrudan `sessionStorage'dan` okur

**Sorun:** Yenileme yapıldığında interceptor `sessionStorage'ı` günceller ama Pinia mağazasını **güncellemez**. `dialysis-auth-changed` olayı dispatch edilir ama **hiçbir abonesi yok**. Pinia refs eskir.

**Etki:** Yenilemeden sonra `authStore.accessToken` eski. `auth-guard.ts` `authStore.isLogin'ı` okur — `isLogin` true kalır ama asıl belirteç yanıltıcı olabilir.

### 5.2 Çift Form Doğrulama Çerçevesi

- `@tanstack/vue-form` + `zod` — hasta editöründe (iyi)
- `vee-validate` + `@vee-validate/zod` — shadcn-vue form bileşenlerinde
- Native HTML5 doğrulama — ayarlar sayfasında

**Karar:** Tek form çerçevesi seçilmeli. Öneri: `@tanstack/vue-form` + `zod`.

### 5.3 Şablon Kalıntıları

`shadcn-vue-admin` iskeletinden kalan kullanılmayan 50+ KB kaynak:

| Klasör | Durum |
|--------|-------|
| `pages/marketing/**` | Pazarlama açılış sayfaları |
| `pages/billing/**` | Faturalama sayfaları |
| `pages/apps/**` | Uygulama başlatıcı ızgarası |
| `pages/ai-talk/**` | Sahte AI sohbeti (`setTimeout` ile) |
| `pages/prop-components/**` | Bileşen demoları |
| `components/inspira-ui/**` | Görsel efektler |
| `components/custom-theme/**` | Tema seçici |
| `components/marketing/**` | Hero/özellikler bölümleri |
| `components/app-sidebar/nav-team-vercel.vue` | Kullanılmıyor |
| `composables/use-sidebar-navigation.ts` | 198 satır kod, mevuct kenar çubuğu tarafından kullanılmıyor |
| `services/api/example-system-config.api.ts` | Alayıcı mock veri |
| `services/api/example-tasks.api.ts` | Örnek kod |
| `pages/auth/components/github-button.vue`, `google-button.vue` | İşlevsiz düğmeler |

**Öneri:** Hepsi silinmeli. `pnpm build'ı` birkaç dosya çiftiyle doğrula.

### 5.4 Dil Yerelleştirme Çelişkisi

- `plugins/i18n/index.ts` — yalnızca `en` ve `zh` yerel ayarları destekler
- `plugins/dayjs/setup.ts` — dayjs `zh-cn`'e zorlanır
- `lib/dialysis.ts:8-9` — `APP_LOCALE = 'ru-RU'`, `APP_TIME_ZONE = 'Asia/Bishkek'`
- Ürün sayfaları hardcoded İngilizce metin kullanır (`'Patient deleted'`, `'Settings saved'`)

**Etki:** Ürün Kırgız/Rus klinik kullanıcılarına İngilizce sunuluyor. i18n yardımı yanıltıcı — altyapı var ama kullanılmıyor.

**Karar:** `ru` yerel ayarı ekle VEYA i18n tamamen kaldır.

### 5.5 CSP Hazırlığı Yetersiz

- `index.html'`da CSP başlığı yok
- `vercel.json'`da güvenlik başlıkları (`headers`) bloğu yok — şu an yalnızca SPA yeniden yazma kuralı var
- `components/ui/chart/utils.ts:40-41` `div.innerHTML = serializedKey` — mevcut CSP riski
- NProgress eğe `<style>` ile yüklenir — katı `'self' CSP'yi` zorlar

**Etki:** Üretime alındığında hiçbir güvenlik başlığı gönderilmez.

### 5.6 Vite Yapılandırması Minimal

`vite.config.ts` 68 satır:
- `build` bloğu **yok** — `manualChunks`, sıkıştırma, hedef yok
- `visualizer` her yapıda çalışır (suv analizi) — `--mode analyze` ile sınırlandırılmalı
- Dev sunucu proxy yok — CORS'u backend'te yapılandırır

### 5.7 İstek Yarış ve İptal Yok

`use-server-data-grid.ts:152-182` `requestVersion'ı` eski yanıtları bırakmak için kullanır (iyi) ama `AbortController` yok. Eski HTTP istekleri bant genişliği ve DB slot tüketmeye devam eder. Axios `AbortSignal'ı` yerel olarak destekler.

### 5.8 Erişilebilirlik

Özel `server-data-grid.vue`:
- Sütun yeniden sıralama `dragstart/drop` ile — klavye alternatifi yok
- Sütun yeniden boyutlandırma tutamaç `aria-label'ı` yok
- Grup satırı açma düğmesi `aria-expanded'ı` yansıtmıyor
- `aria-busy'ı` veri yükleme sırasında ayarlamaz

### 5.9 Çift Tarih Kitaplığı

- `@internationalized/date` (reka-ui takvimden)
- `dayjs` (kendi eklentisi ile, ürün sayfalarında kullanılmıyor)
- `Intl.DateTimeFormat` (asıl `formatDateTime` kullanır)

**Öneri:** Tek tarih kitaplığı seç.

---

## 6. İş Mantığı ve Doğruluk

### 6.1 HD Seans Durum Makinesi

```
┌────────────────┐     ┌──────────┐    ┌────────┐    ┌──────────┐    ┌───────────────┐    ┌──────────┐
│  Identified    │────►│  Started │───►│ Paused │───►│ Finished │───►│ EndIdentified │───►│ SentToPay│
└────────────────┘     └──────────┘    └────────┘    └──────────┘    └───────────────┘    └──────────┘
        │                  │                                              │                    │
        ▼                  ▼                                              ▼                    ▼
┌─────────────────┐  ┌───────────┐                              ┌──────────────────┐    ┌──────────┐
│IdentificationExpired│ │Finished (Auto) │                        │ EndIdentificationOverdue│  │ SendToPayOverdue│
└─────────────────┘  └───────────┘                              └──────────────────┘    └──────────┘
                                                                                          │
                                                                                          ▼
                                                                                    ┌──────────┐
                                                                                    │  Paid    │
                                                                                    └──────────┘
```

`SessionTransitionService.CanMove(from, to, hasOverride)` — statik geçiş tablosu. Korumalı yapı (`internal`), iyi tasarlanmış.

**İyi:** Makine için kısmi-benzersiz indeks `UX_HdSessions_ActiveMachine` DB düzeyinde aynı makinede iki seans başlatılmasını engeller. Ancak hasta için öyle bir indeks **yok** — aynı hasta için eşzamanlı Başlat committed-beat-committed ile çift seansCreated olabilir.

### 6.2 Ölçüm Değerleri Dize Olarak

`HdSessionMeasurement`'a ait `Sys`, `Dia`, `Temp`, `Ritm` hepsi `string?` (max 20).

**Sorun:** Sayısal vital değerler dizede saklanır. Sayısal aralık sorguları, toplamalar ve eşik kontrolü imkansız.

**Düzeltme:** `mixed?` (double?) ve `int?` kullan.

### 6.3 Önbellek ve Soğuma Süreleri

`MedCenterMachine`'da `BetweenSessionCooldownMinutes` ve `DailyLimitCooldownMinutes` tanımlı ama `HdSessionService'`te hiçbir yerde kontrol edilmez. Sadece `DailySessionLimit` kontrol edilir. Kullanılmayan alanlar.

### 6.4 SEVec ДОPARAK ИSİM `Other'ı` şüpheli

`PatientGroup'ı` seed verisinde `"Other"` adı `"foms"` kodu ile tutarsız. `Other` adı kodla uyumsuz.

---

## 7. Performans

### 7.1 Her İstekte DB Çağrısı

Tenant erişim kontrolü her yetkili istekte birden çok sorgu çalıştırır. Önbellek yok.

| İşlem | DB Sorgu Sayısı |
|-----------|---------|
| Giriş | ~3 (kullanıcı, roller/izinler, AuditLog kaydet) |
| Yenileme | ~3 (belirteç, yeni belirteç, AuditLog) |
| `/me` | ~2 (kullanıcı + izinleri) |
| Her İstek (Authorization) | ~1-3 (TenantAccessService.CanAccessTenantAsync) |
| Her GET (denetim) | ~1 (AuditLog yaz) |

Getirme başına sayfa yükleme maliyeti birçok çağrıdır.

### 7.2 Her GET Ucum Denetim Yazımı

Her başarılı/başarısız GET isteği için bir `ActionLog` ekler ve `SaveChangesAsync` çağırır. Bu, her GET'i bir yazma API'sine dönüştürür. Denetim tablosu indeksleri her sorgudan sonra bozulur.

**Çözüm:** Denetim günlükleri için ayrıtaşla DbContext + ayrı işlem (transaction) (yazılan batch + arka plan flush).

### 7.3 Çift SaveChanges

`AuthService.LoginAsync`, `PatientService.CreateAsync` ve birkaç yerde yalnızca denetim için ek `SaveChangesAsync` çağrılır. Yalnızca tek `SaveChangesAsync` çağrıp hem entity hem denetim satırını aynı işlemde işlemek yeterli.

### 7.4 Önbellek Yok

Hiçbir veri önbelleğe alınmaz:
- İzinler her giriş/`/me`'de yüklenir
- Kiracı erişim listesi her yetki kontrolünde yüklenir
- Coğrafi bölgeler/bölgeler her açılan menüde yüklenir
- `PatientGroup'lar` her ızgara açılışında yüklenir

**Öneri:** `IMemoryCache` veya `IDistributedCache` (Redis) ile referans verisi için önbellek.

### 7.5 Frontend Paket Büyüklüğü

- `exceljs` dinamik içe aktarılır — iyi.
- `@unovis/ts` + `@unovis/vue'yu` şüpheyle denetle — muhtemelen yalnızca kontrol panelinde kullanılır.
- `motion-v`, `embla-carousel-vue`, `vaul-vue`, `@formkit/auto-animate` — tmpl kalıntısı ile gelir (tree-shaking'e bağlı).
- `universal-cookie'yi` tek bir kenar çubuğu çerezi için kullanma — kullanın.

---

## 8. Gözlemlenebilirlik ve Hata Takibi

### 8.1 Backend Logging

- `Serilog.AspNetCore` 10.0.0 ve `Serilog.Sinks.Console` 6.1.1 `Api.csproj'ya` eklenir, ancak `Program.cs'ya` `UseSerilog` veya `ReadFrom.Configuration` çağrısı yok.
- **Serilog paketleri atılır.** Gerçek günlük kaydı `ILogger<T>` (ASP.NET Core varsayılanı) ile yapılır.
- `HdSessionWorkflowWorker'ı` `ILogger.LogError'ı` kullanır — doğru.
- Servisler `ILogger'ı` kullanmaz — hatalar yalnızca `AuditLog kayıtlarında` `Succeeded=false` ile gösterilir.
- Yapılandırılmış günlük kapsamı yok (`CorrelationId` scope) — günlük satırları denetim satırlayla ilişkilendirilemez.

### 8.2 Frontend Gözlemlenebilirlik

- `package.json'ı` Sentry/LogRocket/PostHOG içermiyor.
- `App.vue'ı` `app.config.errorHandler` ayarlamaz — işlenmemiş bileşen hataları yalnızca `console.error'ya` gider.
- `http.ts` eksiklik başarısız olduğunda `clearStoredAuthSession` + `Promise.reject` — hata kaybolur.
- İstemci tarafı `CorrelationId` yok — Backend hatası ile kullanıcı tarafı hata arasında bağlantı yok.

### 8.3 İlişki Kimliği Yayılımı

Backend `HttpContext.TraceIdentifier'ı` `IRequestContext.CorrelationId'ya` kopyalar ve `AuditLog'a` yazar. Ama **istemciye geri gönderilmez** — kullanıcı, hatayı "Sunucu günlüğUne bakın" diye çözmeye çalışır. Öneri: `X-Request-Id` yanıt başlığı olarak döndür.

---

## 9. Test Kapsamı

### 9.1 Backend

| Proje | Durum |
|--------|-------|
| `UnitTests` | 4 dosya: `TenantServiceTests`, `TenantAuthorizationTests`, `RegionalSettingsTests`, `ManagementRegionServiceTests` — kısmen dolu |
| `IntegrationTests` | Yalnızca yer tutucu `UnitTest1.cs` ile boş `public void Test1() {}` — **0 gerçek test** |

**Sorunlar:**
- `Microsoft.EntityFrameworkCore.InMemory` kullanılır — Npgsql'a özgü kısıtlar (`HasCheckConstraint`, kısmi benzersiz indeks) sessizce düşürülür — sonuçlar yanıltıcı.
- CI'ya test adımı zorunlu kılınmamış — `simple-git-hooks` yalnızca `pnpm build` çalıştırır.
- Hasta CRUD için test yok.
- AuthService için test yok (auth bypass yakalanmaz).
- Kontrolcüler için test yok.

### 9.2 Frontend

Test edilen dosyalar (TOPlam ~8 dosya):
- `lib/dialysis.test.ts` — tarih formatı
- `services/tenant-selection.test.ts` — 3 durum
- `composables/use-authorized-navigation.test.ts`
- `utils/__tests__/env.test.ts` — 157 satır
- `pages/patients/patient-grid.test.ts`, `patient-identity.test.ts` — sağlıklı
- `pages/tenants/tenant-grid.test.ts`
- `pages/management-regions/management-region-grid.test.ts`
- `components/server-data-grid/server-data-grid.test.ts` — 80 satır

**Test edilmeyen kritik dosyalar:**
- `services/http.ts` — interceptor / yenileme yarışı
- `services/auth-session.ts` — depolama olayları
- `stores/auth.ts` — pinia mağazası
- `composables/use-server-data-grid.ts` — 308 satır çekirdek ızgara orkestratörü
- `router/guard/auth-guard.ts` — 401/403 yönlendirmeleri

---

## 10. Bakım ve Kod Kalitesi

### 10.1 Adlandırma Tutarsızlığı

- `gem.foms'ı` (repo) vs `Dialysis'ı` (kod/docker/edebiyatşar) — geçmiş kalıntı.
- `Authorization` (Application vs `Auth'ı` Api/Infrastructure) — küçük tutarsuzluk.
- `PatientGroup'ı` seed adı `"Other"` kod `"foms"` — ürün tarafıyla uyumlu değil.
- `HdSession'ın` `SessionWorkflowSettings'ı` tek kiracı bazlı bir singleton — ama `SessionWorkflowSettings'ı` (değiştirmez) sadece `GetAsync(tenantId)` ile çekilir.

### 10.2 Çift Store: Önceden Hepsi (idx 0)

Önceleri son bulguların hepsi gecelebildi. Hala önemli bazı ek bulgular:

- `AppDbContext.cs:20'ı` — `IRequestContext'ı` opsiyonel kurucu parametresi — handleChange'den kaynaklı, worker'da zorluk çıkartır.
- `DependencyInjection.cs:33'ı` — `conncetionsString` (fazladan c) yazım hatası.
- `ActionLog'ı` modelleme — `MetadataJson'ı` dizesi, ama ham dize interpolated (`HdSessionService'ı`) vs `JsonSerializer.Serialize'ı` (`TenantService'ı`) — tutarsız.
- `Patient'ı` entity'si `INN'ı` için DB düzeyinde CHECK kısıtlaması iyi (ekte hem uzunluk hem rakam kontrolü). savunma derinliği iyi.
- `HdSessions'ı` için kısmi-benzersiz indeks `UX_HdSessions_ActiveMachine'ı` — PostgreSQL özelliğini doğru kullanır. Makine çifte rezervasyonu önler. mükemmel.

### 10.3 Analiz / Static Analizci Yok

- Roslyn analyzer yok (FxCop, SonarQube, AsyncFixer).
- `TreatWarningsAsErrors'ı` veya `enableNETAnalyzers'ı` yok.
- `.editorconfig'ı` yok.
- Backend lint sıkılaştırması yok.

Frontend:
- `@antfu/eslint-config'ı` (eslint 10) atanır.
- `src/components/ui/**'yi` lint dışında bırakır — (shadcn scaffoldüretü bunları tutar) ama takım elle değiştirmiş olmalı — denetlenmesi gerekir.
- Dışlanan fazla `any` kullanımı `use-sidebar-navigation.ts:31,68'ı` — lint geçer ama olası.

### 10.4 API ProblemDetails Çoğaltmaılı

`ApiProblemDetails'ı` arayüzü 6+ dosyada kopyalanır:
- `pages/patients/index.vue:41-45`
- `patient-editor-popup.vue:29-33`
- `tenants/index.vue:165`
- `management-regions/index.vue`
- `tenants/components/tenant-editor-popup.vue`
- `management-regions/components/management-region-editor-popup.vue`

Tek bir `response.type.ts'ya` taşınmalı.

### 10.5 Çift API Strateji

- Reel alan (`dialysis.api.ts`) — ham axios + `unwrapData`.
- Eski/Örnek API'lar (`example-system-config.api.ts`, `example-tasks.api.ts`) — TanStack Vue Query.
- `use-system-config.ts'ı` sadece eski sayfalarda kullanılır — reel üründe ölü kod yolu.

**Karar:** TanStack Query gerçek alan için benimsenmeli VEYA örnek dosyaların tümü silinmeli.

---

## 11. Sonuç ve Önceliklendirme

### P0 — Düzeltme Engeli (Bu Sprint)

| # | Eylem | Dosya |
|---|-------|-------|
| 1 | Auth bypass'ı düzelt: yanlış parola dalına `return null;` ekle | `Infrastructure/Auth/AuthService.cs` |
| 2 | `Permissions.All` içindeki fazladan boşlukları temizle | `Application/Authorization/Permissions.cs` |
| 3 | `Patient'ı` `TenantActiveSoftDeletableAuditableEntityBase'ı` türet, genel sorgu filtresi ekle | `Domain/Patients/Patient.cs`, `Infrastructure/Data/AppDbContext.cs` |
| 4 | Repo'ya işlenmiş gizli anahtarları kaldır, user-secrets / env vars kullan, mevcut parolaları döndür | `.env`, `appsettings.Development.json` |
| 5 | Frontend: Yenileme belirtecini `HttpOnly` çerezine taşı, `withCredentials: true` | `Frontend/src/services/auth-session.ts`, `http.ts` |
| 6 | Frontend: Pinia mağazasını `dialysis-auth-changed'ı` olayına abone yap | `Frontend/src/stores/auth.ts` |
| 7 | Frontend: `vercel.json'ı` veya `_headers'ı` içine güvenlik başlıkları ekle (CSP, HSTS, X-Content-Type-Options) | `Frontend/vercel.json` |
| 8 | Frontend: `EnvSchema.safeParse'ı` başarısız olduğunda uygulama ölümcül hata verir | `Frontend/src/utils/env.ts` |
| 9 | `simple-git-hooks.pre-push: 'pnpm build && pnpm test'` | `Frontend/package.json` |

### P1 — Sonraki 30 Gün

| # | Eylem | Dosya |
|---|-------|-------|
| 10 | Genel istisna işleyici ara yazılımı ekle, `ProblemDetails'ı` özelleştir | `Backend/src/Api/Program.cs` |
| 11 | `IMemoryCache'ı` ile izin/kiracı erişimi önbelleğe al | `TenantAccessService.cs`, `AuthService.cs` |
| 12 | Denetim günlüklerini arka plan batch + ayrı DB işlemi olarak yaz | `ActionLogService.cs` |
| 13 | Serilog'u yapılandır veya kaldır | `Api/Program.cs`, `Api.csproj` |
| 14 | `X-Request-Id'ı` yanıt başlığı olarak döndür | `Program.cs` |
| 15 | `Application.csproj'ı` `Microsoft.AspNetCore.Identity'ı` bağımlılığını kaldır, parola hash'leme'yi Infrastructure'a taşı | `Application.csproj` |
| 16 | Taban sınıf patlaması yerine komposizyon | `Domain/Common/` |
| 17 | İki `Region'ı` sınıfını yeniden adlandır | `Domain.Tenants.Region`, `Domain.Regions.Region` |
| 18 | Frontend: Şablon kalıntısı dizinleri sil | `pages/marketing/**`, `billing/**`, `apps/**`, `ai-talk/**` vb. |
| 19 | Frontend: Tek form framework seç (`@tanstack/vue-form`) | `package.json`, sayfalar |
| 20 | Frontend: i18n için `ru` ekle VEYA tamamen kaldır | `plugins/i18n/` |
| 21 | Frontend: `AbortSignal'ı` `useServerDataGrid.load'ı` içine yönlendir | `use-server-data-grid.ts` |

### P2 — Kalite İşleri

| # | Eylem | Dosya |
|---|-------|-------|
| 22 | `IEntityTypeConfiguration<T>'ı` ile `AppDbContext.OnModelCreating'ı` böl | `AppDbContext.cs` |
| 23 | `HdSessionMeasurement'ı` vital değerleri string → sayısal tipler | `Domain/Sessions/HdSessionMeasurement.cs` |
| 24 | `HdSessionWorkflowWorker'ı` jitter + dağıtılmış kilit ekle | `HdSessionWorkflowWorker.cs` |
| 25 | Yenileme belirteci yeniden oynatma için atomik `UPDATE` ekle | `AuthService.cs` |
| 26 | `FailedLoginCount'ı` eşik aşılırsa `LockoutEndAt'ı` ayarla | `AuthService.cs` |
| 27 | `Application` katmanı + `Denetleyiciler` için entegrasyon testleri ekle | `tests/IntegrationTests/` |
| 28 | Roslyn analyzer'ları ekle (`FxCopAnalyzers`, `AsyncFixer`) | `Api.csproj` |
| 29 | Frontend: Sentry/GlitchTip entegrasyonu ekle | `App.vue`, `package.json` |
| 30 | Frontend: `server-data-grid.vue'ı` erişilebilirlik düzeltmeleri | `server-data-grid.vue` |
| 31 | Frontend: `manualChunks'ı` ile paket bölme | `vite.config.ts` |
| 32 | Frontend: `http.ts`, `auth-session.ts`, `stores/auth.ts` için birim testleri | yeni test dosyaları |
| 33 | `ActionLogService'ı` Çift `SaveChanges'ı` düzenini tek çağrıya birleştir | Tüm servisler |

---

### Kapanış Görüşü

Backend, domain katmanı için sağlam bir temiz mimari üzerine inşa edilmiş — base sınıf patlamasından bağımsız olarak, katman ayrımı net ve iyi yerleştirilmiş. DbContext, in-lıne yapılandırma ile büyük ama iyi yazılmış. HD Session durum makinesi makul kapsamlı ve makine için DB düzeyinde kısıtlama kullanır — övgüye değer.

En önemli sorunlar **güvenlikte** yoğunlaşmış: Auth bypass, gizli anahtarların repo'ya işlenmesi ve Patient tenant izolasyonu yokluğu. Bunların ilki tek bir eksik `return` ve düzeltme çok kolay — ama keşfedilmezse, kimlik doğrulamanın tamamen baypas edilmesi olur.

Frontend, ciddi iş mantığı iyi bir şekilde parçalanmış: API katmanı, şema doğrulama (zod), özel veri ızgarası birimi testleriyle birlikte — vergü kolay bir ürün düzeyinde. Ama güvenlik altyapısı (auth-session, http interceptor, vite config, CSP) yeterince sertleştirilmemiş ve Pinia mağazası ile sessionStorage arasında çift doğruluk kaynağı açık bir hata sınıfı.

`shadcn-vue-admin'ı` iskeletinden geçen onlarca kullanılmayan dosyaların kaldırılması, bakım yükünü ve yeni başlayanlar için karmaşıklığı büyük ölçüde azaltır.

İlk düzeltme所需的en kritik öğe: AuthService içindeki eksik `return null;` — bu yalnızca 1 satır eklemedir ama tüm güvenlik katmanını devredışı bırakır. Onun dışında, tüm P0 düzeltmeleri tamamlanıncaya kadar üretime ALINMAMALIDIR.

---

*Rapor sonu. Hiçbir dosya değiştirilmedi.*
