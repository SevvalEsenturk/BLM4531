# LicenseTracker Proje Analizi ve Öğrenme Planı

## Proje Hakkında
Bu proje bir **Lisans Takip Sistemi** (License Tracker) uygulamasıdır. Şirketlerin kullandığı yazılım lisanslarını takip etmek için geliştirilmiş bir ASP.NET Core Web API + Blazor/Razor Pages uygulamasıdır.

## Teknolojiler ve Kullanılan Kütüphaneler

### Backend (Sunucu Tarafı)
- **ASP.NET Core 9.0** - Web framework
- **Entity Framework Core** - Veritabanı işlemleri için ORM (Object-Relational Mapping)
- **SQLite** - Hafif veritabanı
- **ASP.NET Core Identity** - Kullanıcı yönetimi, kimlik doğrulama
- **Swagger/OpenAPI** - API dokümantasyonu

### Frontend (İstemci Tarafı)
- **Blazor Server** - C# ile web UI geliştirme
- **Razor Pages** - Sunucu tarafında sayfa oluşturma
- **CORS** - Frontend ile backend arası iletişim için (localhost:3000)

## Proje Yapısı

```
LicenseTracker/
│
├── Models/                          # Veri modelleri
│   ├── License.cs                   # Lisans modeli
│   ├── Company.cs                   # Şirket modeli
│   └── ApplicationUser.cs           # Kullanıcı modeli
│
├── Controllers/                     # API Controllers (REST API endpoints)
│   ├── LicensesController.cs        # Lisans CRUD işlemleri
│   ├── CompaniesController.cs       # Şirket CRUD işlemleri
│   └── VendorSyncController.cs      # Vendor API senkronizasyonu
│
├── DTOs/                            # Data Transfer Objects
│   ├── LicenseDto.cs               # Lisans veri transfer objesi
│   └── CompanyDto.cs               # Şirket veri transfer objesi
│
├── Data/                           # Veritabanı yapılandırması
│   ├── AppDbContext.cs             # EF Core DbContext
│   └── SeedData.cs                 # Test verileri
│
├── Services/                       # İş mantığı servisleri
│   ├── Interfaces/
│   │   └── IVendorLicenseService.cs
│   └── VendorServices/            # Üçüncü parti API servisleri
│       ├── MicrosoftGraphService.cs
│       ├── AdobeService.cs
│       ├── SlackService.cs
│       ├── GoogleWorkspaceService.cs
│       └── ...diğer vendor servisleri
│
└── Areas/Identity/Pages/Account/   # Kimlik doğrulama sayfaları
    ├── Login.cshtml.cs             # Kullanıcı giriş sayfası
    ├── Register.cshtml.cs          # Kullanıcı kayıt sayfası
    └── Logout.cshtml.cs            # Çıkış işlemi
```

## Mevcut API Endpoints (Hazır Kodlar)

### 1. Licenses API (`/api/Licenses`)
Dosya: `Controllers/LicensesController.cs`

- **GET** `/api/Licenses` - Tüm lisansları listele
- **GET** `/api/Licenses/{id}` - Belirli bir lisansı getir
- **POST** `/api/Licenses` - Yeni lisans ekle
- **PUT** `/api/Licenses/{id}` - Lisansı güncelle
- **DELETE** `/api/Licenses/{id}` - Lisansı sil

### 2. Companies API (`/api/Companies`)
Dosya: `Controllers/CompaniesController.cs`

- Şirketlerle ilgili CRUD işlemleri (muhtemelen benzer yapıda)

### 3. Authentication (Kimlik Doğrulama)
Dosya: `Areas/Identity/Pages/Account/`

- Login - Kullanıcı girişi
- Register - Yeni kullanıcı kaydı
- Logout - Çıkış yapma

## Kodların Nasıl Çalıştığı - Basit Akış

### Frontend'ten Backend'e İstek Örneği

```
1. Kullanıcı Web Tarayıcısından giriş yapar
   └─> Login.cshtml.cs dosyası çalışır
       └─> ASP.NET Identity kullanıcıyı doğrular
           └─> Başarılıysa kullanıcı ana sayfaya yönlendirilir

2. Kullanıcı "Lisansları Gör" butonuna tıklar
   └─> Frontend HTTP GET isteği yapar: http://localhost:PORT/api/Licenses
       └─> LicensesController.cs içindeki GetLicenses() metodu çalışır
           └─> AppDbContext üzerinden SQLite veritabanından lisanslar çekilir
               └─> Veriler LicenseDto'ya dönüştürülür
                   └─> JSON formatında frontend'e geri döner
                       └─> Frontend bu veriyi ekranda gösterir
```

