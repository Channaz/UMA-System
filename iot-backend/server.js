const express = require("express");
const cors = require("cors");
const awsIot = require("aws-iot-device-sdk");
const { Server } = require("socket.io");
const http = require("http");
require("dotenv").config();

// ✅ Initialize Express & HTTP Server
const app = express();
const server = http.createServer(app); // Required for WebSockets
const io = new Server(server, {
    cors: { origin: "*" } // Allow Blazor WebAssembly to connect
});

app.use(cors());
app.use(express.json());

// ✅ Connect to AWS IoT Core
const device = awsIot.device({
    keyPath: "./certs/private.pem",
    certPath: "./certs/certificate.pem",
    caPath: "./certs/AmazonRootCA1.pem",
    clientId: "iotClient",
    host: process.env.AWS_IOT_ENDPOINT, // Your AWS IoT endpoint
});

let sensorData = {}; // Store latest sensor data

device.on("connect", () => {
    console.log("✅ Connected to AWS IoT Core!");
    device.subscribe("iot/sensor/data"); // ✅ Subscribe to IoT topic
});

device.on("message", (topic, payload) => {
    sensorData = JSON.parse(payload.toString());
    console.log("📩 Received:", sensorData);

    // 🔥 Send real-time data to Blazor clients
    io.emit("sensorData", sensorData);
});

// ✅ REST API Endpoint to Fetch IoT Data
app.get("/api/iot-data", (req, res) => {
    res.json(sensorData);
});

// ✅ WebSocket Connection Handling
io.on("connection", (socket) => {
    console.log("⚡ Blazor client connected:", socket.id);

    // Send latest sensor data on connection
    socket.emit("sensorData", sensorData);

    socket.on("disconnect", () => {
        console.log("❌ Blazor client disconnected:", socket.id);
    });
});

// ✅ Start Server
const PORT = process.env.PORT || 3000;
server.listen(PORT, () => console.log(`🚀 Server running on port ${PORT}`));
