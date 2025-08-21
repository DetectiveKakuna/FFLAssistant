using FFLAssistant.Components;
using FFLAssistant.Extensions;
using FFLAssistant.Models.Configurations;
using FFLAssistant.Repositories;
using FFLAssistant.Repositories.Interfaces;
using FFLAssistant.Services;
using FFLAssistant.Services.Interfaces;
using MudBlazor.Services;
using Blazored.LocalStorage;
using FFLAssistant.Client.Cache.Services;
using FFLAssistant.Client.Cache.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add configuration
builder.Services.Configure<SleeperConfiguration>(
    builder.Configuration.GetSection(SleeperConfiguration.SectionName));
builder.Services.Configure<DraftRankingsConfiguration>(
    builder.Configuration.GetSection(DraftRankingsConfiguration.SectionName));
builder.Services.Configure<FantasyProsConfiguration>(
    builder.Configuration.GetSection(FantasyProsConfiguration.SectionName));

// Add MudBlazor services
builder.Services.AddMudServices();

// Add Blazored LocalStorage for server-side components
builder.Services.AddBlazoredLocalStorage();

// Add HttpClient for API calls
builder.Services.AddHttpClient<ISleeperApiService, SleeperApiService>();

// Add repositories
builder.Services.AddScoped<IDraftRankingsRepository, DraftRankingsRepository>();
builder.Services.AddScoped<ISleeperPlayersRepository, SleeperPlayersRepository>();

// Add services
builder.Services.AddScoped<IBorisChenService, BorisChenService>();
builder.Services.AddScoped<IDraftRankingsService, DraftRankingsService>();
builder.Services.AddScoped<IFantasyProsService, FantasyProsService>();
builder.Services.AddScoped<ISleeperApiService, SleeperApiService>();
builder.Services.AddScoped<ISleeperLiveDraftService, SleeperLiveDraftService>();
builder.Services.AddScoped<ISleeperPlayersService, SleeperPlayersService>();

// Add cache services for server-side components
builder.Services.AddScoped<IPlayersCache, PlayersCache>();
builder.Services.AddScoped<IPlayersFiltersCache, PlayersFiltersCache>();

// Add initialization service
builder.Services.AddScoped<DataFileInitializationService>();

// Add API controllers
builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

// Initialize critical data BEFORE configuring middleware
await app.InitializeCriticalDataAsync();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapControllers();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(FFLAssistant.Client._Imports).Assembly);

app.Run();