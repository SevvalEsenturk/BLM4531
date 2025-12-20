"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { PlusCircle, ArrowLeft, Loader2 } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Switch } from "@/components/ui/switch"
import { LicenseService } from "@/lib/api"
import { toast } from "sonner"
import Link from "next/link"

export default function AddLicensePage() {
  const router = useRouter()
  const [loading, setLoading] = useState(false)

  const [formData, setFormData] = useState({
    name: "",
    vendor: "",
    category: "Productivity",
    hasLicense: true,
    startDate: "",
    endDate: "",
    cost: "",
    users: "",
  })

  const categories = [
    "Productivity",
    "Development",
    "Design",
    "Security",
    "Communication",
    "Analytics",
    "Marketing",
    "Other"
  ]

  const vendors = [
    "Microsoft",
    "Adobe",
    "Google",
    "Atlassian",
    "Slack",
    "Zoom",
    "GitHub",
    "GitLab",
    "Dropbox",
    "Custom"
  ]

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoading(true)

    try {
      await LicenseService.create({
        name: formData.name,
        vendor: formData.vendor,
        category: formData.category,
        hasLicense: formData.hasLicense,
        startDate: formData.startDate || undefined,
        endDate: formData.endDate || undefined,
        cost: formData.cost ? parseFloat(formData.cost) : undefined,
        users: formData.users ? parseInt(formData.users) : undefined,
      })

      toast.success("Lisans başarıyla eklendi!")
      router.push("/")
    } catch (error) {
      toast.error("Lisans eklenirken hata oluştu")
      console.error(error)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-background to-primary/5">
      {/* Header */}
      <header className="sticky top-0 z-10 border-b border-border/50 bg-card/80 backdrop-blur-xl">
        <div className="px-4 py-6 md:px-8 lg:px-12">
          <div className="flex items-center gap-3">
            <Link href="/">
              <Button variant="ghost" size="icon">
                <ArrowLeft className="size-5" />
              </Button>
            </Link>
            <div className="flex size-10 items-center justify-center rounded-xl bg-gradient-to-br from-primary to-accent">
              <PlusCircle className="size-5 text-primary-foreground" />
            </div>
            <div>
              <h1 className="text-balance text-2xl font-bold tracking-tight md:text-3xl">
                Yeni Lisans Ekle
              </h1>
              <p className="mt-0.5 text-pretty text-xs text-muted-foreground md:text-sm">
                Yeni bir yazılım lisansı kaydedin
              </p>
            </div>
          </div>
        </div>
      </header>

      {/* Content */}
      <div className="px-4 py-6 md:px-8 lg:px-12">
        <div className="mx-auto max-w-2xl">
          <Card>
            <CardHeader>
              <CardTitle>Lisans Bilgileri</CardTitle>
              <CardDescription>
                Yeni lisans için gerekli bilgileri doldurun
              </CardDescription>
            </CardHeader>
            <CardContent>
              <form onSubmit={handleSubmit} className="space-y-6">
                {/* Lisans Adı */}
                <div className="space-y-2">
                  <Label htmlFor="name">Lisans Adı *</Label>
                  <Input
                    id="name"
                    placeholder="örn: Microsoft Office 365"
                    value={formData.name}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    required
                  />
                </div>

                {/* Vendor */}
                <div className="space-y-2">
                  <Label htmlFor="vendor">Satıcı/Vendor *</Label>
                  <Select
                    value={formData.vendor}
                    onValueChange={(value) => setFormData({ ...formData, vendor: value })}
                    required
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Satıcı seçin" />
                    </SelectTrigger>
                    <SelectContent>
                      {vendors.map((vendor) => (
                        <SelectItem key={vendor} value={vendor}>
                          {vendor}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                {/* Kategori */}
                <div className="space-y-2">
                  <Label htmlFor="category">Kategori *</Label>
                  <Select
                    value={formData.category}
                    onValueChange={(value) => setFormData({ ...formData, category: value })}
                    required
                  >
                    <SelectTrigger>
                      <SelectValue placeholder="Kategori seçin" />
                    </SelectTrigger>
                    <SelectContent>
                      {categories.map((category) => (
                        <SelectItem key={category} value={category}>
                          {category}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                {/* Lisans Durumu */}
                <div className="flex items-center justify-between rounded-lg border p-4">
                  <div className="space-y-0.5">
                    <Label htmlFor="hasLicense">Aktif Lisans</Label>
                    <p className="text-sm text-muted-foreground">
                      Bu ürün için aktif bir lisansınız var mı?
                    </p>
                  </div>
                  <Switch
                    id="hasLicense"
                    checked={formData.hasLicense}
                    onCheckedChange={(checked) => setFormData({ ...formData, hasLicense: checked })}
                  />
                </div>

                {/* Tarih Aralığı */}
                <div className="grid gap-4 md:grid-cols-2">
                  <div className="space-y-2">
                    <Label htmlFor="startDate">Başlangıç Tarihi</Label>
                    <Input
                      id="startDate"
                      type="date"
                      value={formData.startDate}
                      onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="endDate">Bitiş Tarihi</Label>
                    <Input
                      id="endDate"
                      type="date"
                      value={formData.endDate}
                      onChange={(e) => setFormData({ ...formData, endDate: e.target.value })}
                    />
                  </div>
                </div>

                {/* Maliyet ve Kullanıcı Sayısı */}
                <div className="grid gap-4 md:grid-cols-2">
                  <div className="space-y-2">
                    <Label htmlFor="cost">Yıllık Maliyet (TL)</Label>
                    <Input
                      id="cost"
                      type="number"
                      placeholder="0.00"
                      step="0.01"
                      min="0"
                      value={formData.cost}
                      onChange={(e) => setFormData({ ...formData, cost: e.target.value })}
                    />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="users">Kullanıcı Sayısı</Label>
                    <Input
                      id="users"
                      type="number"
                      placeholder="0"
                      min="0"
                      value={formData.users}
                      onChange={(e) => setFormData({ ...formData, users: e.target.value })}
                    />
                  </div>
                </div>

                {/* Butonlar */}
                <div className="flex gap-3">
                  <Button
                    type="submit"
                    className="flex-1"
                    disabled={loading || !formData.name || !formData.vendor}
                  >
                    {loading ? (
                      <>
                        <Loader2 className="mr-2 size-4 animate-spin" />
                        Kaydediliyor...
                      </>
                    ) : (
                      <>
                        <PlusCircle className="mr-2 size-4" />
                        Lisans Ekle
                      </>
                    )}
                  </Button>
                  <Button
                    type="button"
                    variant="outline"
                    onClick={() => router.push("/")}
                    disabled={loading}
                  >
                    İptal
                  </Button>
                </div>
              </form>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
