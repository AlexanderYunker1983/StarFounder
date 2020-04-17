namespace StarFounder
{
    public class Centroid
    {
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public int PointsCount { get; set; }
        public double MaxDimensionX { get; set; }
        public double MinDimensionX { get; set; }
        public double MaxDimensionY { get; set; }
        public double MinDimensionY { get; set; }

        public double XSize
        {
            get
            {
                var minDimension = CenterX - MinDimensionY;
                var maxDimension = MaxDimensionX - CenterX;
                var maxDimensionMoreThanMinDimension = maxDimension > minDimension;
                var xSize = maxDimensionMoreThanMinDimension ? maxDimension : minDimension;
                return xSize;
            }
        }

        public double YSize
        {
            get
            {
                var min = CenterY - MinDimensionY;
                var max = MaxDimensionY - CenterY;
                return max > min ? max : min;
            }
        }

        public void AddNewPoint(int x, int y)
        {
            var noPoints = PointsCount == 0;
            if (noPoints)
            {
                PointsCount = 1;
                MaxDimensionX = MinDimensionX = CenterX = x;
                MaxDimensionY = MinDimensionY = CenterY = y;
                return;
            }
            PointsCount ++;
            if (MaxDimensionX < x) MaxDimensionX = x;
            if (MaxDimensionY < y) MaxDimensionY = y;
            if (MinDimensionX > x) MinDimensionX = x;
            if (MinDimensionY > y) MinDimensionY = y;
            CenterX += (x - CenterX)/PointsCount;
            CenterY += (y - CenterY)/PointsCount;
        }
    }
}
