using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlarfReviewTool.Model
{
    public class Die
    {
        public int XIndex { get; set; }
        public int YIndex { get; set; }
        public bool HasDefect { get; set; }
        public List<Defect> Defects { get; set; } = new List<Defect>();
    }
}

