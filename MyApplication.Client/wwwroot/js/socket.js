window.socketFunctions = {
    socket: null,

    connect: (dotNetHelper) => {
        if (!window.socketFunctions.socket) {
            window.socketFunctions.socket = io("http://localhost:3000"); // Adjust API URL
        }

        // Ensure WebSocket is only listening once
        window.socketFunctions.socket.off("sensorData"); // Remove previous listeners
        window.socketFunctions.socket.on("sensorData", (data) => {
            console.log("📡 Received WebSocket Data:", data);
            dotNetHelper.invokeMethodAsync("ReceiveMessage", JSON.stringify(data));
        });

        console.log("✅ WebSocket Connected");
    },

    disconnect: () => {
        if (window.socketFunctions.socket) {
            window.socketFunctions.socket.disconnect();
            window.socketFunctions.socket = null;
            console.log("❌ WebSocket Disconnected");
        }
    }
};
