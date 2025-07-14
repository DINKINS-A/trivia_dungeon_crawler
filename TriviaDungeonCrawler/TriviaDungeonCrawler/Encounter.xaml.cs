namespace TriviaDungeonCrawler;

public partial class Encounter : ContentPage
{
	Button _correct_answer;
	private Label _healthLabel;

	public Encounter()
	{
		InitializeComponent();
	}

	public Encounter(QuestionObj question, IView enemy, Label healthLabel)
	{
		_healthLabel = healthLabel;
		InitializeComponent();
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

			if (answer.Equals(question.Correct_answer)) 
			{
				_correct_answer = button;
			}

			button.Clicked += async (s, e) =>
			{
                if (s is Button)
                {
					var clickedAnswer = ((Button)s);
					if (clickedAnswer.Equals(_correct_answer)) 
					{
                        clickedAnswer.Background = Color.FromHsv(115, 500, 100);
                    } else
					{
                        clickedAnswer.Background = Color.FromHsv(10, 85, 100);
						_correct_answer.Background = Color.FromHsv(115, 500, 100);

						int damage = 0;

						switch(question.Difficulty)
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

					Player.Health -= 10;
					_healthLabel.Text = $"Health: {Player.Health.ToString()}";
                    }
						
                }
                await Task.Delay(1000);
				await Navigation.PopAsync();
			};

            content.Add(button);
		}

		content.Padding = new Thickness(30, 0);
		content.Spacing = 25;

		scrollView.Content = content;
		this.Content = scrollView;
	}

	private async void OnCloseClicked(object? sender, EventArgs e) => await Navigation.PopAsync();
}