using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyApplication.Shared.Models
{
	public class DeviceData
	{
		[JsonPropertyName("Device")]
		public string Device { get; set; }

		[JsonPropertyName("NoF")]
		public int NoF { get; set; }

		[JsonPropertyName("NoR")]
		public int NoR { get; set; }

		[JsonPropertyName("Floors")]
		public List<FloorData> Floors { get; set; }
	}

	public class FloorData
	{
		[JsonPropertyName("FloorNumber")]
		public int FloorNumber { get; set; }

		[JsonPropertyName("Rooms")]
		public List<RoomData> Rooms { get; set; }
	}

	public class RoomData
	{
		[JsonPropertyName("N")]
		public List<int> RoomNumbers { get; set; } = new();

		[JsonPropertyName("V")]
		public List<double> Voltage { get; set; } = new();

		[JsonPropertyName("I")]
		public List<double> Current { get; set; } = new();

		[JsonPropertyName("P")]
		public List<double> Power { get; set; } = new();

		[JsonPropertyName("E")]
		public List<double> Energy { get; set; } = new();
	}
}