## Öğrenmeniz Gereken Konular

### 1. Temel Kavramlar
- ✅ **HTTP Metodları**: GET, POST, PUT, DELETE ne işe yarar?
- ✅ **REST API nedir**: API nasıl çalışır, endpoint nedir?
- ✅ **JSON**: Veri nasıl transfer edilir?
- ✅ **CRUD**: Create, Read, Update, Delete işlemleri

### 2. ASP.NET Core Kavramları
- ✅ **Controller**: API endpoint'lerini nasıl oluşturur?
- ✅ **Model**: Veritabanı tablolarını temsil eder
- ✅ **DTO (Data Transfer Object)**: Neden kullanılır?
- ✅ **Dependency Injection**: Servisler nasıl eklenir? (Program.cs satır 11-65)
- ✅ **Middleware**: Request/Response pipeline nasıl çalışır? (Program.cs satır 69-102)

### 3. Entity Framework Core
- ✅ **DbContext**: Veritabanı ile nasıl iletişim kurulur?
- ✅ **DbSet**: Tablolar nasıl tanımlanır?
- ✅ **Migration**: Veritabanı şeması nasıl güncellenir?
- ✅ **LINQ**: Sorgular nasıl yazılır? (örn: `_context.Licenses.ToListAsync()`)

### 4. Authentication & Authorization
- ✅ **ASP.NET Identity**: Kullanıcı kayıt/giriş sistemi
- ✅ **Cookie Authentication**: Oturum yönetimi
- ✅ **Role-based Authorization**: Yetkilendirme (henüz yok, eklenebilir)

## Pratik Yapabileceğiniz Örnek API'ler

### Örnek 1: Lisans Kategorisi Ekleme
**Senaryo**: Sistemde şu anda her lisansın bir kategorisi var (Productivity, Security vb.). Kategorileri ayrı bir tablo yapalım ve yönetelim.

**Yapılacaklar**:
1. `Models/Category.cs` modeli oluştur
2. `Controllers/CategoriesController.cs` API endpoint'leri ekle
3. `AppDbContext.cs` içine `DbSet<Category>` ekle
4. GET, POST, PUT, DELETE endpoint'lerini yaz

### Örnek 2: Lisans Bildirimleri API
**Senaryo**: Lisansların bitiş tarihine 30 gün kala uyarı göster.

**Yapılacaklar**:
1. `Models/Notification.cs` modeli oluştur
2. `Controllers/NotificationsController.cs` API endpoint'leri ekle
3. GET `/api/Notifications/expiring` endpoint'i yaz (30 gün içinde biten lisansları getir)
4. POST `/api/Notifications/mark-read/{id}` endpoint'i yaz (bildirimi okundu işaretle)

### Örnek 3: Kullanıcı Profili API
**Senaryo**: Kullanıcılar profillerini görüntüleyebilsin ve güncelleyebilsin.

**Yapılacaklar**:
1. `DTOs/UserProfileDto.cs` oluştur
2. `Controllers/ProfileController.cs` API oluştur
3. GET `/api/Profile/me` - Giriş yapmış kullanıcının bilgilerini getir
4. PUT `/api/Profile/me` - Kullanıcı bilgilerini güncelle
5. `[Authorize]` attribute kullan (sadece giriş yapmış kullanıcılar erişebilsin)

### Örnek 4: Dashboard İstatistikleri API
**Senaryo**: Anasayfada toplam lisans sayısı, aktif lisans sayısı, yakında bitecek lisanslar gibi istatistikler göster.

**Yapılacaklar**:
1. `DTOs/DashboardStatsDto.cs` oluştur
2. `Controllers/DashboardController.cs` oluştur
3. GET `/api/Dashboard/stats` endpoint'i yaz
   - Toplam lisans sayısı
   - Aktif lisans sayısı (HasLicense = true)
   - Bu ay biten lisans sayısı
   - Toplam maliyet

