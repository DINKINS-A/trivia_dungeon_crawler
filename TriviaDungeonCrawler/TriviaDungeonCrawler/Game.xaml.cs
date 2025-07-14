using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Layouts;
using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Windows.Input;
using System.Web;
using System.Collections.Concurrent;

namespace TriviaDungeonCrawler;

public partial class Game : ContentPage
{
    private const int MAZE_HEIGHT = 10;
    private const int MAZE_WIDTH = 10;

    private static int current_level = 0;

    private ImageButton? up;
    private ImageButton? down;
    private ImageButton? left;
    private ImageButton? right;

    private List<Cell> maze = [];
    private Cell start;
    private Cell current;
    private Cell exit;
    private Grid mazeUIControl = [];
    private Grid fogUIControl = [];
    private List<Tuple<int, int>> enemy_coordinates = [];

    private static readonly Image knight = new() { Source = "knight.png", Aspect = Aspect.AspectFit };
    private static readonly Image door = new() { Source = "exit.png", Aspect = Aspect.AspectFit };

    private static readonly List<int> question_categories = [17, 18, 19];

    private static readonly string sessionTokenURI = "https://opentdb.com/api_token.php?command=request";
    private static readonly string questionURI = "https://opentdb.com/api.php?amount=50";

    HttpClient _client;
    JsonSerializerOptions _serializerOptions;

    private static ConcurrentBag<QuestionObj> Questions { get; set; }

