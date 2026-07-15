# Dialysis Yeniden Yazım Mimari Dokümanı

**Tarih:** 06 Temmuz 2026  
**Kapsam:** `Dialysis` projesinin yeniden yazımı  
**Hedef:** MedCenter bazlı tenant izolasyonu güçlü, permission sistemi net, seans state machine'i güvenilir, test edilebilir ve kademeli büyüyebilen uygulama.

Bu doküman bir "hemen microservice'e geçelim" planı değildir. Eski proje incelendiğinde en büyük riskin dağıtım ölçeği değil; domain kurallarının dağınık olması, authorization/ownership eksikleri, secret/log güvenliği, dosya saklama ve test yokluğu olduğu görüldü. Bu yüzden ilk mimari **modular monolith** olmalıdır. Microservice, RabbitMQ ve Kubernetes ileride gerekirse çıkarılacak hedef fazlardır.

---

## İçindekiler

1. [Karar Özeti](#1-karar-özeti)
2. [Teknoloji Seçimi](#2-teknoloji-seçimi)
3. [Mimari Stil](#3-mimari-stil)
4. [Multi-Tenant Tasarım](#4-multi-tenant-tasarım)
5. [Authorization ve Ownership](#5-authorization-ve-ownership)
6. [Veri İzolasyonu](#6-veri-izolasyonu)
7. [Domain Modülleri](#7-domain-modülleri)
8. [HDSession State Machine](#8-hdsession-state-machine)
9. [API Tasarımı](#9-api-tasarımı)
10. [Dosya ve Raporlama](#10-dosya-ve-raporlama)
11. [Frontend Mimari](#11-frontend-mimari)
12. [Güvenlik](#12-güvenlik)
13. [Observability ve Background Jobs](#13-observability-ve-background-jobs)
14. [Proje Yapısı](#14-proje-yapısı)
15. [Veritabanı ve Migration Stratejisi](#15-veritabanı-ve-migration-stratejisi)
16. [Test Stratejisi](#16-test-stratejisi)
17. [CI/CD ve Infrastructure](#17-cicd-ve-infrastructure)
18. [Microservice'e Geçiş Kriterleri](#18-microservicee-geçiş-kriterleri)
19. [Uygulama Yol Haritası](#19-uygulama-yol-haritası)
20. [Açık Sorular](#20-açık-sorular)

---

## 1. Karar Özeti

| Karar | Sonuç |
| --- | --- |
| İlk mimari | Modular monolith |
| Runtime | .NET 10 LTS |
| Frontend | Vue 3 + TypeScript + Vite |
| Veritabanı | PostgreSQL, shared DB/shared schema |
| Tenant izolasyonu | `TenantId` + EF filter + PostgreSQL RLS |
| Auth | ASP.NET Core Identity/JWT veya OIDC ihtiyacına göre OpenIddict/Duende |
| Authorization | Permission + tenant membership + resource ownership |
| State machine | `HDSession` domain çekirdeği |
| Dosya saklama | Private storage, `wwwroot` yasak |
| Event yaklaşımı | Domain events + outbox, event sourcing değil |
| Messaging | Faz 1'de yok; gerekirse outbox üzerinden RabbitMQ |
| Kubernetes | Faz 1'de yok; Docker Compose ve tek API deploy yeterli |

Neden böyle?

- Eski projede asıl sorun ölçek değil, iş kurallarının güvenilir olmaması.
- Microservices, domain sınırları netleşmeden başlarsa debugging, transaction ve deployment maliyeti artar.
- Tenant, authorization ve state machine doğru kurulursa sistem daha sonra parçalara ayrılabilir.
- .NET 10 LTS, yeni rewrite için .NET 9 STS'ye göre daha uzun destek penceresi sağlar. Microsoft lifecycle dokümanına göre .NET 10 desteği Kasım 2028'e kadar devam eder; .NET 9 Kasım 2026'da sona erer.
- Kaynaklar: [Microsoft .NET lifecycle](https://learn.microsoft.com/en-us/lifecycle/products/microsoft-net-and-net-core), [.NET releases and support](https://learn.microsoft.com/en-us/dotnet/core/releases-and-support).

---

## 2. Teknoloji Seçimi

### 2.1 Backend

| Teknoloji | Sürüm / Yaklaşım | Amaç |
| --- | --- | --- |
| .NET | 10 LTS | Uzun ömürlü runtime |
| ASP.NET Core | 10.x | Web API |
| EF Core | 10.x | ORM ve migration |
| PostgreSQL | 16+ | Ana veritabanı |
| Npgsql | Güncel stable | PostgreSQL driver |
| FluentValidation | Güncel stable | Request/domain validation |
| MediatR | Opsiyonel | Command/query ayrımı |
| Serilog | Güncel stable | Structured logging |
| OpenTelemetry | Güncel stable | Trace/metrics altyapısı |
| Hangfire veya Quartz | Faz 2 | Background job |
| Redis | Faz 2 | Cache, distributed lock, rate limit |

Notlar:

- AutoMapper zorunlu değil. Kritik domainlerde açık mapping tercih edilir.
- MediatR kullanılabilir, fakat her küçük işlem için zorunlu abstraction haline getirilmemelidir.
- Finbuckle.MultiTenant kullanılabilir; ancak RLS, EF filter ve tenant context ile çakışmamalıdır. Basit bir `ITenantContext` yeterliyse fazladan paket eklenmemelidir.

### 2.2 Authentication

Seçim iki ihtiyaca göre yapılmalıdır:

| İhtiyaç | Öneri |
| --- | --- |
| Sadece bu uygulama login olacak | ASP.NET Core Identity + JWT/refresh token veya BFF cookie |
| Harici sistemler/OIDC federation gerekiyor | OpenIddict veya lisans kabul ediliyorsa Duende IdentityServer |

Duende IdentityServer güçlüdür ama lisans/maliyet kararı gerektirir. Bu karar net değilse mimari buna kilitlenmemelidir.

### 2.3 Frontend

| Teknoloji | Sürüm / Yaklaşım | Amaç |
| --- | --- | --- |
| Vue | 3.x | SPA UI |
| TypeScript | 5.x | Type safety |
| Vite | Güncel stable | Build/dev server |
| Vue Router | 4.x | Routing |
| Pinia | 3.x | UI state |
| TanStack Query veya benzeri | Opsiyonel | Server state/cache |
| Vue I18n | Güncel stable | Rusça/Kırgızca/Türkçe metinler |
| Tailwind CSS + component kit | Kontrollü | UI üretkenliği |
| Vitest + Playwright | Güncel stable | Unit/e2e test |
| OpenAPI generated client | Zorunluya yakın | Backend contract type safety |

Frontend kararında dikkat:

- Eski Blazor ekranları iş akışı envanteri olarak kullanılmalı, mimari olarak kopyalanmamalı.
- Rapor/print ihtiyacı Vue ve backend tarafında bağımsız çözümlerle ayrıca doğrulanmalı.
- Token ve tenant bilgisi için `localStorage` tek güven kaynağı olmamalıdır.

### 2.4 Infrastructure Fazları

Faz 1:

- Docker Compose
- PostgreSQL
- Tek backend API
- Vue SPA
- Serilog console/file veya Seq
- CI test/build

Faz 2:

- Redis
- Hangfire/Quartz
- OpenTelemetry
- Object storage veya MinIO/S3
- Reverse proxy

Faz 3:

- RabbitMQ/MassTransit
- Kubernetes
- Servis ayrıştırma
- Distributed tracing zorunlu hale getirme

---

## 3. Mimari Stil

### 3.1 İlk hedef: Modular Monolith

İlk üretilebilir sistem tek backend deployable olmalıdır, fakat modüller net ayrılmalıdır:

```
Dialysis.Api
  -> Dialysis.Application
      -> Patients
      -> MedCards
      -> Sessions
      -> MedCenters
      -> References
      -> Files
      -> Reports
      -> Admin
  -> Dialysis.Domain
  -> Dialysis.Infrastructure
```

Kurallar:

- Modüller birbirinin entity'sini doğrudan değiştirmez.
- Cross-module iletişim application service veya domain event üzerinden yapılır.
- Transaction gerektiren işlemler aynı process ve aynı DB içinde kalır.
- Public contract DTO ile yapılır, EF entity client'a dönmez.

### 3.2 Neden Microservice Değil?

Eski proje şu sorunları taşıyor:

- `HDSession` state geçişleri hatalı ve dağınık.
- Authorization çoğunlukla `[Authorize]` seviyesinde kalmış.
- Dosya/PII saklama güvenli değil.
- Test altyapısı yok.
- Build temiz değil.

Bu şartlarda 8-9 microservice'e bölmek problemi çözmez; problemi dağıtır. Önce domain ve güvenlik sağlamlaştırılmalıdır.

### 3.3 Geleceğe Açık Sınırlar

İlk sürüm modular monolith olsa da şu sınırlar korunmalıdır:

- `Files` modülü storage detayını saklar.
- `Reports` modülü SQL/report engine detayını saklar.
- `Sessions` modülü seans state machine'in tek sahibidir.
- `Identity/Admin` modülü kullanıcı, tenant, rol ve permission işlerini yönetir.
- `References` modülü status, fiyat, tanı, gösterge gibi ortak veri sözlüklerini yönetir.

---

## 4. Multi-Tenant Tasarım

### 4.1 Tenant Tanımı

Bu projede tenant = MedCenter.

Her tenant kendi hasta, seans, medkart, makine, employee, dosya ve rapor verilerine sahiptir. Global FOMS/admin rolleri tenant üstü rapor ve yönetim görebilir, fakat bu ayrı policy ile yapılmalıdır.

### 4.2 Tenant Entity

```csharp
public sealed class Tenant
{
    public string Id { get; set; } = default!;       // medcenter-123 veya GUID/ULID
    public string Code { get; set; } = default!;     // insan okunabilir kısa kod
    public string Name { get; set; } = default!;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string Locale { get; set; } = "ru-RU";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? DisabledAt { get; set; }
}
```

Başlangıçta `ConnectionString`, `Plan`, `MaxUsers`, `MaxPatients` gibi SaaS alanları zorunlu değildir. Eğer ürün gerçekten subscription modeline dönerse eklenebilir.

### 4.3 Tenant User

```csharp
public sealed class TenantUser
{
    public long Id { get; set; }
    public string TenantId { get; set; } = default!;
    public long UserId { get; set; }
    public bool IsTenantAdmin { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
    public DateTimeOffset? LeftAt { get; set; }
}
```

Kural:

- Kullanıcı birden fazla tenant'a bağlı olabilir.
- Kullanıcı login olduğunda tenant listesi döner.
- Aktif tenant seçimi server tarafından doğrulanır.

### 4.4 Tenant Resolution

Tek güvenilir kaynak server tarafındaki tenant context olmalıdır.

Önerilen akış:

1. Kullanıcı login olur.
2. API kullanıcının erişebileceği tenant listesini döner.
3. Kullanıcı tenant seçer.
4. `POST /api/v1/tenants/{tenantId}/switch` çağrılır.
5. Server membership kontrolü yapar.
6. Server aktif tenant claim'i içeren yeni access token üretir veya BFF session context'i günceller.

Kurallar:

- Query string ile tenant seçimi yapılmamalı.
- `X-Tenant-Id` header sadece debug/internal senaryolarda ve token'daki active tenant ile eşleşirse kabul edilebilir.
- Client `localStorage` içindeki tenant değerini sadece UI hatırlatma amacıyla kullanabilir; backend bunu güvenlik kaynağı saymamalıdır.

---

## 5. Authorization ve Ownership

### 5.1 Yetki Modeli

Authorization üç katmandan oluşmalıdır:

1. Authentication: kullanıcı kim?
2. Permission: bu işlemi yapma yetkisi var mı?
3. Ownership: bu kaynağa bu tenant/kullanıcı erişebilir mi?

Sadece permission yeterli değildir. Örnek: `patient.read` yetkisi olan kullanıcı sadece kendi tenant'ındaki hastaları görebilmelidir.

### 5.2 Permission Kodları

```csharp
public static class Permissions
{
    public const string PatientRead = "patient.read";
    public const string PatientCreate = "patient.create";
    public const string PatientUpdate = "patient.update";
    public const string PatientDelete = "patient.delete";
    public const string PatientExport = "patient.export";

    public const string MedCardRead = "medcard.read";
    public const string MedCardCreate = "medcard.create";
    public const string MedCardUpdate = "medcard.update";

    public const string SessionRead = "session.read";
    public const string SessionIdentify = "session.identify";
    public const string SessionStart = "session.start";
    public const string SessionPause = "session.pause";
    public const string SessionComplete = "session.complete";
    public const string SessionSendToPay = "session.send_to_pay";
    public const string SessionMarkPaid = "session.mark_paid";

    public const string MedCenterRead = "medcenter.read";
    public const string MedCenterManage = "medcenter.manage";
    public const string MedCenterUsers = "medcenter.users";

    public const string ReportView = "report.view";
    public const string ReportCreate = "report.create";
    public const string ReportExport = "report.export";
    public const string ReportDesign = "report.design";

    public const string AdminUsers = "admin.users";
    public const string AdminRoles = "admin.roles";
    public const string AdminTenants = "admin.tenants";
    public const string AdminAudit = "admin.audit";
}
```

### 5.3 Role ve Permission

```csharp
public sealed class Role
{
    public long Id { get; set; }
    public string? TenantId { get; set; } // null = global role
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public bool IsSystem { get; set; }
}

public sealed class RolePermission
{
    public long RoleId { get; set; }
    public long PermissionId { get; set; }
}

public sealed class UserRole
{
    public long UserId { get; set; }
    public long RoleId { get; set; }
    public string? TenantId { get; set; }
}
```

### 5.4 Policy Handler

Handler şu kontrolleri yapmalıdır:

- User authenticated mı?
- Active tenant var mı?
- Tenant aktif mi?
- User bu tenant'a bağlı mı?
- Required permission var mı?
- Kaynak id verilmişse kaynak aynı tenant'a mı ait?

Permission sonuçları cache'lenebilir, fakat cache key `userId + tenantId + permissionVersion` olmalıdır. Rol değişince `permissionVersion` artırılarak cache/token invalidation yapılmalıdır.

### 5.5 Controller Kullanımı

```csharp
[ApiController]
[Route("api/v1/patients")]
[Authorize]
public sealed class PatientsController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "Permission:patient.read")]
    public async Task<ActionResult<PagedResult<PatientListItemDto>>> Get([FromQuery] PatientQuery query)
        => Ok(await _sender.Send(query));

    [HttpPost]
    [Authorize(Policy = "Permission:patient.create")]
    public async Task<ActionResult<CreatedId<long>>> Create(CreatePatientCommand command)
        => CreatedResult(await _sender.Send(command));
}
```

---

## 6. Veri İzolasyonu

### 6.1 Seçilen Model

İlk faz:

- Shared PostgreSQL database
- Shared schema
- Her tenant verisinde `TenantId`
- EF Core global query filter
- PostgreSQL RLS

Database-per-service veya database-per-tenant ilk fazda kullanılmayacaktır.

### 6.2 Tenant Entity Interface

```csharp
public interface ITenantEntity
{
    string TenantId { get; set; }
}

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
}
```

### 6.3 EF Core Filter

Statik helper içinde instance `_tenant` kullanmak compile-time hataya yol açar. Filter context instance property üzerinden kurulmalıdır.

```csharp
public sealed class AppDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public string CurrentTenantId => _tenantContext.RequiredTenantId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Patient>()
            .HasQueryFilter(x => x.TenantId == CurrentTenantId && !x.IsDeleted);

        modelBuilder.Entity<HDSession>()
            .HasQueryFilter(x => x.TenantId == CurrentTenantId && !x.IsDeleted);
    }
}
```

Not: Tüm entity'lere otomatik filter üretilecekse expression builder kullanılmalı ve test edilmelidir. İlk sürümde kritik entity'leri açık yazmak daha güvenlidir.

### 6.4 SaveChanges Koruması

```csharp
public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
    {
        if (entry.State == EntityState.Added)
        {
            entry.Entity.TenantId = _tenantContext.RequiredTenantId;
        }

        if (entry.State == EntityState.Modified)
        {
            entry.Property(nameof(ITenantEntity.TenantId)).IsModified = false;
        }
    }

    return base.SaveChangesAsync(cancellationToken);
}
```

### 6.5 PostgreSQL RLS

```sql
ALTER TABLE patients ENABLE ROW LEVEL SECURITY;

CREATE POLICY patient_tenant_isolation ON patients
USING (tenant_id = current_setting('app.current_tenant', true));
```

Kritik kural:

- `app.current_tenant` her request/transaction için set edilmelidir.
- Connection pooling nedeniyle tenant setting connection üzerinde kalıcı sanılmamalıdır.
- Tercihen transaction içinde `SET LOCAL app.current_tenant = ...` kullanılmalıdır.
- RLS integration testleri zorunludur.

---

## 7. Domain Modülleri

### 7.1 Ana Modüller

| Modül | Sorumluluk |
| --- | --- |
| Identity/Admin | Kullanıcı, rol, permission, tenant membership |
| Patients | Hasta kaydı, hasta dosyaları, dış INN/Tunduk entegrasyonu |
| MedCards | Medikal kart, ilk muayene, analiz, epicrisis, writing-out |
| Sessions | Hemodiyaliz seansı, saat/pause, state machine, ödeme statüsü |
| MedCenters | Merkez, makine, çalışan, merkez dosyaları |
| References | Status, fiyat, tanı kodları, dialyzer type, indicator |
| Files | Upload, storage metadata, secure download |
| Reports | Excel/Word/PDF rapor üretimi |
| Audit | Action log, security log, redacted HTTP log |

### 7.2 Modül Kuralları

- `Sessions` modülü kendi status geçişlerinin tek sahibidir.
- `MedCards` doğrudan seans status değiştirmez; `Sessions` command çağırır.
- `Files` dışında hiçbir modül fiziksel path bilmez.
- `Reports` modülü raw table'lara sınırsız erişmez; view/query contract kullanır.
- `References` verisi seed/migration ile yönetilir.

### 7.3 Tamamlanmamış Eski Modüller

Eski projede riskli görülen modüller rewrite'ta açık kapsamla ele alınmalıdır:

| Modül | Eski durum | Yeni karar |
| --- | --- | --- |
| Epicrisis | Kısmi | MedCard alt modülü olarak tamamlanacak |
| Analysis | Kısmi | Analiz tipleri ve sonuçları ayrılacak |
| WritingOut | Kısmi/template odaklı | Domain + rapor template ayrılacak |
| QualityExam | Stub | Ya tamamlanacak ya kapalı olacak |

---

## 8. HDSession State Machine

### 8.1 Neden Merkezde?

Eski projedeki en kritik mantık hataları hemodiyaliz seans akışındaydı:

- Aktif seans kontrolünde hatalı `OR` kullanımı.
- Aynı seansın iki kez eklenebilmesi.
- `EndSession` metodunun seansı gerçekten bitirmemesi.
- Hasta/makine/seans validasyonlarının transaction ve DB constraint ile desteklenmemesi.

Bu yüzden rewrite'ın çekirdeği `HDSession` state machine olmalıdır.

### 8.2 Önerilen Statuslar

```csharp
public enum SessionStatus
{
    Created = 1,
    Identified = 2,
    Started = 3,
    Paused = 4,
    EndMeasurementsEntered = 5,
    Finished = 6,
    SentToPay = 7,
    Paid = 8,
    Cancelled = 9,
    NeedsReview = 10
}
```

### 8.3 Transition Tablosu

| From | Command | To | Ana validasyon |
| --- | --- | --- | --- |
| Created | IdentifyPatient | Identified | Hasta aktif, medcard var, tenant doğru |
| Identified | StartSession | Started | Makine aktif, tek aktif seans, kapasite uygun |
| Started | PauseSession | Paused | Aktif pause yok |
| Paused | ResumeSession | Started | Aktif pause var |
| Started | EnterEndMeasurements | EndMeasurementsEntered | Vitals/complication valid |
| EndMeasurementsEntered | CompleteSession | Finished | `SessionEnd`, duration, price hesaplanır |
| Finished | SendToPay | SentToPay | Billing kuralları tamam |
| SentToPay | MarkPaid | Paid | Payment/batch doğrulandı |
| Any active | CancelSession | Cancelled | Yetki + sebep zorunlu |
| Started | MarkNeedsReview | NeedsReview | Otomatik süre aşımı veya veri tutarsızlığı |

### 8.4 Otomatik İşler

270 dakika geçen seansı otomatik `Complete()` yapmak güvenli değildir. Tıbbi ve faturalama kaydı kullanıcı onayı gerektirir.

Doğru yaklaşım:

- Background job uzun süren seansı `NeedsReview` yapar.
- Operatör/doktor ekranında uyarı görünür.
- Yetkili kullanıcı end measurements ve tamamlamayı manuel onaylar.

### 8.5 DB Constraint Önerileri

- Aktif statuslarda hasta başına tek aktif seans.
- Aktif statuslarda makine başına tek aktif seans.
- `SessionEnd >= SessionStart`.
- `Paid` için `SentToPay` geçmişi zorunlu.
- `TenantId`, `PatientId`, `MedCenterId`, `MachineId` tutarlılığı.

PostgreSQL partial unique index örneği:

```sql
CREATE UNIQUE INDEX ux_active_session_patient
ON hd_sessions(tenant_id, patient_id)
WHERE status IN ('Identified', 'Started', 'Paused', 'EndMeasurementsEntered');
```

### 8.6 Zorunlu Testler

- Aynı hasta için iki aktif seans açılamaz.
- Aynı makine için iki aktif seans açılamaz.
- `Finished` seans tekrar `Started` olamaz.
- `Started` olmadan `CompleteSession` çalışmaz.
- Süre aşımı `NeedsReview` yapar, `Finished` yapmaz.
- Yetkisiz kullanıcı başka tenant seansını göremez/değiştiremez.

---

## 9. API Tasarımı

### 9.1 Genel Kurallar

- API version: `/api/v1`
- Entity dönülmez, DTO dönülür.
- Command/request DTO ve response DTO ayrılır.
- Validation hatası 400.
- Yetki hatası 403.
- Domain conflict 409.
- Not found 404.
- Beklenmeyen hata 500.
- Hata modeli `ProblemDetails`.
- OpenAPI contract zorunlu.

### 9.2 Endpoint Taslağı

```
/api/v1/auth
  POST /login
  POST /refresh
  POST /logout
  GET  /me

/api/v1/tenants
  GET  /my
  POST /{tenantId}/switch

/api/v1/patients
  GET    /
  POST   /
  GET    /{id}
  PUT    /{id}
  DELETE /{id}
  GET    /search
  GET    /{id}/files
  POST   /{id}/files

/api/v1/med-cards
  GET  /
  POST /
  GET  /{id}
  PUT  /{id}
  GET  /{id}/analyses
  GET  /{id}/epicrisis

/api/v1/sessions
  GET  /
  POST /
  GET  /{id}
  PUT  /{id}/identify
  PUT  /{id}/start
  PUT  /{id}/pause
  PUT  /{id}/resume
  PUT  /{id}/end-measurements
  PUT  /{id}/complete
  PUT  /{id}/send-to-pay
  PUT  /{id}/mark-paid
  PUT  /{id}/needs-review
  GET  /active

/api/v1/med-centers
  GET  /
  GET  /{id}
  PUT  /{id}
  GET  /{id}/machines
  GET  /{id}/employees

/api/v1/references
  GET /statuses
  GET /prices
  GET /diagnosis
  GET /indicators
  GET /dialyzer-types

/api/v1/reports
  GET  /
  POST /generate
  GET  /{id}
  GET  /templates

/api/v1/admin
  GET /users
  GET /roles
  GET /permissions
  GET /audit-logs
```

### 9.3 Response Örneği

```json
{
  "id": 456,
  "status": "Identified",
  "patient": {
    "id": 123,
    "displayName": "Иван Иванов",
    "inn": "12345678901234"
  },
  "tenantId": "medcenter-123",
  "identifiedAt": "2026-07-06T10:30:00Z"
}
```

---

## 10. Dosya ve Raporlama

### 10.1 Dosya Saklama

Eski projedeki `wwwroot` altına upload yaklaşımı tekrar edilmemelidir.

Kurallar:

- Dosyalar private storage alanında tutulur.
- Fiziksel path client'a dönülmez.
- DB'de sadece metadata ve storage key tutulur.
- Original filename sadece display alanıdır.
- MIME, extension, magic bytes ve size limit kontrolü zorunlu.
- Dosya indirme her zaman authorized endpoint üzerinden yapılır.
- Patient/MedCenter ownership kontrolü yapılır.
- Base64 büyük dosya upload'ları yerine multipart upload tercih edilir.

Önerilen metadata:

```csharp
public sealed class StoredFile : ITenantEntity
{
    public long Id { get; set; }
    public string TenantId { get; set; } = default!;
    public string StorageKey { get; set; } = default!;
    public string OriginalFileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public long SizeBytes { get; set; }
    public string Sha256 { get; set; } = default!;
    public long UploadedBy { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
}
```

### 10.2 Raporlama

Raporlama iş açısından kritik ama güvenlik açısından hassastır.

Kurallar:

- Report designer sadece özel permission ile açılır: `report.design`.
- Custom SQL varsayılan olarak kapalıdır.
- Gerekirse sadece readonly DB user ve allowlisted view/procedure kullanılmalıdır.
- Rapor çıktısı audit log'a yazılmalıdır.
- Excel/Word template runtime output ile aynı klasörde tutulmamalıdır.

---

## 11. Frontend Mimari

### 11.1 Klasör Yapısı

```
frontend/
  src/
    app/
      router/
      layouts/
      i18n/
    shared/
      api/
      auth/
      components/
      utils/
    features/
      patients/
      med-cards/
      sessions/
      med-centers/
      references/
      reports/
      admin/
    stores/
    types/
```

### 11.2 State Kuralları

- Pinia UI/session state içindir.
- Server data için query cache kullanılabilir.
- Tenant switch server endpoint ile yapılır.
- `localStorage` token/tenant için tek güven kaynağı değildir.
- Güvenlik açısından BFF + HttpOnly Secure SameSite cookie tercih edilir. Saf SPA JWT kullanılacaksa refresh token rotation, kısa access token süresi ve XSS/CSP önlemleri zorunludur.

### 11.3 API Client

- OpenAPI'den TypeScript client generate edilmelidir.
- 401, 403, 409 ve validation hataları merkezi yakalanmalıdır.
- Tenant ve auth davranışı her componentte tekrar edilmemelidir.

---

## 12. Güvenlik

### 12.1 Secret Management

- `appsettings*.json` içinde gerçek secret bulunmaz.
- Development: user-secrets veya local `.env`.
- Production: secret manager veya environment variables.
- Mevcut projeden sızmış secret'lar rotate edilir.
- Git history temizliği planlanır.

### 12.2 Logging

HTTP body/header loglama varsayılan olarak kapalı olmalıdır.

Asla loglanmayacaklar:

- Authorization
- Cookie / Set-Cookie
- Password
- Token
- INN/passport gibi kimlik verileri
- Dosya base64 içerikleri
- Tıbbi serbest metin alanları

### 12.3 JWT/OIDC

Kurallar:

- HTTPS zorunlu.
- Issuer/audience/lifetime validate edilir.
- Access token kısa ömürlüdür.
- Refresh token rotate edilir ve server tarafında revoke edilebilir.
- Token version veya session version ile toplu invalidation yapılabilir.

### 12.4 Rate Limit

İlk fazda reverse proxy/gateway şart değildir. ASP.NET Core rate limiter yeterlidir:

- Login endpoint: sıkı limit.
- File upload: boyut ve hız limiti.
- Report generate: düşük concurrency.
- Genel API: kullanıcı + tenant bazlı limit.

---

## 13. Observability ve Background Jobs

### 13.1 Logging

- Serilog structured JSON log.
- Correlation id.
- User id ve tenant id redacted/controlled context olarak eklenir.
- PII loglanmaz.

### 13.2 Health Checks

İlk faz:

- API self health
- PostgreSQL health
- Storage health

Faz 2:

- Redis health
- Background job health
- Report engine health

### 13.3 Background Jobs

İlk adaylar:

- Stale session detection -> `NeedsReview`
- Old log cleanup
- Report generation queue
- File cleanup/orphan cleanup

Kural: Background job domain state'i doğrudan "tamamlandı/ödendi" gibi kritik son statülere kullanıcı onayı olmadan taşımaz.

---

## 14. Proje Yapısı

```
Dialysis/
  backend/
    src/
      Dialysis.Api/
        Controllers/
        Middleware/
        Filters/
        Program.cs
      Dialysis.Application/
        Common/
          Authorization/
          Behaviors/
          Errors/
          Interfaces/
        Patients/
        MedCards/
        Sessions/
        MedCenters/
        References/
        Files/
        Reports/
        Admin/
      Dialysis.Domain/
        Common/
        Patients/
        MedCards/
        Sessions/
        MedCenters/
        References/
        Files/
        Users/
      Dialysis.Infrastructure/
        Data/
        Identity/
        Storage/
        Reporting/
        ExternalServices/
        Logging/
      Dialysis.Contracts/
        Patients/
        MedCards/
        Sessions/
        MedCenters/
        References/
        Reports/
    tests/
      Dialysis.UnitTests/
      Dialysis.IntegrationTests/
      Dialysis.ArchTests/

  frontend/
    src/
      app/
      shared/
      features/
      stores/
      types/

  infrastructure/
    docker/
    scripts/
```

### 14.1 Dependency Rule

```
Api -> Application -> Domain
Api -> Infrastructure
Infrastructure -> Application/Domain
Contracts -> bağımsız DTO paketi
Domain -> hiçbir dış katmana referans vermez
```

---

## 15. Veritabanı ve Migration Stratejisi

### 15.1 Tek DB, Modüler Schema

İlk fazda tek PostgreSQL DB kullanılacaktır.

Önerilen schema yaklaşımı:

- `identity`
- `tenant`
- `patient`
- `medcard`
- `session`
- `center`
- `reference`
- `file`
- `report`
- `audit`

Bu, tek DB sadeliğini korurken modül sınırlarını görünür yapar.

### 15.2 Migration Kuralları

- Migration tek projeden yönetilir.
- Seed data migration veya explicit seed script ile yapılır.
- RLS policy migration ile uygulanır.
- Partial unique indexler migration ile uygulanır.
- Her migration CI'da fresh DB üzerine test edilir.

### 15.3 Zorunlu Index/Constraintler

- `users.username` unique.
- `tenants.code` unique.
- `tenant_users(tenant_id, user_id)` active membership unique.
- `patients(tenant_id, inn)` unique veya business kuralına göre controlled duplicate.
- Aktif session patient unique partial index.
- Aktif session machine unique partial index.
- `active_prices` için tek aktif fiyat constraint'i.
- File `storage_key` unique.

---

## 16. Test Stratejisi

### 16.1 Unit Test

- HDSession state machine.
- Patient validation.
- Permission decision logic.
- File metadata validation.
- Price/billing calculation.

### 16.2 Integration Test

- PostgreSQL Testcontainers.
- EF migration fresh DB.
- RLS tenant isolation.
- API authorization.
- File upload/download authorization.
- Session conflict DB constraintleri.

### 16.3 E2E Test

Playwright ile:

- Login.
- Tenant switch.
- Hasta oluşturma.
- Seans identify/start/complete akışı.
- Yetkisiz tenant erişimi engeli.
- Report generate smoke test.

### 16.4 Architecture Test

- Domain infrastructure referansı alamaz.
- API entity dönmez.
- Application modülleri birbirinin internal sınıflarına erişmez.
- Controller action'larında policy attribute zorunlu.

---

## 17. CI/CD ve Infrastructure

### 17.1 CI

Her PR:

```text
restore -> format/analyzer -> build -> unit test -> integration test -> frontend test -> security scan -> docker build
```

### 17.2 Local Development

Docker Compose:

- PostgreSQL
- MinIO veya local file storage emulator
- Seq opsiyonel
- Redis faz 2

### 17.3 Production İlk Faz

- Tek backend API container.
- Vue statik build CDN veya reverse proxy üzerinden.
- PostgreSQL managed veya güvenli VM.
- Private storage.
- Backup/restore prosedürü.
- Kubernetes şart değil.

---

## 18. Microservice'e Geçiş Kriterleri

Microservice'e geçiş ancak şu koşullardan en az birkaçı oluşursa düşünülmelidir:

- Bir modül bağımsız scaling gerektiriyor.
- Takımlar modül bazında ayrı release yapmak zorunda.
- Modülün veri sahipliği net ve transaction ihtiyacı düşük.
- Outbox eventleri stabil ve tüketicileri belirgin.
- Observability ve deployment otomasyonu olgun.

İlk ayrılabilecek adaylar:

1. File Service
2. Reporting Service
3. Notification Service
4. Identity Service

En son ayrılacak veya uzun süre monolith içinde kalacak modül:

- HDSession ve billing/state machine. Çünkü transaction ve domain tutarlılığı çok kritik.

---

## 19. Uygulama Yol Haritası

### Faz 0: Hazırlık

- Mevcut secret'ları rotate et.
- Domain status sözlüğünü iş ekibiyle doğrula.
- Tenant/role/permission matrisi çıkar.
- Aktif ekran ve rapor envanteri çıkar.
- Veri migration stratejisini belirle.

### Faz 1: Çekirdek Backend

- .NET 10 solution.
- PostgreSQL + EF migration.
- Tenant context + EF filter + RLS.
- Identity/auth.
- Permission handler.
- Patient temel CRUD.
- HDSession state machine unit testleri.

### Faz 2: Core Workflow

- MedCard.
- Session identify/start/end/complete/send-to-pay/paid.
- MedCenter/machine.
- File storage.
- Audit log.
- Integration tests.

### Faz 3: Frontend

- Vue app shell.
- Auth + tenant switch.
- Patients.
- Sessions.
- MedCards.
- MedCenters.
- Admin/permissions.

### Faz 4: Reports ve Eksik Modüller

- Reporting templates.
- Epicrisis.
- Analysis.
- WritingOut.
- QualityExam.

### Faz 5: Operasyonel Olgunluk

- Redis.
- Background jobs.
- OpenTelemetry.
- Advanced rate limiting.
- Outbox.
- Gerekiyorsa RabbitMQ.

---

## 20. Açık Sorular

1. `HDSessionEnum` eski statuslarının resmi iş anlamı nedir?
2. Bir hasta aynı gün kaç seans alabilir?
3. Bir makine için günlük kapasite nasıl hesaplanır?
4. `Identification`, `EndIdentification`, `Finished`, `SendToPay`, `Payed` arasındaki resmi akış nedir?
5. Hangi roller global, hangileri tenant bazlı?
6. FOMS/admin kullanıcıları tenant sınırını hangi ekranlarda aşabilir?
7. Rapor designer gerçekten gerekli mi, yoksa sadece hazır template üretimi yeterli mi?
8. Dosyalar için saklama süresi ve yasal gereksinimler nelerdir?
9. Tunduk/INN entegrasyonlarında timeout ve fallback beklentisi nedir?
10. Mevcut DB'de data migration için temizlenmesi gereken duplicate kayıtlar var mı?

---

## Sonuç

Bu mimari, eski projenin gerçek problemlerine daha doğrudan cevap verir:

- İlk adımda microservice karmaşıklığı yok.
- Tenant izolasyonu hem uygulama hem DB katmanında var.
- Permission sistemi ownership ile tamamlanıyor.
- `HDSession` state machine mimarinin merkezine alınıyor.
- Dosya, rapor, log ve secret güvenliği baştan tasarlanıyor.
- Testler opsiyonel değil, temel mimari parçası oluyor.

Vizyon olarak microservice, RabbitMQ ve Kubernetes korunabilir; fakat rewrite'ın ilk başarılı sürümü için hedef **güvenilir modular monolith** olmalıdır.
