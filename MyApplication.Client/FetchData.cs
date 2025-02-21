using MyApplication.Client;
using System.Text.Json.Serialization;
using System.Text.Json;


namespace MyApplication.Client
{
    public class DeviceData
    {
		[JsonPropertyName("Device")]
		public string Device {  get; set; }

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
		public List<int> RoomNumbers { get; set; } = new List<int>();

		[JsonPropertyName("V")]
		public List<double> Voltage { get; set; } = new List<double>();

		[JsonPropertyName("I")]
		public List<double> Current { get; set; } = new List<double>();

		[JsonPropertyName("P")]
		public List<double> Power { get; set; } = new List<double>();

		[JsonPropertyName("E")]
		public List<double> Energy { get; set; } = new List<double>();
	}
}
