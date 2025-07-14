using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TriviaDungeonCrawler
{
    public class QuestionObj
    {
        private string _difficulty;
        private string _category;
        private string _question;
        private string _correct_answer;
        private string[] _incorrect_answers;

        public string Difficulty
        {
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
        public string[] Incorrect_answers
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
}
