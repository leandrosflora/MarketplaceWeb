using System.Net.Http.Headers;
using Marketplace.Web.Clients;
using Marketplace.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Http.Resilience;

var builder = WebApplication.CreateBuilder(args);

var razorPages = builder.Services.AddRazorPages();

razorPages.AddMvcOptions(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddProblemDetails();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdHandler>();

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
app.MapRazorPages();
app.Run();