### Örnek 5: Lisans Arama ve Filtreleme
**Senaryo**: Kullanıcılar lisansları vendor'a, kategoriye veya duruma göre filtrelesin.

**Yapılacaklar**:
1. GET `/api/Licenses/search?vendor=Microsoft&category=Productivity`
2. Query parametrelerini kullan
3. LINQ ile dinamik filtreleme yap

## Örnek Kod - Kategori API (Tam Örnek)

```csharp
// Models/Category.cs
namespace LicenseTracker.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}

// Controllers/CategoriesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LicenseTracker.Data;
using LicenseTracker.Models;

namespace LicenseTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            return await _context.Categories.ToListAsync();
        }

        // POST: api/Categories
        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, category);
        }
    }
}
```

## Projeyi Çalıştırma

1. Visual Studio veya VS Code ile projeyi açın
2. Terminal'de proje klasörüne gidin
3. Komutu çalıştırın: `dotnet run`
4. Tarayıcıda açın: `https://localhost:5001` veya `http://localhost:5000`
5. Swagger UI için: `https://localhost:5001/swagger`

## Swagger ile API Test Etme

1. Projeyi çalıştırın
2. `https://localhost:5001/swagger` adresine gidin
3. API endpoint'lerini görebilir ve test edebilirsiniz
4. "Try it out" butonuna tıklayarak API'yi doğrudan test edin

## Frontend - Backend Bağlantısı Örneği

**Frontend (JavaScript/React - localhost:3000)**
```javascript
// Tüm lisansları getir
async function getLicenses() {
  const response = await fetch('https://localhost:5001/api/Licenses');
  const licenses = await response.json();
  console.log(licenses);
}

// Yeni lisans ekle
async function createLicense() {
  const newLicense = {
    name: "Adobe Creative Cloud",
    vendor: "Adobe",
    category: "Design",
    hasLicense: true,
    startDate: "2025-01-01",
    endDate: "2026-01-01",
    cost: 599.99,
    users: 10
  };

  const response = await fetch('https://localhost:5001/api/Licenses', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(newLicense)
  });

  const result = await response.json();
  console.log('Eklenen lisans:', result);
}
```

## Önerilen Öğrenme Sırası

1. **Hafta 1**: Mevcut kodu incele
   - LicensesController.cs dosyasını satır satır oku
   - Her HTTP metodunun ne yaptığını anla (GET, POST, PUT, DELETE)
   - Swagger ile API'leri test et

2. **Hafta 2**: Basit bir API yaz
   - Category API'yi yukarıdaki örnekten yaz
   - Swagger'da test et
   - Frontend'ten çağır (eğer varsa)

3. **Hafta 3**: Daha karmaşık özellikler ekle
   - Filtreleme ve arama ekle
   - Bildirim sistemi ekle
   - Dashboard istatistikleri yap

4. **Hafta 4**: Authentication ve Authorization
   - API'lere `[Authorize]` ekle
   - Sadece giriş yapmış kullanıcılar erişebilsin
   - Rol bazlı yetkilendirme ekle (Admin, User)

## Faydalı Kaynaklar

- **Microsoft Docs**: https://learn.microsoft.com/aspnet/core
- **Entity Framework Core**: https://learn.microsoft.com/ef/core
- **ASP.NET Core Tutorial**: YouTube'da "ASP.NET Core Web API" ara
- **C# Temelleri**: Microsoft Learn üzerinde ücretsiz kurslar var

## Sorular ve İlerlemeler

Bu dosyaya öğrenme sürecinde karşılaştığınız soruları ve çözdüğünüz problemleri ekleyebilirsiniz.

### Tamamlanan Görevler
- [ ] Projeyi ilk kez çalıştırdım
- [ ] Swagger UI'da API'leri test ettim
- [ ] LicensesController.cs kodunu okudum ve anladım
- [ ] İlk API endpoint'imi yazdım (Category API)
- [ ] Frontend'ten API'yi çağırdım
- [ ] Authentication/Authorization eklediğim bir API yazdım

### Sorular
(Buraya öğrenme sürecinde aklınıza takılan soruları yazabilirsiniz)

---

**Not**: Bu dosya sizin öğrenme sürecinizi takip etmek için oluşturuldu. İstediğiniz zaman güncelleyebilir ve notlar ekleyebilirsiniz.
