"use client"

import { useState, useEffect } from "react"
import { Settings, User, Bell, Shield, Mail, Save, Check } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Switch } from "@/components/ui/switch"
import { Separator } from "@/components/ui/separator"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { toast } from "sonner"
import { useAuthStore } from "@/lib/auth-store"

export default function SettingsPage() {
  const { user } = useAuthStore()
  const [saving, setSaving] = useState(false)
  const [profileData, setProfileData] = useState({
    name: "",
    email: "",
    company: "",
  })

  useEffect(() => {
    if (user) {
      setProfileData({
        name: `${user.firstName} ${user.lastName}`,
        email: user.email || "",
        company: user.companyName || "Şirket bilgisi yok",
      })
    }
  }, [user])

  const [notifications, setNotifications] = useState({
    emailAlerts: true,
    expiryReminders: true,
    weeklyReports: false,
    newLicenseAlerts: true,
  })

  const handleSaveProfile = async () => {
    setSaving(true)
    // Simüle edilmiş kaydetme işlemi
    await new Promise(resolve => setTimeout(resolve, 1000))
    setSaving(false)
    toast.success("Profil bilgileri kaydedildi")
  }

  const handleSaveNotifications = async () => {
    setSaving(true)
    await new Promise(resolve => setTimeout(resolve, 1000))
    setSaving(false)
    toast.success("Bildirim ayarları güncellendi")
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-background to-primary/5">
      {/* Header */}
      <header className="sticky top-0 z-10 border-b border-border/50 bg-card/80 backdrop-blur-xl">
        <div className="px-4 py-6 md:px-8 lg:px-12">
          <div className="flex items-center gap-3">
            <div className="flex size-10 items-center justify-center rounded-xl bg-gradient-to-br from-primary to-accent">
              <Settings className="size-5 text-primary-foreground" />
            </div>
            <div>
              <h1 className="text-balance text-2xl font-bold tracking-tight md:text-3xl">
                Ayarlar
              </h1>
              <p className="mt-0.5 text-pretty text-xs text-muted-foreground md:text-sm">
                Hesap ve uygulama tercihlerinizi yönetin
              </p>
            </div>
          </div>
        </div>
      </header>

      {/* Content */}
      <div className="px-4 py-6 md:px-8 lg:px-12">
        <div className="mx-auto max-w-4xl space-y-6">
          {/* Profil Bilgileri */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <User className="size-5" />
                Profil Bilgileri
              </CardTitle>
              <CardDescription>
                Hesap bilgilerinizi görüntüleyin ve düzenleyin
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              {/* Avatar */}
              <div className="flex items-center gap-4">
                <Avatar className="size-20">
                  <AvatarImage src="/avatar.png" alt={profileData.name} />
                  <AvatarFallback className="bg-gradient-to-br from-primary to-accent text-lg text-primary-foreground">
                    {profileData.name.split(' ').map(n => n[0]).join('')}
                  </AvatarFallback>
                </Avatar>
                <div>
                  <Button variant="outline" size="sm">
                    Fotoğraf Değiştir
                  </Button>
                  <p className="mt-1 text-xs text-muted-foreground">
                    JPG, PNG veya GIF. Maksimum 2MB.
                  </p>
                </div>
              </div>

              <Separator />

              {/* Form */}
              <div className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="name">Ad Soyad</Label>
                  <Input
                    id="name"
                    value={profileData.name}
                    onChange={(e) => setProfileData({ ...profileData, name: e.target.value })}
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="email">E-posta</Label>
                  <Input
                    id="email"
                    type="email"
                    value={profileData.email}
                    onChange={(e) => setProfileData({ ...profileData, email: e.target.value })}
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="company">Şirket</Label>
                  <Input
                    id="company"
                    value={profileData.company}
                    onChange={(e) => setProfileData({ ...profileData, company: e.target.value })}
                  />
                </div>
              </div>

              <Button onClick={handleSaveProfile} disabled={saving} className="w-full sm:w-auto">
                {saving ? (
                  <>
                    <Check className="mr-2 size-4 animate-pulse" />
                    Kaydediliyor...
                  </>
                ) : (
                  <>
                    <Save className="mr-2 size-4" />
                    Değişiklikleri Kaydet
                  </>
                )}
              </Button>
            </CardContent>
          </Card>

          {/* Bildirim Ayarları */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Bell className="size-5" />
                Bildirim Tercihleri
              </CardTitle>
              <CardDescription>
                Hangi bildirimleri almak istediğinizi seçin
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="space-y-4">
                <div className="flex items-center justify-between rounded-lg border p-4">
                  <div className="space-y-0.5">
                    <div className="flex items-center gap-2">
                      <Mail className="size-4 text-muted-foreground" />
                      <Label htmlFor="emailAlerts" className="font-medium">
                        E-posta Bildirimleri
                      </Label>
                    </div>
                    <p className="text-sm text-muted-foreground">
                      Önemli güncellemeler için e-posta alın
                    </p>
                  </div>
                  <Switch
                    id="emailAlerts"
                    checked={notifications.emailAlerts}
                    onCheckedChange={(checked) =>
                      setNotifications({ ...notifications, emailAlerts: checked })
                    }
                  />
                </div>

                <div className="flex items-center justify-between rounded-lg border p-4">
                  <div className="space-y-0.5">
                    <div className="flex items-center gap-2">
                      <Bell className="size-4 text-muted-foreground" />
                      <Label htmlFor="expiryReminders" className="font-medium">
                        Süre Dolum Hatırlatıcıları
                      </Label>
                    </div>
                    <p className="text-sm text-muted-foreground">
                      Lisanslar dolmadan 30 gün önce hatırlat
                    </p>
                  </div>
                  <Switch
                    id="expiryReminders"
                    checked={notifications.expiryReminders}
                    onCheckedChange={(checked) =>
                      setNotifications({ ...notifications, expiryReminders: checked })
                    }
                  />
                </div>

                <div className="flex items-center justify-between rounded-lg border p-4">
                  <div className="space-y-0.5">
                    <div className="flex items-center gap-2">
                      <Mail className="size-4 text-muted-foreground" />
                      <Label htmlFor="weeklyReports" className="font-medium">
                        Haftalık Raporlar
                      </Label>
                    </div>
                    <p className="text-sm text-muted-foreground">
                      Her hafta lisans özeti raporu alın
                    </p>
                  </div>
                  <Switch
                    id="weeklyReports"
                    checked={notifications.weeklyReports}
                    onCheckedChange={(checked) =>
                      setNotifications({ ...notifications, weeklyReports: checked })
                    }
                  />
                </div>

                <div className="flex items-center justify-between rounded-lg border p-4">
                  <div className="space-y-0.5">
                    <div className="flex items-center gap-2">
                      <Bell className="size-4 text-muted-foreground" />
                      <Label htmlFor="newLicenseAlerts" className="font-medium">
                        Yeni Lisans Bildirimleri
                      </Label>
                    </div>
                    <p className="text-sm text-muted-foreground">
                      Yeni lisans eklendiğinde bildirim al
                    </p>
                  </div>
                  <Switch
                    id="newLicenseAlerts"
                    checked={notifications.newLicenseAlerts}
                    onCheckedChange={(checked) =>
                      setNotifications({ ...notifications, newLicenseAlerts: checked })
                    }
                  />
                </div>
              </div>

              <Button onClick={handleSaveNotifications} disabled={saving} className="w-full sm:w-auto">
                {saving ? (
                  <>
                    <Check className="mr-2 size-4 animate-pulse" />
                    Kaydediliyor...
                  </>
                ) : (
                  <>
                    <Save className="mr-2 size-4" />
                    Bildirimleri Güncelle
                  </>
                )}
              </Button>
            </CardContent>
          </Card>

          {/* Güvenlik */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Shield className="size-5" />
                Güvenlik
              </CardTitle>
              <CardDescription>
                Hesap güvenliğinizi yönetin
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label>Şifre</Label>
                <div className="flex gap-2">
                  <Input type="password" value="••••••••" disabled />
                  <Button variant="outline">Değiştir</Button>
                </div>
              </div>

              <Separator />

              <div className="rounded-lg border border-red-200 bg-red-50 p-4">
                <h4 className="mb-2 font-medium text-red-900">Tehlikeli Bölge</h4>
                <p className="mb-3 text-sm text-red-700">
                  Hesabınızı kalıcı olarak silmek istiyorsanız, aşağıdaki butona tıklayın. Bu işlem geri alınamaz.
                </p>
                <Button variant="destructive" size="sm">
                  Hesabı Sil
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
