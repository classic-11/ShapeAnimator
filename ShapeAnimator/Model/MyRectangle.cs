namespace ShapeAnimator.Model
{
    class MyRectangle
    {
        public double X;          // center X
        public double Y;          // center Y
        public double Width;
        public double Height;
        public int XDirection = 1;
        public int YDirection = 1;

        public MyRectangle(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width  = width;
            Height = height;
        }
    }
}
