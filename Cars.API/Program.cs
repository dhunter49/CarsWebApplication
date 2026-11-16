using Cars.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Cars.Application.Cars;
using Cars.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Cars.API.Middleware;
using System.Text;
using Cars.API.Services;
using Cars.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithOrigins("https://localhost:5173");
    }));

builder.Services
    .AddIdentityCore<AppUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Configuration["Jwt:Key"]
                     ?? throw new InvalidOperationException("Brak Jwt:Key.");
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.FromSeconds(30),
            };
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<ITokenService, TokenService>();


builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(GetCarsList.Handler).Assembly);
});

builder.Services.AddValidatorsFromAssemblyContaining<Create.CommandValidator>();

builder.Services.AddTransient(
        typeof(IPipelineBehavior<,>),
        typeof(ValidationBehavior<,>));

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("Smtp"));

builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseCors("CorsPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
    await Seed.SeedData(context);
    
    var userManager = services.GetRequiredService<
        UserManager<AppUser>>();

    var roleManager = services.GetRequiredService<
        RoleManager<IdentityRole>>();

    await IdentitySeed.SeedAsync(
        userManager,
        roleManager);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration.");
}

app.Run();