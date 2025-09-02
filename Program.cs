using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RH.Components;
using RH.Components.Account;
using RH.Data;
using RH.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = Environments.Development // or "Development" as a string
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Register the EmployeeService

builder.Services.AddDbContextFactory<ApplicationDbContext>(
    options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions =>
        {
            sqlServerOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlServerOptions.CommandTimeout(60);  // 60 seconds timeout
        })
    .EnableSensitiveDataLogging(),
    ServiceLifetime.Transient
);

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IJobTitleService, JobTitleService>();


builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<PayrollAdjustmentRuleService>();
builder.Services.AddScoped<JobTitleService>();
builder.Services.AddScoped<IPayrollAdjustmentRuleService, PayrollAdjustmentRuleService>();
builder.Services.AddScoped<ILeavePolicyService, LeavePolicyService>();
builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();

builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<AttestationGeneratorService>();
builder.Services.AddScoped<AbsenceService>();
builder.Services.AddScoped<RecoveryDayService>();
builder.Services.AddScoped<WorkedDaysOffService>();
builder.Services.AddScoped<CompanyInfoService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.AddScoped<IAdvanceService, AdvanceService>();
builder.Services.AddScoped<AdvanceService>();
builder.Services.AddScoped<PayrollService>();

builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddAntiforgery(options =>
{
    options.FormFieldName = "__RequestVerificationToken";
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(System.Net.IPAddress.Any, 70);
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
// Register the email sender
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddSingleton<ModalService>();
builder.Host.UseWindowsService();
// Configure HttpClient


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    using var dbContext = factory.CreateDbContext();
    dbContext.Database.Migrate();
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapIdentityApi<ApplicationUser>();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
// Required for .NET Windows Services

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();
var culture = new CultureInfo("fr-DZ"); // Or "fr-FR", "ar-DZ", etc.
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    /* await dbContext.Database.MigrateAsync(); */// Optional but good for dev

    if (!dbContext.Users.Any())
    {
        var adminUser = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@example.com",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, "Admin1234@");

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Failed to create default admin user: {errors}");
        }
    }
}
app.Run();
