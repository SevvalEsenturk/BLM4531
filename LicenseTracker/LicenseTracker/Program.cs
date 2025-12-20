using LicenseTracker.Data;
using LicenseTracker.Models;
using LicenseTracker.Services.Interfaces;
using LicenseTracker.Services.VendorServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// **SERVİSLERİN EKLEMESİ**

// 0. CORS politikası ekleniyor (Frontend için)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "http://localhost:3001",
                "https://localhost:3001",
                "http://localhost:3002",
                "https://localhost:3002")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 1. Entity Framework Core ve SQL Server bağlantısı
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// (NEW) JWT Service Dependency Injection
builder.Services.AddScoped<LicenseTracker.Services.JwtService>();

// Moved after Identity

// 2. ASP.NET Core Identity servisi ekleniyor
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Şifre gereksinimleri (örnek)
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// (NEW) JWT Authentication Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? ""))
    };
});

// Configure cookie to return 401 for API calls instead of redirecting to login
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

// 3. Frontend (Blazor/Razor Pages) Servisleri
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// 4. API'den veri çekmek için HttpClient servisi
builder.Services.AddHttpClient();

// 5. Vendor API Services (Lisans senkronizasyonu için)
builder.Services.AddScoped<IVendorLicenseService, MicrosoftGraphService>();
builder.Services.AddScoped<IVendorLicenseService, AdobeService>();
builder.Services.AddScoped<IVendorLicenseService, SlackService>();
builder.Services.AddScoped<IVendorLicenseService, AtlassianService>();
builder.Services.AddScoped<IVendorLicenseService, GoogleWorkspaceService>();
builder.Services.AddScoped<IVendorLicenseService, ZoomService>();
builder.Services.AddScoped<IVendorLicenseService, GitHubService>();
builder.Services.AddScoped<IVendorLicenseService, DropboxService>();
builder.Services.AddScoped<IVendorLicenseService, GitLabService>();

// 6. API Controller'lar ve Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// **HTTP İSTEK KANALININ YAPILANDIRILMASI**

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Veritabanı migration'larını uygula
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();

        // Test verileri ekle (ilk çalıştırmada)
        SeedData.Initialize(scope.ServiceProvider);
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // wwwroot içeriği için

app.UseRouting(); // Yönlendirme servisini etkinleştirir

// CORS middleware'i etkinleştir
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Blazor ve API Endpoint'lerinin Haritalanması
app.MapRazorPages();
app.MapBlazorHub();
app.MapControllers();

app.Run();