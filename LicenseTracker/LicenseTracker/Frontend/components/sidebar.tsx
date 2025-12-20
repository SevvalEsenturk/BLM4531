"use client"

import { useState, useEffect } from "react"
import Link from "next/link"
import { usePathname, useRouter } from "next/navigation"
import { cn } from "@/lib/utils"
import {
  LayoutList,
  PlusCircle,
  LogOut,
  ChevronLeft,
  ChevronRight,
  User,
  Settings,
  BarChart3,
} from "lucide-react"
import { Button } from "@/components/ui/button"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip"
import { Separator } from "@/components/ui/separator"
import { useAuthStore } from "@/lib/auth-store"

interface SidebarProps {
  className?: string
}

export function Sidebar({ className }: SidebarProps) {
  const [collapsed, setCollapsed] = useState(false)
  const pathname = usePathname()
  const router = useRouter()
  const { user, logout } = useAuthStore()
  const [mounted, setMounted] = useState(false)

  useEffect(() => {
    setMounted(true)
  }, [])

  const handleLogout = () => {
    logout()
    router.push('/login')
  }

  // Create initials from user name or fallback
  const firstInitial = user?.firstName ? user.firstName[0].toUpperCase() : "U"
  const lastInitial = user?.lastName ? user.lastName[0].toUpperCase() : "ser"
  const initials = `${firstInitial}${lastInitial}`
  const displayName = user ? `${user.firstName} ${user.lastName}` : "Kullanıcı"
  const displayEmail = user?.email || "user@example.com"

  const navigationItems = [
    {
      title: "Lisans Listesi",
      href: "/",
      icon: LayoutList,
      description: "Tüm lisansları görüntüle",
    },
    {
      title: "Yeni Lisans Ekle",
      href: "/add-license",
      icon: PlusCircle,
      description: "Yeni lisans kaydı oluştur",
    },
    {
      title: "İstatistikler",
      href: "/statistics",
      icon: BarChart3,
      description: "Lisans istatistiklerini gör",
    },
    {
      title: "Ayarlar",
      href: "/settings",
      icon: Settings,
      description: "Uygulama ayarları",
    },
  ]

  if (!mounted) return null; // Avoid hydration mismatch

  return (
    <TooltipProvider delayDuration={0}>
      <aside
        className={cn(
          "relative hidden h-screen flex-col border-r border-border/50 bg-card/50 backdrop-blur-xl transition-all duration-300 ease-in-out md:flex",
          collapsed ? "w-16" : "w-64",
          className
        )}
      >
        {/* Header - Logo ve Collapse Button */}
        <div className="flex h-16 items-center justify-between border-b border-border/50 px-4">
          {!collapsed && (
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
          )}

          <Button
            variant="ghost"
            size="icon"
            onClick={() => setCollapsed(!collapsed)}
            className={cn(
              "size-8 transition-transform",
              collapsed && "mx-auto"
            )}
          >
            {collapsed ? (
              <ChevronRight className="size-4" />
            ) : (
              <ChevronLeft className="size-4" />
            )}
          </Button>
        </div>

        {/* Navigation Links */}
        <nav className="flex-1 space-y-1 overflow-y-auto p-3">
          {navigationItems.map((item) => {
            const Icon = item.icon
            const isActive = pathname === item.href

            const linkContent = (
              <Link
                href={item.href}
                className={cn(
                  "flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-all",
                  "hover:bg-accent/50 hover:text-accent-foreground",
                  isActive
                    ? "bg-accent text-accent-foreground shadow-sm"
                    : "text-muted-foreground",
                  collapsed && "justify-center px-2"
                )}
              >
                <Icon className={cn("size-5 shrink-0", isActive && "text-primary")} />
                {!collapsed && <span className="truncate">{item.title}</span>}
              </Link>
            )

            if (collapsed) {
              return (
                <Tooltip key={item.href}>
                  <TooltipTrigger asChild>{linkContent}</TooltipTrigger>
                  <TooltipContent side="right" className="flex flex-col gap-1">
                    <span className="font-semibold">{item.title}</span>
                    <span className="text-xs text-muted-foreground">
                      {item.description}
                    </span>
                  </TooltipContent>
                </Tooltip>
              )
            }

            return <div key={item.href}>{linkContent}</div>
          })}
        </nav>

        <Separator className="bg-border/50" />

        {/* User Section - Alt Kısım */}
        <div className="border-t border-border/50 bg-card/30 p-3">
          {collapsed ? (
            <div className="flex flex-col items-center gap-2">
              <Tooltip>
                <TooltipTrigger asChild>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="size-10 rounded-full"
                  >
                    <Avatar className="size-8">
                      {/* <AvatarImage src="/avatar.png" alt="Kullanıcı" /> */}
                      <AvatarFallback className="bg-gradient-to-br from-primary to-accent text-xs text-primary-foreground">
                        {initials}
                      </AvatarFallback>
                    </Avatar>
                  </Button>
                </TooltipTrigger>
                <TooltipContent side="right">
                  <div className="flex flex-col gap-1">
                    <span className="font-semibold">{displayName}</span>
                    <span className="text-xs text-muted-foreground">
                      {displayEmail}
                    </span>
                  </div>
                </TooltipContent>
              </Tooltip>

              <Separator className="w-8 bg-border/50" />

              <Tooltip>
                <TooltipTrigger asChild>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="size-8 text-muted-foreground hover:text-destructive"
                    onClick={handleLogout}
                  >
                    <LogOut className="size-4" />
                  </Button>
                </TooltipTrigger>
                <TooltipContent side="right">
                  <span>Çıkış Yap</span>
                </TooltipContent>
              </Tooltip>
            </div>
          ) : (
            <div className="space-y-3">
              <div className="flex items-center gap-3 rounded-lg bg-accent/20 p-2.5">
                <Avatar className="size-9">
                  {/* <AvatarImage src="/avatar.png" alt="Kullanıcı" /> */}
                  <AvatarFallback className="bg-gradient-to-br from-primary to-accent text-sm text-primary-foreground">
                    {initials}
                  </AvatarFallback>
                </Avatar>
                <div className="flex flex-1 flex-col overflow-hidden">
                  <span className="truncate text-sm font-semibold">
                    {displayName}
                  </span>
                  <span className="truncate text-xs text-muted-foreground">
                    {displayEmail}
                  </span>
                </div>
              </div>

              <Button
                variant="outline"
                className="w-full justify-start gap-2 border-destructive/30 text-destructive hover:bg-destructive hover:text-destructive-foreground"
                size="sm"
                onClick={handleLogout}
              >
                <LogOut className="size-4" />
                <span>Çıkış Yap</span>
              </Button>
            </div>
          )}
        </div>
      </aside>
    </TooltipProvider>
  )
}
