using DinkToPdf;
using DinkToPdf.Contracts;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OficinaMVC.Data;
using OficinaMVC.Data.Entities;
using OficinaMVC.Data.Repositories;
using OficinaMVC.Helpers;
using OficinaMVC.Hubs;
using OficinaMVC.Services;
using System.Runtime.InteropServices;
using System.Text;

// --- DllImport for DinkToPdf ---
[DllImport("kernel32.dll", SetLastError = true)]
static extern IntPtr LoadLibrary(string lpFileName);

// --- Load DinkToPdf Native Library ---
string wkHtmlToPdfPath = Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll");
if (!File.Exists(wkHtmlToPdfPath))
{
    throw new FileNotFoundException(
        "Could not find the RENAMED wkhtmltox library (libwkhtmltox.dll) in the project root directory.",
        wkHtmlToPdfPath);
}
LoadLibrary(wkHtmlToPdfPath);

var builder = WebApplication.CreateBuilder(args);

// --- 1. Core Services Configuration ---
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure()
    ));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication()
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Error/403";
    })
    .AddJwtBearer(cfg =>
    {
        cfg.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Tokens:Issuer"],
            ValidAudience = builder.Configuration["Tokens:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Tokens:Key"]))
        };
    });

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

// --- 2. Background Job Configuration (Hangfire) ---
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));
builder.Services.AddHangfireServer();

// --- 3. Real-Time Communication (SignalR) ---
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, NameUserIdProvider>();

// --- 4. Application Services and Repositories ---
// Helpers
builder.Services.AddTransient<SeedDb>();
builder.Services.AddScoped<IUserHelper, UserHelper>();
builder.Services.AddScoped<IMailHelper, MailHelper>();
builder.Services.AddScoped<IImageHelper, ImageHelper>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

// Repositories
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<ICarModelRepository, CarModelRepository>();
builder.Services.AddScoped<IPartRepository, PartRepository>();
builder.Services.AddScoped<IRepairTypeRepository, RepairTypeRepository>();
builder.Services.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IRepairRepository, RepairRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IMechanicRepository, MechanicRepository>();

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IRepairService, RepairService>();
builder.Services.AddScoped<ICommunicationService, CommunicationService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IBulkEmailService, BulkEmailService>();
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddScoped<IViewRendererService, ViewRendererService>();
builder.Services.AddHttpClient();

// PDF Services
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
builder.Services.AddScoped<IPdfService, PdfService>();

//Stripe API key
Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// =================== BUILD THE APP ===================
var app = builder.Build();

// --- 5. Seeding ---
// Using a separate function to allow async call
await SeedDatabaseAsync(app);

// --- 6. HTTP Request Pipeline Configuration ---
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    // Use custom error handler for production
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Handles non-exception errors like 404, 403
app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// --- 7. Endpoint and Dashboard Mapping ---
app.UseHangfireDashboard("/hangfire", new DashboardOptions { Authorization = new[] { new HangfireAuthorizationFilter() } });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<NotificationHub>("/notificationHub");

// --- 8. Recurring Jobs ---
RecurringJob.AddOrUpdate<IReminderService>(
    "daily-appointment-reminders",
    service => service.SendAppointmentReminders(),
    "0 7 * * *", // Runs at 7:00 AM UTC every day
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Utc
    });

// =================== RUN THE APP ===================
app.Run();


// --- Helper function for seeding ---
static async Task SeedDatabaseAsync(IHost app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var seeder = services.GetRequiredService<SeedDb>();
        await seeder.SeedAsync();
    }
}

// --- Hangfire Authorization Filter ---
public class HangfireAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated ?? false;
    }
}