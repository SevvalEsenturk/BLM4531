import { LicenseTracker } from "@/components/license-tracker"
import { MobileNav } from "@/components/mobile-nav"
import { Sparkles } from "lucide-react"

export default function Home() {
  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-background to-primary/5">
      {/* Header */}
      <header className="sticky top-0 z-10 border-b border-border/50 bg-card/80 backdrop-blur-xl">
        <div className="px-4 py-6 md:px-8 lg:px-12">
          <div className="flex items-center gap-3">
            {/* Mobile Menu Button */}
            <MobileNav />

            <div className="flex size-10 items-center justify-center rounded-xl bg-gradient-to-br from-primary to-accent">
              <Sparkles className="size-5 text-primary-foreground" />
            </div>
            <div>
              <h1 className="text-balance text-2xl font-bold tracking-tight md:text-3xl">
                Lisans Listesi
              </h1>
              <p className="mt-0.5 text-pretty text-xs text-muted-foreground md:text-sm">
                Tüm yazılım lisanslarınızı profesyonel bir şekilde takip edin
              </p>
            </div>
          </div>
        </div>
      </header>

      {/* Content */}
      <div className="px-4 py-6 md:px-8 lg:px-12">
        <LicenseTracker />
      </div>
    </div>
  )
}
