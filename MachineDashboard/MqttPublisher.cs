using MachineDashboard.Models;
using MQTTnet;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;

namespace MachineDashboard.Services {
    public class MqttPublisher {
        private readonly IMqttClient _client;

        public MqttPublisher() {
            var factory = new MqttClientFactory();
            _client = factory.CreateMqttClient();
        }

        public async Task ConnectAsync() {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("localhost", 1883)
                .Build();

            await _client.ConnectAsync(options);
        }

        public async Task PublishAsync(MachineData data) {
            var json = JsonSerializer.Serialize(data);
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("factory/machine1/data")
                .WithPayload(Encoding.UTF8.GetBytes(json))
                .Build();

            await _client.PublishAsync(message);
        }

        public async Task StartSimulationAsync() {
            await ConnectAsync();
            var rand = new Random();

            while (true) {
                var data = new MachineData {
                    MachineName = "Machine 1",
                    Temperature = 20 + rand.NextDouble() * 10,
                    IsRunning = rand.Next(0, 2) == 1,
                    Timestamp = DateTime.Now
                };

                await PublishAsync(data);
                await Task.Delay(1000);
            }
        }
    }
}
