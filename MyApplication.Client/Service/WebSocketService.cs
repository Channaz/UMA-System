using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace MyApplication.Client.Service
{
    //public class WebSocketService : IAsyncDisposable
    //{
    //    private readonly IJSRuntime _jsRuntime;
    //    private IJSObjectReference? _jsModule;
    //    private DotNetObjectReference<WebSocketService>? _dotNetRef;

    //    public event Action<string>? OnMessageReceived; // Event to notify components

    //    public WebSocketService(IJSRuntime jsRuntime)
    //    {
    //        _jsRuntime = jsRuntime;
    //    }

    //    public async Task ConnectAsync()
    //    {
    //        if (_jsModule == null)
    //        {
    //            _dotNetRef = DotNetObjectReference.Create(this);
    //            _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "/js/socket.js");
    //            await _jsModule.InvokeVoidAsync("connect", _dotNetRef);
    //        }
    //    }

    //    [JSInvokable]
    //    public void ReceiveMessage(string data)
    //    {
    //        OnMessageReceived?.Invoke(data); // Trigger event to update components
    //    }

    //    public async ValueTask DisposeAsync()
    //    {
    //        if (_jsModule != null)
    //        {
    //            await _jsModule.InvokeVoidAsync("disconnect");
    //            await _jsModule.DisposeAsync();
    //        }
    //        _dotNetRef?.Dispose();
    //    }
    //}
}
