using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPlot.App
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ChartTester ViewModel { get; } = new ChartTester();

        public MainPage()
        {            
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            HandleInitAsync();
        }

        private async void HandleInitAsync()
        {
            try
            {
                await ViewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            HandleAsync(() => ViewModel.Change());
        }

        private async void HandleAsync(Func<Task> func)
        {
            await func();
        }
    }
}
