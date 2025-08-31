using IdentityProvider.Server.Api.Services;
using IdentityProvider.Server.Configuration;
using IdentityProvider.Server.Configuration.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using MinimalEndpoints.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Identity Provider settings
builder.Services.AddIdentityProviderConfiguration(builder.Configuration);

// Add authentication with both Cookie and JWT Bearer schemes
var identityConfig = builder.Configuration.GetSection(IdentityProviderConfiguration.SectionName).Get<IdentityProviderConfiguration>();
var jwtConfig = identityConfig?.Jwt ?? throw new InvalidOperationException("JWT configuration is required");

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "IdentityProvider.Auth";
        options.Cookie.HttpOnly = true;

        // Configuration for development with HTTP frontend and API
        if (builder.Environment.IsDevelopment())
        {
            // Use Lax for HTTP development - requires same-site navigation
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Allow HTTP
        }
        else
        {
            options.Cookie.SameSite = SameSiteMode.None; // Required for cross-origin in production
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Require HTTPS in production
        }

        options.Cookie.Domain = null; // Let browser handle domain
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";

        // Configure external redirects for frontend
        options.Events.OnRedirectToLogin = context =>
        {
            if (identityConfig?.FrontendUrls?.LoginUrl != null)
            {
                var returnUrl = context.Request.Path + context.Request.QueryString;
                var loginUrl = $"{identityConfig.FrontendUrls.LoginUrl}?returnUrl={Uri.EscapeDataString(returnUrl)}";
                context.Response.Redirect(loginUrl);
                return Task.CompletedTask;
            }

            // Fallback to default behavior
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtConfig.Issuer,
            ValidateAudience = true,
            ValidAudiences = [.. identityConfig.OAuthClients.Values.Select(x => x.ClientId)], // Accept any configured client ID as valid audience
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwtConfig.RsaPublicKey, // Use RSA public key instead of symmetric key
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        var identityConfig = builder.Configuration.GetSection(IdentityProviderConfiguration.SectionName).Get<IdentityProviderConfiguration>();
        var allowedOrigins = identityConfig?.Cors?.AllowedOrigins ?? ["http://localhost:5173"];

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Identity Provider API",
        Version = "v1",
    });
});

builder.Services.AddEndpoints(typeof(Program).Assembly);

// Add authorization code repository
builder.Services.AddSingleton<IAuthorizationCodeRepository, InMemoryAuthorizationCodeRepository>();

// Add user repository
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

// Add cleanup background service
builder.Services.AddHostedService<AuthorizationCodeCleanupService>();

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!builder.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.Run();
