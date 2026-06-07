namespace ShapeAnimator.Model
{
    class Circle
    {
        public double X;          // center X
        public double Y;          // center Y
        public double Radius;
        public int XDirection = 1;   // +1 = right,  -1 = left
        public int YDirection = 1;   // +1 = down,   -1 = up

        public Circle(double x, double y, double radius)
        {
            X = x;
            Y = y;
            Radius = radius;
        }
    }
}
