using Microsoft.Maui.Controls;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace TriviaDungeonCrawler;

public partial class Game : ContentPage
{
    private ImageButton? up;
    private ImageButton? down;
    private ImageButton? left;
    private ImageButton? right;

	public Game()
	{
		InitializeComponent();
		CreateLevel();
	}

    /*
     * Creates maze and populates it with enemies
     */
	private void CreateLevel()
	{
        Grid grid = CreateGrid();
        CreateArrowButtons();

        HorizontalStackLayout firstArrowContainer = new HorizontalStackLayout();
        firstArrowContainer.Children.Add(up);
        firstArrowContainer.Children.Add(down);

        HorizontalStackLayout secondArrowContainer = new HorizontalStackLayout();
        secondArrowContainer.Children.Add(left);
        secondArrowContainer.Children.Add(right);

        firstArrowContainer.HorizontalOptions = LayoutOptions.Center;
        secondArrowContainer.HorizontalOptions = LayoutOptions.Center;

        var content = new VerticalStackLayout
        {
            new Label
            {
                Text = "Gameplay Screen",
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
            },
            grid,
            firstArrowContainer,
            secondArrowContainer,
        };

        this.Content = content;
	}

    /*
     * Create the Grid container for the maze
     */
	private Grid CreateGrid()
	{
		Grid grid = new Grid();

        for (int i = 0; i < 10; i++)
        {
            RowDefinition row = new RowDefinition
            {
                Height = GridLength.Auto
            };
            grid.AddRowDefinition(row);

            ColumnDefinition column = new ColumnDefinition
            {
                Width = GridLength.Auto
            };
            grid.AddColumnDefinition(column);
        }

        for(int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                grid.Add(new Image
                {
                    Source = "wall_all.png"
                }, i, j);
            }
        }

        grid.HorizontalOptions = LayoutOptions.Center;
        grid.Margin = new Thickness(0, 10, 0, 10);

        return grid;
	}

    /* 
     * Create controls for player movement
     */
    private void CreateArrowButtons()
    {
        up = new ImageButton
        {
            Source = "arrow_up.png"
        };
        up.Clicked += (s, e) => { UpArrowClicked(s, e); };

        down = new ImageButton
        {
            Source = "arrow_down.png"
        };
        down.Clicked += (s, e) => { DownArrowClicked(s, e); };

        left = new ImageButton
        {
            Source = "arrow_left.png"
        };
        left.Clicked += (s, e) => { LeftArrowClicked(s, e); };

        right = new ImageButton
        {
            Source = "arrow_right.png"
        };
        right.Clicked += (s, e) => { RightArrowClicked(s, e); };

    }

    private void UpArrowClicked(object? sender, EventArgs e)
    {

    }

    private void DownArrowClicked(object? sender, EventArgs e)
    {

    }

    private void LeftArrowClicked(object? sender, EventArgs e)
    {

    }

    private void RightArrowClicked(object? sender, EventArgs e)
    {

    }

}