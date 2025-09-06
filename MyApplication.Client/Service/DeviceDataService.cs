using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;

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
                _http.BaseAddress = new Uri("http://localhost:3000");
        }

        /// <summary>
        /// Fetches initial data from the REST API to populate the UI.
        /// Real-time updates will be handled by the WebSocket.
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                if (_http.BaseAddress == null)
                {
                    throw new InvalidOperationException("HttpClient BaseAddress is not set!");
                }

                DeviceData = await _http.GetFromJsonAsync<DeviceData>("api/iot-data");
                Console.WriteLine("✅ Initial data fetch succeeded.");
            }
            catch (Exception ex)
            {
                // In a real application, you'd want better error handling here.
                Console.WriteLine($"❌ Initial API error: {ex.Message}");
            }
        }

        /// <summary>
        /// Establishes the WebSocket connection with the Node.js backend.
        /// </summary>
        public async Task ConnectWebSocketAsync()
        {
            if (!_isInitialized)
            {
                _dotNetRef = DotNetObjectReference.Create(this);
                await _js.InvokeVoidAsync("socketFunctions.connect", "http://localhost:3000", _dotNetRef);
                Console.WriteLine("🔌 WebSocket connected.");
                _isInitialized = true;
            }
        }

        /// <summary>
        /// Handles incoming real-time data from the WebSocket. This method is called from JavaScript.
        /// </summary>
        /// <param name="jsonData">The JSON string containing the device data.</param>
        [JSInvokable("ReceiveMessage")]
        public void ReceiveMessage(string jsonData)
        {
            DeviceData = System.Text.Json.JsonSerializer.Deserialize<DeviceData>(jsonData);
            IsDataFetchTimedOut = false; // Reset the timeout flag when data is received
            Console.WriteLine($"🔄 WebSocket updated data.");

            // Notify components that the data has changed
            OnChange?.Invoke();
        }

        /// <summary>
        /// Handles the timeout event from the server via WebSocket.
        /// This method is called from JavaScript.
        /// </summary>
        [JSInvokable("HandleTimeout")]
        public void HandleTimeout()
        {
            IsDataFetchTimedOut = true;
            Console.WriteLine("⚠️ Server-side timeout detected.");
            OnChange?.Invoke();
        }

        /// <summary>
        /// Handles the data resumed event from the server via WebSocket.
        /// This method is called from JavaScript.
        /// </summary>
        [JSInvokable("HandleDataResumed")]
        public void HandleDataResumed()
        {
            IsDataFetchTimedOut = false;
            Console.WriteLine("✅ Data flow resumed from server.");
            OnChange?.Invoke();
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
