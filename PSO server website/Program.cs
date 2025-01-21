using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PSOServerWebsite;
using PSOServerWebsite.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddSingleton<ItemsService>();
builder.Services.AddScoped<RareDropsService>();
builder.Services.AddScoped<ItemPMTService>();
builder.Services.AddScoped<LocationsService>();
builder.Services.AddScoped<LevelTableService>();
builder.Services.AddScoped<ConfigService>();

WebAssemblyHost build = builder.Build();
// Preload the data
build.Services.GetRequiredService<ItemsService>().LoadData();
await build.RunAsync();
