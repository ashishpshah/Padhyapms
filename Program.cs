using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using PMMS.Infra;
using System.Globalization;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

        builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNameCaseInsensitive = false;
            options.SerializerOptions.PropertyNamingPolicy = null;
            options.SerializerOptions.WriteIndented = true;
        });

        builder.Services.AddHttpClient();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddDataProtection();

        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            var cultureInfo = new CultureInfo("en-IN");
            cultureInfo.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            cultureInfo.DateTimeFormat.LongDatePattern = "dd/MM/yyyy HH:mm:ss";

            var supportedCultures = new List<CultureInfo> { cultureInfo };

            options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-IN");

            options.DefaultRequestCulture.Culture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            options.DefaultRequestCulture.Culture.DateTimeFormat.LongDatePattern = "dd/MM/yyyy HH:mm:ss";

            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
        });

        builder.Services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(30); });

        builder.Services.AddDbContext<DataContext>(db => db.UseSqlServer(builder.Configuration.GetConnectionString("DataConnection")), ServiceLifetime.Singleton);

        builder.Services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestLineSize = 16 * 1024;
            options.Limits.MaxRequestHeadersTotalSize = 32 * 1024;
        });

        builder.Services.Configure<FormOptions>(o =>
        {
            o.ValueCountLimit = int.MaxValue;
            o.ValueLengthLimit = int.MaxValue;
            o.MultipartBodyLengthLimit = long.MaxValue;
        });

        var app = builder.Build();

        AppHttpContextAccessor.Configure(((IApplicationBuilder)app).ApplicationServices.GetRequiredService<IHttpContextAccessor>(), ((IApplicationBuilder)app).ApplicationServices.GetRequiredService<IHostEnvironment>(), builder.Environment, ((IApplicationBuilder)app).ApplicationServices.GetRequiredService<IDataProtectionProvider>(), ((IApplicationBuilder)app).ApplicationServices.GetRequiredService<IConfiguration>(), ((IApplicationBuilder)app).ApplicationServices.GetRequiredService<IHttpClientFactory>());

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRequestLocalization();

        app.UseRouting();

        // ✅ AUTH FIRST
        app.UseAuthentication();

        // ✅ SESSION AFTER AUTH
        app.UseSession();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}