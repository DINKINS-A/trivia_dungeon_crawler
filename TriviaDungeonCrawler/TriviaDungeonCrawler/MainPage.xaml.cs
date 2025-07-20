using System.Threading.Tasks;

namespace TriviaDungeonCrawler
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
           InitializeComponent();
        }

        private async void OnStartClicked(object? sender, EventArgs e)
        {
            if (sender is Button)
            {
                

                var temp = ((Button)sender).Background;
                ((Button)sender).Background = Color.FromHsv(100, 0, 100);
                await Task.Delay(100);
                ((Button)sender).Background = temp;

                ((Button)sender).IsEnabled = false;

                Player.Health = 50;
                
                await Navigation.PushAsync(new Game());

                ((Button)sender).IsEnabled = true;
            }
            
        }

        private async void OnExitClicked(object? sender, EventArgs e) => Application.Current.Quit();

    }
}
