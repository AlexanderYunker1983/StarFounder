using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OpenCvSharp;

namespace StarFounder
{
    class Program
    {
        private static byte[] _data;
        private const int Threshold = 50;
        private static int _widthOfImage;
        private static int _heightOfImage;
        private static string _inputFileName;
        private static string _fileName;
        private static readonly List<Centroid> Centroids = new List<Centroid>();

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                var ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _inputFileName = ofd.FileName;
                    _fileName = Path.Combine(Path.GetDirectoryName(_inputFileName) ?? string.Empty, Path.GetFileNameWithoutExtension(_inputFileName));
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (!File.Exists(args.First()))
                {
                    Console.WriteLine("Wrong file name");
                    Console.ReadLine();
                    return;
                }

                _inputFileName = args.First();
                _fileName = Path.Combine(Path.GetDirectoryName(_inputFileName) ?? string.Empty, Path.GetFileNameWithoutExtension(_inputFileName));
            }

            var inputImageOriginal = Cv2.ImRead(_inputFileName, ImreadModes.Unchanged);
            var workCopy = Cv2.ImRead(_inputFileName, ImreadModes.Grayscale);

            _widthOfImage = workCopy.Cols;
            _heightOfImage = workCopy.Rows;

            var work = new Mat(new Size(workCopy.Height, workCopy.Width), MatType.CV_8UC1);

            Cv2.Transpose(workCopy, work);
            _data = new byte[work.Total() * work.Channels()];

            unsafe
            {
                var pointer = work.DataPointer;
                for (var index = 0; index < work.Total() * work.Channels(); index++)
                {
                    _data[index] = *(pointer + index);
                }
            }

            var startTime = DateTime.Now;
            for (var xIndex = 0; xIndex < _widthOfImage; xIndex++)
            {
                for (var yIndex = 0; yIndex < _heightOfImage; yIndex++)
                {
                    if (_data[xIndex * _heightOfImage + yIndex] > Threshold)
                    {
                        var centroid = new Centroid();
                        ProceedPixel(xIndex, yIndex, centroid);
                        Centroids.Add(centroid);
                    }
                    else
                    {
                        _data[xIndex * _heightOfImage + yIndex] = 0;
                    }
                }
            }

            var endTime = DateTime.Now;
            var deltaTime = endTime - startTime;

            foreach (var centroid in Centroids.Where(c => c.PointsCount >= 3))
            {
                inputImageOriginal.Circle(
                    center: new Point(
                        x: centroid.CenterX,
                        y: centroid.CenterY),
                    radius: 10,
                    color: Scalar.Red,
                    thickness: 3,
                    lineType: LineTypes.AntiAlias);
            }

            Cv2.ImWrite(_fileName + "_result.jpg", inputImageOriginal, new ImageEncodingParam(ImwriteFlags.JpegQuality, value: 100));
            Console.WriteLine($"Estimated time: {deltaTime.TotalSeconds} s");
            Console.ReadLine();
        }

        static void ProceedPixel(int x, int y, Centroid centroid)
        {
            centroid.AddNewPoint(x, y);
            _data[x * _heightOfImage + y] = 0;

            var canCheckPixelDown = y + 1 < _heightOfImage;
            if (canCheckPixelDown)
            {
                if (_data[x * _heightOfImage + y + 1] > Threshold)
                {
                    ProceedPixel(x, y + 1, centroid);
                }
            }

            var canCheckPixelLeft = x + 1 < _widthOfImage;
            if (canCheckPixelLeft)
            {
                if (_data[(x + 1) * _heightOfImage + y] > Threshold)
                {
                    ProceedPixel(x + 1, y, centroid);
                }
            }

            var canCheckPixelUp = y - 1 >= 0;
            if (canCheckPixelUp)
            {
                if (_data[x * _heightOfImage + y - 1] > Threshold)
                {
                    ProceedPixel(x, y - 1, centroid);
                }
            }
        }
    }
}
