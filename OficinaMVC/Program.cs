using DinkToPdf;
using DinkToPdf.Contracts;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Data.Entities;
using OficinaMVC.Data.Repositories;
using OficinaMVC.Helpers;
using OficinaMVC.Hubs;
using OficinaMVC.Services;
using System.Runtime.InteropServices;

internal class Program
{
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr LoadLibrary(string lpFileName);

    private static async Task Main(string[] args)
    {
        string wkHtmlToPdfPath = Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll");
        if (!File.Exists(wkHtmlToPdfPath))
        {
            throw new FileNotFoundException(
                "Could not find the RENAMED wkhtmltox library (libwkhtmltox.dll) in the project root directory. Please ensure it has been copied, renamed, and set to 'Copy always'.",
                wkHtmlToPdfPath);
        }
        LoadLibrary(wkHtmlToPdfPath);

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<DataContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure()
            ));

        builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

        //Identity
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
            .AddCookie()
            .AddJwtBearer(cfg =>
            {
                cfg.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidIssuer = builder.Configuration["Tokens:Issuer"],
                    ValidAudience = builder.Configuration["Tokens:Audience"],
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Tokens:Key"]))
                };
            });

        // HANGFIRE SERVICE CONFIGURATION
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


        //Register services
        builder.Services.AddControllersWithViews();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        builder.Services.AddTransient<SeedDb>();
        builder.Services.AddScoped<IUserHelper, UserHelper>();
        builder.Services.AddScoped<IMailHelper, MailHelper>();
        builder.Services.AddScoped<IImageHelper, ImageHelper>();
        builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
        builder.Services.AddScoped<IMechanicRepository, MechanicRepository>();
        builder.Services.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
        builder.Services.AddScoped<IRepairTypeRepository, RepairTypeRepository>();
        builder.Services.AddScoped<IBrandRepository, BrandRepository>();
        builder.Services.AddScoped<ICarModelRepository, CarModelRepository>();
        builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        builder.Services.AddScoped<IPartRepository, PartRepository>();
        builder.Services.AddScoped<IRepairRepository, RepairRepository>();
        builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        builder.Services.AddScoped<IReminderService, ReminderService>();
        builder.Services.AddScoped<IViewRendererService, ViewRendererService>();
        builder.Services.AddScoped<IBulkEmailService, BulkEmailService>();

        builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
        builder.Services.AddScoped<IPdfService, PdfService>();
        builder.Services.AddSignalR();
        builder.Services.AddSingleton<IUserIdProvider, NameUserIdProvider>();

        var app = builder.Build();

        //Seeding
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var seeder = services.GetRequiredService<SeedDb>();
            await seeder.SeedAsync();
        }

        //HTTP pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            //app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        //app.UseStatusCodePagesWithReExecute("/Error/{0}");

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseHangfireDashboard("/hangfire", new DashboardOptions { });

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapHub<NotificationHub>("/notificationHub");

        app.MapHangfireDashboard();

        RecurringJob.AddOrUpdate<IReminderService>(
            "daily-appointment-reminders",
            service => service.SendAppointmentReminders(),
            "0 7 * * *",
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")
            });

        app.Run();
    }
}