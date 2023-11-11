using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BezierCurve
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int numberOfPoints = 3;
        List<Point> controlPoints = new List<Point>();
        public MainWindow()
        {
            InitializeComponent();
        }
        private Point selectedPoint;
        private bool isDragging = false;
        private void DrawBezierCurve()
        {
            canvas.Children.Clear();

            List<Point> bezierPoints = new List<Point>();

            for (double t = 0; t <= 1; t += 0.01)
            {
                Point bezierPoint = CalculateBezierPoint(t);
                bezierPoints.Add(bezierPoint);
            }

            //here I have all bezier points

            for (int i = 0; i < bezierPoints.Count - 1; i++)
            {
                Line line = new Line
                {
                    X1 = bezierPoints[i].X,
                    Y1 = bezierPoints[i].Y,
                    X2 = bezierPoints[i + 1].X,
                    Y2 = bezierPoints[i + 1].Y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };

                canvas.Children.Add(line);
            }

            foreach (var point in controlPoints)
            {
                Ellipse ellipse = new Ellipse
                {
                    Width = 5,
                    Height = 5,
                    Fill = Brushes.Red
                };

                Canvas.SetLeft(ellipse, point.X - ellipse.Width / 2);
                Canvas.SetTop(ellipse, point.Y - ellipse.Height / 2);

                canvas.Children.Add(ellipse);
            }
        }

        private Point CalculateBezierPoint(double t)
        {
            int exponent = controlPoints.Count - 1;
            double x = 0, y = 0;

            for (int i = 0; i <= exponent; i++)
            {
                double bezierSum = NewtonBinomial(exponent, i) * Math.Pow(1 - t, exponent - i) * Math.Pow(t, i);
                x += bezierSum * controlPoints[i].X;
                y += bezierSum * controlPoints[i].Y;    //https://javascript.info/bezier-curve
            }

            return new Point(x, y);
        }

        private int NewtonBinomial(int exponent, int index)  //https://pomax.github.io/bezierinfo/
        {
            int result = 1;

            for (int i = 1; i <= index; i++)
            {
                result = result * (exponent - i + 1) / i;
            }

            return result;
        }

        private Point fillValuesOfPoints(int index)
        {
            if (controlPoints.Count - 1 < index)
                return new Point(0, 0);
            return controlPoints[index];
        }


        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                int indexOfPoint = controlPoints.IndexOf(selectedPoint);
                if (indexOfPoint == -1) return;

                Point mousePosition = e.GetPosition(canvas);
                selectedPoint.X = mousePosition.X;
                selectedPoint.Y = mousePosition.Y;
                controlPoints[indexOfPoint] = selectedPoint;
                DrawBezierCurve();
            }
        }
        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
                isDragging = false;         //doesnt work correctly
        }
        private void setPointByClick(object sender, MouseButtonEventArgs e)
        {
            Point mousePosition = e.GetPosition(canvas);
            bool existsPoint = controlPoints.Contains(new Point(mousePosition.X, mousePosition.Y)); //have to add an offset

            if (!existsPoint & numberOfPoints > controlPoints.Count) controlPoints.Add(new Point(mousePosition.X, mousePosition.Y));
            else if (existsPoint)
            {
                isDragging = true;
                selectedPoint = controlPoints.FirstOrDefault(point =>
                            Math.Abs(point.X - mousePosition.X) < 10 && Math.Abs(point.Y - mousePosition.Y) < 10
                        );
            }
            DrawBezierCurve();
        }

        private void setCords_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Window();
            var sp = new StackPanel();
            dialog.Width = 150;
            dialog.Height = 350;
            sp.Children.Add(new TextBlock(new Run("Provide coordinates of your points")));
            var button = new Button();

            for (int i = 0; i < numberOfPoints; i++)
            {

                TextBlock xText = new TextBlock { Text = $"Point {i} (X):" };
                TextBlock yText = new TextBlock { Text = $"Point {i} (Y):" };
                TextBox yValue = new TextBox { Width = 50, Name = $"pointY{i}" };
                TextBox xValue = new TextBox { Width = 50, Name = $"pointX{i}" };
                if (controlPoints.Count > 0)
                {
                    Point existingPoint = fillValuesOfPoints(i);
                    xValue.Text = existingPoint.X.ToString();
                    yValue.Text = existingPoint.Y.ToString();
                }

                sp.Children.Add(xText);
                sp.Children.Add(xValue);
                sp.Children.Add(yText);
                sp.Children.Add(yValue);
            }

            button.Content = "Set";
            button.Width = 50;
            button.Margin = new Thickness(10);
            button.Click += (s, e) =>
            {
                controlPoints.Clear();

                for (int i = 0; i < numberOfPoints; i++)
                {
                    double x = Convert.ToDouble(((TextBox)sp.Children.OfType<TextBox>().First(tb => tb.Name == $"pointX{i}")).Text);
                    double y = Convert.ToDouble(((TextBox)sp.Children.OfType<TextBox>().First(tb => tb.Name == $"pointY{i}")).Text);

                    controlPoints.Add(new Point(x, y));
                }

                DrawBezierCurve();
                dialog.DialogResult = true;
            };
            sp.Children.Add(button);

            dialog.Content = sp;
            dialog.ShowDialog();
        }

        private void addPointsNumber_Click(object sender, RoutedEventArgs e)
        {
            numberOfPoints = Int32.Parse(pointsNumber.Text);
        }

        private void clearPointsArray(object sender, RoutedEventArgs e)
        {
            controlPoints.Clear();
            canvas.Children.Clear();
        }
 
    }


}


