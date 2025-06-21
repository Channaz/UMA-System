using MyApplication.Client;
using MyApplication.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using MudBlazor.Services;
//using MqttServiceData;
using Microsoft.AspNetCore.Components;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

//builder.Services.AddSingleton<WebSocketService>();
//builder.RootComponents.Add<App>("#app");

builder.Services.AddSingleton<DeviceDataService>();

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:3000/") });

builder.Services.AddMudServices();



await builder.Build().RunAsync();
