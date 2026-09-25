using System.Text;
using Microsoft.OpenApi;
using MediCore.Api.Data;
using MediCore.Api.Models;
using MediCore.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (
    string.IsNullOrWhiteSpace(jwtKey) ||
    string.IsNullOrWhiteSpace(jwtIssuer) ||
    string.IsNullOrWhiteSpace(jwtAudience)
)
{
    throw new InvalidOperationException(
        "JWT settings are missing. Check Manage User Secrets."
    );
}

builder.Services.AddControllers();

//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Paste your JWT access token here."
        }
    );

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        }
    );
});

builder.Services.AddDbContext<MediCoreDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<MediCoreDbContext>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMobileApp", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddHostedService<DocumentCleanupService>();

builder.Services.AddSingleton<OpenAiChatService>();

builder.Services.AddScoped<JwtTokenService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();

    var roles = new[] { "Patient", "Doctor", "Admin" };

    foreach (var role in roles)
    {
        var roleExists = await roleManager.RoleExistsAsync(role);

        if (roleExists)
        {
            continue;
        }

        var result = await roleManager.CreateAsync(
            new IdentityRole(role)
        );

        if (!result.Succeeded)
        {
            var errors = string.Join(
                ", ",
                result.Errors.Select(error => error.Description)
            );

            throw new InvalidOperationException(
                $"Could not create role '{role}': {errors}"
            );
        }
    }
}

//using (var scope = app.Services.CreateScope())
//{
//    var roleManager = scope.ServiceProvider
//        .GetRequiredService<RoleManager<IdentityRole>>();

//    var roles = new[] { "Patient", "Doctor", "Admin" };

//    foreach (var role in roles)
//    {
//        var roleExists = await roleManager.RoleExistsAsync(role);

//        if (!roleExists)
//        {
//            var result = await roleManager.CreateAsync(
//                new IdentityRole(role)
//            );

//            if (!result.Succeeded)
//            {
//                throw new InvalidOperationException(
//                    $"Could not create role: {role}"
//                );
//            }
//        }
//    }
//}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Keep HTTP for local phone testing.
// app.UseHttpsRedirection();

app.UseCors("AllowMobileApp");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();