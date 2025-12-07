# Sidebar (Yan Menü) Kullanım Kılavuzu

## Genel Bakış

Modern, profesyonel ve responsive bir sidebar (yan navigasyon çubuğu) eklendi. Sidebar hem masaüstü hem de mobil cihazlarda mükemmel çalışır.

## Özellikler

### 1. Masaüstü Görünümü (Desktop)
- **Konum**: Sayfanın sol tarafında sabit pozisyonda
- **Genişlik**: Normal modda 256px (w-64), daraltılmış modda 64px (w-16)
- **Daraltılabilir**: Sidebar üst kısmındaki ok butonu ile daraltılıp genişletilebilir
- **Görünürlük**: Tablet ve üzeri cihazlarda (`md:` breakpoint - 768px+) otomatik olarak gösterilir

### 2. Mobil Görünümü (Mobile)
- **Hamburger Menü**: Sayfa başlığının sol tarafında bir hamburger menü butonu
- **Sheet/Drawer**: Butona tıklandığında soldan açılır menü
- **Kapat**: Menü dışına tıklayarak veya bir link seçerek kapatılabilir

## Sidebar Bölümleri

### Üst Kısım - Logo ve Daraltma Butonu
```
┌─────────────────────────────┐
│ [Logo] LicenseTracker  [<]  │  ← Daraltma butonu
│        v1.0                  │
└─────────────────────────────┘
```

Daraltılmış halde:
```
┌──────┐
│ [<>] │  ← Logo ve buton
└──────┘
```

### Orta Kısım - Navigasyon Linkleri

Mevcut menü öğeleri:
1. **Lisans Listesi** (📋 LayoutList ikonu)
   - Route: `/`
   - Tüm lisansları görüntüleme sayfası

2. **Yeni Lisans Ekle** (➕ PlusCircle ikonu)
   - Route: `/add-license`
   - Yeni lisans ekleme formu

3. **İstatistikler** (📊 BarChart3 ikonu)
   - Route: `/statistics`
   - Dashboard ve istatistikler

4. **Ayarlar** (⚙️ Settings ikonu)
   - Route: `/settings`
   - Uygulama ayarları

### Alt Kısım - Kullanıcı Bilgileri

**Normal görünümde:**
- Kullanıcı avatarı (AK - Ahmet Kaya)
- Kullanıcı adı: Ahmet Kaya
- E-posta: ahmet@example.com
- Çıkış Yap butonu (kırmızı renkte)

**Daraltılmış görünümde:**
- Sadece avatar ikonu
- Hover yapınca tooltip ile bilgiler gösterilir

## Dosya Yapısı

```
Frontend/
├── components/
│   ├── sidebar.tsx           # Ana sidebar komponenti (masaüstü)
│   ├── mobile-nav.tsx        # Mobil navigasyon komponenti
│   └── ui/                   # Shadcn/ui bileşenleri
│       ├── button.tsx
│       ├── avatar.tsx
│       ├── tooltip.tsx
│       ├── separator.tsx
│       └── sheet.tsx
├── app/
│   ├── layout.tsx            # Sidebar'ı içeren ana layout
│   └── page.tsx              # Ana sayfa (mobile-nav ile)
```

## Kod Özellikleri

### Kullanılan Teknolojiler
- **Next.js 16** (App Router)
- **React 19**
- **Tailwind CSS 4**
- **Radix UI** (Primitives)
- **Lucide React** (İkonlar)
- **TypeScript**

### State Yönetimi
```typescript
const [collapsed, setCollapsed] = useState(false) // Sidebar daraltma durumu
const pathname = usePathname() // Aktif sayfa kontrolü
```

### Responsive Breakpoints
- `md:` (768px ve üzeri) - Sidebar gösterilir
- `< 768px` - Sidebar gizlenir, mobile nav gösterilir

## Nasıl Özelleştirilir?

### 1. Yeni Menü Öğesi Eklemek

`sidebar.tsx` ve `mobile-nav.tsx` dosyalarındaki `navigationItems` dizisine yeni öğe ekleyin:

```typescript
const navigationItems = [
  // Mevcut öğeler...
  {
    title: "Yeni Sayfa",
    href: "/yeni-sayfa",
    icon: IconName, // lucide-react'tan import edin
    description: "Yeni sayfa açıklaması", // Tooltip için
  },
]
```

### 2. Kullanıcı Bilgilerini Değiştirmek

`sidebar.tsx` dosyasında kullanıcı bölümünü bulun (satır ~158) ve güncelleyin:

