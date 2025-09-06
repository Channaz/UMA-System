window.socketFunctions = {
    socket: null,

    connect: function (url, dotNetHelper) {
        // Build correct API URL based on the current hostname, port 3000
        const apiUrl = `${window.location.protocol}//${window.location.hostname}:3000`;

        // Initialize the socket.io client
        this.socket = io(apiUrl);

        // Disconnect any previous listeners to prevent duplicates
        this.socket.off("sensorData");
        this.socket.off("sensorTimeout");
        this.socket.off("sensorResumed");

        // Listen for real-time sensor data from the server
        this.socket.on("sensorData", (data) => {
            // The server is now responsible for handling timeouts.
            // We just forward the data to the Blazor app.
            dotNetHelper.invokeMethodAsync("ReceiveMessage", JSON.stringify(data));
            console.log("Receiving real-time data:", data);
        });

        // Listen for a timeout event from the server
        this.socket.on("sensorTimeout", (message) => {
            console.warn("⚠️ Server-side timeout detected:", message);
            dotNetHelper.invokeMethodAsync("HandleTimeout");
        });

        // Listen for a data resumed event from the server
        this.socket.on("sensorResumed", (message) => {
            console.log("✅ Server reports data has resumed:", message);
            dotNetHelper.invokeMethodAsync("HandleDataResumed");
        });

        console.log("✅ WebSocket connection attempt initiated...");
    },

    disconnect: function () {
        if (this.socket) {
            this.socket.disconnect();
            this.socket = null;
            console.log("❌ WebSocket Disconnected");
        }
    }
};
