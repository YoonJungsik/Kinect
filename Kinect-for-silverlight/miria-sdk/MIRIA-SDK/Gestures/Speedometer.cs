/*
|| 
|| MIRIA - http://miria.codeplex.com
|| Copyright (C) 2008-2011 Generoso Martello <generoso@martello.com>
||
|| This program is free software: you can redistribute it and/or modify
|| it under the terms of the GNU General Public License as published by
|| the Free Software Foundation, either version 3 of the License, or
|| (at your option) any later version.
||  
|| This program is distributed in the hope that it will be useful,
|| but WITHOUT ANY WARRANTY; without even the implied warranty of
|| MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
|| GNU General Public License for more details.
|| 
|| You should have received a copy of the GNU General Public License
|| along with this program. If not, see
|| <http://www.gnu.org/licenses/>.
|| 
*/

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

namespace MIRIA.Gestures
{
    public class Speedometer
    {
        private System.Windows.Threading.DispatcherTimer _timercheck;
        private FrameworkElement _targetelement;
        private Point _lastpoint;
        private Point _currentspeed;

        public Speedometer(FrameworkElement element)
        {
            _targetelement = element;
            _currentspeed = new Point(0, 0);
            _timercheck = new System.Windows.Threading.DispatcherTimer();
            _timercheck.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _timercheck.Tick += new EventHandler(delegate(object ss, EventArgs ee) { SpeedCalculate(); });
        }
        public Point GetSpeed()
        {
            Point a = new Point(_currentspeed.X * 10, _currentspeed.Y * 10);
            return a;
        }
        public void Start()
        {
            if (!_timercheck.IsEnabled)
            {
                _currentspeed = new Point(0, 0);
                _lastpoint = _targetelement.TransformToVisual(_targetelement.Parent as FrameworkElement).Transform(new Point()); 
                _timercheck.Start();
            }
        }
        public void Stop()
        {
            if (_timercheck.IsEnabled) _timercheck.Stop();
        }
        public bool IsEnabled {
            get { return _timercheck.IsEnabled; }
        }
        private void SpeedCalculate()
        {
            try
            {
                Point currentPoint = _targetelement.TransformToVisual(_targetelement.Parent as FrameworkElement).Transform(new Point());
                Point cp = currentPoint;
                Point lp = _lastpoint;

                _currentspeed = new Point(((cp.X - lp.X)),
                                           ((cp.Y - lp.Y)));

                _lastpoint = currentPoint;
            }
            catch (Exception ex) { }
        }

    }
}
