using System.Net.Http.Headers;
using Marketplace.Web.Clients;
using Marketplace.Web.Infrastructure.Auth;
using Marketplace.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Http.Resilience;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:5107";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options => options.Filter = httpContext =>
            !httpContext.Request.Path.StartsWithSegments("/metrics") &&
            !httpContext.Request.Path.StartsWithSegments("/health"))
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint)));

var razorPages = builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Orders");
    options.Conventions.AuthorizeFolder("/Admin", MarketplaceAuthConstants.AdminPolicy);
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/Logout");
    options.Conventions.AllowAnonymousToPage("/Account/AccessDenied");
});

razorPages.AddMvcOptions(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddProblemDetails();

builder.Services.Configure<MarketplaceAuthOptions>(
    builder.Configuration.GetSection(MarketplaceAuthOptions.SectionName));
builder.Services.AddSingleton<IMarketplaceCredentialValidator, MarketplaceCredentialValidator>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "MarketplaceWeb.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        MarketplaceAuthConstants.AdminPolicy,
        policy => policy.RequireRole(MarketplaceAuthConstants.AdminRoles));
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdHandler>();
builder.Services.AddTransient<DevAdminIdentityHandler>();
builder.Services.AddTransient<W3CTraceContextHandler>();

builder.Services
    .AddHttpClient<IMarketplaceBffClient, MarketplaceBffClient>(client =>
    {
        var baseUrl = builder.Configuration["Bff:BaseUrl"]
            ?? throw new InvalidOperationException("BFF BaseUrl is not configured");

        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = Timeout.InfiniteTimeSpan;
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    })
    .AddHttpMessageHandler<W3CTraceContextHandler>()
    .AddHttpMessageHandler<CorrelationIdHandler>();

builder.Services
    .AddHttpClient<IMarketplaceAdminBffClient, MarketplaceAdminBffClient>(client =>
    {
        var baseUrl = builder.Configuration["AdminBff:BaseUrl"]
            ?? throw new InvalidOperationException("Admin BFF BaseUrl is not configured");

        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = Timeout.InfiniteTimeSpan;
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    })
    .AddHttpMessageHandler<W3CTraceContextHandler>()
    .AddHttpMessageHandler<CorrelationIdHandler>()
    .AddHttpMessageHandler<DevAdminIdentityHandler>();

builder.Services
    .AddHttpClient<IOrderVisibilityClient, OrderVisibilityClient>(client =>
    {
        var baseUrl = builder.Configuration["OrderVisibility:BaseUrl"]
            ?? throw new InvalidOperationException("OrderVisibility BaseUrl is not configured");

        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    })
    .AddHttpMessageHandler<W3CTraceContextHandler>()
    .AddHttpMessageHandler<CorrelationIdHandler>();
    //.AddStandardResilienceHandler(options =>
    //{
    //    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(50);
    //    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(20);
    //    options.Retry.MaxRetryAttempts = 2;
    //    //options.Retry.DisableForUnsafeHttpMethods();
    //    //options.CircuitBreaker.FailureRatio = 0.5;
    //    //options.CircuitBreaker.MinimumThroughput = 20;
    //    //options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
    //    //options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
    //});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();

public partial class Program;
