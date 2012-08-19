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
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;

using MIRIA.Animations;
using MIRIA.Gestures;
using MIRIA.Interaction.MultiTouch;

namespace MIRIA.UIKit
{
    public class PerspectivePanel : System.Windows.Controls.Panel
    {
        /// <summary>
        /// MIRIA.UIKit.PerspectivePanel
        /// (c) 2009 by Generoso Martello  -- generoso@martello.com
        /// </summary>
        /// 

        public PerspectivePanel()
        {
            this.Loaded += new RoutedEventHandler(PerspectivePanel_Loaded);
        }

        void PerspectivePanel_Loaded(object sender, RoutedEventArgs e)
        {
            _init();
        }

        #region Public events
        public delegate void TappedHandler(FrameworkElement element, Point p);
        public event TappedHandler Tapped;
        public delegate void OpenHandler(FrameworkElement element);
        public event OpenHandler Open;
        #endregion

        #region Private members

        /*
         *  Gestures interpreter for detecting fingers gestures over the panel
         */
        private TouchGestures _gesturesinterpreter;
        private TouchGesture _currentgesture = TouchGesture.NONE;


        private MIRIA.Animations.TransformHelper _transformhelper;

        private string _mytouchid = "";

        private double _currentshift = 0;
        private double _currentangle = 0;

        private double _startdragx = 0;
        private double _maxdraglenght = 500;

        private TCanvas _backgroundcanvas;

        private DispatcherTimer _dispatchertimer = new DispatcherTimer();

        private double _baseglobaloffsetz = -500;
        private int _currentitem = -1;
        private int _ticks = 0;
        private int _currentaction = 2;

        private bool _openeventfired = false;

        private int _columns = 40;
        private int _thumbsize = 220;
        private int _itemscount = 0;

        #endregion
















        private void _init()
        {
            this.Projection = new PlaneProjection() { };
            this.Background = new SolidColorBrush(Colors.Transparent);
            //
            _backgroundcanvas = new TCanvas();
            _backgroundcanvas.Width = this.Width;
            _backgroundcanvas.Height = this.Height;
            _backgroundcanvas.Background = new SolidColorBrush(Colors.Transparent);
            _backgroundcanvas.FingerAdded += new TCanvas.FingerAdddedHandler(_backgroundcanvas_FingerAdd);
            _backgroundcanvas.FingerRemoved += new TCanvas.FingerRemovedHandler(_backgroundcanvas_FingerRemove);
            _backgroundcanvas.FingerUpdated += new TCanvas.FingerUpdatedHandler(_backgroundcanvas_FingerUpdate);
            (this.Parent as Panel).Children.Insert((this.Parent as Panel).Children.IndexOf(this), _backgroundcanvas);
            //
            _dispatchertimer = new DispatcherTimer();
            _dispatchertimer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            _dispatchertimer.Tick += (sender, e) => {
                if (_currentaction == 1 && _currentitem < _itemscount && _ticks == 2)
                    _folditem();
                else if (_currentaction == 0 && _currentitem != -1 && _ticks == 2)
                    _unfolditem();
                else
                    _animate();
                if (_ticks == 2)
                    _ticks = 0;
                else
                    _ticks++;
            };
            _dispatchertimer.Start();

            _transformhelper = new TransformHelper(this);

            _gesturesinterpreter = new TouchGestures(_backgroundcanvas);
            _gesturesinterpreter.GestureDetected += new TouchGestures.GestureDetectedHandler(_gesturesinterpreter_GestureDetected);
            _gesturesinterpreter.Scale += new TouchGestures.GestureScaleHandler(_gesturesinterpreter_Scale);
            _gesturesinterpreter.Tap += new TouchGestures.GestureTapHandler(_gesturesinterpreter_Tap);
        }


        void _gesturesinterpreter_Tap(object sender, GestureTapEventArgs e)
        {
             IEnumerable<UIElement> hitresults = VisualTreeHelper.FindElementsInHostCoordinates(e.TapLocation, this);
             foreach (UIElement el in hitresults)
             {
                 if (Tapped != null) Tapped(el as FrameworkElement, e.TapLocation);
             }

            _transformhelper.Animate(0.0, new Point(1.0, 1.0), _transformhelper.Translate, 1.0);
            Animations.Animator.AnimateProperty(this, "Opacity", this.Opacity, 1.0, 1.0, null);
        }

