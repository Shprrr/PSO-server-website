using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PSOServerWebsite;
using PSOServerWebsite.Repositories;
using PSOServerWebsite.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddSingleton<ItemsRepository>();
builder.Services.AddScoped<RareDropsRepository>();
builder.Services.AddScoped<ItemPMTRepository>();
builder.Services.AddScoped<LocationsRepository>();
builder.Services.AddScoped<LevelTableRepository>();
builder.Services.AddScoped<ConfigurationRepository>();

builder.Services.AddScoped<DropsLocationsService>();

WebAssemblyHost build = builder.Build();
// Preload the data
build.Services.GetRequiredService<ItemsRepository>().LoadData();
build.Services.GetRequiredService<ItemPMTRepository>().LoadData();
await build.RunAsync();
