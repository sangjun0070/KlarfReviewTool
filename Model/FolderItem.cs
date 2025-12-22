using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KlarfReviewTool.Model
{
    public class FolderItem
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public ObservableCollection<FileInfo> Files { get; set; }
        public ObservableCollection<FolderItem> Children { get; set; }

        public FolderItem(string fullPath)
        {
            FullPath = fullPath;
            Name = Path.GetFileName(fullPath) == string.Empty ? fullPath : Path.GetFileName(fullPath);
            Children = new ObservableCollection<FolderItem>();
        }
    }
}
