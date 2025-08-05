using Asp.Versioning;
using Asp.Versioning.Builder;
using IdentityProvider.Server.Configuration;
using IdentityProvider.Server.Configuration.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using MinimalEndpoints.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Identity Provider settings
builder.Services.AddIdentityProviderConfiguration(builder.Configuration);

// Add cookie-based authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        var identityConfig = builder.Configuration.GetSection("IdentityProvider").Get<IdentityProviderConfiguration>();

        options.Cookie.Name = "IdentityProvider.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
        options.LoginPath = "/account/login";
        options.LogoutPath = "/account/logout";
        options.AccessDeniedPath = "/account/access-denied";

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

        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (identityConfig?.FrontendUrls?.AccessDeniedUrl != null)
            {
                context.Response.Redirect(identityConfig.FrontendUrls.AccessDeniedUrl);
                return Task.CompletedTask;
            }

            // Fallback to default behavior
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173") // Vite dev server
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new QueryStringApiVersionReader(),
        new HeaderApiVersionReader("X-Version"),
        new MediaTypeApiVersionReader("ver")
    );
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Identity Provider API",
        Version = "v1",
    });
});

builder.Services.AddEndpoints(typeof(Program).Assembly);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();

RouteGroupBuilder versionedGroup = app
    .MapGroup("api/v{version:apiVersion}")
    .WithApiVersionSet(apiVersionSet);

app.MapEndpoints(versionedGroup);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
