namespace TriviaDungeonCrawler
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        protected override bool OnBackButtonPressed()
        {
            // do nothing
            return true;
        }
    }
}
