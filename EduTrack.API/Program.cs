using System.Text;
using EduTrack.API.Authorization;
using QuestPDF.Infrastructure;
using EduTrack.API.Data;
using EduTrack.API.Helpers;
using EduTrack.API.Hubs;
using EduTrack.API.Models;
using EduTrack.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddSignalR();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<DbSeeder>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAccessControlService, AccessControlService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<INotificationRealtimeService, NotificationRealtimeService>();

builder.Services.AddDbContext<EduTrackDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = false;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<EduTrackDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppPolicies.CanManageUsers, p => p.RequireClaim("permission", AppPermissions.UsersManage));
    options.AddPolicy(AppPolicies.CanManageRoles, p => p.RequireClaim("permission", AppPermissions.RolesManage));
    options.AddPolicy(AppPolicies.CanConfigureSystem, p => p.RequireClaim("permission", AppPermissions.SystemConfigure));
    options.AddPolicy(AppPolicies.CanViewStudents, p => p.RequireClaim("permission", AppPermissions.StudentsView));
    options.AddPolicy(AppPolicies.CanEditStudents, p => p.RequireClaim("permission", AppPermissions.StudentsEdit));
    options.AddPolicy(AppPolicies.CanViewOwnStudents, p => p.RequireClaim("permission", AppPermissions.StudentsViewOwn));
    options.AddPolicy(AppPolicies.CanViewScores, p => p.RequireClaim("permission", AppPermissions.ScoresView));
    options.AddPolicy(AppPolicies.CanEditScores, p => p.RequireClaim("permission", AppPermissions.ScoresEdit));
    options.AddPolicy(AppPolicies.CanManageFinance, p => p.RequireClaim("permission", AppPermissions.FinanceManage));
    options.AddPolicy(AppPolicies.CanViewFinance, p => p.RequireClaim("permission", AppPermissions.FinanceView));
    options.AddPolicy(AppPolicies.CanSendNotifications, p => p.RequireClaim("permission", AppPermissions.NotificationsSend));
    options.AddPolicy(AppPolicies.CanViewDashboard, p => p.RequireClaim("permission", AppPermissions.DashboardView));
    options.AddPolicy(AppPolicies.CanViewReports, p => p.RequireClaim("permission", AppPermissions.ReportsView));
    options.AddPolicy(AppPolicies.CanViewTeachers, p => p.RequireClaim("permission", AppPermissions.TeachersView));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(10),
            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments(NotificationHub.HubPath))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAsync();

    var db = scope.ServiceProvider.GetRequiredService<EduTrackDbContext>();
    var needPatch = await db.HocSinhs.Where(h => h.TrangThai == "").ToListAsync();
    if (needPatch.Count > 0)
    {
        needPatch.ForEach(h => h.TrangThai = "Đang học");
        await db.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>(NotificationHub.HubPath);

app.Run();
