
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Ninject.Planning.Bindings;
using MyApplication.Shared.Models;
using Microsoft.JSInterop;

namespace MyApplication.Client.Services
{
	public class DeviceDataService
	{
		private readonly HttpClient _http;

		public DeviceData? DeviceData { get; private set; }
		public bool IsDataFetchTimedOut { get; private set; }

		public DeviceDataService(HttpClient http)
		{
			_http = http;
		}

		public async Task InitializeAsync()
		{
			try
			{
				DeviceData = await _http.GetFromJsonAsync<DeviceData>("api/DeviceData");
				IsDataFetchTimedOut = DeviceData == null;
			}
			catch (Exception ex)
			{
				IsDataFetchTimedOut = true;
				Console.WriteLine($"❌ Error fetching DeviceData: {ex.Message}");
			}
		}

		public (double voltage, double current, double power, double energy) GetBuildingTotals()
		{
			if (DeviceData == null) return (0, 0, 0, 0);

			double voltage = 0, current = 0, power = 0, energy = 0;

			foreach (var floor in DeviceData.Floors)
			{
				foreach (var room in floor.Rooms)
				{
					voltage += room.Voltage.Sum();
					current += room.Current.Sum();
					power += room.Power.Sum();
					energy += room.Energy.Sum();
				}
			}

			return (voltage, current, power, energy);
		}

		public double[] GetEnergyPerFloor()
		{
			return DeviceData?.Floors?
				.Select(f => f.Rooms?.Sum(r => r.Energy?.Sum() ?? 0) ?? 0)
				.ToArray() ?? new double[0];
		}
	}
}
