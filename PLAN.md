# Dialysis Yeni Proje Başlangıç Planı

## Summary
İlk adım sadece sağlam backend proje iskeleti olacak. Eski `Dialysis/` projesine dokunulmayacak; yeni yapı mevcut boş `backend/` klasörü altında kurulacak. Bu adımda Patient, MedCard, Session, User, Permission veya Tenant entityleri henüz yazılmayacak. Ama proje yapısı onları eklemeye hazır olacak.

Seçilen kararlar:
- Runtime: `.NET 10 LTS`
- İlk mimari: modular monolith
- İlk kapsam: sadece proje iskeleti + PostgreSQL + Swagger + health check
- Auth yönü: ileride custom User/Role/Permission + JWT
- Tenant yönü: ileride `TenantId + EF Core global filter`; RLS hedef olarak sonraya bırakılacak
- Frontend: ilk adımda dokunulmayacak

## Key Changes
- `backend/` altında yeni solution oluştur:
  - `Dialysis.Backend.sln`
  - `src/Dialysis.Api`
  - `src/Dialysis.Application`
  - `src/Dialysis.Domain`
  - `src/Dialysis.Infrastructure`
  - `src/Dialysis.Contracts`
  - `tests/Dialysis.UnitTests`
  - `tests/Dialysis.IntegrationTests`
- Proje referansları:
  - `Api -> Application, Infrastructure, Contracts`
  - `Application -> Domain, Contracts`
  - `Infrastructure -> Application, Domain`
  - `Contracts` bağımsız kalır
  - `Domain` hiçbir dış katmana referans almaz
- `backend/global.json` ekle:
  - SDK `10.0.103`
  - `rollForward: latestFeature`
- `docker-compose.yml` ekle:
  - PostgreSQL `16-alpine`
  - DB: `dialysis_dev`
  - User: `dialysis_app`
  - Local dev password: sadece geliştirme amaçlı default değer
  - Volume: `dialysis_postgres_data`
- `Dialysis.Api` içinde:
  - Controllers aktif
  - Swagger UI aktif
  - `GET /health` endpoint’i
  - PostgreSQL bağlantısı `ConnectionStrings:Default`
  - `AppDbContext` DI kaydı
- `Dialysis.Infrastructure` içinde:
  - Boş `AppDbContext`
  - Npgsql EF Core bağlantısı
  - Henüz migration yok; ilk migration User/Permission adımında açılacak
- `backend/README.md` ekle:
  - `docker compose up -d`
  - `dotnet build`
  - `dotnet run --project src/Dialysis.Api`
  - Swagger ve health URL’leri

## Step-by-Step Roadmap
1. **Step 1: Project Skeleton**
   - Yeni backend solution ve katman projeleri oluştur.
   - PostgreSQL docker-compose kur.
   - API, Swagger, health check ve DB bağlantısını doğrula.
   - Kabul kriteri: `dotnet build` başarılı, Postgres container çalışıyor, `/swagger` açılıyor, `/health` healthy dönüyor.

2. **Step 2: Custom User + Permission Foundation**
   - `User`, `Role`, `Permission`, `RolePermission`, `UserRole`, `UserSession` entityleri ekle.
   - Password hashing için ASP.NET Core `PasswordHasher` kullan.
   - JWT access token + refresh token altyapısı kur.
   - Seed: default admin user, admin role, temel permissionlar.
   - Kabul kriteri: login, me, logout/revoke akışı çalışır.

3. **Step 3: Tenant Foundation**
   - `Tenant`, `TenantUser`, `ITenantEntity`, `ITenantContext` ekle.
   - Tenant switch endpoint’i ekle.
   - EF Core global filter ile `TenantId` izolasyonu kur.
   - Kabul kriteri: kullanıcı sadece aktif tenant verisini görebilir.

4. **Step 4: Patient CRUD**
   - `Patient` entity, DTO, validation, CRUD endpointleri.
   - Tenant izolasyonu ve permission policy zorunlu.
   - Kabul kriteri: farklı tenant hastaları birbirinden izole.

5. **Step 5: MedCard CRUD**
   - `MedCard` entity ve temel CRUD.
   - Patient ilişkisi ve tenant ownership kontrolü.
   - Kabul kriteri: sadece aynı tenant içindeki patient için medcard oluşturulur.

6. **Step 6: Session State Machine**
   - `HDSession` entity ve state machine.
   - Identify, start, pause, resume, end-measurements, complete akışı.
   - Kabul kriteri: invalid transitionlar 409 döner; duplicate active session engellenir.

## Test Plan
İlk adım testleri:
- `dotnet build backend/Dialysis.Backend.sln`
- `docker compose -f backend/docker-compose.yml up -d`
- API run sonrası:
  - `/swagger` açılmalı
  - `/health` healthy dönmeli
  - API PostgreSQL’e bağlanabilmeli
- Architecture test başlangıcı:
  - `Domain` katmanı `Infrastructure` veya `Api` referansı almamalı
  - `Contracts` bağımsız kalmalı

Sonraki adımlarda eklenecek testler:
- User/permission unit testleri
- JWT login integration testleri
- Tenant isolation integration testleri
- Patient/MedCard CRUD API testleri
- Session state machine unit testleri

## Assumptions
- Yeni backend mevcut `backend/` klasörüne kurulacak.
- Eski `Dialysis/` klasörü sadece referans olarak kalacak, değiştirilmeyecek.
- İlk adımda frontend değişmeyecek.
- İlk adımda entity/migration yazılmayacak; DB bağlantısı health check ile doğrulanacak.
- Auth için ASP.NET Identity veya Duende kullanılmayacak; custom entity + JWT yaklaşımı sonraki adımda kurulacak.
- Tenant izolasyonunda ilk uygulama `TenantId + EF filter`; PostgreSQL RLS daha sonraki güvenlik sertleştirme adımı olacak.
