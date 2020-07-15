using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueMasteryChecker
{
    public class Champ
    {
        public string Name { get; set; } = "";
        public long Id { get; set; } = 0;
        public int masteryLevel { get; set; } = 0;
        public int masteryPoints { get; set; } = 0;
        public long masteryPointsUntilNextLvl { get; set; } = 0;
        public string summonerId { get; set; } = "";
    }
}
