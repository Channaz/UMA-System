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

		public event Action? OnChange;
		public DeviceData? DeviceData { get; set; }

		public DeviceDataService(HttpClient http, IJSRuntime js)
		{
			_http = http ?? throw new ArgumentNullException(nameof(http));
			_js = js;

			if (_http.BaseAddress == null)
				_http.BaseAddress = new Uri("http://localhost:3000/");
		}

		public async Task InitializeAsync()
		{
			try
			{
				if (_http.BaseAddress == null)
				{
					throw new InvalidOperationException("HttpClient BaseAddress is not set!");
				}

				DeviceData = await _http.GetFromJsonAsync<DeviceData>("api/iot-data");
				Console.WriteLine($"📡 Initial API Data: {DeviceData}");

				OnChange?.Invoke(); // Notify UI
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ Error fetching API data: {ex.Message}");
			}
		}

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
			DeviceData = System.Text.Json.JsonSerializer.Deserialize<DeviceData>(jsonData);
			Console.WriteLine($"🔄 WebSocket Updated Data: {DeviceData}");

			OnChange?.Invoke(); // 🔄 Notify UI
		}

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

		public double[] GetEnergyPerFloor()
		{
			if (DeviceData?.Floors == null) return new double[0]; // Prevent null reference

			return DeviceData.Floors
				.Select(floor => floor.Rooms?.Sum(room => room.Energy?.Sum() ?? 0) ?? 0)
				.ToArray();
		}
	}
}
