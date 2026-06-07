using ShapeAnimator.Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ShapeAnimator
{
    /// <summary>
    /// Assignment 2 - ShapeShooting (Part 2 completion)
    /// Adds: Rectangle + Triangle drawing, top/bottom bounce, Play/Stop buttons
    /// </summary>
    public partial class MainWindow : Window
    {
        // ── MODEL 
        GameModel Model = new GameModel();

        // ── SHAPE MODE 
        enum ShapeMode { Circle, Rectangle, Triangle }
        ShapeMode currentMode = ShapeMode.Circle;  // default: circle

        // ── DRAWING STATE 
        bool isFirstClick = true;   // true = waiting for center click
        double x1, y1;              // center point from first click
        Ellipse? centerMark;        // small red dot shown at center

        // Ghost shape: dashed preview that follows the mouse
        Shape ghostShape = new Ellipse();

        // ── ANIMATION 
        DispatcherTimer timer = new DispatcherTimer();
        const double SPEED = 5.0;   // pixels per step

        
        //  CONSTRUCTOR

        public MainWindow()
        {
            InitializeComponent();

            // Ghost shape stays on canvas permanently (invisible until drawing)
            StyleGhost(ghostShape, ShapeMode.Circle);
            ShapeCanvas.Children.Add(ghostShape);

            // Timer for Play/Stop  (~60 fps)
            timer.Interval = TimeSpan.FromMilliseconds(16);
            timer.Tick += (s, e) => Step();
        }


        //  SHAPE SELECTOR BUTTONS
      
        private void CircleButton_Click(object sender, RoutedEventArgs e)
        {
            currentMode = ShapeMode.Circle;
            ResetDrawing();
        }

        private void RectangleButton_Click(object sender, RoutedEventArgs e)
        {
            currentMode = ShapeMode.Rectangle;
            ResetDrawing();
        }

        private void TriangleButton_Click(object sender, RoutedEventArgs e)
        {
            currentMode = ShapeMode.Triangle;
            ResetDrawing();
        }

        // Cancels any in-progress first-click so we start fresh
        private void ResetDrawing()
        {
            isFirstClick = true;
            if (centerMark != null)
            {
                ShapeCanvas.Children.Remove(centerMark);
                centerMark = null;
            }
        }


        //  ANIMATION BUTTONS

        private void StepButton_Click(object sender, RoutedEventArgs e) => Step();
        private void PlayButton_Click(object sender, RoutedEventArgs e)  => timer.Start();
        private void StopButton_Click(object sender, RoutedEventArgs e)  => timer.Stop();


        //  STEP  –  move every shape one frame

        private void Step()
        {
            double W = ShapeCanvas.ActualWidth;
            double H = ShapeCanvas.ActualHeight;

            // Circles 
            foreach (Circle c in Model.Circles)
            {
                c.X += c.XDirection * SPEED;
                c.Y += c.YDirection * SPEED;

                // Bounce off LEFT and RIGHT walls
                if (c.X + c.Radius >= W || c.X - c.Radius <= 0)
                    c.XDirection = -c.XDirection;

                // Bounce off TOP and BOTTOM walls
                if (c.Y + c.Radius >= H || c.Y - c.Radius <= 0)
                    c.YDirection = -c.YDirection;
            }

            //  Rectangles 
            foreach (MyRectangle r in Model.Rectangles)
            {
                r.X += r.XDirection * SPEED;
                r.Y += r.YDirection * SPEED;

                // X is CENTER, so half-width is the boundary offset
                if (r.X + r.Width  / 2 >= W || r.X - r.Width  / 2 <= 0)
                    r.XDirection = -r.XDirection;

                if (r.Y + r.Height / 2 >= H || r.Y - r.Height / 2 <= 0)
                    r.YDirection = -r.YDirection;
            }

            //  Triangles 
            foreach (MyTriangle t in Model.Triangles)
            {
                t.X += t.XDirection * SPEED;
                t.Y += t.YDirection * SPEED;

                // Use circumradius as bounding box half-size
                if (t.X + t.Size >= W || t.X - t.Size <= 0)
                    t.XDirection = -t.XDirection;

                if (t.Y + t.Size >= H || t.Y - t.Size <= 0)
                    t.YDirection = -t.YDirection;
            }

            Render();
        }


        //  MOUSE INPUT  –  two-click drawing

        private void ShapeCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            double mx = e.GetPosition(ShapeCanvas).X;
            double my = e.GetPosition(ShapeCanvas).Y;

            if (isFirstClick)
            {
                //  FIRST CLICK: save center point 
                x1 = mx;
                y1 = my;

                // Show a tiny red dot at the center
                centerMark = new Ellipse
                {
                    Width  = 6,
                    Height = 6,
                    Fill   = Brushes.Red
                };
                ShapeCanvas.Children.Add(centerMark);
                Canvas.SetLeft(centerMark, x1 - 3);
                Canvas.SetTop (centerMark, y1 - 3);
            }
            else
            {
                // ── SECOND CLICK: commit shape to model 
                double dx   = mx - x1;
                double dy   = my - y1;
                double dist = Math.Sqrt(dx * dx + dy * dy);

                switch (currentMode)
                {
                    case ShapeMode.Circle:
                        Model.Circles.Add(new Circle(x1, y1, dist));
                        break;

                    case ShapeMode.Rectangle:
                        // Width  = 2 × horizontal drag distance
                        // Height = 2 × vertical   drag distance
                        Model.Rectangles.Add(
                            new MyRectangle(x1, y1,
                                            Math.Abs(dx) * 2,
                                            Math.Abs(dy) * 2));
                        break;

                    case ShapeMode.Triangle:
                        Model.Triangles.Add(new MyTriangle(x1, y1, dist));
                        break;
                }

                // Remove the center dot and refresh the canvas
                if (centerMark != null)
                {
                    ShapeCanvas.Children.Remove(centerMark);
                    centerMark = null;
                }

                Render();
            }

            isFirstClick = !isFirstClick;
        }


        //  MOUSE MOVE  –  update ghost preview shape

        private void ShapeCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isFirstClick) return;   // no center yet, nothing to preview

            double mx = e.GetPosition(ShapeCanvas).X;
            double my = e.GetPosition(ShapeCanvas).Y;
            double dx   = mx - x1;
            double dy   = my - y1;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            // Swap ghost type if mode changed
            ShapeCanvas.Children.Remove(ghostShape);
            ghostShape = CreateGhost(currentMode);
            ShapeCanvas.Children.Add(ghostShape);

            switch (currentMode)
            {
                case ShapeMode.Circle:
                    ghostShape.Width  = dist * 2;
                    ghostShape.Height = dist * 2;
                    Canvas.SetLeft(ghostShape, x1 - dist);
                    Canvas.SetTop (ghostShape, y1 - dist);
                    break;

                case ShapeMode.Rectangle:
                    double rw = Math.Abs(dx) * 2;
                    double rh = Math.Abs(dy) * 2;
                    ghostShape.Width  = rw;
                    ghostShape.Height = rh;
                    Canvas.SetLeft(ghostShape, x1 - rw / 2);
                    Canvas.SetTop (ghostShape, y1 - rh / 2);
                    break;

                case ShapeMode.Triangle:
                    if (ghostShape is Polygon poly)
                        poly.Points = TrianglePoints(x1, y1, dist);
                    break;
            }
        }


        //  RENDER  –  clear canvas and redraw everything from model

        private void Render()
        {
            ShapeCanvas.Children.Clear();

            foreach (Circle      c in Model.Circles)    DrawCircle(c);
            foreach (MyRectangle r in Model.Rectangles) DrawRectangle(r);
            foreach (MyTriangle  t in Model.Triangles)  DrawTriangle(t);

            // Always keep the ghost on top so the preview stays visible
            ShapeCanvas.Children.Add(ghostShape);

            // Re-add center dot if user is mid-drawing
            if (!isFirstClick && centerMark != null)
                ShapeCanvas.Children.Add(centerMark);
        }


        //  DRAW HELPERS

        private void DrawCircle(Circle c)
        {
            Ellipse ell = new Ellipse
            {
                Width  = c.Radius * 2,
                Height = c.Radius * 2,
                Stroke = Brushes.Cyan,
                StrokeThickness = 2
            };
            ShapeCanvas.Children.Add(ell);
            Canvas.SetLeft(ell, c.X - c.Radius);
            Canvas.SetTop (ell, c.Y - c.Radius);
        }

        private void DrawRectangle(MyRectangle r)
        {
            Rectangle rect = new Rectangle
            {
                Width  = r.Width,
                Height = r.Height,
                Stroke = Brushes.LimeGreen,
                StrokeThickness = 2
            };
            ShapeCanvas.Children.Add(rect);
            Canvas.SetLeft(rect, r.X - r.Width  / 2);
            Canvas.SetTop (rect, r.Y - r.Height / 2);
        }

        private void DrawTriangle(MyTriangle t)
        {
            Polygon poly = new Polygon
            {
                Stroke = Brushes.Yellow,
                StrokeThickness = 2,
                Points = TrianglePoints(t.X, t.Y, t.Size)
            };
            ShapeCanvas.Children.Add(poly);
        }

        // ── Geometry helper: equilateral triangle pointing UP
        // Three corners at -90°, 150°, 30° around (cx, cy) with given radius
        private PointCollection TrianglePoints(double cx, double cy, double r)
        {
            double a0 = -Math.PI / 2;                   // top corner
            double a1 =  a0 + 2 * Math.PI / 3;          // bottom-left
            double a2 =  a1 + 2 * Math.PI / 3;          // bottom-right
            return new PointCollection
            {
                new Point(cx + r * Math.Cos(a0), cy + r * Math.Sin(a0)),
                new Point(cx + r * Math.Cos(a1), cy + r * Math.Sin(a1)),
                new Point(cx + r * Math.Cos(a2), cy + r * Math.Sin(a2))
            };
        }

        // ── Create a fresh ghost shape of the right type
        private Shape CreateGhost(ShapeMode mode)
        {
            Shape s = mode switch
            {
                ShapeMode.Rectangle => new Rectangle(),
                ShapeMode.Triangle  => new Polygon { Fill = Brushes.Transparent },
                _                   => new Ellipse()
            };
            StyleGhost(s, mode);
            return s;
        }

        private void StyleGhost(Shape s, ShapeMode mode)
        {
            s.Stroke          = Brushes.White;
            s.StrokeThickness = 1;
            s.StrokeDashArray = new DoubleCollection { 4, 3 };
            if (s is not Polygon)
            {
                s.Width  = 0;
                s.Height = 0;
            }
        }
    }
}
