﻿using System.Threading.Tasks;

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
            await Navigation.PushAsync(new Game());
        }

        private void OnExitClicked(object? sender, EventArgs e) => Application.Current.Quit();

    }
}
