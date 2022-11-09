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

        public ObservableCollection<LedgerPoint> Data4 { get; } = new ObservableCollection<LedgerPoint>();

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

            Data4.Add(new LedgerPoint() { Category = "Ledger 1", Name = "Data4-1", Value = 4500 });
            Data4.Add(new LedgerPoint() { Category = "Ledger 2", Name = "Data4-2", Value = 3400 });
            Data4.Add(new LedgerPoint() { Category = "Ledger 3", Name = "Data4-3", Value = 65890 });
            Data4.Add(new LedgerPoint() { Category = "Ledger 4", Name = "Data4-4", Value = 24000 });
            Data4.Add(new LedgerPoint() { Category = "Ledger 5", Name = "Data4-5", Value = 15000 });
            Data4.Add(new LedgerPoint() { Category = "Ledger 6", Name = "Data4-6", Value = 9000 });
            Data4.Add(new LedgerPoint() { Category = "Ledger 7", Name = "Data4-7", Value = 2500 });
            Data4.Add(new LedgerPoint() { Category = "Ledger 8", Name = "Data4-8", Value = 1000 });
            Data4.Add(new LedgerPoint() { Category = "Ledger 9", Name = "Data4-9", Value = 1000 });
            Data4.Add(new LedgerPoint() { Category = "Ledger 10", Name = "Data4-10", Value = 3400 });

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Data4)));
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

    public class LedgerPoint
    {
        public double Value { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
    }
}
