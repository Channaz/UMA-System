window.socketFunctions = {
    socket: null,
    timeoutId: null,

    connect: function (url, dotNetHelper) {
        // Build correct API URL
        const apiUrl = `${window.location.protocol}//${window.location.hostname}:3000`;

        this.socket = io(apiUrl);

        // Ensure WebSocket is only listening once
        this.socket.off("sensorData");

        // Timeout reset function
        const resetTimeout = () => {
            if (this.timeoutId) {
                clearTimeout(this.timeoutId);
            }

            this.timeoutId = setTimeout(() => {
                // Timeout triggered: No data received in 1 second
                dotNetHelper.invokeMethodAsync("HandleTimeout");
            }, 1000);
        };

        this.socket.on("sensorData", (data) => {
            resetTimeout();
            dotNetHelper.invokeMethodAsync("ReceiveMessage", JSON.stringify(data));
        });

        console.log("✅ WebSocket Connected");
    },

    disconnect: function () {
        if (this.socket) {
            this.socket.disconnect();
            this.socket = null;
            console.log("❌ WebSocket Disconnected");
        }

        // Clear any timeout
        if (this.timeoutId) {
            clearTimeout(this.timeoutId);
        }
    }
};
