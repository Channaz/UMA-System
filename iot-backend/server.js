const express = require("express");
const cors = require("cors");
const mqtt = require("mqtt");
const { Server } = require("socket.io");
const http = require("http");
require("dotenv").config();

// ✅ Initialize Express & HTTP Server
const app = express();
const server = http.createServer(app); // Required for WebSockets
const io = new Server(server, {
  cors: { origin: "*" }, // Allow Blazor WebAssembly to connect
});

app.use(cors());
app.use(express.json());

//  MQTT HiveMQ Configuration
const brokerUrl = "mqtts://c228b1fa8c334dbfa59cce52d95da105.s1.eu.hivemq.cloud";
const username = "panha";
const password = "Panha@123";

//  Connect to HiveMQ
const client = mqtt.connect(brokerUrl, {
  username,
  password,
});

//  Store sensor data & handle timeouts
let sensorData = {};
let timeoutHandle = null;
let isTimedOut = false;
const TIMEOUT_MS = 1000; // 1 second

function resetTimeout() {
  if (timeoutHandle) clearTimeout(timeoutHandle);
  timeoutHandle = setTimeout(() => {
    if (!isTimedOut) {
      console.warn("⚠️ No data received from device.");
      io.emit("sensorTimeout", {
        message: "No data received from device in 1 second.",
      });
      isTimedOut = true;
    }
  }, TIMEOUT_MS);
}

// ✅ Handle MQTT Connection
client.on("connect", () => {
  console.log("✅ Connected to HiveMQ!");
  client.subscribe("iot/sensor/data", (err) => {
    if (err) {
      console.error("❌ Subscribe error:", err);
    } else {
      console.log("📡 Subscribed to topic: iot/sensor/data");
    }
  });
});

// ✅ Handle Incoming MQTT Messages
client.on("message", (topic, payload) => {
  try {
    sensorData = JSON.parse(payload.toString());
    console.log("📩 Received:", sensorData);

    // 🔁 Emit to WebSocket clients
    io.emit("sensorData", sensorData);
    resetTimeout();

    if (isTimedOut) {
      console.log("✅ Data resumed.");
      io.emit("sensorResumed", { message: "Data resumed." });
      isTimedOut = false;
    }
  } catch (err) {
    console.error("❌ Failed to parse MQTT payload:", err.message);
  }
});

// ✅ REST API Endpoint to Get Latest Sensor Data
app.get("/api/iot-data", (req, res) => {
  res.json(sensorData);
});

// ✅ WebSocket Connections
io.on("connection", (socket) => {
  console.log("⚡ Blazor client connected:", socket.id);

  // Immediately send latest sensor data on connect
  socket.emit("sensorData", sensorData);

  socket.on("disconnect", () => {
    console.log("❌ Blazor client disconnected:", socket.id);
  });
});

// ✅ Start Server
const PORT = process.env.PORT || 3000;
server.listen(PORT, () =>
  console.log(`🚀 Server running on http://localhost:${PORT}`)
);
