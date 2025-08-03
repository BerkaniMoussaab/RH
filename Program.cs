
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RH.Components;
using RH.Components.Account;
using RH.Data;
using RH.Services;
using System.Globalization;
using System.IO; // Required for Path.Combine

// Create the WebApplicationBuilder with options required for a Windows Service
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    // Set the content root to the application's base directory. This is CRITICAL for services.
    ContentRootPath = AppContext.BaseDirectory,
    EnvironmentName = Environments.Development
});

// Configure all services in a separate method for clarity
ConfigureServices(builder);

var app = builder.Build();

// Configure the application's request pipeline
await ConfigureAppAsync(app);

try
{
    // Use RunAsync to start the application without blocking the main thread.
    // This allows the service to report a successful start.
    await app.RunAsync();
}
catch (Exception ex)
{
    // Fallback logging in case of a catastrophic failure during startup.
    File.WriteAllText("C:\\RH2ServiceError.log", ex.ToString());
    throw;
}


// --- Configuration Methods ---

void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddRazorComponents().AddInteractiveServerComponents();
    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddScoped<IdentityUserAccessor>();
    builder.Services.AddScoped<IdentityRedirectManager>();
    builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    }).AddIdentityCookies();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
        throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();

    builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString,
            sqlServerOptions =>
            {
                sqlServerOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                sqlServerOptions.CommandTimeout(60);
            }).EnableSensitiveDataLogging());

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

    // Registering application-specific services
    builder.Services.AddScoped<IEmployeeService, EmployeeService>();
    builder.Services.AddScoped<IJobTitleService, JobTitleService>();
    builder.Services.AddScoped<AttendanceService>();
    builder.Services.AddScoped<PayrollAdjustmentRuleService>();
    builder.Services.AddScoped<IPayrollAdjustmentRuleService, PayrollAdjustmentRuleService>();
    builder.Services.AddScoped<ILeavePolicyService, LeavePolicyService>();
    builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
    builder.Services.AddScoped<AttestationGeneratorService>();
    builder.Services.AddScoped<AbsenceService>();
    builder.Services.AddScoped<RecoveryDayService>();
    builder.Services.AddScoped<WorkedDaysOffService>();
    builder.Services.AddScoped<CompanyInfoService>();
    builder.Services.AddScoped<IPayrollService, PayrollService>();
    builder.Services.AddScoped<IAdvanceService, AdvanceService>();
    builder.Services.AddScoped<IStatisticsService, StatisticsService>();

    // Note: Some services were registered multiple times (e.g., EmployeeService).
    // This is harmless but redundant. I've kept one registration for each.
    // If a service is registered with both its interface and class, ensure that's intended.
    // For example, IAdvanceService and AdvanceService are both registered.
    // If you resolve IAdvanceService, you get a different instance than if you resolve AdvanceService.
    // It's usually best to register with the interface and resolve with the interface.
    // For simplicity, I have left them as-is from your original file.

    builder.Services.AddAntiforgery(options =>
    {
        options.FormFieldName = "__RequestVerificationToken";
    });

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Listen(System.Net.IPAddress.Any, 70);
    });

    builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
    builder.Services.AddSingleton<ModalService>();

    // Conditionally configure the host to run as a Windows Service.
    // This allows the app to also be run directly from the command line for debugging.
    if (!Environment.UserInteractive)
    {
        builder.Host.UseWindowsService();
    }
}

async Task ConfigureAppAsync(WebApplication app)
{
    // It's best practice to run migrations once at startup.
    // The duplicate migration logic has been removed from the end of this method.
    try
    {
        using var scope = app.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        using var dbContext = factory.CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logFile = Path.Combine(AppContext.BaseDirectory, "RHServiceDbMigrationError.log");
        var message = $"[{DateTime.Now}] DB Migration error: {ex}";
        File.AppendAllText(logFile, message + Environment.NewLine);
        // Depending on requirements, you might want to re-throw or shut down.
        // For a service, logging the error and continuing might be acceptable if the app can run without a perfect DB state.
        // However, throwing here will stop the service from starting, which is often safer.
        throw;
    }


    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAntiforgery();

    app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

    // Add Identity endpoints
    app.MapAdditionalIdentityEndpoints();

    // Set default culture for the application
    var culture = new CultureInfo("fr-DZ");
    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;
}
