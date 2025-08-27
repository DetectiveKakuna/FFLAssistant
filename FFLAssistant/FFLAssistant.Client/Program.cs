using Blazored.LocalStorage;
using FFLAssistant.Client.Cache.Services;
using FFLAssistant.Client.Cache.Services.Interfaces;
using FFLAssistant.Client.Services;
using FFLAssistant.Models.Interfaces;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Register cache services
builder.Services.AddScoped<IPlayersCache, PlayersCache>();
builder.Services.AddScoped<IPlayersFiltersCache, PlayersFiltersCache>();

// Register client-side API services
builder.Services.AddScoped<ISleeperLiveDraftService, ClientSleeperLiveDraftService>();
builder.Services.AddScoped<ISleeperPlayersService, ClientSleeperPlayersService>();
builder.Services.AddScoped<IDraftRankingsService, ClientDraftRankingsService>();
builder.Services.AddScoped<IFantasyProsService, ClientFantasyProsService>();

// Configure HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();