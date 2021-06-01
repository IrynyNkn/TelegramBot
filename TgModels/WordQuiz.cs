using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishBot.TgModels
{
    public class Quizlist
    {
        public List<string> Quiz { get; set; }
        public List<string> Option { get; set; }
        public int Correct { get; set; }
    }

    public class WordQuiz
    {
        public int Level { get; set; }
        public List<Quizlist> Quizlist { get; set; }
    }
}
