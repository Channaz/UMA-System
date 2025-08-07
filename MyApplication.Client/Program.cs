using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using MudBlazor.Services;
using MqttServiceData;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using MyApplication.Client.Service;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

//builder.Services.AddSingleton<WebSocketService>();
builder.Services.AddSingleton<DeviceDataService>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:3000/") });


builder.Services.AddMudServices();

//builder.RootComponents.Add<App>("#app");

await builder.Build().RunAsync();
