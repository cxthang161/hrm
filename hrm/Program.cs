using System.Text;
using CloudinaryDotNet;
using hrm;
using hrm.Authorization;
using hrm.Context;
using hrm.Providers;
using hrm.Respository.Auth;
using hrm.Respository.Configs;
using hrm.Respository.Permissions;
using hrm.Respository.Roles;
using hrm.Respository.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https:
//aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagerGenWithAuth();

// Add database context
builder.Services.AddSingleton<HRMContext>();

// Add repositories
builder.Services.AddScoped<IUserRespository, UserRepository>();
builder.Services.AddScoped<IConfigRespository, ConfigRespository>();
builder.Services.AddScoped<IRoleRespository, RoleRespository>();
builder.Services.AddScoped<IAuthRespository, AuthRespository>();
builder.Services.AddScoped<IPermissionRespository, PermissionRespository>();

// Add providers
builder.Services.AddSingleton<TokenProvider>();
builder.Services.AddSingleton<RefreshTokenProvider>();
builder.Services.AddSingleton<UploadFileProvider>();
builder.Services.AddSingleton<AesCryptoProvider>();

// Add authorization handlers and policy provider
builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

// Add CORS policy to allow frontend to access the API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add config JWT
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Add config Cloudinary
builder.Services.Configure<hrm.DTOs.CloudinarySettingsDto>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddSingleton(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<IOptions<hrm.DTOs.CloudinarySettingsDto>>().Value;
    var account = new Account(config.CloudName, config.ApiKey, config.ApiSecret);
    return new CloudinaryDotNet.Cloudinary(account);
});

// Add config host for Swagger
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5005);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
