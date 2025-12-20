"use client"

import { useState, useEffect } from "react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import {
  AlertCircle,
  CheckCircle2,
  XCircle,
  Calendar,
  TrendingUp,
  Search,
  Filter,
  Bell,
  DollarSign,
  Users,
  Package,
  Loader2,
  RefreshCw,
} from "lucide-react"
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert"
import { LicenseService, VendorSyncService, type LicenseDto } from "@/lib/api"
import { useToast } from "@/hooks/use-toast"

interface License {
  id: string
  name: string
  vendor: string
  hasLicense: boolean
  startDate?: string
  endDate?: string
  cost?: number
  users?: number
  category: string
  companyId?: number
}

// Converter function to transform API data to component format
function convertLicenseDto(dto: LicenseDto): License {
  return {
    id: dto.id.toString(),
    name: dto.name,
    vendor: dto.vendor,
    hasLicense: dto.hasLicense,
    startDate: dto.startDate,
    endDate: dto.endDate,
    cost: dto.cost,
    users: dto.users,
    category: dto.category,
    companyId: dto.companyId,
  }
}

function getDaysUntilExpiry(endDate: string): number {
  const today = new Date()
  const expiry = new Date(endDate)
  const diffTime = expiry.getTime() - today.getTime()
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))
  return diffDays
}

function getLicenseStatus(license: License): "active" | "expiring" | "expired" | "none" {
  if (!license.hasLicense || !license.endDate) return "none"

  const daysUntilExpiry = getDaysUntilExpiry(license.endDate)

  if (daysUntilExpiry < 0) return "expired"
  if (daysUntilExpiry <= 30) return "expiring"
  return "active"
}

function formatDate(dateString: string): string {
  const date = new Date(dateString)
  return date.toLocaleDateString("tr-TR", {
    year: "numeric",
    month: "long",
    day: "numeric",
  })
}

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat("tr-TR", {
    style: "currency",
    currency: "TRY",
  }).format(amount)
}

