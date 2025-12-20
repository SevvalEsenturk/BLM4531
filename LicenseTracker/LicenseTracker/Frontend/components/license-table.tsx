"use client"

import { useState, useMemo } from "react"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { ArrowUpDown, ArrowUp, ArrowDown, Search } from "lucide-react"
import type { License } from "@/lib/license-data"

interface LicenseTableProps {
  licenses: License[]
}

type SortField = "name" | "expirationDate" | "cost" | "status"
type SortDirection = "asc" | "desc" | null

export function LicenseTable({ licenses }: LicenseTableProps) {
  const [searchQuery, setSearchQuery] = useState("")
  const [statusFilter, setStatusFilter] = useState<string>("all")
  const [productFilter, setProductFilter] = useState<string>("all")
  const [sortField, setSortField] = useState<SortField | null>(null)
  const [sortDirection, setSortDirection] = useState<SortDirection>(null)

  // Get unique products for filter dropdown
  const uniqueProducts = useMemo(() => {
    const products = new Set(licenses.map((l) => l.product))
    return Array.from(products).sort()
  }, [licenses])

  // Handle sorting
  const handleSort = (field: SortField) => {
    if (sortField === field) {
      // Cycle through: asc -> desc -> null
      if (sortDirection === "asc") {
        setSortDirection("desc")
      } else if (sortDirection === "desc") {
        setSortDirection(null)
        setSortField(null)
      }
    } else {
      setSortField(field)
      setSortDirection("asc")
    }
  }

  // Filter and sort licenses
  const filteredAndSortedLicenses = useMemo(() => {
    let result = [...licenses]

    // Apply search filter
    if (searchQuery) {
      const query = searchQuery.toLowerCase()
      result = result.filter(
        (license) =>
          license.name.toLowerCase().includes(query) ||
          license.product.toLowerCase().includes(query) ||
          license.vendor.toLowerCase().includes(query),
      )
    }

    // Apply status filter
    if (statusFilter !== "all") {
      result = result.filter((license) => license.status === statusFilter)
    }

    // Apply product filter
    if (productFilter !== "all") {
      result = result.filter((license) => license.product === productFilter)
    }

    // Apply sorting
    if (sortField && sortDirection) {
      result.sort((a, b) => {
        let aValue: string | number | Date
        let bValue: string | number | Date

        switch (sortField) {
          case "name":
            aValue = a.name.toLowerCase()
            bValue = b.name.toLowerCase()
            break
          case "expirationDate":
            aValue = new Date(a.expirationDate).getTime()
            bValue = new Date(b.expirationDate).getTime()
            break
          case "cost":
            aValue = a.cost
            bValue = b.cost
            break
          case "status":
            aValue = a.status
            bValue = b.status
            break
          default:
            return 0
        }

        if (aValue < bValue) return sortDirection === "asc" ? -1 : 1
        if (aValue > bValue) return sortDirection === "asc" ? 1 : -1
        return 0
      })
    }

    return result
  }, [licenses, searchQuery, statusFilter, productFilter, sortField, sortDirection])

  const getSortIcon = (field: SortField) => {
    if (sortField !== field) {
      return <ArrowUpDown className="ml-2 h-4 w-4" />
    }
    if (sortDirection === "asc") {
      return <ArrowUp className="ml-2 h-4 w-4" />
    }
    return <ArrowDown className="ml-2 h-4 w-4" />
  }

  const getStatusBadgeVariant = (status: string) => {
    switch (status) {
      case "active":
        return "default"
      case "expiring":
        return "warning"
      case "expired":
        return "destructive"
      default:
        return "secondary"
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search licenses..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="pl-9"
          />
        </div>
        <div className="flex gap-2">
          <Select value={statusFilter} onValueChange={setStatusFilter}>
            <SelectTrigger className="w-[140px]">
              <SelectValue placeholder="Status" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Status</SelectItem>
              <SelectItem value="active">Active</SelectItem>
              <SelectItem value="expiring">Expiring</SelectItem>
              <SelectItem value="expired">Expired</SelectItem>
            </SelectContent>
          </Select>
          <Select value={productFilter} onValueChange={setProductFilter}>
            <SelectTrigger className="w-[160px]">
              <SelectValue placeholder="Product" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Products</SelectItem>
              {uniqueProducts.map((product) => (
                <SelectItem key={product} value={product}>
                  {product}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>

      <div className="rounded-lg border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>
                <Button
                  variant="ghost"
                  onClick={() => handleSort("name")}
                  className="-ml-4 h-auto p-2 hover:bg-transparent"
                >
                  License Name
                  {getSortIcon("name")}
                </Button>
              </TableHead>
              <TableHead>Product</TableHead>
              <TableHead>Vendor</TableHead>
              <TableHead>
                <Button
                  variant="ghost"
                  onClick={() => handleSort("expirationDate")}
                  className="-ml-4 h-auto p-2 hover:bg-transparent"
                >
                  Expiration Date
                  {getSortIcon("expirationDate")}
                </Button>
              </TableHead>
              <TableHead>
                <Button
                  variant="ghost"
                  onClick={() => handleSort("cost")}
                  className="-ml-4 h-auto p-2 hover:bg-transparent"
                >
                  Annual Cost
                  {getSortIcon("cost")}
                </Button>
              </TableHead>
              <TableHead>
                <Button
                  variant="ghost"
                  onClick={() => handleSort("status")}
                  className="-ml-4 h-auto p-2 hover:bg-transparent"
                >
                  Status
                  {getSortIcon("status")}
                </Button>
              </TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {filteredAndSortedLicenses.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} className="h-24 text-center">
                  No licenses found.
                </TableCell>
              </TableRow>
            ) : (
              filteredAndSortedLicenses.map((license) => (
                <TableRow key={license.id}>
                  <TableCell className="font-medium">{license.name}</TableCell>
                  <TableCell>{license.product}</TableCell>
                  <TableCell>{license.vendor}</TableCell>
                  <TableCell>{license.expirationDate}</TableCell>
                  <TableCell>${license.cost.toLocaleString()}</TableCell>
                  <TableCell>
                    <Badge variant={getStatusBadgeVariant(license.status)}>{license.status}</Badge>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      <div className="text-sm text-muted-foreground">
        Showing {filteredAndSortedLicenses.length} of {licenses.length} licenses
      </div>
    </div>
  )
}
