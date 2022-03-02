using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace UWPlot.App
{
    public class ChartTester : INotifyPropertyChanged
    {
        public ObservableCollection<DataPoint> Data1 { get; } = new ObservableCollection<DataPoint>();
        public ObservableCollection<DataPoint> Data2 { get; } = new ObservableCollection<DataPoint>();
        public ObservableCollection<DataPoint> Data3 { get; } = new ObservableCollection<DataPoint>();

        public event PropertyChangedEventHandler PropertyChanged;

        public void Initialize()
        {
            int count = 10;

            var rnd = new Random();
            Enumerable.Range(0, count).Select(x => new DataPoint()
            {
                Value = x * 50,
                Name = $"Data1-{x}",
                Category = DateTime.Today.AddDays(x)
            })
                .ToList()
                .ForEach(x => Data1.Add(x));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Data1)));

            Enumerable.Range(0, count).Select(x => new DataPoint()
            {
                Value = 500 - (x * 50),
                Name = $"Data2-{x}",
                Category = DateTime.Today.AddDays(x)
            })
                .ToList()
                .ForEach(x => Data2.Add(x));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Data2)));

            Enumerable.Range(0, count).Select(x => new DataPoint()
            {
                Value = Evaluate(x, v => v * 25 + 200, v => (v * v) - (v * 500) + 50000),
                Name = $"Data3-{x}",
                Category = DateTime.Today.AddDays(x)
            })
                .ToList()
                .ForEach(x => Data3.Add(x));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Data3)));
        }

        private static double Evaluate(int x, Func<int, double> multiplier, Func<double, double> equation)
        {
            return equation(multiplier(x));
        }
    }

    public class DataPoint
    {
        public double Value { get; set; }
        public string Name { get; set; }
        public DateTime Category { get; set; }
    }
}
