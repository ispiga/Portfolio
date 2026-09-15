using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Portfolio.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization();

// Add services for modern Blazor Web App (Interactive Server)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

var supportedCultures = new[] { "es-ES", "en-US" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("es-ES")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
