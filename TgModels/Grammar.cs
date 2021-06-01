using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishBot.TgModels
{
    public class Grammar
    {
        public List<Matches> matches { get; set; }
    }

    public class Matches
    {
        public string Message { get; set; }
        public List<Replacements> replacements { get; set; }
        public Rule rule { get; set; }
    }

    public class Replacements
    {
        public string Value { get; set; }
    }

    public class Rule
    {
        public string description { get; set; }
    }
}
