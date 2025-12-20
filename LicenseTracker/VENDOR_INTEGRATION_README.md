# Vendor API Entegrasyonu Dokümantasyonu

## Genel Bakış

LicenseTracker artık **9 farklı vendor'dan otomatik lisans bilgisi çekme** özelliğine sahiptir. Bu özellik, şirket genelindeki tüm yazılım lisanslarını merkezi olarak yönetmenizi sağlar.

## Desteklenen Vendor'lar

1. **Microsoft** - Office 365, Microsoft 365 lisansları
2. **Adobe** - Creative Cloud lisansları
3. **Slack** - Workspace kullanıcıları
4. **Atlassian** - Jira, Confluence
5. **Google** - Workspace (G Suite)
6. **Zoom** - Video conferencing
7. **GitHub** - Enterprise
8. **Dropbox** - Business
9. **GitLab** - Self-hosted veya Cloud

## Nasıl Çalışır?

### Backend Mimarisi

```
Services/
├── Interfaces/
│   └── IVendorLicenseService.cs       # Base interface
└── VendorServices/
    ├── MicrosoftGraphService.cs       # Microsoft 365 API
    ├── AdobeService.cs                # Adobe Admin Console API
    ├── SlackService.cs                # Slack API
    ├── AtlassianService.cs            # Jira/Confluence API
    ├── GoogleWorkspaceService.cs      # Google Workspace API
    ├── ZoomService.cs                 # Zoom API
    ├── GitHubService.cs               # GitHub API
    ├── DropboxService.cs              # Dropbox API
    └── GitLabService.cs               # GitLab API

Controllers/
└── VendorSyncController.cs            # Sync API endpoint'leri
```

### API Endpoint'leri

#### 1. Tüm Vendor'ları Senkronize Et
```http
POST /api/vendorsync/sync-all
```

Tüm yapılandırılmış vendor'lardan lisans bilgilerini çeker.

**Response:**
```json
[
  {
    "success": true,
    "vendorName": "Microsoft",
    "licensesFound": 5,
    "licensesAdded": 2,
    "licensesUpdated": 3,
    "errorMessage": null,
    "syncTime": "2025-11-02T10:30:00Z"
  }
]
```

#### 2. Tek Bir Vendor'ı Senkronize Et
```http
POST /api/vendorsync/sync/{vendorName}
```

Örnek:
```http
POST /api/vendorsync/sync/microsoft
POST /api/vendorsync/sync/slack
```

#### 3. Tüm Bağlantıları Test Et
```http
GET /api/vendorsync/test-all
```

**Response:**
```json
{
  "Microsoft": true,
  "Adobe": false,
  "Slack": true
}
```

#### 4. Tek Bir Vendor Bağlantısını Test Et
```http
GET /api/vendorsync/test/{vendorName}
```

#### 5. Desteklenen Vendor Listesi
```http
GET /api/vendorsync/vendors
```

**Response:**
```json
["Adobe", "Atlassian", "Dropbox", "GitHub", "GitLab", "Google", "Microsoft", "Slack", "Zoom"]
```

## Yapılandırma

### appsettings.json

Her vendor için API credential'ları `appsettings.json` dosyasına eklenmelidir:

```json
{
  "VendorAPIs": {
    "Microsoft": {
      "TenantId": "your-tenant-id",
      "ClientId": "your-client-id",
      "ClientSecret": "your-client-secret"
    },
    "Slack": {
      "BotToken": "xoxb-your-bot-token"
    },
    "GitHub": {
      "PersonalAccessToken": "ghp_your_token",
      "Organization": "your-org-name"
    }
  }
}
```

### Vendor API Credential'larını Alma

#### Microsoft Graph API
1. Azure Portal → App Registrations → New Registration
2. API Permissions → Microsoft Graph → Application Permissions
   - `Organization.Read.All`
   - `User.Read.All`
   - `Directory.Read.All`
3. Certificates & Secrets → New Client Secret
4. `TenantId`, `ClientId`, `ClientSecret` değerlerini kopyalayın

#### Slack
1. https://api.slack.com/apps → Create New App
2. OAuth & Permissions → Bot Token Scopes:
   - `team:read`
   - `admin.teams:read`
3. Install to Workspace
4. Bot User OAuth Token'ı kopyalayın

