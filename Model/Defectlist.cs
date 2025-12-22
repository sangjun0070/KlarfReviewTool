using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlarfReviewTool.Model
{
    public class Defectlist
    {
        public ObservableCollection<Defect> Defects { get; set; } = new ObservableCollection<Defect>();
    }
}

