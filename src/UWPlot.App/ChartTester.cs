using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace UWPlot.App
{
    public class ChartTester : INotifyPropertyChanged
    {
        private ElementTheme _elementTheme = ThemeSelectorService.Theme;
        public ElementTheme ElementTheme
        {
            get { return _elementTheme; }

            set 
            { 
                _elementTheme = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ElementTheme"));
            }
        }

        private ICommand _switchThemeCommand;

        public ICommand SwitchThemeCommand
        {
            get
            {
                if (_switchThemeCommand == null)
                {
                    _switchThemeCommand = new RelayCommand<ElementTheme>(
                        async (param) =>
                        {
                            ElementTheme = param;
                            await ThemeSelectorService.SetThemeAsync(param);
                        });
                }

                return _switchThemeCommand;
            }
        }

        public ObservableCollection<DataPoint> Data1 { get; } = new ObservableCollection<DataPoint>();
        public ObservableCollection<DataPoint> Data2 { get; } = new ObservableCollection<DataPoint>();
        public ObservableCollection<DataPoint> Data3 { get; } = new ObservableCollection<DataPoint>();

        public ObservableCollection<LedgerPoint> Data4 { get; } = new ObservableCollection<LedgerPoint>();
        public ObservableCollection<LedgerPoint> Data4_1 { get; } = new ObservableCollection<LedgerPoint>();
        public ObservableCollection<LedgerPoint> Data5 { get; } = new ObservableCollection<LedgerPoint>();
        public ObservableCollection<LedgerPoint> Data6 { get; } = new ObservableCollection<LedgerPoint>();


        public event PropertyChangedEventHandler PropertyChanged;

        public Task InitializeAsync()
        {
            return Task.Run(() =>
            {
                int count = 18;

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

                Data4.Add(new LedgerPoint() { Category = "Ledger 1", Name = "Data4-1", Value = -1000 });
                Data4.Add(new LedgerPoint() { Category = "Ledger 2", Name = "Data4-2", Value = -1000 });
                Data4.Add(new LedgerPoint() { Category = "Ledger 3", Name = "Data4-3", Value = -1000 });
                Data4.Add(new LedgerPoint() { Category = "Ledger 4", Name = "Data4-4", Value = -8000 });
                Data4.Add(new LedgerPoint() { Category = "Ledger 5", Name = "Data4-5", Value = -10000 });

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Data4)));

                Data4_1.Add(new LedgerPoint() { Category = "Ledger 1", Name = "Data4_1-1", Value = 0 });
                Data4_1.Add(new LedgerPoint() { Category = "Ledger 2", Name = "Data4_1-2", Value = -15200 });
                Data4_1.Add(new LedgerPoint() { Category = "Ledger 3", Name = "Data4_1-3", Value = -33560 });
                Data4_1.Add(new LedgerPoint() { Category = "Ledger 4", Name = "Data4_1-4", Value = -65200 });
                Data4_1.Add(new LedgerPoint() { Category = "Ledger 5", Name = "Data4_1-5", Value = -98562 });
                Data4_1.Add(new LedgerPoint() { Category = "Ledger 6", Name = "Data4_1-6", Value = -120854 });
                Data4_1.Add(new LedgerPoint() { Category = "Ledger 7", Name = "Data4_1-7", Value = -150054 });
                Data4_1.Add(new LedgerPoint() { Category = "Ledger 8", Name = "Data4_1-8", Value = -186452 });
                Data4_1.Add(new LedgerPoint() { Category = "Ledger 9", Name = "Data4_1-9", Value = -223000 });
                Data4_1.Add(new LedgerPoint() { Category = "Ledger 10", Name = "Data4_1-10", Value = -259654 });

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Data4_1)));

                Data5.Add(new LedgerPoint() { Category = "Ledger 1", Name = "Data5-1", Value = 0 });
                Data5.Add(new LedgerPoint() { Category = "Ledger 2", Name = "Data5-2", Value = 0 });
                Data5.Add(new LedgerPoint() { Category = "Ledger 3", Name = "Data5-3", Value = 0 });
                Data5.Add(new LedgerPoint() { Category = "Ledger 4", Name = "Data5-4", Value = 0 });
                Data5.Add(new LedgerPoint() { Category = "Ledger 5", Name = "Data5-5", Value = 0 });
                Data5.Add(new LedgerPoint() { Category = "Ledger 6", Name = "Data5-6", Value = 0 });
                Data5.Add(new LedgerPoint() { Category = "Ledger 7", Name = "Data5-7", Value = 0 });
                Data5.Add(new LedgerPoint() { Category = "Ledger 8", Name = "Data5-8", Value = 0 });
                Data5.Add(new LedgerPoint() { Category = "Ledger 9", Name = "Data5-9", Value = 0 });
                Data5.Add(new LedgerPoint() { Category = "Ledger 10", Name = "Data5-10", Value = -340 });
                Data5.Add(new LedgerPoint() { Category = "Ledger 6", Name = "Data4-6", Value = -100 });
                Data5.Add(new LedgerPoint() { Category = "Ledger 7", Name = "Data4-7", Value = 0 });
                Data5.Add(new LedgerPoint() { Category = "Ledger 8", Name = "Data4-8", Value = 0 });
                Data5.Add(new LedgerPoint() { Category = "Ledger 9", Name = "Data4-9", Value = 0 });
                Data5.Add(new LedgerPoint() { Category = "Ledger 10", Name = "Data4-10", Value = 0 });
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Data5)));

                Data6.Add(new LedgerPoint() { Category = "Ledger 1", Name = "Data6-1", Value = -4500 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 2", Name = "Data6-2", Value = -3400 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 3", Name = "Data6-3", Value = -65890 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 4", Name = "Data6-4", Value = -24000 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 5", Name = "Data6-5", Value = -15000 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 6", Name = "Data6-6", Value = -9000 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 7", Name = "Data6-7", Value = -2500 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 8", Name = "Data6-8", Value = -100 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 9", Name = "Data6-9", Value = -1000 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 10", Name = "Data6-10", Value = -3400 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 11", Name = "Data6-1", Value = -4500 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 12", Name = "Data6-2", Value = -3400 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 13", Name = "Data6-3", Value = -65890 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 14", Name = "Data6-4", Value = -24000 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 15", Name = "Data6-5", Value = -15000 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 16", Name = "Data6-6", Value = -9000 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 17", Name = "Data6-7", Value = -2500 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 18", Name = "Data6-8", Value = -100 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 19", Name = "Data6-9", Value = -1000 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 20", Name = "Data6-10", Value = -3400 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 21", Name = "Data6-1", Value = -4500 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 22", Name = "Data6-2", Value = -3400 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 23", Name = "Data6-3", Value = -65890 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 24", Name = "Data6-4", Value = -24000 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 25", Name = "Data6-5", Value = -15000 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 26", Name = "Data6-6", Value = -9000 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 27", Name = "Data6-7", Value = -2500 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 28", Name = "Data6-8", Value = -100 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 29", Name = "Data6-9", Value = -1000 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 30", Name = "Data6-10", Value = -3400 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 31", Name = "Data6-1", Value = -4500 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 32", Name = "Data6-2", Value = -3400 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 33", Name = "Data6-3", Value = -65890 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 34", Name = "Data6-4", Value = -24000 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 35", Name = "Data6-5", Value = -15000 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 36", Name = "Data6-6", Value = -9000 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 37", Name = "Data6-7", Value = -2500 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 38", Name = "Data6-8", Value = -100 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 39", Name = "Data6-9", Value = -1000 });
                Data6.Add(new LedgerPoint() { Category = "Ledger 40", Name = "Data6-10", Value = -3400 });
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Data6)));
            });
        }

        private static double Evaluate(int x, Func<int, double> multiplier, Func<double, double> equation)
        {
            return equation(multiplier(x));
        }

        public Task Change()
        {
            Data1.Clear();

            int count = 18;

            var rnd = new Random();
            Enumerable.Range(0, count).Select(x => new DataPoint()
            {
                Value = x * rnd.Next(100),
                Name = $"Data1-{x}",
                Category = DateTime.Today.AddDays(x)
            })
                .ToList()
                .ForEach(x => Data1.Add(x));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Data1)));

            return Task.CompletedTask;
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
