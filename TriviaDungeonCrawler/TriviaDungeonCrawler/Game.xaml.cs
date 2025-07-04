using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Input;

namespace TriviaDungeonCrawler;

public partial class Game : ContentPage
{
    private const int MAZE_HEIGHT = 10;
    private const int MAZE_WIDTH = 10;

    private ImageButton? up;
    private ImageButton? down;
    private ImageButton? left;
    private ImageButton? right;

    private List<Cell> maze = [];
    private Cell start;
    private Cell current;
    private Grid mazeUIControl = [];

    private static readonly Image knight = new() { Source = "knight.png", Aspect = Aspect.AspectFit };

	public Game()
	{
		InitializeComponent();
        CreateMaze();
		CreateLevel();
	}

    /*
     * Creates maze and populates it with enemies
     */
	private void CreateLevel()
	{
        var grid = CreateGrid();
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
	private AbsoluteLayout CreateGrid()
	{
		Grid grid = [];

        for (int i = 0; i < 10; i++)
        {
            RowDefinition row = new RowDefinition
            {
                Height = new GridLength(32)
            };
            grid.AddRowDefinition(row);

            RowDefinition fogRow = new RowDefinition
            {
                Height = new GridLength(32)
            };
            mazeUIControl.AddRowDefinition(fogRow);

            ColumnDefinition column = new ColumnDefinition
            {
                Width = new GridLength(32)
            };
            grid.AddColumnDefinition(column);

            ColumnDefinition fogColumn = new ColumnDefinition
            {
                Width = new GridLength(32)
            };
            mazeUIControl.AddColumnDefinition(fogColumn);
        }

        // Add wall images to grid
        for(int i = 0; i < MAZE_HEIGHT; i++)
        {
            for (int j = 0; j < MAZE_WIDTH; j++)
            {
                string pathToImage = GetWallImage(i, j);
                grid.Add(new Image
                {
                    Source = pathToImage
                }, i, j);
            }
        }

        // Add character image to grid at the starting cell
        mazeUIControl.Add(knight, start.coordniate.Item1, start.coordniate.Item2);

        grid.HorizontalOptions = LayoutOptions.Center;
        grid.Margin = new Thickness(0, 10, 0, 10);

        mazeUIControl.HorizontalOptions = LayoutOptions.Center;
        mazeUIControl.Margin = new Thickness(0, 10, 0, 10);


        AbsoluteLayout display = [];

        display.HorizontalOptions = LayoutOptions.Center;

        display.Add(grid);
        display.Add(mazeUIControl);

        return display;
	}

    /*
     * Returns path to image correspoing to the number of walls the cell at given corrdinate has
     */
    private string GetWallImage(int x, int y)
    {
        Cell cell = maze[x * MAZE_HEIGHT + y];

        // Check which walls have been torn down and return corresponding path to image
        if (cell.north_wall && cell.east_wall && cell.south_wall)
        {
            return "wall_nse.png";
        }
        else if (cell.north_wall && cell.south_wall && cell.west_wall) 
        {
            return "wall_nsw.png";
        }
        else if (cell.north_wall && cell.east_wall && cell.west_wall) 
        {
            return "wall_new.png";
        }
        else if (cell.south_wall && cell.east_wall && cell.west_wall) 
        {
            return "wall_sew.png";
        }
        else if (cell.north_wall && cell.south_wall)
        {
            return "wall_ns.png";
        }
        else if (cell.north_wall && cell.east_wall)
        {
            return "wall_ne.png";
        }
        else if (cell.north_wall && cell.west_wall)
        {
            return "wall_nw.png";
        }
        else if (cell.south_wall && cell.east_wall)
        {
            return "wall_se.png";
        }
        else if (cell.south_wall && cell.west_wall)
        {
            return "wall_sw.png";
        }
        else if (cell.east_wall && cell.west_wall)
        {
            return "wall_ew.png";
        }
        else if (cell.north_wall)
        {
            return "wall_n.png";
        }
        else if (cell.south_wall)
        {
            return "wall_s.png";
        }
        else if (cell.east_wall)
        {
            return "wall_e.png";
        }
        else if (cell.west_wall)
        {
            return "wall_w.png";
        }

        return "wall_none.png";
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
        var x = current.coordniate.Item1;
        var y = current.coordniate.Item2;
        // if there is a neighbor above
        if (!current.HasWall("north") && current.HasNeightborAtLoc(x, y - 1))
        {
            mazeUIControl.Remove(knight);
            mazeUIControl.Add(knight, x, y - 1);

            current = maze[x * MAZE_HEIGHT + (y - 1)];
        }
    }
        
    private void DownArrowClicked(object? sender, EventArgs e)
    {
        var x = current.coordniate.Item1;
        var y = current.coordniate.Item2;
        // if there is a neighbor above
        if (!current.HasWall("south") && current.HasNeightborAtLoc(x, y + 1))
        {
            mazeUIControl.Remove(knight);
            mazeUIControl.Add(knight, x, y + 1);

            current = maze[x * MAZE_HEIGHT + (y + 1)];
        }
    }

    private void LeftArrowClicked(object? sender, EventArgs e)
    {
        var x = current.coordniate.Item1;
        var y = current.coordniate.Item2;
        // if there is a neighbor above
        if (!current.HasWall("west") && current.HasNeightborAtLoc(x - 1, y))
        {
            mazeUIControl.Remove(knight);
            mazeUIControl.Add(knight, x - 1, y);

            current = maze[(x - 1) * MAZE_HEIGHT + y];
        }
    }

    private void RightArrowClicked(object? sender, EventArgs e)
    {
        var x = current.coordniate.Item1;
        var y = current.coordniate.Item2;
        // if there is a neighbor above
        if (!current.HasWall("east") && current.HasNeightborAtLoc(x + 1, y))
        {
            mazeUIControl.Remove(knight);
            mazeUIControl.Add(knight, x + 1, y);

            current = maze[(x + 1) * MAZE_HEIGHT + y];
        }
    }

    private void InitializeMaze()
    {
        for (int i = 0; i < MAZE_HEIGHT * MAZE_WIDTH; i++)
        {
            maze.Add(new Cell(i / MAZE_HEIGHT, i % MAZE_WIDTH));
        }
    }
    
    private void CreateMaze()
    {
        InitializeMaze();
        PopulateNeighbors();

        // Choose starting cell
        Random rand = new();
        int randX = rand.Next(MAZE_HEIGHT);
        int randY = rand.Next(MAZE_HEIGHT);
        start = maze[randX * MAZE_HEIGHT + randY];
        current = start;
        RunDepthFirstSearch(start);
    }

    private void RunDepthFirstSearch(Cell cell)
    {
        cell.visited = true;
        while (HasUnvisitedNeighbor(cell))
        {
            Cell neighbor = GetRandomNeighbor(cell);
            RunDepthFirstSearch(neighbor);
        }
    }

    /* 
     * Returns true if this cell has a neighbor that has not been visited
     */
    private static bool HasUnvisitedNeighbor(Cell cell) => cell.neighbors.FindAll(neighbor => !neighbor.visited).Count() > 0;

    /*
     * Add neighbors to each cell
     */
    private void PopulateNeighbors()
    {
        for (int i = 0; i < MAZE_HEIGHT; i++)
        {
            for (int j = 0; j < MAZE_WIDTH; j++)
            {
                if (j > 0)
                {
                    // left neighbor
                    maze[i * MAZE_HEIGHT + j].neighbors.Add(maze[i * MAZE_HEIGHT + (j - 1)]);
                }
                if (j < MAZE_WIDTH - 1)
                {
                    // right neighbor
                    maze[i * MAZE_HEIGHT + j].neighbors.Add(maze[i * MAZE_HEIGHT + (j + 1)]);

                }
                if (i > 0)
                {
                    // top neighbor
                    maze[i * MAZE_HEIGHT + j].neighbors.Add(maze[(i - 1) * MAZE_HEIGHT + j]);
                }
                if (i < MAZE_HEIGHT - 1)
                {
                    // bottom neighbor
                    maze[i * MAZE_HEIGHT + j].neighbors.Add(maze[(i + 1) * MAZE_HEIGHT + j]);
                }
            }
        }
        Console.WriteLine("");
    }

    private Cell GetRandomNeighbor(Cell cell)
    {
        Random r = new();
        List<Cell> neighbors = cell.neighbors.FindAll(neighbor => !neighbor.visited);
        Cell chosenNeighbor = neighbors[r.Next(neighbors.Count())];
        // tear down walls
        if (chosenNeighbor.coordniate.Item1 > cell.coordniate.Item1)
        {
            chosenNeighbor.west_wall = false;
            cell.east_wall = false;
        } else if (chosenNeighbor.coordniate.Item1 < cell.coordniate.Item1) {
            chosenNeighbor.east_wall = false;
            cell.west_wall = false;
        } else if (chosenNeighbor.coordniate.Item2 > cell.coordniate.Item2)
        {
            chosenNeighbor.north_wall = false;
            cell.south_wall = false;
        } else if (chosenNeighbor.coordniate.Item2 < cell.coordniate.Item2) {
            chosenNeighbor.south_wall = false;
            cell.north_wall = false;
        }

        return chosenNeighbor;
    }

    private class Cell
    {
        public Tuple<int, int> coordniate;
        public bool north_wall = true;
        public bool south_wall = true;
        public bool east_wall = true;
        public bool west_wall = true;
        public bool visited = false;
        public List<Cell> neighbors = new List<Cell>();

        public Cell(int x, int y)
        {
            coordniate = new Tuple<int, int>(x, y);
        }

        public bool HasNeightborAtLoc(int x, int y) => neighbors.Where(
            neighbor => neighbor.coordniate.Item1 == x &&
                        neighbor.coordniate.Item2 == y).Count() == 1;

        public bool HasWall(string wall)
        {
            switch (wall)
            {
                case "north": 
                    return north_wall;

                case "east":
                    return east_wall;

                case "south":
                    return south_wall;

                case "west":
                    return west_wall;

                default:
                    return false;
            }
        }
    };

}