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
        private static byte[] data;
        private const int Threshold = 50;
        private static int widthOfImage;
        private static int heightOfImage;
        private static string inputFileName;
        private static string fileName;
        private static readonly List<Centroid> centroids = new List<Centroid>();

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                var ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    inputFileName = ofd.FileName;
                    fileName = Path.GetFileNameWithoutExtension(inputFileName);
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

                inputFileName = args.First();
                fileName = Path.GetFileNameWithoutExtension(inputFileName);
            }
            var inputImageOriginal = Cv2.ImRead(inputFileName, ImreadModes.Unchanged);
            var workCopy = Cv2.ImRead(inputFileName, ImreadModes.Grayscale);
            widthOfImage = workCopy.Cols;
            heightOfImage = workCopy.Rows;
            var work = new Mat(new Size(workCopy.Height, workCopy.Width), MatType.CV_8UC1);
            Cv2.Transpose(workCopy, work);
            data = new byte[work.Total() * work.Channels()];
            unsafe
            {
                var pointer = work.DataPointer;
                for (var index = 0; index < work.Total() * work.Channels(); index++)
                {
                    data[index] = *(pointer + index);
                }
            }
            var startTime = DateTime.Now;
            for (var xIndex = 0; xIndex < widthOfImage; xIndex++)
            {
                for (var yIndex = 0; yIndex < heightOfImage; yIndex++)
                {
                    if (data[xIndex * heightOfImage + yIndex] > Threshold)
                    {
                        var centroid = new Centroid();
                        ProceedPixel(xIndex, yIndex, centroid);
                        centroids.Add(centroid);
                    }
                    else
                    {
                        data[xIndex * heightOfImage + yIndex] = 0;
                    }
                }
            }
            var endTime = DateTime.Now;
            var deltaTime = endTime - startTime;
            foreach (var centroid in centroids.Where(c => c.PointsCount >= 4))
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
            Cv2.ImWrite(fileName + "_result.jpg", inputImageOriginal, new ImageEncodingParam(ImwriteFlags.JpegQuality, value: 100));
            Console.WriteLine($"Estimated time: {deltaTime.TotalSeconds} s");
            Console.ReadLine();
        }

        static void ProceedPixel(int x, int y, Centroid centroid)
        {
            centroid.AddNewPoint(x, y);
            data[x * heightOfImage + y] = 0;
            var canCheckPixelDown = y + 1 < heightOfImage;
            if (canCheckPixelDown)
            {
                if (data[x * heightOfImage + y + 1] > Threshold)
                {
                    ProceedPixel(x, y + 1, centroid);
                }
            }
            if (x + 1 < widthOfImage)
            {
                if (data[(x + 1) * heightOfImage + y] > Threshold)
                {
                    ProceedPixel(x + 1, y, centroid);
                }
            }
            if (y - 1 >= 0)
            {
                if (data[x * heightOfImage + y - 1] > Threshold)
                {
                    ProceedPixel(x, y - 1, centroid);
                }
            }
        }
    }
}
