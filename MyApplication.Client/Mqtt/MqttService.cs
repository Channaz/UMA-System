using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using uPLibrary;
using uPLibrary.Networking.M2Mqtt;
using MyApplication.Client;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace MqttServiceData 
{
	public class MqttService : IDisposable
	{

		private SynchronizationContext? _syncContext;
		private MqttClient? _client;
		private readonly string _brokerAddress = "test.mosquitto.org";
		private bool _tryReconnectMQTT = true;
		private bool _isConnected = false;

		public event Action? OnChange; // UI update event

		private DeviceData? _deviceData;


		public DeviceData? DeviceData
		{
			get => _deviceData;
			private set
			{
				_deviceData = value;
				NotifyStateChanged();
			}
		}

		public void CaptureSynchronizationContext()
		{
			_syncContext = SynchronizationContext.Current;
		}

		private void NotifyStateChanged()
		{
			if (_syncContext != null)
			{
				_syncContext.Post(_ => OnChange?.Invoke(), null);
			}
		}

		public async Task ConnectAsync()
		{
			if (_isConnected) return;

			try
			{
				Console.WriteLine("Initializing MQTT client...");
				_client = new MqttClient(_brokerAddress);
				_client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;

				string clientId = Guid.NewGuid().ToString();
				_client.Connect(clientId);
				_isConnected = true;

				Console.WriteLine("Connected to MQTT broker Successfully!!!!");

				// Start reconnection loop
				_ = PersistConnectionAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"MQTT Connection Error: {ex.Message}");
			}
		}

		private async Task PersistConnectionAsync()
		{
			while (_tryReconnectMQTT)
			{
				if (!_isConnected)
				{
					try
					{
						Console.WriteLine("Attempting to reconnect to MQTT broker...");
						string clientId = Guid.NewGuid().ToString();
						_client?.Connect(clientId);
						_isConnected = true;
						Console.WriteLine("Reconnected to MQTT broker.");
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Failed to reconnect: {ex.Message}");
					}
				}
				await Task.Delay(1000);
			}
		}

		public void SubscribeToTopic(string topic)
		{
			if (_client?.IsConnected ?? false)
			{
				_client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
				Console.WriteLine($"Subscribed to topic: {topic}");
			}
		}

		private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
		{
			string receivedMessage = Encoding.UTF8.GetString(e.Message);
			Console.WriteLine($"MQTT Message Received: {receivedMessage}");

			Task.Run(async () =>
			{
				try
				{
					var receivedData = JsonSerializer.Deserialize<DeviceData>(receivedMessage, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					if (receivedData != null)
					{
						// Ensure the UI update happens on the main thread
						await MainThreadInvokeAsync(() =>
						{
							DeviceData = receivedData;
							Console.WriteLine("Data Updated! Notifying UI...");
							NotifyStateChanged();
						});
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error parsing MQTT message: {ex.Message}");
				}
			});
		}

		private async Task MainThreadInvokeAsync(Action action)
		{
			if (SynchronizationContext.Current == null)
			{
				await Task.Delay(1); // Small delay to ensure the UI thread is available
			}

			await Task.Yield(); // Give control back to Blazor UI

			action();
		}

		public void Disconnect()
		{
			_tryReconnectMQTT = false;
			_client?.Disconnect();
			_isConnected = false;
			Console.WriteLine("Disconnected from MQTT broker.");
		}

		public void Dispose()
		{
			Disconnect();

		}
	}
}
