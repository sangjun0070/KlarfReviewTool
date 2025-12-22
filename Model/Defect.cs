using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlarfReviewTool.Model
{
    public class Defect
    {
        public int DefectID { get; set; }
        public double XRel { get; set; }
        public double YRel { get; set; }
        public int XIndex { get; set; }
        public int YIndex { get; set; }
        public double XSize { get; set; }
        public double YSize { get; set; }
        public double DefectArea { get; set; }
        public double DSize { get; set; }
        public int ClassNumber { get; set; }
        public int Test { get; set; }
        public int ClusterNumber { get; set; }
        public int RoughBinNumber { get; set; }
        public int FineBinNumber { get; set; }
        public int ReviewSample { get; set; }
        public int ImageCount { get; set; }
        public string ImageList { get; set; }
    }
}
