using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OficinaMVC.Data;
using OficinaMVC.Data.Entities;
using OficinaMVC.Data.Repositories;
using OficinaMVC.Helpers;

internal class Program
{
    private static async Task Main(string[] args)
    {
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

        //Register services
        builder.Services.AddControllersWithViews();
        builder.Services.AddScoped<IWorkshopRepository, WorkshopRepository>();
        builder.Services.AddTransient<SeedDb>();
        builder.Services.AddScoped<IUserHelper, UserHelper>();
        builder.Services.AddScoped<IMailHelper, MailHelper>();
        builder.Services.AddScoped<IImageHelper, ImageHelper>();
        builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
        builder.Services.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
        builder.Services.AddScoped<IRepairTypeRepository, RepairTypeRepository>();
        var app = builder.Build();

        //Seeding
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var seeder = services.GetRequiredService<SeedDb>();
            await seeder.SeedAsync();
        }

        //HTTP pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}