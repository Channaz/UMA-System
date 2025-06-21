using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using MyApplication.Shared.Models;

namespace MyApplication.Service
{

	public class DeviceDataService
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<DeviceDataService> _logger;
		private readonly string _brokerUrl;
		private readonly string _username;
		private readonly string _password;
		private readonly string _topic;

		public DeviceData? DeviceData { get; private set; }
		public bool IsDataFetchTimedOut { get; private set; } = false;	

		public async Task InitializeAsync()
		{
			// Direct call to HiveMQ's REST API
			var response = await _httpClient.GetAsync("https://your-hivemq-cluster.com:8080/api/v1/mqtt/retained-messages/your-topic");
			var data = await response.Content.ReadFromJsonAsync<DeviceData>();
		}

		public DeviceDataService(
			HttpClient httpClient,
			ILogger<DeviceDataService> logger,
			string brokerUrl,
			string username,
			string password,
			string topic = "device/data")
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_brokerUrl = brokerUrl ?? throw new ArgumentNullException(nameof(brokerUrl));
			_username = username ?? throw new ArgumentNullException(nameof(username));
			_password = password ?? throw new ArgumentNullException(nameof(password));
			_topic = topic ?? throw new ArgumentNullException(nameof(topic));

			// Set up HTTP client for HiveMQ REST API
			var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_username}:{_password}"));
			_httpClient.DefaultRequestHeaders.Authorization =
				new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

			// Set base address to HiveMQ Cloud REST API
			if (_httpClient.BaseAddress == null)
			{
				_httpClient.BaseAddress = new Uri($"https://{_brokerUrl}:8080/");
			}
		}

		public async Task InitializeAsync()
		{
			try
			{
				_logger.LogInformation("Fetching device data from HiveMQ REST API...");
				using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

				// HiveMQ Cloud REST API endpoint to get retained message
				var encodedTopic = Uri.EscapeDataString(_topic);
				var response = await _httpClient.GetAsync($"api/v1/mqtt/clients/rest-client/subscriptions/{encodedTopic}/messages", cts.Token);

				if (!response.IsSuccessStatusCode)
				{
					_logger.LogError("Failed to fetch data from HiveMQ REST API. Status: {StatusCode}, Reason: {ReasonPhrase}",
						response.StatusCode, response.ReasonPhrase);
					IsDataFetchTimedOut = true;
					return;
				}

				var jsonResponse = await response.Content.ReadAsStringAsync(cts.Token);

				if (string.IsNullOrEmpty(jsonResponse))
				{
					_logger.LogWarning("Received empty response from HiveMQ REST API.");
					IsDataFetchTimedOut = true;
					return;
				}

				// Parse the HiveMQ REST API response
				var mqttMessages = JsonSerializer.Deserialize<HiveMqRestResponse>(jsonResponse, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				if (mqttMessages?.Messages == null || !mqttMessages.Messages.Any())
				{
					_logger.LogWarning("No messages found in HiveMQ REST API response.");
					IsDataFetchTimedOut = true;
					return;
				}

				// Get the latest message
				var latestMessage = mqttMessages.Messages.OrderByDescending(m => m.Timestamp).FirstOrDefault();

				if (latestMessage?.Payload == null)
				{
					_logger.LogWarning("Latest message has no payload.");
					IsDataFetchTimedOut = true;
					return;
				}

				// Decode the payload (assuming it's base64 encoded)
				var payloadJson = DecodePayload(latestMessage.Payload);

				var data = JsonSerializer.Deserialize<DeviceData>(payloadJson, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				if (data == null)
				{
					_logger.LogWarning("Failed to deserialize device data from MQTT payload.");
					IsDataFetchTimedOut = true;
					return;
				}

				DeviceData = data;
				IsDataFetchTimedOut = false;
				_logger.LogInformation("Device data successfully fetched from HiveMQ REST API.");
			}
			catch (OperationCanceledException)
			{
				IsDataFetchTimedOut = true;
				_logger.LogWarning("HiveMQ REST API request timed out.");
			}
			catch (Exception ex)
			{
				IsDataFetchTimedOut = true;
				_logger.LogError(ex, "Error fetching device data from HiveMQ REST API.");
			}
		}

		// Alternative method using WebSocket API (for real-time updates)
		public async Task InitializeWithWebSocketAsync()
		{
			try
			{
				_logger.LogInformation("Fetching device data from HiveMQ WebSocket API...");
				using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

				var webSocketRequest = new
				{
					type = "subscribe",
					topic = _topic,
					qos = 0
				};

				var json = JsonSerializer.Serialize(webSocketRequest);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				// WebSocket API endpoint
				var response = await _httpClient.PostAsync("api/v1/ws/mqtt", content, cts.Token);

				response.EnsureSuccessStatusCode();

				var responseJson = await response.Content.ReadAsStringAsync(cts.Token);

				// Parse response and extract device data
				var wsResponse = JsonSerializer.Deserialize<WebSocketResponse>(responseJson, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				if (wsResponse?.Payload != null)
				{
					var payloadJson = DecodePayload(wsResponse.Payload);
					var data = JsonSerializer.Deserialize<DeviceData>(payloadJson, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					if (data != null)
					{
						DeviceData = data;
						IsDataFetchTimedOut = false;
						_logger.LogInformation("Device data successfully fetched from HiveMQ WebSocket API.");
					}
				}
			}
			catch (OperationCanceledException)
			{
				IsDataFetchTimedOut = true;
				_logger.LogWarning("HiveMQ WebSocket API request timed out.");
			}
			catch (Exception ex)
			{
				IsDataFetchTimedOut = true;
				_logger.LogError(ex, "Error fetching device data from HiveMQ WebSocket API.");
			}
		}

		// Alternative method: Polling approach
		public async Task StartPollingAsync(CancellationToken cancellationToken = default)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				await InitializeAsync();

				// Poll every 30 seconds (adjust as needed)
				try
				{
					await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
				}
				catch (OperationCanceledException)
				{
					break;
				}
			}
		}

		private string DecodePayload(string payload)
		{
			try
			{
				// Try to decode as base64 first
				var bytes = Convert.FromBase64String(payload);
				return Encoding.UTF8.GetString(bytes);
			}
			catch
			{
				// If not base64, assume it's already plain text
				return payload;
			}
		}

		public (double Voltage, double Current, double Power, double Energy) GetBuildingTotals()
		{
			double totalVoltage = 0, totalCurrent = 0, totalPower = 0, totalEnergy = 0;

			if (DeviceData?.Floors == null)
				return (0, 0, 0, 0);

			foreach (var floor in DeviceData.Floors)
			{
				if (floor?.Rooms == null) continue;

				foreach (var room in floor.Rooms)
				{
					if (room == null) continue;

					totalVoltage += room.Voltage?.Sum() ?? 0;
					totalCurrent += room.Current?.Sum() ?? 0;
					totalPower += room.Power?.Sum() ?? 0;
					totalEnergy += room.Energy?.Sum() ?? 0;
				}
			}

			return (totalVoltage, totalCurrent, totalPower, totalEnergy);
		}

		public double[] GetEnergyPerFloor()
		{
			if (DeviceData?.Floors == null)
				return Array.Empty<double>();

			return DeviceData.Floors
				.Select(floor => floor?.Rooms?.Sum(room => room?.Energy?.Sum() ?? 0) ?? 0)
				.ToArray();
		}
	}

}
