using KlarfReviewTool.Model;
using KlarfReviewTool.VIewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MessageBox = System.Windows.Forms.MessageBox;
using Path = System.IO.Path;
using Point = System.Windows.Point;

namespace KlarfReviewTool
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public string tiffFilePath = @"C:\psj\ATI\KlarfReviewTool\Klarf\Klarf Format.tif";

        public Defectlist DefectList { get; set; } = new Defectlist();
        public Dielist DieList { get; set; } = new Dielist();

        public ObservableCollection<UIElement> WaferMapElements { get; set; } = new ObservableCollection<UIElement>();

        public TifImageIndex FrameIndex { get; set; } = new TifImageIndex();

        public ObservableCollection<FolderItem> Items { get; set; }


        private BitmapSource _imageSource;

        public BitmapSource ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged(nameof(ImageSource));
            }
        }

        private BitmapSource _changeImage;

        public BitmapSource ChangeImage
        {
            get => _changeImage;
            set
            {
                _changeImage = value;
                OnPropertyChanged(nameof(ChangeImage));
            }
        }

        public int currentDieIndex = 0;
        public int currentDefectIndex = 0;
        public int currentDefectInDieIndex = 0;

        public bool IsNextClick = true;
        public bool IsDieChanged = true;

        private Defect _selectedDefect;
        public Defect SelectedDefect
        {
            get => _selectedDefect;
            set
            {
                _selectedDefect = value;
                OnPropertyChanged(nameof(SelectedDefect));

                if (_selectedDefect != null)
                {
                    var matchingDieIndex = DieList.Dies.FindIndex(d => d.XIndex == _selectedDefect.XIndex && d.YIndex == _selectedDefect.YIndex);

                    if (matchingDieIndex != -1)
                    {
                        if (currentDieIndex != matchingDieIndex)
                        {
                            IsDieChanged = true;
                        }
                        currentDieIndex = matchingDieIndex;
                        SelectedDie = DieList.Dies[currentDieIndex];

                    }
                }
                OnPropertyChanged(nameof(SelectedDefect));

                //OnPropertyChanged(nameof(CurrentDieInfo));
                //OnPropertyChanged(nameof(CurrentDefectInfo));
                //OnPropertyChanged(nameof(CurrentDefectInDieInfo));
                CurrentDieInfo = TotalDieCount > 0 ? $"{currentDieIndex + 1} / {TotalDieCount}" : " ";
                CurrentDefectInfo = (TotalDefectCount > 0 && SelectedDefect != null) ? $"{SelectedDefect.DefectID} / {TotalDefectCount}" : " ";
                CurrentDefectInDieInfo = TotalDefectCount > 0 ? $"{currentDefectInDieIndex + 1} / {TotalDefectInSelectedDie.Count}" : " ";
                //if (currentDefectInDieIndex + 1 > TotalDefectInSelectedDie.Count)
                //    currentDefectInDieIndex = TotalDefectInSelectedDie.Count - 1;
                //else if (currentDefectInDieIndex + 1 < 0)
                //    currentDefectInDieIndex = 1;
                //else if (currentDefectInDieIndex + 1 == -1 && !IsNextClick)
                //    currentDefectInDieIndex = TotalDefectInSelectedDie.Count - 1;

            }
        }


        private Die _selectedDie;
        public Die SelectedDie
        {
            get => _selectedDie;
            set
            {
                if (value == null)
                {
                    // Die가 없는 경우 무시
                    return;
                }

                // 선택된 Die의 Defect가 있는지 확인
                var defectsInDie = DefectList.Defects
                    .Where(d => d.XIndex == value.XIndex && d.YIndex == value.YIndex)
                    .ToList();

                // Defect가 없는 경우 해당 작업 무시
                if (defectsInDie.Count == 0)
                {
                    // Defect가 없는 Die를 선택한 경우 무시
                    return;
                }

                _selectedDie = value;
                TotalDefectInSelectedDie = defectsInDie;
                //_selectedDie = value;
                //TotalDefectInSelectedDie = DefectList.Defects.Where(d => d.XIndex == SelectedDie.XIndex && d.YIndex == SelectedDie.YIndex).ToList();
                OnPropertyChanged(nameof(SelectedDie));
                OnPropertyChanged(nameof(SelectedDefect));
                OnPropertyChanged(nameof(CurrentDefectInDieInfo));
                UpdateImage();
            }
        }

        public int TotalDieCount => DieList.Dies.Count;

        private string currentDieInfo;
        public string CurrentDieInfo// 첫번째
        {
            get => currentDieInfo;
            set
            {
                currentDieInfo = value;
                OnPropertyChanged(nameof(CurrentDieInfo));
            }
            //{
            //return TotalDieCount > 0 ? $"{currentDieIndex + 1} / {TotalDieCount}" : " ";
            //}
        }

        private string currentDefectInDieInfo;
        public string CurrentDefectInDieInfo // 중간
        {
            get => currentDefectInDieInfo;
            //{
            //    if (TotalDefectInSelectedDie.Count > 0)
            //    {
            //        return $"{currentDefectInDieIndex + 1} / {TotalDefectInSelectedDie.Count}";
            //    }

            //    return " ";
            //}
            set
            {
                currentDefectInDieInfo = value;
                OnPropertyChanged(nameof(CurrentDefectInDieInfo));
            }
        }

        public int TotalDefectCount => DefectList.Defects.Count;

        private string currentDefectInfo;
        public string CurrentDefectInfo // 마지막
        {
            get => currentDefectInfo;
            //{
            //    return TotalDefectCount > 0 ? $"{SelectedDefect.DefectID} / {TotalDefectCount}" : " ";
            //}
            set
            {
                currentDefectInfo = value;
                OnPropertyChanged(nameof(CurrentDefectInfo));
            }
        }

        private List<Defect> TotalDefectInSelectedDie = new List<Defect>();


        public ObservableCollection<FolderItem> Drives { get; set; }
        public ObservableCollection<FileInfo> Files { get; set; }

        private FolderItem _selectedFolder;

        public FolderItem SelectedFolder
        {
            get { return _selectedFolder; }
            set
            {
                _selectedFolder = value;
                OnPropertyChanged();
                LoadFiles(_selectedFolder.FullPath);
            }
        }
        public ICommand OpenFileCommand { get; set; }
        public ICommand SaveFileCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand NextDieCommand { get; set; }
        public ICommand NextDefectInDieCommand { get; set; }
        public ICommand NextAllDefectCommand { get; set; }
        public ICommand PrevDieCommand { get; set; }
        public ICommand PrevDefectInDieCommand { get; set; }
        public ICommand PrevAllDefectCommand { get; set; }

        public MainViewModel()
        {
            OpenFileCommand = new RelayCommand(OpenFile_Click);
            SaveFileCommand = new RelayCommand(SaveFile_Click);
            RefreshCommand = new RelayCommand(Refresh_Click);
            NextDieCommand = new RelayCommand(NextDiebtn_Click);
            NextDefectInDieCommand = new RelayCommand(NextDefectInDiebtn_Click);
            NextAllDefectCommand = new RelayCommand(NextAllDefectbtn_Click);
            PrevDieCommand = new RelayCommand(PrevDiebtn_Click);
            PrevDefectInDieCommand = new RelayCommand(PrevDefectInDiebtn_Click);
            PrevAllDefectCommand = new RelayCommand(PrevAllDefectbtn_Click);

            Drives = new ObservableCollection<FolderItem>();
            Files = new ObservableCollection<FileInfo>();
            LoadDrives();
        }

        public void UpdateImage()
        {
            if (SelectedDefect != null)
            {
                ImageSource = Tool.DefectIDandFrameIndex(DefectList, tiffFilePath, SelectedDefect.DefectID);
            }
            else
            {
                ImageSource = null;
            }
        }

        public void DrawWaferMap()
        {
            WaferMapElements.Clear();

            int dieWidth = 22;
            int dieHeight = 7;

            var dieRectangles = Tool.CreateWaferMap(DieList, dieWidth, dieHeight);

            foreach (var dieRect in dieRectangles)
            {
                WaferMapElements.Add(dieRect);
            }

            var defectEllipses = Tool.CreateDefectOnDie(DieList, DefectList, dieWidth, dieHeight);

            foreach (var defect in defectEllipses)
            {
                WaferMapElements.Add(defect);
            }
        }

        public void OpenFile_Click(object obj)
        {
            Tool.LoadKlarfFile(DieList, DefectList);
            DrawWaferMap();
            if (DefectList.Defects.Count > 0)
            {
                int defectID = DefectList.Defects[0].DefectID;

                ImageSource = Tool.DefectIDandFrameIndex(DefectList, tiffFilePath, defectID);

            }
        }

        public void OpenKlarfFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                MessageBox.Show($"파일을 찾을 수 없습니다:\n{filePath}", "오류");
                return;
            }

            try
            {
                // Klarf 파일 읽기
                string[] lines = File.ReadAllLines(filePath);
                
                // TiffFilename 추출
                string tiffFilename = ExtractTiffFilename(lines);
                string klarfDirectory = Path.GetDirectoryName(filePath);
                string foundTiffPath = null;
                
                if (!string.IsNullOrEmpty(tiffFilename))
                {
                    // Klarf 파일과 같은 디렉토리에서 Tiff 파일 찾기
                    foundTiffPath = Path.Combine(klarfDirectory, tiffFilename);
                    
                    if (!File.Exists(foundTiffPath))
                    {
                        // Klarf 하위 폴더에서 찾기
                        string klarfFolder = Path.Combine(klarfDirectory, "Klarf");
                        if (Directory.Exists(klarfFolder))
                        {
                            string altPath = Path.Combine(klarfFolder, tiffFilename);
                            if (File.Exists(altPath))
                            {
                                foundTiffPath = altPath;
                            }
                        }
                    }
                }
                
                // 기존 데이터 초기화
                DefectList.Defects.Clear();
                DieList.Dies.Clear();
                
                // 파싱
                Tool.ParseDefects(lines, DefectList);
                Tool.ParseDies(lines, DieList, DefectList);
                var (gridWidth, gridHeight) = Tool.GetGridSize(DieList);
                Tool.Coordinates(DieList, DefectList);
                
                // Tiff 파일 경로 업데이트
                if (!string.IsNullOrEmpty(foundTiffPath) && File.Exists(foundTiffPath))
                {
                    tiffFilePath = foundTiffPath;
                }
                
                // 웨이퍼 맵 그리기
                DrawWaferMap();
                
                // 첫 번째 Defect 이미지 표시
                if (DefectList.Defects.Count > 0)
                {
                    int defectID = DefectList.Defects[0].DefectID;
                    if (!string.IsNullOrEmpty(tiffFilePath) && File.Exists(tiffFilePath))
                    {
                        ImageSource = Tool.DefectIDandFrameIndex(DefectList, tiffFilePath, defectID);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"파일을 읽는 중 오류가 발생했습니다:\n{ex.Message}", "오류");
            }
        }

        private string ExtractTiffFilename(string[] lines)
        {
            foreach (string line in lines)
            {
                if (line.StartsWith("TiffFilename"))
                {
                    string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        string filename = parts[1].TrimEnd(';');
                        return filename;
                    }
                }
            }
            return null;
        }


        public void SaveFile_Click(object obj)
        {
            BitmapSource temp = ImageSource.Clone();
            Tool.Saveimg(temp);
        }


        public void Refresh_Click(object obj)
        {
            WaferMapElements.Clear();
            DieList.Dies.Clear();
            DefectList.Defects.Clear();
            ImageSource = null;
            SelectedDie = null;
            SelectedDefect = null;

        }

        public void NextDiebtn_Click(object obj)
        {
            int nextDieIndex = currentDieIndex + 1;

            while (nextDieIndex < DieList.Dies.Count)
            {
                var currentDie = DieList.Dies[nextDieIndex];
                var defectsInDie = DefectList.Defects.Where(d => d.XIndex == currentDie.XIndex && d.YIndex == currentDie.YIndex).ToList();

                if (defectsInDie.Count > 0)
                {
                    SelectedDie = currentDie;
                    SelectedDefect = defectsInDie[0];
                    currentDieIndex = nextDieIndex;
                    break;
                }
                nextDieIndex++;
            }
        }

        public void PrevDiebtn_Click(object obj)
        {
            int prevDieIndex = currentDieIndex - 1;
            while (prevDieIndex >= 0)
            {
                var currentDie = DieList.Dies[prevDieIndex];
                var defectsInDie = DefectList.Defects.Where(d => d.XIndex == currentDie.XIndex && d.YIndex == currentDie.YIndex).ToList();

                if (defectsInDie.Count > 0)
                {
                    SelectedDie = currentDie;
                    SelectedDefect = defectsInDie[0];
                    currentDieIndex = prevDieIndex;
                    break;
                }
                prevDieIndex--;
            }
        }

        public void NextDefectInDiebtn_Click(object obj)
        {
            if (SelectedDie == null)
                return;

            var defectsInDie = DefectList.Defects.Where(d => d.XIndex == SelectedDie.XIndex && d.YIndex == SelectedDie.YIndex).ToList();

            currentDefectIndex = defectsInDie.IndexOf(SelectedDefect);

            if (currentDefectIndex < defectsInDie.Count - 1)
            {
                currentDefectInDieIndex += 1;
                SelectedDefect = defectsInDie[currentDefectIndex + 1];
            }
        }

        public void PrevDefectInDiebtn_Click(object obj)
        {
            if (SelectedDie == null)
                return;

            var defectsInDie = DefectList.Defects.Where(d => d.XIndex == SelectedDie.XIndex && d.YIndex == SelectedDie.YIndex).ToList();

            currentDefectIndex = defectsInDie.IndexOf(SelectedDefect);

            if (currentDefectIndex > 0)
            {
                currentDefectInDieIndex -= 1;
                SelectedDefect = defectsInDie[currentDefectIndex - 1];
            }
        }


        public void NextAllDefectbtn_Click(object obj)
        {
            IsNextClick = true;
            currentDefectInDieIndex++;
            var allDefects = DefectList.Defects;
            var defectsInDie = DefectList.Defects.Where(d => d.XIndex == SelectedDie.XIndex && d.YIndex == SelectedDie.YIndex).ToList();
            currentDefectIndex = allDefects.IndexOf(SelectedDefect);
            if (currentDefectIndex < allDefects.Count)
            {
                SelectedDefect = allDefects[currentDefectIndex + 1];
                if (IsDieChanged == true)
                {
                    currentDefectInDieIndex = 0;
                    IsDieChanged = false;
                }
            }
            CurrentDefectInDieInfo = TotalDefectCount > 0 ? $"{currentDefectInDieIndex + 1} / {TotalDefectInSelectedDie.Count}" : " ";
        }

        public void PrevAllDefectbtn_Click(object obj)
        {
            currentDefectInDieIndex--;
            var allDefects = DefectList.Defects;
            var defectsInDie = DefectList.Defects.Where(d => d.XIndex == SelectedDie.XIndex && d.YIndex == SelectedDie.YIndex).ToList();
            int currentIndex = allDefects.IndexOf(SelectedDefect);
            if (currentIndex > 0)
            {
                SelectedDefect = allDefects[currentIndex - 1];
                if (IsDieChanged == true)
                {
                    currentDefectInDieIndex = TotalDefectInSelectedDie.Count - 1;
                    IsDieChanged = false;
                }
            }
            CurrentDefectInDieInfo = TotalDefectCount > 0 ? $"{currentDefectInDieIndex + 1} / {TotalDefectInSelectedDie.Count}" : " ";
        }

        // 원하는 폴더 경로들을 여기에 설정하세요
        // 여러 폴더를 지정하려면 배열에 추가하세요
        private string[] _targetFolderPaths = new string[]
        {
            @"C:\psj\ATI"
        };

        public void LoadDrives()
        {
            Drives.Clear();
            
            // 특정 폴더들만 로드
            if (_targetFolderPaths != null && _targetFolderPaths.Length > 0)
            {
                foreach (var folderPath in _targetFolderPaths)
                {
                    if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                    {
                        var folderItem = new FolderItem(folderPath);
                        LoadSubFolders(folderItem);
                        Drives.Add(folderItem);
                    }
                }
                
                // 지정된 폴더가 없으면 모든 드라이브 로드
                if (Drives.Count == 0)
                {
                    LoadAllDrives();
                }
            }
            else
            {
                // 기존 방식: 모든 드라이브 로드
                LoadAllDrives();
            }
        }

        private void LoadAllDrives()
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    var driveItem = new FolderItem(drive.RootDirectory.FullName);
                    LoadSubFolders(driveItem);
                    Drives.Add(driveItem);
                }
            }
        }

        public void LoadSubFolders(FolderItem folder)
        {
            try
            {
                var directories = Directory.GetDirectories(folder.FullPath);
                foreach (var dir in directories)
                {
                    var subFolder = new FolderItem(dir);
                    folder.Children.Add(subFolder);

                    LoadSubFolders(subFolder);
                }
                LoadFiles(folder.FullPath);
            }
            catch
            {
                Console.WriteLine($"Error");
            }

        }

        public void LoadFiles(string folderPath)
        {
            Files.Clear();
            try
            {
                var directoryInfo = new DirectoryInfo(folderPath);
                foreach (var file in directoryInfo.GetFiles("*.*"))
                {
                    Files.Add(file);
                }
            }
            catch
            {
                Console.WriteLine($"Error");
            }
        }

        public void OpenFolder()
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string folderPath = folderBrowserDialog.SelectedPath;
                    var selectedFolder = new FolderItem(folderPath);
                    LoadSubFolders(selectedFolder);
                    Drives.Add(selectedFolder);
                }
            }
        }

        public void SelectDefect(int xIndex, int yIndex)
        {
            var defects = DefectList.Defects.Where(d => d.XIndex == xIndex && d.YIndex == yIndex).ToList();

            if (defects != null && defects.Count > 0)
            {
                TotalDefectInSelectedDie = defects;
                if (currentDefectInDieIndex != -1)
                {
                    SelectedDefect = TotalDefectInSelectedDie[currentDefectInDieIndex];
                    UpdateImage();
                }
            }
            else
            {
                TotalDefectInSelectedDie.Clear();
                SelectedDefect = null;
                ImageSource = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
    }
}