        void _gesturesinterpreter_Scale(object sender, GestureScaleEventArgs e)
        {
            //_transformhelper.Scale *= (e.ScaleFactor);
            //this.Opacity = (_transformhelper.Scale <= 1 ? _transformhelper.Scale : 1);

            if (_currentcanvas == null)
            {
                IEnumerable<UIElement> hitresults = VisualTreeHelper.FindElementsInHostCoordinates(_gesturesinterpreter.Centroid, this);
                foreach (UIElement el in hitresults)
                {
                    if (el is MIRIA.UIKit.TCanvas)
                    {
                        _currentcanvas = (el as MIRIA.UIKit.TCanvas);
                        _currentcanvas.SetValue(TCanvas.ZIndexProperty, 255);
                        break;
                    }
                }
            }

            if (_currentcanvas != null && _currentcanvas.TransformHelper.Scale != 1.3 && !_openeventfired)
            {
                _currentcanvas.TransformHelper.Scale *= e.ScaleFactor;
                if (_currentcanvas.TransformHelper.Scale < 1.0)
                    _currentcanvas.TransformHelper.Scale = 1.0;
                else if (_currentcanvas.TransformHelper.Scale > 1.3)
                {
                    _currentcanvas.TransformHelper.Scale = 1.3;
                }
                if (_currentcanvas.TransformHelper.Scale == 1.3)
                {
                    _openeventfired = true;
                    if (Open != null) Open(_currentcanvas);
                }
            }
        }


        private bool _dragenable = false;
        private MIRIA.UIKit.TCanvas _currentcanvas = null;
        void _gesturesinterpreter_GestureDetected(object sender, GestureDetectedEventArgs e)
        {
            if (e.Gesture == TouchGesture.MOVE_SOUTH)
            {
                _dragenable = true;
                _currentshift = 0;
                _currentangle = 0;

            }
            
        }

        void _backgroundcanvas_FingerUpdate(object sender, FingerTouchEventArgs e)
        {
            _gesturesinterpreter.FingerUpdate(e.Finger.Identifier, e.Finger.Position);

            _dragenable = false;
            try
            {
                Finger finger = _gesturesinterpreter.Fingers.First<Finger>(el => el.Identifier == _mytouchid);
                double draglen = MIRIA.Utility.Vector2D.Distance(finger.StartPosition, finger.Position);
                if (draglen > 50)
                    _dragenable = true;
            }
            catch (Exception ex) { }

            if (e.Finger.Identifier != _mytouchid || _gesturesinterpreter.Fingers.Count > 1 || !_dragenable) return;

            double dx = e.Finger.Position.X - _startdragx;
            if (Math.Abs(dx) > _maxdraglenght)
                dx = _maxdraglenght * Math.Sign(dx);

            _currentangle = (45 / _maxdraglenght) * dx;
            _currentshift = dx;
        }

        void _backgroundcanvas_FingerRemove(object sender, FingerTouchEventArgs e)
        {
            _dragenable = false;
            _openeventfired = false;

            _gesturesinterpreter.FingerRemove(e.Finger.Identifier);

            if (_currentcanvas != null)
            {
                _currentcanvas.SetValue(TCanvas.ZIndexProperty, 1);
                _currentcanvas.TransformHelper.Delay = 0.5;
                _currentcanvas.TransformHelper.Scale = 1.0;
                _currentcanvas.TransformHelper.Delay = 0.0;
            }
            _currentcanvas = null;

            if (e.Finger.Identifier != _mytouchid) return;
            _mytouchid = "";

            
            
            _dragenable = false;
        }

        void _backgroundcanvas_FingerAdd(object sender, FingerTouchEventArgs e)
        {
            if (_mytouchid == "")
            {
                _mytouchid = e.Finger.Identifier;
                _startdragx = e.Finger.Position.X;
            }

            _gesturesinterpreter.FingerUpdate(e.Finger.Identifier, e.Finger.Position);
        }

