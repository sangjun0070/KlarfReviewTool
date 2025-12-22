using KlarfReviewTool.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace KlarfReviewTool.VIewModel
{
    public class Tool
    {
        public static void LoadKlarfFile(Dielist dieList, Defectlist defectList)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                string[] lines = File.ReadAllLines(openFileDialog.FileName);
                ParseDefects(lines, defectList);
                ParseDies(lines, dieList, defectList);
                var (gridWidth, gridHeight) = GetGridSize(dieList);
                Coordinates(dieList, defectList);
            }
        }

        public static void Saveimg(BitmapSource bmp)
        {
            SaveFileDialog sd = new SaveFileDialog();
            sd.Filter = "BMP Image|*.bmp";

            if (sd.ShowDialog() == true)
            {
                BitmapSource bitmapSource = bmp.Clone();

                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using (FileStream fileStream = new FileStream(sd.FileName, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
        }


        public static void ParseDefects(string[] lines, Defectlist defectList)
        {
            bool isDefectSection = false;

            foreach (string line in lines)
            {
                if (line.StartsWith("DefectList"))
                {
                    isDefectSection = true;
                    continue;
                }

                if (isDefectSection && !string.IsNullOrWhiteSpace(line))
                {
                    string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 17)
                    {
                        Defect defect = new Defect
                        {
                            DefectID = int.Parse(parts[0]),
                            XRel = double.Parse(parts[1]),
                            YRel = double.Parse(parts[2]),
                            XIndex = int.Parse(parts[3]),
                            YIndex = int.Parse(parts[4]),
                            XSize = double.Parse(parts[5]),
                            YSize = double.Parse(parts[6]),
                            DefectArea = double.Parse(parts[7]),
                            DSize = double.Parse(parts[8]),
                            ClassNumber = int.Parse(parts[9]),
                            Test = int.Parse(parts[10]),
                            ClusterNumber = int.Parse(parts[11]),
                            RoughBinNumber = int.Parse(parts[12]),
                            FineBinNumber = int.Parse(parts[13]),
                            ReviewSample = int.Parse(parts[14]),
                            ImageCount = int.Parse(parts[15]),
                            ImageList = parts[16]
                        };


                        defectList.Defects.Add(defect);
                    }
                }
            }
        }

        public static void ParseDies(string[] lines, Dielist dieList, Defectlist defectList)
        {
            bool isDieSection = false;

            foreach (string line in lines)
            {
                if (line.StartsWith("SampleTestPlan 784"))
                {
                    isDieSection = true;
                    continue;
                }

                if (isDieSection && !string.IsNullOrWhiteSpace(line))
                {
                    string tempLine = line;
                    if (line.EndsWith(";"))
                    {
                        tempLine = line.Replace(";", "");
                        isDieSection = false;
                    }

                    string[] parts = tempLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        Die die = new Die
                        {
                            XIndex = int.Parse(parts[0]),
                            YIndex = int.Parse(parts[1]),
                            HasDefect = false
                        };

                        if (defectList.Defects.Any(def => def.XIndex == die.XIndex && def.YIndex == die.YIndex))
                        {
                            die.HasDefect = true;
                        }
                        dieList.Dies.Add(die);
                    }
                }
            }
        }
        public static (int gridWidth, int gridHeight) GetGridSize(Dielist dieList)
        {
            if (dieList.Dies.Count == 0)
                return (0, 0);

            int minX = dieList.Dies.Min(d => d.XIndex);
            int maxX = dieList.Dies.Max(d => d.XIndex);
            int minY = dieList.Dies.Min(d => d.YIndex);
            int maxY = dieList.Dies.Max(d => d.YIndex);

            int gridWidth = maxX - minX + 1;
            int gridHeight = maxY - minY + 1;

            return (gridWidth, gridHeight);
        }

        public static void Coordinates(Dielist dieList, Defectlist defectList)
        {
            if (dieList.Dies.Count == 0)
                return;

            int minX = dieList.Dies.Min(d => d.XIndex);
            int minY = dieList.Dies.Min(d => d.YIndex);

            foreach (var die in dieList.Dies)
            {
                die.XIndex -= minX;
                die.YIndex -= minY;
            }

            if (defectList.Defects.Count == 0)
                return;

            foreach (var defect in defectList.Defects)
            {
                defect.XIndex -= minX;
                defect.YIndex -= minY;
            }
        }

        public static List<Rectangle> CreateWaferMap(Dielist dieList, double dieWidth, double dieHeight)
        {
            var dieRectangles = new List<Rectangle>();

            foreach (var die in dieList.Dies)
            {
                var dieRect = new Rectangle
                {
                    Width = dieWidth,
                    Height = dieHeight,
                    Stroke = System.Windows.Media.Brushes.Black,
                    Fill = die.HasDefect ? System.Windows.Media.Brushes.LightBlue : System.Windows.Media.Brushes.Green
                };

                Canvas.SetLeft(dieRect, die.XIndex * dieWidth);
                Canvas.SetTop(dieRect, die.YIndex * dieHeight);

                dieRectangles.Add(dieRect);
            }

            return dieRectangles;
        }

        public static List<Ellipse> CreateDefectOnDie(Dielist dieList, Defectlist defectList, double dieWidth, double dieHeight)
        {
            var defectEllipses = new List<Ellipse>();

            foreach (var defect in defectList.Defects)
            {
                var matchingDie = dieList.Dies.FirstOrDefault(d => d.XIndex == defect.XIndex && d.YIndex == defect.YIndex);
                if (matchingDie != null)
                {

                    double defectXPos = matchingDie.XIndex * dieWidth + (defect.XRel / 1000.0);
                    double defectYPos = matchingDie.YIndex * dieHeight + (defect.YRel / 1000.0);

                    var defectEllipse = new Ellipse
                    {
                        Width = 5,
                        Height = 5,
                        Fill = System.Windows.Media.Brushes.Red
                    };

                    Canvas.SetLeft(defectEllipse, defectXPos);
                    Canvas.SetTop(defectEllipse, defectYPos);

                    defectEllipses.Add(defectEllipse);
                }
            }

            return defectEllipses;
        }

        public static BitmapSource DefectIDandFrameIndex(Defectlist defectList, string tiffFilePath, int defectID)
        {
            TiffBitmapDecoder tifImages = new TiffBitmapDecoder(new Uri(tiffFilePath, UriKind.Absolute), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

            foreach (var defect in defectList.Defects)
            {
                if (defect.DefectID == defectID)
                {
                    int frameIndex = defect.DefectID - 1;

                    if (frameIndex >= 0 && frameIndex < tifImages.Frames.Count)
                    {
                        return tifImages.Frames[frameIndex];
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return null;
        }
    }
}


