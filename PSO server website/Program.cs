using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PSOServerWebsite;
using PSOServerWebsite.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<ItemsService>();
builder.Services.AddScoped<RareDropsService>();
builder.Services.AddScoped<ItemPMTService>();
builder.Services.AddScoped<LocationsService>();

await builder.Build().RunAsync();
