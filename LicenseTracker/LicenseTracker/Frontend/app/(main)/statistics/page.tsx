"use client"

import { useEffect, useState } from "react"
import { BarChart3, TrendingUp, TrendingDown, DollarSign, Users, Package, AlertTriangle, CheckCircle2 } from "lucide-react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { LicenseService, type LicenseDto } from "@/lib/api"
import { Skeleton } from "@/components/ui/skeleton"

export default function StatisticsPage() {
  const [licenses, setLicenses] = useState<LicenseDto[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    fetchLicenses()
  }, [])

  const fetchLicenses = async () => {
    try {
      const data = await LicenseService.getAll()

      if (!Array.isArray(data)) {
        console.error("API response is not an array:", data)
        setLicenses([])
        return
      }

      setLicenses(data)
    } catch (error) {
      console.error("Failed to fetch licenses:", error)
    } finally {
      setLoading(false)
    }
  }

  // İstatistikleri hesapla
  const stats = {
    total: licenses.length,
    active: licenses.filter(l => l.hasLicense).length,
    inactive: licenses.filter(l => !l.hasLicense).length,
    expiringSoon: licenses.filter(l => l.hasLicense && l.remainingDays <= 30 && l.remainingDays > 0).length,
    expired: licenses.filter(l => l.hasLicense && l.remainingDays < 0).length,
    totalCost: licenses.reduce((sum, l) => sum + (l.cost || 0), 0),
    totalUsers: licenses.reduce((sum, l) => sum + (l.users || 0), 0),
  }

  // Kategoriye göre grupla
  const byCategory = licenses.reduce((acc, license) => {
    acc[license.category] = (acc[license.category] || 0) + 1
    return acc
  }, {} as Record<string, number>)

  // Vendor'a göre grupla
  const byVendor = licenses.reduce((acc, license) => {
    acc[license.vendor] = (acc[license.vendor] || 0) + 1
    return acc
  }, {} as Record<string, number>)

  const StatCard = ({
    title,
    value,
    description,
    icon: Icon,
    trend
  }: {
    title: string
    value: string | number
    description: string
    icon: any
    trend?: "up" | "down" | "neutral"
  }) => (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <CardTitle className="text-sm font-medium">{title}</CardTitle>
        <Icon className="size-4 text-muted-foreground" />
      </CardHeader>
      <CardContent>
        <div className="text-2xl font-bold">{value}</div>
        <p className="text-xs text-muted-foreground">
          {description}
        </p>
        {trend && (
          <div className={`mt-2 flex items-center text-xs ${trend === "up" ? "text-green-600" :
              trend === "down" ? "text-red-600" :
                "text-gray-600"
            }`}>
            {trend === "up" && <TrendingUp className="mr-1 size-3" />}
            {trend === "down" && <TrendingDown className="mr-1 size-3" />}
          </div>
        )}
      </CardContent>
    </Card>
  )

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-background via-background to-primary/5">
        <header className="sticky top-0 z-10 border-b border-border/50 bg-card/80 backdrop-blur-xl">
          <div className="px-4 py-6 md:px-8 lg:px-12">
            <div className="flex items-center gap-3">
              <div className="flex size-10 items-center justify-center rounded-xl bg-gradient-to-br from-primary to-accent">
                <BarChart3 className="size-5 text-primary-foreground" />
              </div>
              <div>
                <h1 className="text-balance text-2xl font-bold tracking-tight md:text-3xl">
                  İstatistikler
                </h1>
                <p className="mt-0.5 text-pretty text-xs text-muted-foreground md:text-sm">
                  Lisans yönetimi özet bilgileri
                </p>
              </div>
            </div>
          </div>
        </header>
        <div className="px-4 py-6 md:px-8 lg:px-12">
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
            {[...Array(4)].map((_, i) => (
              <Card key={i}>
                <CardHeader className="space-y-2">
                  <Skeleton className="h-4 w-24" />
                </CardHeader>
                <CardContent>
                  <Skeleton className="h-8 w-16" />
                  <Skeleton className="mt-2 h-3 w-32" />
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-background to-primary/5">
      {/* Header */}
      <header className="sticky top-0 z-10 border-b border-border/50 bg-card/80 backdrop-blur-xl">
        <div className="px-4 py-6 md:px-8 lg:px-12">
          <div className="flex items-center gap-3">
            <div className="flex size-10 items-center justify-center rounded-xl bg-gradient-to-br from-primary to-accent">
              <BarChart3 className="size-5 text-primary-foreground" />
            </div>
            <div>
              <h1 className="text-balance text-2xl font-bold tracking-tight md:text-3xl">
                İstatistikler
              </h1>
              <p className="mt-0.5 text-pretty text-xs text-muted-foreground md:text-sm">
                Lisans yönetimi özet bilgileri
              </p>
            </div>
          </div>
        </div>
      </header>

      {/* Content */}
      <div className="px-4 py-6 md:px-8 lg:px-12">
        <div className="space-y-6">
          {/* Genel İstatistikler */}
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
            <StatCard
              title="Toplam Lisans"
              value={stats.total}
              description="Sistemdeki tüm lisanslar"
              icon={Package}
            />
            <StatCard
              title="Aktif Lisans"
              value={stats.active}
              description={`${stats.inactive} pasif lisans`}
              icon={CheckCircle2}
              trend="up"
            />
            <StatCard
              title="Yıllık Maliyet"
              value={`₺${stats.totalCost.toLocaleString('tr-TR')}`}
              description="Toplam yıllık harcama"
              icon={DollarSign}
            />
            <StatCard
              title="Toplam Kullanıcı"
              value={stats.totalUsers}
              description="Lisans kullanan kişi sayısı"
              icon={Users}
            />
          </div>

          {/* Uyarılar */}
          <div className="grid gap-4 md:grid-cols-2">
            <Card className="border-orange-200 bg-orange-50">
              <CardHeader>
                <CardTitle className="flex items-center gap-2 text-orange-700">
                  <AlertTriangle className="size-5" />
                  Yakında Bitecek Lisanslar
                </CardTitle>
                <CardDescription>30 gün içinde süresi dolacak</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="text-3xl font-bold text-orange-700">{stats.expiringSoon}</div>
                <p className="mt-2 text-sm text-orange-600">
                  Bu lisansları yenilemeyi unutmayın!
                </p>
              </CardContent>
            </Card>

            <Card className="border-red-200 bg-red-50">
              <CardHeader>
                <CardTitle className="flex items-center gap-2 text-red-700">
                  <AlertTriangle className="size-5" />
                  Süresi Dolmuş Lisanslar
                </CardTitle>
                <CardDescription>Acil yenileme gerekiyor</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="text-3xl font-bold text-red-700">{stats.expired}</div>
                <p className="mt-2 text-sm text-red-600">
                  Bu lisanslar artık kullanılamıyor
                </p>
              </CardContent>
            </Card>
          </div>

          {/* Kategoriye Göre Dağılım */}
          <Card>
            <CardHeader>
              <CardTitle>Kategoriye Göre Dağılım</CardTitle>
              <CardDescription>Lisansların kategori bazlı analizi</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {Object.entries(byCategory).map(([category, count]) => (
                  <div key={category} className="flex items-center justify-between">
                    <div className="flex items-center gap-2">
                      <div className="size-2 rounded-full bg-primary" />
                      <span className="font-medium">{category}</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <div className="h-2 w-32 overflow-hidden rounded-full bg-muted">
                        <div
                          className="h-full bg-primary transition-all"
                          style={{ width: `${(count / stats.total) * 100}%` }}
                        />
                      </div>
                      <span className="w-12 text-right text-sm font-medium">{count}</span>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>

          {/* Vendor'a Göre Dağılım */}
          <Card>
            <CardHeader>
              <CardTitle>Satıcıya Göre Dağılım</CardTitle>
              <CardDescription>En çok kullanılan yazılım satıcıları</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {Object.entries(byVendor)
                  .sort(([, a], [, b]) => b - a)
                  .slice(0, 10)
                  .map(([vendor, count]) => (
                    <div key={vendor} className="flex items-center justify-between">
                      <div className="flex items-center gap-2">
                        <div className="size-2 rounded-full bg-accent" />
                        <span className="font-medium">{vendor}</span>
                      </div>
                      <div className="flex items-center gap-2">
                        <div className="h-2 w-32 overflow-hidden rounded-full bg-muted">
                          <div
                            className="h-full bg-accent transition-all"
                            style={{ width: `${(count / stats.total) * 100}%` }}
                          />
                        </div>
                        <span className="w-12 text-right text-sm font-medium">{count}</span>
                      </div>
                    </div>
                  ))}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
