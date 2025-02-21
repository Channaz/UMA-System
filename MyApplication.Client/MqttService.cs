using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;


namespace MyApplication.Client 
{
    public class MqttService 
    {

        private readonly IJSRuntime _jsRuntime;

        public MqttService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public static void Connect()
        {
            Console.WriteLine("MQTT Service Connected!");
        }

        public async Task ConnectAsync(string brokerUrl, string clientId)
        {
            await _jsRuntime.InvokeVoidAsync("mqttInterop.connect", brokerUrl, clientId);
        }

        public async Task SubscribeAsync(string topic)
        {
            await _jsRuntime.InvokeVoidAsync("mqttInterop.subscribe", topic);
        }

        public async Task PublishAsync(string topic, string message)
        {
            await _jsRuntime.InvokeVoidAsync("mqttInterop.publish", topic, message);
        }

        [JSInvokable]
        public static Task OnMqttConnected()
        {
            Console.WriteLine("MQTT Connected!");
            return Task.CompletedTask;
        }

        [JSInvokable]
        public static Task OnMqttMessageReceived(string topic, string message)
        {
            Console.WriteLine($"Received Message: {message} on Topic: {topic}");
            return Task.CompletedTask;
        }
    }
}
