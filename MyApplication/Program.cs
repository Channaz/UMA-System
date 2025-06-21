using ChartJs.Blazor.ChartJS.LineChart;
//using MqttServiceData;
using MudBlazor.Services;
using MyApplication.Client.Pages;
using MyApplication.Service;
using MyApplication.Components;

//using MyApplication.MqttService;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

//builder.Services.AddScoped<MqttService>();
//builder.Services.AddSingleton<MqttService>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddAntiforgery();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();
//builder.Services.AddScoped<WebSocketService>();
builder.Services.AddSingleton<DeviceDataService>();

//builder.Services.AddHttpClient("API", client =>
//{
//	client.BaseAddress = new Uri("http://localhost:3000/");
//});   

builder.Services.AddHttpClient<DeviceDataService>(client =>
{
	client.BaseAddress = new Uri("http://localhost:3000"); // Or your MQTT backend
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyApplication",
        policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
    );
});

// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents()
	.AddInteractiveWebAssemblyComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAntiforgery();
app.UseAuthorization();
app.UseCors("AllowMyApplication");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapRazorPages();

    endpoints.MapBlazorHub();
});

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MyApplication.Client._Imports).Assembly);

app.Run();
