using Microsoft.AspNetCore.Mvc;
using MyApplication.Controllers;
using MyApplication.Service;
using MyApplication.Shared.Models;

namespace MyApplication.Controllers
{
    [ApiController]
	[Route("api/[controller]")]
	public class DeviceDataController : ControllerBase
	{
		private readonly DeviceDataService _deviceDataService;

		public DeviceDataController(DeviceDataService deviceDataService)
		{
			_deviceDataService = deviceDataService;
		}

		[HttpGet]
		public async Task<ActionResult<DeviceData>> GetDeviceData()
		{
			await _deviceDataService.InitializeAsync();

			if (_deviceDataService.IsDataFetchTimedOut || _deviceDataService.DeviceData == null)
			{
				return StatusCode(504, "Unable to retrieve device data (timeout or error).");
			}

			return Ok(_deviceDataService.DeviceData);
		}
		// Additional methods for updating or deleting data can be added here

	}
}