        public void FoldUnfold()
        {
            _currentaction = Math.Abs(_currentaction - 1);

            if (_currentaction == 1)
            {
                _currentitem = 0;
                _transformhelper.Animate(0.0, new Point(0.8, 0.8), _transformhelper.Translate, 10.0);
            }
            else
            {
                // Arrange elements
                IEnumerable<UIElement> res = this.Children.Where(d => d is TCanvas);
                _currentitem = res.Count() - 1;
                _transformhelper.Animate(0.0, new Point(1.1, 1.1), _transformhelper.Translate, 60.0);
            }

        }


        #region Panel Measure, Arrange ovverrides and Layout updates

        /*
         *  Panel children arrangement logic
         */
        protected override Size MeasureOverride(Size availablesize)
        {
            FrameworkElement parent = VisualTreeHelper.GetParent(this) as FrameworkElement;

            if (!double.IsInfinity(availablesize.Width))
                this.Width = availablesize.Width;
            else
                this.Width = parent.ActualWidth;
            if (!double.IsInfinity(availablesize.Height))
                this.Height = availablesize.Height;
            else
                this.Height = parent.ActualHeight;

            Size infinite = new Size(double.PositiveInfinity,
                                     double.PositiveInfinity);
            foreach (FrameworkElement child in Children)
                child.Measure(infinite);

            return new Size(this.Width, this.Height);
        }

        protected override Size ArrangeOverride(Size availablesize)
        {

            // Arrange elements
            IEnumerable<UIElement> res = this.Children.Where(d => d is TCanvas);
            int i = 0;
            double x = 0;
            double y = 0;
            foreach (TCanvas c in res)
            {
                if (i % _columns == 0 && i != 0)
                {
                    x = 0;
                    y += _thumbsize;
                }


                Point location = new Point(x, y);
                c.Width = _thumbsize;
                c.Height = _thumbsize;
                RectangleGeometry clip = new RectangleGeometry() { Rect = new Rect(0, 0, _thumbsize, _thumbsize) };
                //c.Clip = clip;

                c.Arrange(new Rect(location, new Size(_thumbsize, _thumbsize) /* c.DesiredSize */));

                x += _thumbsize;

                i++;
            }

            _itemscount = i;

            /*
            double cx = 0, cy = 0;
            foreach (FrameworkElement child in Children)
            {
//                Point location = new Point(cx, cy);
//                child.Arrange(new Rect(location, child.DesiredSize));
                cx += child.DesiredSize.Width;
                cy += child.DesiredSize.Height;
            }
            */
            return availablesize;
        }
        #endregion
    

        
        private void _animate()
        {
            if (_currentshift != 0)
            {
                //this.Dispatcher.BeginInvoke(() =>
                //{
                    (this.Projection as PlaneProjection).LocalOffsetX += (_currentshift / 5);
                    (this.Projection as PlaneProjection).RotationY = _currentangle;
                    (this.Projection as PlaneProjection).GlobalOffsetZ = _baseglobaloffsetz + Math.Abs(_currentangle * 20);
                //});
                if (_mytouchid == "")
                {
                    int csign = Math.Sign(_currentshift);
                    _currentshift -= (csign * 5);
                    if (Math.Sign(_currentshift) != csign)
                    {
                        _currentshift = 0;
                        _currentangle = 0;
                    }
                    else
                    {
                        _currentangle -= (Math.Sign(_currentangle));
                    }
                }
            }
        }

        private void _unfolditem()
        {
            // Arrange elements
            IEnumerable<UIElement> res = this.Children.Where(d => d is TCanvas);

            TCanvas c = this.Children[_currentitem] as TCanvas;
            c.TransformHelper.Animate(0, new Point(1, 1), new Point(), 1);

            _currentitem--;
        }

        private void _folditem()
        {
            // Arrange elements
            IEnumerable<UIElement> res = this.Children.Where(d => d is TCanvas);

            TCanvas c = this.Children[_currentitem] as TCanvas;
            c.TransformHelper.Animate((new Random().NextDouble() - .5) * 360, new Point(.5, .5), _backgroundcanvas.TransformToVisual(c).Transform(new Point(50, 50)), 1);

            _currentitem++;
        }

    }
}
