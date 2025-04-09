window.socketFunctions = {
    socket: null,
    timeoutId: null,    

    connect: (dotNetHelper) => {
        if (!window.socketFunctions.socket) {
            window.socketFunctions.socket = io("http://localhost:3000"); // Adjust API URL
        }

        // Ensure WebSocket is only listening once
        window.socketFunctions.socket.off("sensorData"); // Remove previous listeners

        // Start timeout checker
        const resetTimeout = () => {
            if (window.socketFunctions.timeoutId) {
                clearTimeout(window.socketFunctions.timeoutId);
            }

            window.socketFunctions.timeoutId = setTimeout(() => {
                // Timeout triggered: No data received in 1 second
                dotNetHelper.invokeMethodAsync("HandleTimeout");
            }, 1000); // 1 second timeout
        };

        window.socketFunctions.socket.on("sensorData", (data) => {

            resetTimeout();
            //console.log("📡 Received WebSocket Data:", data);
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

        // Clear any timeout
        if (window.socketFunctions.timeoutId) {
            clearTimeout(window.socketFunctions.timeoutId);
        }
    }
};