export function LicenseTracker() {
  const [licenses, setLicenses] = useState<License[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [searchQuery, setSearchQuery] = useState("")
  const [filterStatus, setFilterStatus] = useState<"all" | "active" | "expiring" | "expired" | "none">("all")
  const [syncing, setSyncing] = useState(false)
  const { toast } = useToast()

  // Fetch licenses from API on component mount
  useEffect(() => {
    async function fetchLicenses() {
      try {
        setLoading(true)
        setError(null)
        const data = await LicenseService.getAll()

        if (!Array.isArray(data)) {
          console.error("API response is not an array:", data);
          setLicenses([]);
          return;
        }

        const convertedLicenses = data.map(convertLicenseDto)
        setLicenses(convertedLicenses)
      } catch (err) {
        console.error("Failed to fetch licenses:", err)
        setError("Lisanslar yüklenirken bir hata oluştu. Lütfen backend'in çalıştığından emin olun.")
      } finally {
        setLoading(false)
      }
    }

    fetchLicenses()
  }, [])

  // Sync all vendors
  const handleSyncAll = async () => {
    setSyncing(true)
    try {
      const results = await VendorSyncService.syncAll()

      const successCount = results.filter(r => r.success).length
      const totalAdded = results.reduce((sum, r) => sum + r.licensesAdded, 0)
      const totalUpdated = results.reduce((sum, r) => sum + r.licensesUpdated, 0)

      toast({
        title: "Senkronizasyon Tamamlandı",
        description: `${successCount}/${results.length} vendor başarıyla senkronize edildi. ${totalAdded} yeni lisans eklendi, ${totalUpdated} lisans güncellendi.`,
      })

      // Refresh licenses
      const data = await LicenseService.getAll()
      const convertedLicenses = data.map(convertLicenseDto)
      setLicenses(convertedLicenses)
    } catch (err) {
      console.error("Sync failed:", err)
      toast({
        title: "Senkronizasyon Hatası",
        description: "Lisanslar senkronize edilemedi. API ayarlarınızı kontrol edin.",
        variant: "destructive",
      })
    } finally {
      setSyncing(false)
    }
  }

  const filteredLicenses = licenses.filter((license) => {
    const matchesSearch =
      license.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      license.vendor.toLowerCase().includes(searchQuery.toLowerCase()) ||
      license.category.toLowerCase().includes(searchQuery.toLowerCase())

    const status = getLicenseStatus(license)
    const matchesFilter = filterStatus === "all" || status === filterStatus

    return matchesSearch && matchesFilter
  })

  const expiringLicenses = licenses.filter((license) => getLicenseStatus(license) === "expiring")
  const activeLicenses = licenses.filter((license) => license.hasLicense).length
  const totalLicenses = licenses.length
  const totalCost = licenses.reduce((sum, license) => sum + (license.cost || 0), 0)
  const totalUsers = licenses.reduce((sum, license) => sum + (license.users || 0), 0)

  // Show loading state
  if (loading) {
    return (
      <div className="flex min-h-[400px] items-center justify-center">
        <div className="text-center">
          <Loader2 className="mx-auto size-12 animate-spin text-primary" />
          <p className="mt-4 text-lg text-muted-foreground">Lisanslar yükleniyor...</p>
        </div>
      </div>
    )
  }

  // Show error state
  if (error) {
    return (
      <div className="space-y-4">
        <Alert className="border-destructive/50 bg-destructive/5">
          <AlertCircle className="size-5 text-destructive" />
          <AlertTitle className="font-heading text-destructive">Hata</AlertTitle>
          <AlertDescription className="text-sm text-muted-foreground">{error}</AlertDescription>
        </Alert>
        <Button onClick={() => window.location.reload()} variant="outline">
          Yeniden Dene
        </Button>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card className="card-hover glass-effect">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Toplam Lisans</CardTitle>
            <div className="flex size-10 items-center justify-center rounded-lg bg-primary/10">
              <Package className="size-5 text-primary" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="font-heading text-3xl font-bold">{totalLicenses}</div>
            <p className="mt-1 text-xs text-muted-foreground">Takip edilen lisans</p>
          </CardContent>
        </Card>

        <Card className="card-hover glass-effect">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Aktif Lisanslar</CardTitle>
            <div className="flex size-10 items-center justify-center rounded-lg bg-success/10">
              <CheckCircle2 className="size-5 text-success" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="font-heading text-3xl font-bold">{activeLicenses}</div>
            <p className="mt-1 flex items-center gap-1 text-xs text-muted-foreground">
              <TrendingUp className="size-3" />
              {((activeLicenses / totalLicenses) * 100).toFixed(0)}% kullanımda
            </p>
          </CardContent>
        </Card>

        <Card className="card-hover glass-effect">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Yakında Dolacak</CardTitle>
            <div className="flex size-10 items-center justify-center rounded-lg bg-warning/10">
              <Bell className="size-5 text-warning" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="font-heading text-3xl font-bold">{expiringLicenses.length}</div>
            <p className="mt-1 text-xs text-muted-foreground">30 gün içinde</p>
          </CardContent>
        </Card>

        <Card className="card-hover glass-effect">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Toplam Maliyet</CardTitle>
            <div className="flex size-10 items-center justify-center rounded-lg bg-accent/10">
              <DollarSign className="size-5 text-accent" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="font-heading text-3xl font-bold">{formatCurrency(totalCost)}</div>
            <p className="mt-1 flex items-center gap-1 text-xs text-muted-foreground">
              <Users className="size-3" />
              {totalUsers} kullanıcı
            </p>
          </CardContent>
        </Card>
      </div>

      {expiringLicenses.length > 0 && (
        <Alert className="glass-effect border-warning/50 bg-warning/5">
          <AlertCircle className="size-5 text-warning" />
          <AlertTitle className="font-heading text-warning">Dikkat Gerekli</AlertTitle>
          <AlertDescription className="text-sm text-muted-foreground">
            <span className="font-semibold text-warning">{expiringLicenses.length} lisansınız</span> 30 gün içinde sona
            erecek. Yenileme işlemlerini zamanında yapmanızı öneririz.
          </AlertDescription>
        </Alert>
      )}

      <Card className="glass-effect">
        <CardHeader>
          <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
            <div>
              <CardTitle className="font-heading">Lisans Detayları</CardTitle>
              <CardDescription>Tüm lisanslarınızı görüntüleyin ve yönetin</CardDescription>
            </div>
            <div className="flex flex-col gap-2 sm:flex-row">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  placeholder="Lisans ara..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="pl-9 md:w-64"
                />
              </div>
              <div className="flex gap-2">
                <Button
                  variant="default"
                  size="sm"
                  onClick={handleSyncAll}
                  disabled={syncing}
                  className="gap-2"
                >
                  <RefreshCw className={`size-4 ${syncing ? "animate-spin" : ""}`} />
                  {syncing ? "Senkronize Ediliyor..." : "Vendor'ları Senkronize Et"}
                </Button>
                <Button
                  variant={filterStatus === "all" ? "default" : "outline"}
                  size="sm"
                  onClick={() => setFilterStatus("all")}
                >
                  Tümü
                </Button>
                <Button
                  variant={filterStatus === "active" ? "default" : "outline"}
                  size="sm"
                  onClick={() => setFilterStatus("active")}
                >
                  Aktif
                </Button>
                <Button
                  variant={filterStatus === "expiring" ? "default" : "outline"}
                  size="sm"
                  onClick={() => setFilterStatus("expiring")}
                >
                  Dolacak
                </Button>
              </div>
            </div>
          </div>
        </CardHeader>
      </Card>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {filteredLicenses.map((license) => {
          const status = getLicenseStatus(license)
          const daysUntilExpiry = license.endDate ? getDaysUntilExpiry(license.endDate) : null

          return (
            <Card key={license.id} className="card-hover glass-effect group relative overflow-hidden">
              <div
                className={`absolute left-0 top-0 h-1 w-full ${status === "active"
                    ? "bg-success"
                    : status === "expiring"
                      ? "bg-warning"
                      : status === "expired"
                        ? "bg-destructive"
                        : "bg-muted"
                  }`}
              />

              <CardHeader className="pb-3">
                <div className="flex items-start justify-between gap-2">
                  <div className="flex-1 space-y-1">
                    <CardTitle className="font-heading text-lg leading-tight">{license.name}</CardTitle>
                    <CardDescription className="text-xs">{license.vendor}</CardDescription>
                  </div>
                  {status === "active" && (
                    <Badge className="bg-success/10 text-success hover:bg-success/20">
                      <CheckCircle2 className="mr-1 size-3" />
                      Aktif
                    </Badge>
                  )}
                  {status === "expiring" && (
                    <Badge className="bg-warning/10 text-warning hover:bg-warning/20">
                      <AlertCircle className="mr-1 size-3" />
                      Dolacak
                    </Badge>
                  )}
                  {status === "expired" && (
                    <Badge className="bg-destructive/10 text-destructive hover:bg-destructive/20">
                      <XCircle className="mr-1 size-3" />
                      Doldu
                    </Badge>
                  )}
                  {status === "none" && (
                    <Badge variant="secondary">
                      <XCircle className="mr-1 size-3" />
                      Yok
                    </Badge>
                  )}
                </div>
                <Badge variant="outline" className="w-fit text-xs">
                  {license.category}
                </Badge>
              </CardHeader>

              <CardContent className="space-y-3">
                {license.hasLicense && license.endDate ? (
                  <>
                    <div className="space-y-2 rounded-lg bg-muted/30 p-3">
                      <div className="flex items-center justify-between text-sm">
                        <span className="flex items-center gap-1.5 text-muted-foreground">
                          <Calendar className="size-3.5" />
                          Başlangıç
                        </span>
                        <span className="font-medium">{license.startDate && formatDate(license.startDate)}</span>
                      </div>
                      <div className="flex items-center justify-between text-sm">
                        <span className="flex items-center gap-1.5 text-muted-foreground">
                          <Calendar className="size-3.5" />
                          Bitiş
                        </span>
                        <span className="font-medium">{formatDate(license.endDate)}</span>
                      </div>
                    </div>

                    {daysUntilExpiry !== null && daysUntilExpiry >= 0 && (
                      <div className="flex items-center justify-between rounded-lg bg-muted/30 p-3 text-sm">
                        <span className="text-muted-foreground">Kalan Süre</span>
                        <span
                          className={`font-heading text-lg font-bold ${daysUntilExpiry <= 30 ? "text-warning" : "text-success"
                            }`}
                        >
                          {daysUntilExpiry} gün
                        </span>
                      </div>
                    )}

                    {daysUntilExpiry !== null && daysUntilExpiry < 0 && (
                      <div className="flex items-center justify-between rounded-lg bg-destructive/10 p-3 text-sm">
                        <span className="text-muted-foreground">Durum</span>
                        <span className="font-semibold text-destructive">
                          {Math.abs(daysUntilExpiry)} gün önce doldu
                        </span>
                      </div>
                    )}

                    {license.cost && (
                      <div className="flex items-center justify-between border-t border-border/50 pt-3 text-sm">
                        <span className="flex items-center gap-1.5 text-muted-foreground">
                          <DollarSign className="size-3.5" />
                          Yıllık Maliyet
                        </span>
                        <span className="font-heading font-semibold">{formatCurrency(license.cost)}</span>
                      </div>
                    )}

                    {license.users && (
                      <div className="flex items-center justify-between text-sm">
                        <span className="flex items-center gap-1.5 text-muted-foreground">
                          <Users className="size-3.5" />
                          Kullanıcı Sayısı
                        </span>
                        <span className="font-medium">{license.users}</span>
                      </div>
                    )}
                  </>
                ) : (
                  <div className="rounded-lg bg-muted/30 p-4 text-center">
                    <XCircle className="mx-auto mb-2 size-8 text-muted-foreground/50" />
                    <p className="text-sm text-muted-foreground">Bu yazılım için aktif lisans bulunmamaktadır.</p>
                  </div>
                )}
              </CardContent>
            </Card>
          )
        })}
      </div>

      {filteredLicenses.length === 0 && (
        <Card className="glass-effect">
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Filter className="mb-4 size-12 text-muted-foreground/50" />
            <p className="text-lg font-medium text-muted-foreground">Lisans bulunamadı</p>
            <p className="mt-1 text-sm text-muted-foreground">Arama kriterlerinizi değiştirmeyi deneyin</p>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
