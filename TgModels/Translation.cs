using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnglishBot.TgModels
{
    public class Translation
    {
        public Data data { get; set; }
    }
    
    public class Data
    {
        public string translation { get; set; }
    }
}
