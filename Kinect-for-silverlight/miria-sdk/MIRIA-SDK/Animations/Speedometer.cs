using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MIRIA.Animations
{
    public class Speedometer
    {
        System.Windows.Threading.DispatcherTimer _timer;
        FrameworkElement _targetelement;
        Point _lastlocation;
        Point _speed;

        TranslateTransform tt;

        public Speedometer(FrameworkElement element)
        {
            _targetelement = element;
            _speed = new Point(0, 0);
            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _timer.Tick += new EventHandler(delegate(object ss, EventArgs ee) { SpeedCalculate(); });
        }
        public Point GetSpeed()
        {
            Point a = new Point(_speed.X / 10,_speed.Y / 10); // al secondo
            return a;
        }
        public void Start()
        {
            if (!_timer.IsEnabled)
            {
                _speed = new Point(0, 0);
                System.Windows.Media.MatrixTransform t = (System.Windows.Media.MatrixTransform)_targetelement.TransformToVisual((UIElement)_targetelement.Parent);
                _lastlocation = new Point(t.Matrix.OffsetX, t.Matrix.OffsetY);
                _timer.Start();
            }
        }
        public void Stop()
        {
            if (_timer.IsEnabled) _timer.Stop();
        }
        public bool IsEnabled {
            get { return _timer.IsEnabled; }
        }
        private void SpeedCalculate()
        {
            Point currentPoint;

            System.Windows.Media.MatrixTransform t = (System.Windows.Media.MatrixTransform)_targetelement.TransformToVisual((UIElement)_targetelement.Parent);
            currentPoint = new Point(t.Matrix.OffsetX, t.Matrix.OffsetY);

            Point cp = currentPoint;
            Point lp = _lastlocation;

            _speed = new Point(((cp.X - lp.X)), ((cp.Y - lp.Y)));

            _lastlocation = currentPoint;

        }

    }
}
