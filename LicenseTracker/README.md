# LicenseTracker - Yazılım Lisans Takip Sistemi

Şirketlerin kullandığı yazılım lisanslarını takip eden full-stack web uygulaması.

## Teknoloji Stack

### Backend
- ASP.NET Core 9.0 Web API
- Entity Framework Core + SQL Server
- ASP.NET Identity (Kullanıcı Yönetimi)
- Swagger/OpenAPI

### Frontend
- Next.js 16.0 + React 19
- TypeScript
- Tailwind CSS
- shadcn/ui (Radix UI)

## Özellikler

- ✅ Lisans yönetimi (CRUD operasyonları)
- ✅ Şirket yönetimi
- ✅ Son kullanma tarihi takibi
- ✅ 30 gün içinde dolacak lisanslar için uyarı sistemi
- ✅ Maliyet ve kullanıcı sayısı takibi
- ✅ Kategori bazlı filtreleme
- ✅ Arama fonksiyonu
- ✅ Dark/Light tema desteği

## Kurulum ve Çalıştırma

### Gereksinimler

- .NET 9.0 SDK
- SQL Server (LocalDB veya tam sürüm)
- Node.js 18+ ve npm

### 1. Backend Kurulumu

```bash
# Backend klasörüne git
cd LicenseTracker

# Bağımlılıkları yükle
dotnet restore

# Migration oluştur (License modelini güncelledik)
dotnet ef migrations add UpdateLicenseModel

# Veritabanını güncelle
dotnet ef database update

# Backend'i çalıştır
dotnet run
```

Backend varsayılan olarak `https://localhost:7001` üzerinde çalışacak.

### 2. Frontend Kurulumu

```bash
# Frontend klasörüne git
cd Frontend

# Bağımlılıkları yükle
npm install

# Development server'ı başlat
npm run dev
```

Frontend `http://localhost:3000` üzerinde çalışacak.

## API Endpoints

### Licenses
- `GET /api/licenses` - Tüm lisansları getir
- `GET /api/licenses/{id}` - Belirli bir lisansı getir
- `POST /api/licenses` - Yeni lisans oluştur
- `PUT /api/licenses/{id}` - Lisansı güncelle
- `DELETE /api/licenses/{id}` - Lisansı sil

### Companies
- `GET /api/companies` - Tüm şirketleri getir
- `GET /api/companies/{id}` - Belirli bir şirketi getir
- `POST /api/companies` - Yeni şirket oluştur
- `PUT /api/companies/{id}` - Şirketi güncelle
- `DELETE /api/companies/{id}` - Şirketi sil

## Veritabanı Connection String

`appsettings.json` dosyasında connection string'i kendi ortamınıza göre ayarlayın:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LicenseTrackerDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

## Test Verileri

Uygulama ilk çalıştırıldığında otomatik olarak test verileri eklenecek:
- 1 demo şirketi
- 10 örnek lisans (farklı kategorilerde)

Test verileri `Data/SeedData.cs` dosyasında tanımlanmıştır.

## Swagger/API Dokümantasyonu

Backend çalıştığında Swagger UI'a şu adresten erişebilirsiniz:
`https://localhost:7001/swagger`

## Değişiklikler

### Backend
- ✅ License modeli güncellendi (Name, Vendor, Category, HasLicense, Cost, Users alanları eklendi)
- ✅ API Controller'lar oluşturuldu (LicensesController, CompaniesController)
- ✅ DTOs eklendi (veri transfer nesneleri)
- ✅ CORS yapılandırması (Frontend için)
- ✅ Seed data eklendi

### Frontend
- ✅ API servis katmanı oluşturuldu (`lib/api.ts`)
- ✅ Mock data kaldırıldı
- ✅ Backend API'ye bağlandı
- ✅ Loading ve error state'leri eklendi
- ✅ Environment variables (`.env.local`)


