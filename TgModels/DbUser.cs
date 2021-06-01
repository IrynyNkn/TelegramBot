using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishBot.TgModels
{
    public class DbUser
    {
        public int ChatId { get; set; }
        public List<UserWords> VocabItems { get; set; }
    }
}