#### GitHub
1. Settings → Developer Settings → Personal Access Tokens → Generate New Token
2. Permissions: `admin:org`, `read:org`
3. Token'ı kopyalayın

#### Atlassian (Jira/Confluence)
1. https://id.atlassian.com/manage-profile/security/api-tokens
2. Create API Token
3. Email ve API Token kombinasyonunu kullanın

#### Google Workspace
1. Google Cloud Console → Create Service Account
2. Enable Admin SDK API
3. Create Key (JSON)
4. Domain-wide delegation ekleyin

#### Zoom
1. Zoom App Marketplace → Build App → Server-to-Server OAuth
2. Account ID, Client ID, Client Secret alın

#### Dropbox
1. https://www.dropbox.com/developers/apps → Create App
2. Dropbox Business API → Full Dropbox access
3. Generate Access Token

#### GitLab
1. Settings → Access Tokens → Add New Token
2. Scopes: `api`, `read_api`
3. Token'ı kopyalayın

## Frontend Kullanımı

### Senkronizasyon Butonu

Frontend'de "Vendor'ları Senkronize Et" butonu eklenmiştir:

```tsx
import { VendorSyncService } from "@/lib/api"

const handleSyncAll = async () => {
  const results = await VendorSyncService.syncAll()
  // Sonuçları işle
}
```

### API Service Fonksiyonları

```typescript
// Tüm vendor'ları senkronize et
await VendorSyncService.syncAll()

// Tek vendor senkronize et
await VendorSyncService.syncVendor("microsoft")

// Bağlantıları test et
await VendorSyncService.testAll()
await VendorSyncService.testVendor("slack")

// Vendor listesi al
await VendorSyncService.getVendors()
```

## Senkronizasyon Süreci

1. **API Call**: Backend ilgili vendor'ın API'sine bağlanır
2. **Authentication**: OAuth/Token ile kimlik doğrulama yapılır
3. **Data Fetch**: Lisans bilgileri çekilir
4. **Mapping**: Vendor-specific data → LicenseTracker formatına dönüştürülür
5. **Database Update**:
   - Yeni lisanslar eklenir (`LicensesAdded`)
   - Mevcut lisanslar güncellenir (`LicensesUpdated`)
6. **Result**: Sonuç frontend'e döndürülür

## Güvenlik

- **API Keys**: Production'da Azure Key Vault veya benzeri kullanın
- **appsettings.json**: Git'e commit etmeyin (.gitignore'a ekleyin)
- **Environment Variables**: Hassas bilgileri environment variable olarak tutun
- **HTTPS**: Tüm API iletişimi HTTPS üzerinden yapılmalı

## Örnek Kullanım Senaryosu

1. Admin kullanıcı frontend'e giriş yapar
2. "Vendor'ları Senkronize Et" butonuna tıklar
3. Backend tüm vendor'lardan eş zamanlı lisans bilgisi çeker:
   - Microsoft: 50 Office 365 lisansı bulundu
   - Slack: 35 aktif kullanıcı
   - GitHub: 25 Enterprise seat
4. Veritabanına kaydedilir
5. Frontend otomatik olarak güncellenir
6. Kullanıcı güncel lisans durumunu görebilir

## Sorun Giderme

### Bağlantı Testi Başarısız
```bash
# Test endpoint'ini kullanın
curl http://localhost:5100/api/vendorsync/test/microsoft
```

### API Credential Hataları
- `appsettings.json` içindeki credential'ları kontrol edin
- Vendor console'da API permission'ları doğrulayın
- Token'ların süresi dolmamış olduğundan emin olun

### Senkronizasyon Hataları
- Backend loglarını kontrol edin
- Swagger UI kullanarak manuel test yapın: http://localhost:5100/swagger

## Gelecek İyileştirmeler

- [ ] Otomatik senkronizasyon (cron job)
- [ ] Webhook desteği (vendor'dan push notification)
- [ ] Daha fazla vendor desteği (Salesforce, AWS, Azure)
- [ ] Lisans kullanım trendleri ve raporlama
- [ ] Maliyet optimizasyonu önerileri

## Swagger Dokümantasyonu

Tüm API endpoint'leri Swagger UI'da görülebilir:

```
http://localhost:5100/swagger
```

## Lisans

Bu proje MIT lisansı altındadır.
