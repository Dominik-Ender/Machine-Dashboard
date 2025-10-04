using MachineDashboard.Models;
using MachineDashboard.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace MachineDashboard.ViewModels {

    public class MainViewModel : INotifyPropertyChanged {


        private readonly MqttService _mqttService;

        public ObservableCollection<MachineData> Machines { get; } = new();

        public ISeries[] Series { get; set; }
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }


        public MainViewModel() {
            _mqttService = new MqttService();
            _mqttService.OnDataReceived += HandleData;

            Task.Run(() => _mqttService.ConnectAsync());

            Series = new ISeries[] {
                new ColumnSeries<double> {
                    Values = new List<double>(),
                    Name = "Temperature (°C)"
                }
            };

            XAxes = new Axis[] {
                new Axis {
                    Labels = new List<string>(),
                    Name = "Machine"
                }
            };

            YAxes = new Axis[] {
                new Axis {
                    Name = "°C"
                }
            };

            var publisher = new MqttPublisher();
            Task.Run(() => publisher.StartSimulationAsync());
        }

        private void HandleData(MachineData data) {
            App.Current.Dispatcher.Invoke(() => {
                Machines.Add(data);
                if (Machines.Count > 15)
                    Machines.RemoveAt(0);

                var colSeries = (ColumnSeries<double>)Series[0];
                var values = Machines.Select(m => m.Temperature).ToList();
                colSeries.Values = values;

                XAxes[0].Labels = Machines.Select(m => m.MachineName).ToList();

                OnPropertyChanged(nameof(Series));
                OnPropertyChanged(nameof(XAxes));
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
