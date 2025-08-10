using Blazored.LocalStorage;
using FFLAssistant.Client.Cache.Services;
using FFLAssistant.Client.Cache.Services.Interfaces;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Register cache services
builder.Services.AddScoped<IPlayersCache, PlayersCache>();
builder.Services.AddScoped<IPlayersFiltersCache, PlayersFiltersCache>();

// Configure HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();