namespace ShapeAnimator.Model
{
    class MyTriangle
    {
        public double X;          // center X
        public double Y;          // center Y
        public double Size;       // circumradius (center to corner)
        public int XDirection = 1;
        public int YDirection = 1;

        public MyTriangle(double x, double y, double size)
        {
            X    = x;
            Y    = y;
            Size = size;
        }
    }
}
