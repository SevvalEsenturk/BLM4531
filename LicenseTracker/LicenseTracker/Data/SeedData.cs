using LicenseTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace LicenseTracker.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new AppDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>()))
            {
                Console.WriteLine("Seeding: Checking database status...");

                // 1. Check/Seed Company
                var company = context.Companies.FirstOrDefault(c => c.Name == "Demo Şirketi A.Ş.");
                if (company == null)
                {
                    Console.WriteLine("Seeding: Creating Demo Company...");
                    company = new Company
                    {
                        Name = "Demo Şirketi A.Ş.",
                        ApiKeyVaultReference = "demo-api-key-vault"
                    };
                    context.Companies.Add(company);
                    context.SaveChanges();
                    Console.WriteLine($"Seeding: Company created with ID {company.Id}");
                }
                else
                {
                    Console.WriteLine($"Seeding: Company already exists with ID {company.Id}");
                }

                // 2. Check/Seed Licenses
                if (context.Licenses.Any())
                {
                    Console.WriteLine("Seeding: Licenses already exist. Skipping license seed.");
                    return;
                }

                Console.WriteLine("Seeding: Creating Licenses...");
                var licenses = new List<License>
                {
                    new License
                    {
                        Name = "Microsoft Office 365",
                        Vendor = "Microsoft",
                        Category = "Productivity",
                        CompanyId = company.Id,
                        HasLicense = true,
                        StartDate = new DateTime(2024, 1, 15),
                        EndDate = new DateTime(2025, 1, 15),
                        Cost = 12000,
                        Users = 50
                    },
                    new License
                    {
                        Name = "Adobe Creative Cloud",
                        Vendor = "Adobe",
                        Category = "Design",
                        CompanyId = company.Id,
                        HasLicense = true,
                        StartDate = new DateTime(2024, 3, 1),
                        EndDate = new DateTime(2024, 12, 15),
                        Cost = 8500,
                        Users = 15
                    },
                    new License
                    {
                        Name = "Slack Enterprise",
                        Vendor = "Slack",
                        Category = "Communication",
                        CompanyId = company.Id,
                        HasLicense = true,
                        StartDate = new DateTime(2024, 6, 1),
                        EndDate = new DateTime(2025, 6, 1),
                        Cost = 6000,
                        Users = 100
                    },
                    new License
                    {
                        Name = "GitHub Enterprise",
                        Vendor = "GitHub",
                        Category = "Development",
                        CompanyId = company.Id,
                        HasLicense = false
                    },
                    new License
                    {
                        Name = "Figma Professional",
                        Vendor = "Figma",
                        Category = "Design",
                        CompanyId = company.Id,
                        HasLicense = true,
                        StartDate = new DateTime(2024, 2, 10),
                        EndDate = new DateTime(2025, 2, 10),
                        Cost = 4500,
                        Users = 20
                    },
                    new License
                    {
                        Name = "Zoom Business",
                        Vendor = "Zoom",
                        Category = "Communication",
                        CompanyId = company.Id,
                        HasLicense = true,
                        StartDate = new DateTime(2024, 4, 20),
                        EndDate = new DateTime(2024, 11, 20),
                        Cost = 3000,
                        Users = 50
                    },
                    new License
                    {
                        Name = "Jira Software",
                        Vendor = "Atlassian",
                        Category = "Project Management",
                        CompanyId = company.Id,
                        HasLicense = false
                    },
                    new License
                    {
                        Name = "Salesforce",
                        Vendor = "Salesforce",
                        Category = "CRM",
                        CompanyId = company.Id,
                        HasLicense = true,
                        StartDate = new DateTime(2024, 1, 1),
                        EndDate = new DateTime(2025, 1, 1),
                        Cost = 15000,
                        Users = 30
                    },
                    new License
                    {
                        Name = "Dropbox Business",
                        Vendor = "Dropbox",
                        Category = "Storage",
                        CompanyId = company.Id,
                        HasLicense = false
                    },
                    new License
                    {
                        Name = "Notion Team",
                        Vendor = "Notion",
                        Category = "Productivity",
                        CompanyId = company.Id,
                        HasLicense = true,
                        StartDate = new DateTime(2024, 5, 15),
                        EndDate = new DateTime(2024, 12, 1),
                        Cost = 2400,
                        Users = 25
                    }
                };

                context.Licenses.AddRange(licenses);
                context.SaveChanges();
                Console.WriteLine($"Seeding: {licenses.Count} licenses added.");
            }
        }
    }
}
