namespace StarFounder
{
    public class Centroid
    {
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public int PointsCount { get; set; }

        public void AddNewPoint(int x, int y)
        {
            if (PointsCount == 0)
            {
                PointsCount = 1;
                CenterX = x;
                CenterY = y;
                return;
            }
            PointsCount ++;
            CenterX += (x - CenterX)/PointsCount;
            CenterY += (y - CenterY)/PointsCount;
        }
    }
}
