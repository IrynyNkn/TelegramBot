using EnglishBot.TgModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishBot
{
    public class TgUser
    {
        public int Id { get; set; }

        public TgUser(int id)
        {
            Id = id;
            rights = 0;
            wrongs = 0;
        }

        public int GameIterator { get; set; }
        public int rights { get; set; }
        public int wrongs { get; set; }

        public UserWords globalUserWord = new UserWords();

        public WordQuiz GlobalQuiz = new WordQuiz();
    }
}
