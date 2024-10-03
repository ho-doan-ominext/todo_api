const mqtt = require("mqtt");
const express = require("express");
const bodyParser = require("body-parser");

// Connect to the local MQTT broker
const mqttClient = mqtt.connect("mqtt://10.50.10.70", {
  username: "user1",
  password: "user1@1",
});

const app = express();
app.use(bodyParser.json());

mqttClient.on("connect", () => {
  console.log("Connected to MQTT Broker");
});

mqttClient.on('disconnect',()=>{
  console.log("disconnected to MQTT Broker");
})

app.post("/send-message", (req, res) => {
  const { deviceId, message } = req.body;

  // Publish the message to the MQTT topic
  const topic = `devices/${deviceId}/messages`;
  mqttClient.publish(topic, message, () => {
    console.log(`Message sent to ${deviceId}: ${message}`);
    res.send("Message sent");
  });
});

app.get("/send", (req, res) => {
  // const deviceId = req.query.deviceId
  const message = "req.query.message";
  // const { deviceId, message } = req.body;

  // Publish the message to the MQTT topic
  const topic = `your/topic`;
  console.log("sending");
  mqttClient.publish(topic, message, () => {
    console.log(`Message sent to ios-client: ${message}`);
    res.send("Message sent");
  });
});

// Start the server
app.listen(3000, () => {
  console.log("Local Push Server running on port 3000");
});
