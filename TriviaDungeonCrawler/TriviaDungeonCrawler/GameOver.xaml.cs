namespace TriviaDungeonCrawler;

public partial class GameOver : ContentPage
{
	public GameOver()
	{
		InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);

		MakeLayout();
    }

	private  void MakeLayout()
	{
		var button = new Button();
		button.Text = "HOME";

		button.Clicked += (s, e) => { OnHomeClicked(s, e); };
		button.HorizontalOptions = LayoutOptions.Fill;

		this.Content = new VerticalStackLayout 
		{
			new Label
			{
				Text = "GameOver!",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
			},
			
			new Label
			{
				Text = $"Final Score: {Player.Score.ToString()}",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions= LayoutOptions.Center,
			},
            button,
        };
	}

	public async void OnHomeClicked(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}
}