    public Game()
	{
		InitializeComponent();
        CreateMaze();
		CreateLevel();

        if (Questions == null)
        {
            InitalizeHttpClient();
            Questions = [];
            GetQuestions();
        }
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

    private void ClearUIGrid()
    {
        while(mazeUIControl.Count > 0)
        {
            mazeUIControl.RemoveAt(0);
        }
    }

    /*
     * Create the Grid container for the maze
     */
	private Layout CreateGrid()
	{
		Grid grid = [];

        for (int i = 0; i < MAZE_HEIGHT; i++)
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
            fogUIControl.AddRowDefinition(fogRow);
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
            fogUIControl.AddColumnDefinition(fogColumn);
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
        mazeUIControl.Add(knight, start.coordinate.Item1, start.coordinate.Item2);

        // Add exit image to grid at the exit cell
        mazeUIControl.Add(door, exit.coordinate.Item1, exit.coordinate.Item2);

        // Add enemies images to grid.
        foreach(var enemy_coordinate in enemy_coordinates)
        {
            var enemy = maze[enemy_coordinate.Item1 * MAZE_HEIGHT + enemy_coordinate.Item2].Enemy;

            switch (enemy)
            {
                case "EASY":
                    mazeUIControl.Add(new Image
                    {
                        Source = "green_slime.png"
                    }, enemy_coordinate.Item1, enemy_coordinate.Item2);
                    continue;
                case "MEDIUM":
                    mazeUIControl.Add(new Image
                    {
                        Source = "blue_slime.png"
                    }, enemy_coordinate.Item1, enemy_coordinate.Item2);
                    continue;
                case "HARD":
                    mazeUIControl.Add(new Image
                    {
                        Source = "red_slime.png"
                    }, enemy_coordinate.Item1, enemy_coordinate.Item2);
                    break;
            }
        }

        // Add fog images to ui grid
        for (int i = 0; i < MAZE_HEIGHT; i++)
        {
            for (int j = 0; j < MAZE_WIDTH; j++)
            {
                string pathToImage = "fog.png";
                fogUIControl.Add(new Image
                {
                    Source = pathToImage
                }, i, j);
            }
        }

        // set initial vision
        ProvideVision();

        grid.HorizontalOptions = LayoutOptions.Center;
        grid.Margin = new Thickness(0, 10, 0, 10);

        mazeUIControl.HorizontalOptions = LayoutOptions.Center;
        mazeUIControl.Margin = new Thickness(0, 10, 0, 10);

        fogUIControl.HorizontalOptions = LayoutOptions.Center;
        fogUIControl.Margin = new Thickness(0, 10, 0, 10);


        AbsoluteLayout display = [];

        display.HorizontalOptions = LayoutOptions.Center;

        display.Add(grid);
        display.Add(mazeUIControl);
        display.Add(fogUIControl);


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
        var x = current.coordinate.Item1;
        var y = current.coordinate.Item2;
        // if there is a neighbor above
        if (!current.HasWall("north") && current.HasNeightborAtLoc(x, y - 1))
        {
            mazeUIControl.Remove(knight);
            mazeUIControl.Add(knight, x, y - 1);

            current = maze[x * MAZE_HEIGHT + (y - 1)];
            HandleClick();
        }

    }
        
    private void DownArrowClicked(object? sender, EventArgs e)
    {
        var x = current.coordinate.Item1;
        var y = current.coordinate.Item2;
        // if there is a neighbor above
        if (!current.HasWall("south") && current.HasNeightborAtLoc(x, y + 1))
        {
            mazeUIControl.Remove(knight);
            mazeUIControl.Add(knight, x, y + 1);

            current = maze[x * MAZE_HEIGHT + (y + 1)];
            HandleClick();
        }
    }

    private void LeftArrowClicked(object? sender, EventArgs e)
    {
        var x = current.coordinate.Item1;
        var y = current.coordinate.Item2;
        // if there is a neighbor above
        if (!current.HasWall("west") && current.HasNeightborAtLoc(x - 1, y))
        {
            mazeUIControl.Remove(knight);
            mazeUIControl.Add(knight, x - 1, y);

            current = maze[(x - 1) * MAZE_HEIGHT + y];
            HandleClick();
        }
    }

    private void RightArrowClicked(object? sender, EventArgs e)
    {
        var x = current.coordinate.Item1;
        var y = current.coordinate.Item2;
        // if there is a neighbor above
        if (!current.HasWall("east") && current.HasNeightborAtLoc(x + 1, y))
        {
            mazeUIControl.Remove(knight);
            mazeUIControl.Add(knight, x + 1, y);

            current = maze[(x + 1) * MAZE_HEIGHT + y];
            HandleClick();
        }
    }

    private async void HandleClick()
    {
        // When the use steps on the exit
        if (current.coordinate.Item1 == exit.coordinate.Item1 &&
            current.coordinate.Item2 == exit.coordinate.Item2)
        {
            // remove the Image views so they can be used in new grid
            ClearUIGrid();

            // use navigation stack to load new level
            current_level++;
            Navigation.InsertPageBefore(new Game(), this);
            await Navigation.PopAsync();
            return;
        }

        ProvideVision();
    }

    private void ProvideVision()
    {
        int index;

        // current square
        index = current.coordinate.Item1 * MAZE_HEIGHT + current.coordinate.Item2;
        ((Image)fogUIControl[index]).Opacity = 0;

        // if there is not a wall
        if (!current.west_wall)
        {
            index = (current.coordinate.Item1 - 1) * MAZE_HEIGHT + current.coordinate.Item2;
            ((Image)fogUIControl[index]).Opacity = 0;
        }

        // if there is not a wall
        if (!current.north_wall)
        {
            index = current.coordinate.Item1 * MAZE_HEIGHT + (current.coordinate.Item2 - 1);
            ((Image)fogUIControl[index]).Opacity = 0;
        }

        // if there is not a wall
        if (!current.east_wall)
        {
            index = (current.coordinate.Item1 + 1) * MAZE_HEIGHT + current.coordinate.Item2;
            ((Image)fogUIControl[index]).Opacity = 0;
        }


        // if there is not a wall
        if (!current.south_wall)
        {
            index = current.coordinate.Item1 * MAZE_HEIGHT + (current.coordinate.Item2 + 1);
            ((Image)fogUIControl[index]).Opacity = 0;
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

        // create maze
        RunDepthFirstSearch(start);

        // add enemies. avoid start and exit
        AddEnemies();

    }

    private void RunDepthFirstSearch(Cell cell)
    {
        cell.visited = true;
        while (HasUnvisitedNeighbor(cell))
        {
            Cell neighbor = GetRandomNeighbor(cell);
            RunDepthFirstSearch(neighbor);
        }

        // set the exit equal to first time algorithm backtracks
        if (exit == null)
        {
            exit = cell;
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
        if (chosenNeighbor.coordinate.Item1 > cell.coordinate.Item1)
        {
            chosenNeighbor.west_wall = false;
            cell.east_wall = false;
        } else if (chosenNeighbor.coordinate.Item1 < cell.coordinate.Item1) {
            chosenNeighbor.east_wall = false;
            cell.west_wall = false;
        } else if (chosenNeighbor.coordinate.Item2 > cell.coordinate.Item2)
        {
            chosenNeighbor.north_wall = false;
            cell.south_wall = false;
        } else if (chosenNeighbor.coordinate.Item2 < cell.coordinate.Item2) {
            chosenNeighbor.south_wall = false;
            cell.north_wall = false;
        }

        return chosenNeighbor;
    }

    private int GetNumEasyEnemies() 
    {
       return 3 * current_level + 1; 
    }

    private int GetNumMediumEnemies() 
    {
        return 2 * current_level + 1; 
    }

    private int GetNumHardEnemies()
    {
        return current_level + 1; 
    }

    private void AddEnemies()
    {
        // difficulties[0] = # easy enemies, difficulties[1] = # medium enemies, difficulties[2] = # hard enemies
        List<int> difficulties = 
        [
            GetNumEasyEnemies(),
            GetNumMediumEnemies(),
            GetNumHardEnemies()
        ];

        // adds enemies. avoids start and exit
        for (int difficulty_index = 0; difficulty_index < difficulties.Count; difficulty_index++) {
            for (int num_enemy = 0; num_enemy < difficulties[difficulty_index]; num_enemy++)
            {
                var enenmy_coordinate = GetValidSpawnPoint();
                var enemy_cell = maze[enenmy_coordinate.Item1 * MAZE_HEIGHT + enenmy_coordinate.Item2];
                switch (difficulty_index)
                {
                    // easy
                    case 0:
                        enemy_cell.Enemy = "EASY";
                        break;
                    // medium
                    case 1:
                        enemy_cell.Enemy = "MEDIUM";
                        break;
                    // hard
                    case 2:
                        enemy_cell.Enemy = "HARD";
                        break;
                }
            }
        }
    }

    private Tuple<int, int> GetValidSpawnPoint()
    {
        var rand = new Random();
        int randX = rand.Next(MAZE_HEIGHT);
        int randY = rand.Next(MAZE_HEIGHT);
        var potential_coordinate = Tuple.Create(randX, randY);

        // while there is another entity there
        while (enemy_coordinates.Contains(potential_coordinate) ||
            (potential_coordinate.Item1 == start.coordinate.Item1 && potential_coordinate.Item2 == start.coordinate.Item2) ||
            (potential_coordinate.Item1 == exit.coordinate.Item1 && potential_coordinate.Item2 == exit.coordinate.Item2))
        {
            // generate a new location
            randX = rand.Next(MAZE_HEIGHT);
            randY = rand.Next(MAZE_HEIGHT);
            potential_coordinate = Tuple.Create(randX, randY);
        }

        enemy_coordinates.Add(potential_coordinate);

        return potential_coordinate;
    }

    private class Cell
    {
        public Tuple<int, int> coordinate;
        public bool north_wall = true;
        public bool south_wall = true;
        public bool east_wall = true;
        public bool west_wall = true;
        public bool visited = false;
        public List<Cell> neighbors = new List<Cell>();
        public string Enemy { get; set; }
        

        public Cell(int x, int y)
        {
            coordinate = new Tuple<int, int>(x, y);
            Enemy = "";
        }

        public bool HasNeightborAtLoc(int x, int y) => neighbors.Where(
            neighbor => neighbor.coordinate.Item1 == x &&
                        neighbor.coordinate.Item2 == y).Count() == 1;

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

    private class QuestionObj
    {

        private string _difficulty;
        private string _category;
        private string _question;
        private string _correct_answer;
        private string[] _incorrect_answers;

        public string Difficulty { 
            get => _difficulty; 
            set 
            {
                _difficulty = HttpUtility.HtmlDecode(value).ToUpper();
            } 
        }

        public string Category 
        {
            get => _category;
            set 
            {
                _category = HttpUtility.HtmlDecode(value);
            }
        }
        public string Question
        {
            get => _question;
            set
            {
                _question = HttpUtility.HtmlDecode(value);
            }
        }
        public string Correct_answer
        {
            get => _correct_answer;
            set
            {
                _correct_answer = HttpUtility.HtmlDecode(value);
            }
        }
        public  string[] Incorrect_answers 
        { 
            get => _incorrect_answers;
            set 
            {
                var new_incorrect_answers = new string[value.Length];
                for (int i = 0; i < value.Length; i++)
                {
                    new_incorrect_answers[i] = HttpUtility.HtmlDecode(value[i]);
                }
                _incorrect_answers = new_incorrect_answers;
            }
        }

    }

    private void InitalizeHttpClient()
    {
        _client = new HttpClient();
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

    }

    private async void GetQuestions()
    {
        Questions = [];
        string sessionKey;

        Uri uriSessionToken = new (string.Format(sessionTokenURI, string.Empty));
        try
        {
            HttpResponseMessage response = await _client.GetAsync(uriSessionToken);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var end = content.Split(":");
                sessionKey = end[end.Length - 1].Replace("\"", "").Replace("}", "");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(@"\tERROR {0}", ex.Message);
        }

        Uri uriQuestion;
        foreach (var category in question_categories)
        {
            uriQuestion = new(string.Format($"{questionURI}&category={category}", string.Empty));

            try
            {
                HttpResponseMessage response = await _client.GetAsync(uriQuestion);
                if (response.IsSuccessStatusCode) 
                {
                    string content = await response.Content.ReadAsStringAsync();
                    var startPosition = content.IndexOf("[");
                    content = content.Substring(startPosition);
                    var endPosition = content.LastIndexOf("]");
                    var tempContent = content.Substring(endPosition);
                    content = content[..(content.Length - tempContent.Length + 1)];
                    Questions = [.. Questions.Union(JsonSerializer.Deserialize<List<QuestionObj>>(content, _serializerOptions))];
                    await Task.Delay(5000);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(@"\tERROR {0}", ex.Message);
            }
        }


    }

}