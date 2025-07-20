using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriviaDungeonCrawler
{
    public class Player
    {
        static int _health = 50;
        static int _score = 0;

        public static int Health { get { return _health; } set { _health = value; } }
        public static int Score { get { return _score; } set { _score = value; } }
    }
}
