using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Ninject.Planning.Bindings;

namespace MyApplication.Client.Service
{
	public class DeviceDataService
	{
		private readonly HttpClient _http;
		private readonly IJSRuntime _js;
		private DotNetObjectReference<DeviceDataService>? _dotNetRef;
		private bool _isInitialized = false;
		public bool IsDataFetchTimedOut { get; private set; } = false;

		public event Action? OnChange;
		public DeviceData? DeviceData { get; set; }

		public DeviceDataService(HttpClient http, IJSRuntime js)
		{
			_http = http ?? throw new ArgumentNullException(nameof(http));
			_js = js;

			if (_http.BaseAddress == null)
				_http.BaseAddress = new Uri("http://localhost:3000/");
		}

		//for alert message
		public async Task InitializeAsync()
		{
			//using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
			try
			{
				if (_http.BaseAddress == null)
				{
					throw new InvalidOperationException("HttpClient BaseAddress is not set!");
				}

				DeviceData = await _http.GetFromJsonAsync<DeviceData>("api/iot-data");

				//IsDataFetchTimedOut = false;

				//Console.WriteLine("🟢 Starting data fetch...");

				//var data = await GetDeviceDataAsync(cts.Token); // Try to fetch data with timeout
				//if (data == null)
				//	throw new Exception("❌ Received null data from API.");


				//DeviceData = data;

				Console.WriteLine("✅ Data fetch succeeded.");
				//if(cts.Token.IsCancellationRequested)
				//{
				//	IsDataFetchTimedOut = true;
				//	throw new OperationCanceledException("Data fetch timed out.");
				//}

				OnChange?.Invoke(); // Notify UI
			}
			catch(OperationCanceledException)
			{
				IsDataFetchTimedOut = true;
				Console.WriteLine("⏱️ API data fetch timeout.");
				OnChange?.Invoke(); // notify layout
			}
			catch (Exception ex)
			{
				// Handle other errors (e.g. log)
				IsDataFetchTimedOut = true;
				Console.WriteLine($"❌ API error: {ex.Message}");
				OnChange?.Invoke(); // notify layout
			}
			finally
			{
				// Always notify the UI
				OnChange?.Invoke();
			}
		}

		//private async Task<DeviceData> GetDeviceDataAsync(CancellationToken token)
		//{
		//	// Replace this with your actual API endpoint
		//	string url = "api/iot-data";

		//	var response = await _http.GetAsync(url, token);

		//	response.EnsureSuccessStatusCode();

		//	var data = await response.Content.ReadFromJsonAsync<DeviceData>(cancellationToken: token);
		//	Console.WriteLine($"📡 API Data: {data}");
		//	if (data == null)
		//	{
		//		throw new Exception("Received null data from API.");
		//	}

		//	return data;
		//}

		public async Task ConnectWebSocketAsync()
		{
			if (!_isInitialized)
			{
				_dotNetRef = DotNetObjectReference.Create(this);
				await _js.InvokeVoidAsync("socketFunctions.connect", _dotNetRef);
				_isInitialized = true;
			}
		}

		[JSInvokable("ReceiveMessage")]
		public void ReceiveMessage(string jsonData)
		{
			IsDataFetchTimedOut = false;
			DeviceData = System.Text.Json.JsonSerializer.Deserialize<DeviceData>(jsonData);
			Console.WriteLine($"🔄 WebSocket Updated Data: {DeviceData}");

			OnChange?.Invoke(); // 🔄 Notify UI
		}

		[JSInvokable("HandleTimeout")]
		public void HandleTimeout()
		{
			IsDataFetchTimedOut = true;
			Console.WriteLine("⚠️ No data received via WebSocket in 1 second.");
			OnChange?.Invoke(); // Update UI
		}

		//set total values for the building
		public (double voltage, double current, double power, double energy) GetBuildingTotals()
		{
			double totalVoltage = 0, totalCurrent = 0, totalPower = 0, totalEnergy = 0;

			if (DeviceData == null) return (0, 0, 0, 0);

			foreach (var floor in DeviceData.Floors)
			{
				foreach (var room in floor.Rooms)
				{
					totalVoltage += room.Voltage.Sum();
					totalCurrent += room.Current.Sum();
					totalPower += room.Power.Sum();
					totalEnergy += room.Energy.Sum();
				}
			}

			return (totalVoltage, totalCurrent, totalPower, totalEnergy);
		}

		//get energy for each floor apply to Donut Chat
		public double[] GetEnergyPerFloor()
		{
			if (DeviceData?.Floors == null) return new double[0]; // Prevent null reference

			return DeviceData.Floors
				.Select(floor => floor.Rooms?.Sum(room => room.Energy?.Sum() ?? 0) ?? 0)
				.ToArray();
		}
	}
}
