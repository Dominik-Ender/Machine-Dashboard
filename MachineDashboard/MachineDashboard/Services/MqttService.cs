using MachineDashboard.Models;
using MQTTnet;
using System.Text;
using System.Text.Json;

namespace MachineDashboard.Services {

    public class MqttService {

        private readonly IMqttClient _client;
        public event Action<MachineData>? OnDataReceived;

        public MqttService() {
            var factory = new MqttClientFactory();
            _client = factory.CreateMqttClient();
        }

        public async Task ConnectAsync() {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("localhost", 1883)
                .Build();

            _client.ApplicationMessageReceivedAsync += e => {
                var json = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                var data = JsonSerializer.Deserialize<MachineData>(json);
                if (data != null)
                    OnDataReceived?.Invoke(data);
                return Task.CompletedTask;
            };

            await _client.ConnectAsync(options);
            await _client.SubscribeAsync("factory/machine1/data");
        }
    }
}