```typescript
<span className="truncate text-sm font-semibold">
  {user.name} {/* Dinamik kullanıcı adı */}
</span>
<span className="truncate text-xs text-muted-foreground">
  {user.email} {/* Dinamik e-posta */}
</span>
```

**Örnek**: Authentication context ile entegrasyon:
```typescript
import { useAuth } from "@/hooks/use-auth"

export function Sidebar() {
  const { user } = useAuth() // Kullanıcı bilgilerini context'ten al

  // ...
}
```

### 3. Çıkış Yap Fonksiyonunu Bağlamak

```typescript
const handleLogout = async () => {
  // Logout API çağrısı
  await fetch("/api/auth/logout", { method: "POST" })
  // Redirect
  router.push("/login")
}

<Button onClick={handleLogout}>
  <LogOut className="size-4" />
  <span>Çıkış Yap</span>
</Button>
```

### 4. Renk Temasını Değiştirmek

Tailwind CSS değişkenlerini kullanarak tema renklerini `globals.css` dosyasında özelleştirin:

```css
:root {
  --primary: 210 100% 50%; /* Mavi */
  --accent: 330 100% 50%;  /* Pembe */
}
```

## Stil Detayları

### Gradient Efektleri
- Logo arka planı: `bg-gradient-to-br from-primary to-accent`
- Avatar fallback: Aynı gradient

### Blur ve Transparency
- Sidebar: `bg-card/50 backdrop-blur-xl`
- Header (sticky): `bg-card/80 backdrop-blur-xl`

### Hover Efektleri
- Menü öğeleri: `hover:bg-accent/50`
- Aktif öğe: `bg-accent shadow-sm`
- İkon rengi (aktif): `text-primary`

### Animasyonlar
- Sidebar genişlik değişimi: `transition-all duration-300 ease-in-out`
- Buton hover: `transition-all`

## Erişilebilirlik (Accessibility)

- ✅ Keyboard navigasyonu desteklenir
- ✅ ARIA labels (örn: `aria-label="Menüyü aç"`)
- ✅ Tooltip'ler daraltılmış modda ekstra bilgi sağlar
- ✅ Renk kontrastları WCAG AA standardına uygun
- ✅ Focus görünürlüğü

## Performans

- **Client-side only**: Sidebar `"use client"` directive ile işaretli
- **Lazy loading**: Tooltip'ler ihtiyaç duyulduğunda yüklenir
- **Optimized re-renders**: State değişiklikleri sadece gerekli componentleri yeniden render eder

## Bilinen Sorunlar ve Çözümler

### Sorun 1: Sheet (mobil menü) açılmıyor
**Çözüm**: `ui/sheet.tsx` componentinin doğru yüklendiğinden emin olun:
```bash
npx shadcn@latest add sheet
```

### Sorun 2: İkonlar görünmüyor
**Çözüm**: `lucide-react` yüklü mü kontrol edin:
```bash
npm install lucide-react
```

### Sorun 3: Responsive çalışmıyor
**Çözüm**: Tailwind config'de breakpoint'lerin doğru tanımlı olduğundan emin olun.

## Gelecek Geliştirmeler

Potansiyel iyileştirmeler:
- [ ] Multi-level menü (alt menüler)
- [ ] Arama kutusu (Command palette)
- [ ] Bildirim rozeti (notification badge)
- [ ] Tema değiştirici (light/dark mode toggle)
- [ ] Kullanıcı avatar'ına tıklayınca profil dropdown'ı
- [ ] Klavye kısayolları (Ctrl+B ile sidebar toggle)
- [ ] Favori sayfalar bölümü

## Projeyi Çalıştırma

```bash
# Frontend klasörüne gidin
cd Frontend

# Bağımlılıkları yükleyin (ilk seferde)
npm install

# Development server'ı başlatın
npm run dev

# Tarayıcıda açın
# http://localhost:3000
```

## Test Etme

### Masaüstü Testi
1. Tarayıcıyı tam ekran yapın
2. Sidebar sol tarafta görünmeli
3. Ok butonuna tıklayın - sidebar daraltılmalı
4. Menü öğelerine tıklayın - sayfa değişmeli

### Mobil Testi
1. Tarayıcı geliştirici araçlarını açın (F12)
2. Responsive mod'a geçin
3. Mobil cihaz seçin (iPhone, Android)
4. Hamburger menü butonu görünmeli
5. Butona tıklayın - menü soldan açılmalı
6. Bir link seçin - menü kapanmalı

## Destek ve Katkı

Sorularınız için: [GitHub Issues](https://github.com/yourusername/licensetracker/issues)

---

**Oluşturulma Tarihi**: 2025-11-02
**Geliştirici**: Claude Code
**Versiyon**: 1.0.0
