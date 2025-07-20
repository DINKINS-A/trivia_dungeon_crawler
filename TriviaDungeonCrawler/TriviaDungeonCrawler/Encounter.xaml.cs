
namespace TriviaDungeonCrawler;

public partial class Encounter : ContentPage
{
	Button _correct_answer;
	private List<Button> _incorrect_answers = [];
	private Label _healthLabel;
	private Label _scoreLabel;
	

	public Encounter()
	{
		InitializeComponent();
		Shell.SetNavBarIsVisible(this, false);
	}

	public Encounter(QuestionObj question, IView enemy, Label healthLabel, Label scoreLabel)
	{
		_healthLabel = healthLabel;
		_scoreLabel = scoreLabel;
		InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);
        CreateQuestionView(question, enemy);
	}

	private void CreateQuestionView(QuestionObj question, IView enemy)
	{
		ScrollView scrollView = new();

		Label category = new();
		category.Text = question.Category;

		Label questionLabel = new();
		questionLabel.Text = question.Question;

        Random random = new();
        var answers = question.Incorrect_answers.Append(question.Correct_answer)
												.OrderBy(_ => random.Next());


		category.FontFamily = "NotoSansMath";
		questionLabel.FontFamily = "NotoSansMath";

		category.HorizontalOptions = LayoutOptions.Center;
		questionLabel.HorizontalOptions = LayoutOptions.Center;

		var content = new VerticalStackLayout
		{
			category,
			enemy,
			questionLabel,
		};

		foreach(var answer in answers)
		{
			var button = new Button
			{
				Text = answer,
			};

            int damage = 0;

            switch (question.Difficulty)
            {
                case "EASY":
                    damage = 1;
                    break;
                case "MEDIUM":
                    damage = 5;
                    break;
                case "HARD":
                    damage = 10;
                    break;
            }

            if (answer.Equals(question.Correct_answer)) 
			{
				_correct_answer = button;
				
			} else
			{
				_incorrect_answers.Add(button);
			}

				button.Clicked += async (s, e) =>
				{
					if (s is Button)
					{

						foreach (var answer in _incorrect_answers)
						{
							answer.IsEnabled = false;
						}

						var clickedAnswer = ((Button)s);
						if (clickedAnswer.Equals(_correct_answer))
						{
							clickedAnswer.Background = Color.FromHsv(115, 500, 100);
                            Player.Score += damage;
							_scoreLabel.Text = $"Score: {Player.Score}";
                        }
						else
						{
							clickedAnswer.Background = Color.FromHsv(10, 85, 100);
							_correct_answer.Background = Color.FromHsv(115, 500, 100);


							Player.Health -= damage;
							
						}

					}
					await Task.Delay(1000);
					await Navigation.PopAsync();
                    _healthLabel.Text = $"Health: {Player.Health}";
                };

			button.FontFamily = "NotoSansMath";
            content.Add(button);
		}

		

		content.Padding = new Thickness(30, 0);
		content.Spacing = 25;

		scrollView.Content = content;


		this.Content = scrollView;
	}

	private async void OnCloseClicked(object? sender, EventArgs e) => await Navigation.PopAsync();
}