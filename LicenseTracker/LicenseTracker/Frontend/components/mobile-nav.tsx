"use client"

import { useState } from "react"
import Link from "next/link"
import { usePathname } from "next/navigation"
import { cn } from "@/lib/utils"
import {
  LayoutList,
  PlusCircle,
  LogOut,
  Menu,
  X,
  User,
  Settings,
  BarChart3,
} from "lucide-react"
import { Button } from "@/components/ui/button"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Sheet, SheetContent, SheetTrigger } from "@/components/ui/sheet"
import { Separator } from "@/components/ui/separator"

export function MobileNav() {
  const [open, setOpen] = useState(false)
  const pathname = usePathname()

  const navigationItems = [
    {
      title: "Lisans Listesi",
      href: "/",
      icon: LayoutList,
    },
    {
      title: "Yeni Lisans Ekle",
      href: "/add-license",
      icon: PlusCircle,
    },
    {
      title: "İstatistikler",
      href: "/statistics",
      icon: BarChart3,
    },
    {
      title: "Ayarlar",
      href: "/settings",
      icon: Settings,
    },
  ]

  return (
    <Sheet open={open} onOpenChange={setOpen}>
      <SheetTrigger asChild>
        <Button
          variant="ghost"
          size="icon"
          className="md:hidden"
          aria-label="Menüyü aç"
        >
          <Menu className="size-5" />
        </Button>
      </SheetTrigger>
      <SheetContent side="left" className="w-64 p-0">
        <div className="flex h-full flex-col">
          {/* Header */}
          <div className="flex h-16 items-center border-b border-border/50 px-4">
            <div className="flex items-center gap-2">
              <div className="flex size-8 items-center justify-center rounded-lg bg-gradient-to-br from-primary to-accent">
                <LayoutList className="size-4 text-primary-foreground" />
              </div>
              <div className="flex flex-col">
                <span className="text-sm font-semibold leading-none">
                  LicenseTracker
                </span>
                <span className="text-xs text-muted-foreground">v1.0</span>
              </div>
            </div>
          </div>

          {/* Navigation */}
          <nav className="flex-1 space-y-1 overflow-y-auto p-3">
            {navigationItems.map((item) => {
              const Icon = item.icon
              const isActive = pathname === item.href

              return (
                <Link
                  key={item.href}
                  href={item.href}
                  onClick={() => setOpen(false)}
                  className={cn(
                    "flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-all",
                    "hover:bg-accent/50 hover:text-accent-foreground",
                    isActive
                      ? "bg-accent text-accent-foreground shadow-sm"
                      : "text-muted-foreground"
                  )}
                >
                  <Icon className={cn("size-5 shrink-0", isActive && "text-primary")} />
                  <span>{item.title}</span>
                </Link>
              )
            })}
          </nav>

          <Separator className="bg-border/50" />

          {/* User Section */}
          <div className="border-t border-border/50 bg-card/30 p-3">
            <div className="space-y-3">
              <div className="flex items-center gap-3 rounded-lg bg-accent/20 p-2.5">
                <Avatar className="size-9">
                  <AvatarImage src="/avatar.png" alt="Kullanıcı" />
                  <AvatarFallback className="bg-gradient-to-br from-primary to-accent text-sm text-primary-foreground">
                    AK
                  </AvatarFallback>
                </Avatar>
                <div className="flex flex-1 flex-col overflow-hidden">
                  <span className="truncate text-sm font-semibold">
                    Ahmet Kaya
                  </span>
                  <span className="truncate text-xs text-muted-foreground">
                    ahmet@example.com
                  </span>
                </div>
              </div>

              <Button
                variant="outline"
                className="w-full justify-start gap-2 border-destructive/30 text-destructive hover:bg-destructive hover:text-destructive-foreground"
                size="sm"
              >
                <LogOut className="size-4" />
                <span>Çıkış Yap</span>
              </Button>
            </div>
          </div>
        </div>
      </SheetContent>
    </Sheet>
  )
}
