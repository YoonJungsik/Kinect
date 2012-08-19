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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;


using System.Collections;
using System.Collections.Generic;

using MIG;
using MIG.Client;

using MIRIA.Animations;
using MIRIA.Gestures;
using MIRIA.Interaction.MultiTouch;

namespace MIRIA.UIKit
{
    public enum ScrollDirection
    {
        Both = 0,
        Vertical,
        Horizontal
    }
    public class ScrollView : System.Windows.Controls.Panel, Touchable
    {
        /// <summary>
        /// MIRIA.UIKit.ScrollView Panel
        /// (c) 2008-2010 by Generoso Martello  -- generoso@martello.com
        /// </summary>
        /// 


        #region Public events
        
        /*
         *  ScrollViewer public events
         */
        public delegate void GestureDetectedHandler(object sender, GestureDetectedEventArgs e);
        public event GestureDetectedHandler GestureDetected;
        public delegate void ElementTappedHandler(object sender, ElementTappedEventArgs e);
        public event ElementTappedHandler ElementTapped;
        public class ElementTappedEventArgs : EventArgs
        {
            private FrameworkElement _tappedelement;
            private Point _taplocation;

            public ElementTappedEventArgs(FrameworkElement element, Point location)
            {
                _tappedelement = element;
                _taplocation = location;
            }

            public FrameworkElement TappedElement
            {
                get { return _tappedelement; }
            }

            public Point TapLocation
            {
                get { return _taplocation; }
            }
        }

        public delegate void ElementHoldHandler(object sender, ElementHoldEventArgs e);
        public event ElementHoldHandler ElementHold;
        public class ElementHoldEventArgs : EventArgs
        {
            private FrameworkElement _holdedelement;
            private Point _holdlocation;

            public ElementHoldEventArgs(FrameworkElement element, Point location)
            {
                _holdedelement = element;
                _holdlocation = location;
            }

            public FrameworkElement HoldedElement
            {
                get { return _holdedelement; }
            }
            public Point HoldLocation
            {
                get { return _holdlocation; }
            }
        }


//        public delegate void PageChangedHandler(object sender, RoutedEventArgs e);
//        public event PageChangedHandler PageChanged;
        
        #endregion


        #region Dependency Properties

        public static readonly DependencyProperty HookableProperty = DependencyProperty.RegisterAttached("Hookable", typeof(bool), typeof(ScrollView),
                                                                        new PropertyMetadata(false, null));

        public static void SetHookable( DependencyObject obj, bool hookable )
        {
            obj.SetValue(HookableProperty, hookable);
        }
        public static bool GetHookable( DependencyObject obj )
        {
                 return ( bool )obj.GetValue( HookableProperty );
        }

        #endregion


        #region Private members

        /*
         *  Gestures interpreter for detecting fingers gestures over the scroller
         */
        private TouchGestures _gesturesinterpreter;

        /*
         *  Animation helper for animating the scroller containing items
         */
        private Animations.TransformHelper _scrollertransformhelper;

        /*
         *  Reference to the child panel containing items
         */
        private Panel _scrollerpanel;

        private string _mytouchid = "";
        private ScrollDirection _direction = ScrollDirection.Both;
        //
        private Canvas _scrollbarscanvas;
        private Border _scrollbarh;
        private Border _scrollbarv;
        private bool _scrollbarhpressed = false;
        internal bool _scrollbarvpressed = false;
        private bool _scrollbarsenabled = true;

        private bool _hbarinvert = false;
        private bool _vbarinvert = false;

        private bool _gesturescale = false;
        private bool _justtapped = false;

        private Point _starttranslate = new Point();
        private Point _dragstart = new Point();

        #endregion


        #region Public interface

        public ScrollView()
        {
            this.Background = new SolidColorBrush(Colors.Transparent);
            this.SizeChanged += new SizeChangedEventHandler(ScrollView_SizeChanged);
            this.LayoutUpdated += new EventHandler(ScrollView_LayoutUpdated);
            this.MouseEnter += new MouseEventHandler(ScrollView_MouseEnter);
            this.MouseLeave += new MouseEventHandler(ScrollView_MouseLeave);
            this.MouseMove += new MouseEventHandler(ScrollView_MouseMove);
            this.MouseWheel += new MouseWheelEventHandler(ScrollView_MouseWheel);
        }

        public ScrollDirection Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }

        public bool ScrollBarsEnabled
        {
            get { return _scrollbarsenabled; }
            set { _scrollbarsenabled = value; }
        }

        public Panel Scroller
        {
            get { return _scrollerpanel; }
        }

        public TransformHelper ScrollerTransformHelper
        {
            get { return _scrollertransformhelper; }
        }

        public TouchGestures Gestures
        {
            get { return _gesturesinterpreter; }
        }

        public void ZoomToElement(FrameworkElement element)
        {
            ZoomToElement(element, _scrollertransformhelper.Scale);
        }

        public void ZoomToElement(FrameworkElement element, double scale)
        {
            if (element == null) return;
            this.Dispatcher.BeginInvoke(() =>
            {
                _scrollertransformhelper.Animate(0D, new Point(scale, scale), _scrollertransformhelper.CenterElement(element, scale), _scrollertransformhelper.Delay);
            });
        }

        public void ZoomToElementHeight(FrameworkElement element)
        {
            double scale = this.RenderSize.Height / element.RenderSize.Height / 1.0;
            ZoomToElement(element, scale);
        }

        public void ZoomToElementWidth(FrameworkElement element)
        {
            double scale = (this.RenderSize.Width / element.RenderSize.Width / 1.0);
            ZoomToElement(element, scale);
        }

        public void ZoomToFullView()
        {
            double scaleh = (this.RenderSize.Height / _scrollerpanel.RenderSize.Height / 1.0);
            double scalew = (this.RenderSize.Width / _scrollerpanel.RenderSize.Width / 1.0);
            if (scaleh < scalew)
            {
                ZoomToElement(_scrollerpanel, scaleh);
            }
            else
            {
                ZoomToElement(_scrollerpanel, scalew);
            }
        }

        public void ZoomToFullHeight()
        {
            double scale = (this.RenderSize.Height / _scrollerpanel.RenderSize.Height / 1.0);
            FrameworkElement el = GetElementInView();
            if (el == null) el = _scrollerpanel;
            Point pc = new Point(_scrollertransformhelper.CenterElement(el, scale).X, _scrollertransformhelper.CenterElement(_scrollerpanel, scale).Y);
            this.Dispatcher.BeginInvoke(() =>
            {
                _scrollertransformhelper.Animate(0D, new Point(scale, scale), pc, _scrollertransformhelper.Delay);
            });
        }

        public FrameworkElement FirstTouchedElement
        {
            get { return (_firsttouchedelement as FrameworkElement); }
        }

        public FrameworkElement GetElementInView()
        {
            Point pc = new Point(this.ActualWidth / 2, this.ActualHeight / 2);
            pc = this.TransformToVisual(Application.Current.RootVisual).Transform(pc);
            return _elementatlocation(pc);
        }

        public bool HorizontalScrollbarInvert
        {
            get { return _hbarinvert; }
            set { _hbarinvert = value; }
        }

        public bool VerticalScrollbarInvert
        {
            get { return _vbarinvert; }
            set { _vbarinvert = value; }
        }

        public void Move(Point moveshift)
        {
            _translate(moveshift);
        }

        public void SimulateGesture(Gestures.TouchGesture gesture)
        {
            _simulategesture(gesture);
        }

        #endregion


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
            {
                if (_direction == ScrollDirection.Vertical)
                {
                    infinite.Width = this.Width;
                }
                if (_direction == ScrollDirection.Horizontal)
                {
                    infinite.Height = this.Height;
                }
                child.Measure(infinite);
            }

            return new Size(this.Width, this.Height);
        }

        protected override Size ArrangeOverride(Size availablesize)
        {
            double cx = 0, cy = 0;
            foreach (FrameworkElement child in Children)
            {
                if (child.Equals(_scrollbarscanvas))
                {
                    child.Arrange(new Rect(new Point(), child.DesiredSize));
                }
                else
                {
                    Point location = new Point(cx, cy);
                    Size sz = child.DesiredSize;
                    //if (!_gesturescale) // TODO: check if this is really useful, otherwise remove this 'if' line
                    {
                        //sz = new Size(this.Width, this.Height);
                        if (sz.Width < this.Width) sz.Width = this.Width;
                        if (sz.Height < this.Height) sz.Height = this.Height;
                        child.Arrange(new Rect(location, sz));
                    }
                    cx += sz.Width;
                    cy += sz.Height;
                }
            }

            return availablesize;
        }

        void ScrollView_LayoutUpdated(object sender, EventArgs e)
        {
            FrameworkElement parent = VisualTreeHelper.GetParent(this) as FrameworkElement;
            if (_scrollerpanel == null)
            {
                _init();
                if (this.Width == 0)
                    this.Width = parent.ActualWidth;
                if (this.Height == 0)
                    this.Height = parent.ActualHeight;
            }

            if (this.Width == 0)
                this.Width = parent.ActualWidth;
            if (this.Height == 0)
                this.Height = parent.ActualHeight;

        }

        void ScrollView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _clipview();
        }
        #endregion


        #region Mouse input handling

        void ScrollView_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _scrollertransformhelper.Delay = 0.3;
            _translate(new Point(0, e.Delta / 3));
        }

        void ScrollView_MouseLeave(object sender, MouseEventArgs e)
        {
            _scrollbarshide();
        }

        void ScrollView_MouseEnter(object sender, MouseEventArgs e)
        {
            _scrollbarsshow();
        }

        void ScrollView_MouseMove(object sender, MouseEventArgs e)
        {
            _scrollbarsupdate();
        }

        #endregion


        #region Touch input and Gestures handling

        private UIElement _firsttouchedelement = null;

        // Touchable interface events (these are fired by TouchListener over any touchable element)
        public void FingerDown(object sender, FingerTouchEventArgs e)
        {
            IEnumerable<UIElement> hitsh = VisualTreeHelper.FindElementsInHostCoordinates(e.Finger.Position, this);
            if (_gesturesinterpreter.Fingers.Count == 0 && hitsh.Count() > 0)
            {
                _firsttouchedelement = hitsh.ElementAt(0);
            }
            foreach (FrameworkElement uiel in hitsh)
            {
                if (uiel != null)
                {
                    double vxf = this.ActualWidth / _scrollerpanel.ActualWidth;
                    double vyf = this.ActualHeight / _scrollerpanel.ActualHeight;
                    if (uiel.Equals(_scrollbarh) && uiel.Visibility == Visibility.Visible)
                    {
                        _scrollbarhpressed = true;
                        break;
                    }
                    else if (uiel.Equals(_scrollbarv) && uiel.Visibility == Visibility.Visible)
                    {
                        _scrollbarvpressed = true;
                        break;
                    }
                }
            }
            //
            _scrollbarv.Opacity = 0.25;
            _scrollbarh.Opacity = 0.25;
            //
            if (_scrollertransformhelper.IsRunning)
            {
                _scrollertransformhelper.Stop();
            }
            //
            double delay = _scrollertransformhelper.Delay;
            _scrollertransformhelper.Delay = 0;
            _gesturesinterpreter.FingerUpdate(e.Finger.Identifier, e.Finger.Position);
            _scrollertransformhelper.Delay = delay;
            //
            if (_gesturesinterpreter.Fingers.Count == 1)
            {
                _mytouchid = e.Finger.Identifier;
                _starttranslate = _scrollertransformhelper.Translate;
                _dragstart = e.Finger.Position;
            }
            //
//            _justtapped = false;
            e.Handled = true;
        }
        public void FingerMove(object sender, FingerTouchEventArgs e)
        {
            if (!_scrollbarhpressed && !_scrollbarvpressed)
            {
                _gesturesinterpreter.FingerUpdate(e.Finger.Identifier, e.Finger.Position);
            }
            if (e.Finger.Identifier != _mytouchid || _gesturesinterpreter.Fingers.Count > 1) return;
            //
            _scrollertransformhelper.Delay = 0.25;

            double vxf = this.ActualWidth / _scrollerpanel.ActualWidth;
            double vyf = this.ActualHeight / _scrollerpanel.ActualHeight;
            if (_scrollbarhpressed)
            {
                _scrollertransformhelper.Translate = new Point(_starttranslate.X - ((e.Finger.Position.X - _dragstart.X) / vxf) , _starttranslate.Y);
            }
            else if (_scrollbarvpressed)
            {
                _scrollertransformhelper.Translate = new Point(_starttranslate.X, _starttranslate.Y - ((e.Finger.Position.Y - _dragstart.Y) / vyf));
            }
            else
            {
                switch (_direction)
                {
                    case ScrollDirection.Vertical:
                        _scrollertransformhelper.Translate = new Point(_starttranslate.X, _starttranslate.Y + (e.Finger.Position.Y - _dragstart.Y));
                        break;
                    case ScrollDirection.Horizontal:
                        _scrollertransformhelper.Translate = new Point(_starttranslate.X + (e.Finger.Position.X - _dragstart.X), (e.Finger.Position.Y - _dragstart.Y));
                        break;
                    case ScrollDirection.Both:
                        if (_gesturesinterpreter.CurrentGesture == TouchGesture.MOVE_EAST || _gesturesinterpreter.CurrentGesture == TouchGesture.MOVE_WEST)// _scrollerpanel.ActualHeight <= this.Height)
                        {
                            _scrollertransformhelper.Translate = new Point(_starttranslate.X + (e.Finger.Position.X - _dragstart.X), _scrollertransformhelper.Translate.Y);
                        }
                        else if (_gesturesinterpreter.CurrentGesture == TouchGesture.MOVE_NORTH || _gesturesinterpreter.CurrentGesture == TouchGesture.MOVE_SOUTH) //_scrollerpanel.ActualWidth <= this.Width)
                        {
                            _scrollertransformhelper.Translate = new Point(_scrollertransformhelper.Translate.X, _starttranslate.Y + (e.Finger.Position.Y - _dragstart.Y));
                        }
                        //else
                        // {
                        //     _scrollertransformhelper.Translate = new Point(_starttranslate.X + (e.Finger.Position.X - _dragstart.X), _starttranslate.Y + (e.Finger.Position.Y - _dragstart.Y));
                        //}
                        break;
                }
            }
            //
            _scrollbarsupdate();
            //
            e.Handled = true;
        }
        public void FingerUp(object sender, FingerTouchEventArgs e)
        {
//            if (e.Finger.Identifier != _mytouchid)
//            {
//                _gesturesinterpreter.FingerRemove(e.Finger.Identifier);
//                return;
//            }

            if (e.Finger.Identifier == _mytouchid)
            {
                _mytouchid = "";
            }

            // it has to be read before gessture interpreter remove the last finger
            // otherwise it is going to be set to NONE
            TouchGesture lastgesture = _gesturesinterpreter.CurrentGesture;
            Finger lastfinger = _gesturesinterpreter.Fingers.First<Finger>(delegate(Finger f) { return f.Identifier == e.Finger.Identifier; });
            //
            _gesturesinterpreter.FingerRemove(e.Finger.Identifier);
            //
            if (_gesturesinterpreter.Fingers.Count == 0 && !_justtapped)
            {
                if (!_gesturescale && !(_scrollbarhpressed || _scrollbarvpressed))
                {
                    double speed = Utility.Vector2D.Distance(new Point(0, 0), new Point(_gesturesinterpreter.Acceleration.X / 100, _gesturesinterpreter.Acceleration.Y / 100));

                    //                Animator.AnimatePropertyStop(_scrollbarv, "Opacity");
                    //                Animator.AnimatePropertyStop(_scrollbarh, "Opacity");
                    //                _scrollbarv.Opacity = 0.0;
                    //                _scrollbarh.Opacity = 0.0;

                    Point t = new Point();
                    FrameworkElement uiel = GetElementInView();

                    if (speed <= 8 && lastfinger.PathLength > 20 && lastfinger.PathLength < 150)
                    {
                        _scrollertransformhelper.Delay = 0.75; // *speed;
                        bool hooked = false;
                        if (uiel != null && _elementfitview(uiel))
                        {
                            Point pc = this.TransformToVisual(Application.Current.RootVisual).Transform(new Point(this.ActualWidth / 2, this.ActualHeight / 2));
                            uiel = _elementatlocation(pc);
                            _translate(new Point());
                            hooked = (_elementhook(uiel, lastgesture) != null);
                            if (hooked) return;
                        }
                        if (!hooked)
                        {
                            Point p1 = this.TransformToVisual(_scrollerpanel).Transform(new Point());
                            Point p2 = this.TransformToVisual(_scrollerpanel).Transform(new Point(this.ActualWidth, this.ActualHeight));

                            double szx = p2.X - p1.X;
                            double szy = p2.Y - p1.Y;
                            if (lastgesture == TouchGesture.MOVE_NORTH || lastgesture == TouchGesture.MOVE_NORTHEAST || lastgesture == TouchGesture.MOVE_NORTHWEST)
                            {
                                t.Y = -szy + ((-lastfinger.Position.Y + lastfinger.StartPosition.Y) / _scrollertransformhelper.ScaleY);
                            }
                            if (lastgesture == TouchGesture.MOVE_SOUTH || lastgesture == TouchGesture.MOVE_SOUTHEAST || lastgesture == TouchGesture.MOVE_SOUTHWEST)
                            {
                                t.Y = szy + ((-lastfinger.Position.Y + lastfinger.StartPosition.Y) / _scrollertransformhelper.ScaleY);
                            }
                            if (lastgesture == TouchGesture.MOVE_EAST || lastgesture == TouchGesture.MOVE_NORTHEAST || lastgesture == TouchGesture.MOVE_SOUTHEAST)
                            {
                                t.X = szx + ((-lastfinger.Position.X + lastfinger.StartPosition.X) / _scrollertransformhelper.ScaleX);
                            }
                            if (lastgesture == TouchGesture.MOVE_WEST || lastgesture == TouchGesture.MOVE_NORTHWEST || lastgesture == TouchGesture.MOVE_SOUTHWEST)
                            {
                                t.X = -szx + ((-lastfinger.Position.X + lastfinger.StartPosition.X) / _scrollertransformhelper.ScaleX);
                            }
                        }
                    }
                    else
                    {
                        if (speed > 20) speed = 20;
                        _scrollertransformhelper.Delay = 0.50 * speed;
                        //
                        t = new Point(_gesturesinterpreter.Acceleration.X / 100, _gesturesinterpreter.Acceleration.Y / 100); // _gesturesinterpreter.Acceleration;
                        t.X *= (_scrollertransformhelper.Delay * speed * 2); //(speed < 12 ? 3 : 1.2));
                        t.Y *= (_scrollertransformhelper.Delay * speed * 2); //(speed < 12 ? 3 : 1.2));
                        //
                        t.X /= _scrollertransformhelper.ScaleX;
                        t.Y /= _scrollertransformhelper.ScaleY;
                    }
                    //
                    _translate(t);
                }
                else
                {
                    _translate(new Point());
                }
                //
                _scrollbarhpressed = false;
                _scrollbarvpressed = false;
            }
            _justtapped = false;
            //
            e.Handled = true;
        }


        #endregion


        #region Other private methods

        private void _init()
        {
            if (VisualTreeHelper.GetChildrenCount(this) == 0) return;

            if (_scrollerpanel != null) // already inited
                return;

            DependencyObject child = VisualTreeHelper.GetChild(this, 0);
            if (child != null && child is Panel)
            {
                _scrollerpanel = (Panel)child;

                _scrollertransformhelper = new Animations.TransformHelper(_scrollerpanel);
                _scrollertransformhelper.AnimationStarting += new TransformHelper.AnimationStartingHandler(_scrollertransformhelper_AnimationStarting);
                _scrollertransformhelper.AnimationComplete += new TransformHelper.AnimationCompleteHandler(_scrollertransformhelper_AnimationComplete);

                _gesturesinterpreter = new TouchGestures(_scrollerpanel);
                _gesturesinterpreter.Translate += new TouchGestures.GestureTranslateHandler(_gesturesinterpreter_Translate); //Translate;
                _gesturesinterpreter.Tap += new TouchGestures.GestureTapHandler(_gesturesinterpreter_Tap);
                _gesturesinterpreter.Hold += new TouchGestures.GestureHold(_gesturesinterpreter_Hold);
                _gesturesinterpreter.Scale += new TouchGestures.GestureScaleHandler(_gesturesinterpreter_Scale);
                _gesturesinterpreter.GestureDetected += new TouchGestures.GestureDetectedHandler(_gesturesinterpreter_GestureDetected);

                // add scrollbars
                //
                _scrollbarscanvas = new Canvas();
                _scrollbarscanvas.SetValue(Canvas.ZIndexProperty, 255);
                //
                _scrollbarh = new Border();
                _scrollbarh.BorderBrush = new SolidColorBrush(Colors.White);
                _scrollbarh.BorderThickness = new Thickness(2.0);
                _scrollbarh.CornerRadius = new CornerRadius(5.0);
                _scrollbarh.Background = new SolidColorBrush(Colors.Black);
                _scrollbarh.Opacity = 0.3;
                _scrollbarh.Width = this.Width;
                _scrollbarh.Height = 40D;
                _scrollbarh.Cursor = Cursors.Hand;
                _scrollbarscanvas.Children.Add(_scrollbarh);
                //
                _scrollbarv = new Border();
                _scrollbarv.BorderBrush = new SolidColorBrush(Colors.White);
                _scrollbarv.BorderThickness = new Thickness(2.0);
                _scrollbarv.CornerRadius = new CornerRadius(5.0);
                _scrollbarv.Background = new SolidColorBrush(Colors.Black);
                _scrollbarv.Opacity = 0.3;
                _scrollbarv.Width = 40D;
                _scrollbarv.Height = this.Height;
                _scrollbarv.Cursor = Cursors.Hand;
                _scrollbarscanvas.Children.Add(_scrollbarv);
                //
                this.Children.Add(_scrollbarscanvas);
                //
                _scrollbarsupdate();
            }
            else
            {
                // THROW EXCEPTION IF NO PANEL FOUND
                throw(new Exception("MIRIA.UIKIT.ScrollView child element must be subclass of Panel"));
            }
        }

        void _clipview()
        {
            RectangleGeometry clip = new RectangleGeometry();
            clip.Rect = new Rect(0, 0, this.ActualWidth, this.ActualHeight);
            this.Clip = clip;
        }

        void _gesturesinterpreter_GestureDetected(object sender, GestureDetectedEventArgs e)
        {
            if (GestureDetected != null) GestureDetected(this, e);
        }

        void _scrollertransformhelper_AnimationStarting(object sender)
        {
        }

        void _gesturesinterpreter_Hold(object sender, GestureHoldEventArgs e)
        {
            IEnumerable<UIElement> hits = VisualTreeHelper.FindElementsInHostCoordinates(this.TransformToVisual(Application.Current.RootVisual).Transform(e.HoldLocation), this);
            foreach (FrameworkElement uiel in hits)
            {
                bool hookable = (bool)uiel.GetValue(HookableProperty);
                if (hookable && ElementHold != null) ElementHold(this, new ElementHoldEventArgs(uiel, e.HoldLocation));
                break;
            }
        }

        void _simulategesture(TouchGesture gesture)
        {
            //FrameworkElement hookedel = _elementhook(GetElementInView(), gesture);
            //if (hookedel == null)
            {
                Point t = new Point();

                Point p1 = this.TransformToVisual(_scrollerpanel).Transform(new Point());
                Point p2 = this.TransformToVisual(_scrollerpanel).Transform(new Point(this.RenderSize.Width, this.RenderSize.Height));

                double szx = p2.X - p1.X;
                double szy = p2.Y - p1.Y;
                if (gesture == TouchGesture.MOVE_NORTH || gesture == TouchGesture.MOVE_NORTHEAST || gesture == TouchGesture.MOVE_NORTHWEST)
                {
                    t.Y = -szy;
                }
                if (gesture == TouchGesture.MOVE_SOUTH || gesture == TouchGesture.MOVE_SOUTHEAST || gesture == TouchGesture.MOVE_SOUTHWEST)
                {
                    t.Y = szy;
                }
                if (gesture == TouchGesture.MOVE_EAST || gesture == TouchGesture.MOVE_NORTHEAST || gesture == TouchGesture.MOVE_SOUTHEAST)
                {
                    t.X = szx;
                }
                if (gesture == TouchGesture.MOVE_WEST || gesture == TouchGesture.MOVE_NORTHWEST || gesture == TouchGesture.MOVE_SOUTHWEST)
                {
                    t.X = -szx;
                }

                _translate(t);
            }
        }

        void _gesturesinterpreter_Tap(object sender, GestureTapEventArgs e)
        {
            IEnumerable<UIElement> hits = VisualTreeHelper.FindElementsInHostCoordinates(e.TapLocation, this);
            foreach (FrameworkElement uiel in hits)
            {
                bool hookable = (bool)uiel.GetValue(HookableProperty);
                if (uiel != null && hookable)
                {
                    _justtapped = true;
                    if (ElementTapped != null) ElementTapped(this, new ElementTappedEventArgs(uiel, e.TapLocation));
                    break;
                }
            }
        }

        void _gesturesinterpreter_Translate(object sender, GestureTranslateEventArgs e)
        {
        }

        void _gesturesinterpreter_Scale(object sender, GestureScaleEventArgs e)
        {
            // Use GestureDetected event insted from this.Gestures object property
            /*
            if (e.ScaleFactor <= 0.94 && !_scrollertransformhelper.IsRunning && !_gesturescale)
            {
                _gesturescale = true;
                //
                IEnumerable<UIElement> hits = VisualTreeHelper.FindElementsInHostCoordinates(e.FingerA.StartPosition, Application.Current.RootVisual);
                foreach (FrameworkElement el in hits)
                {
                    bool hookable = (bool)el.GetValue(HookableProperty);
                    if (hookable)
                    {
                        _scrollertransformhelper.Delay = 0.5D;
                        ZoomToElement(el, _scrollertransformhelper.Scale * 0.5); //e.ScaleFactor);
                        break;
                    }
                }
            }
            else if (e.ScaleFactor >= 1.06 && !_scrollertransformhelper.IsRunning && !_gesturescale)
            {
                _gesturescale = true;
                //
                IEnumerable<UIElement> hits = VisualTreeHelper.FindElementsInHostCoordinates(e.FingerA.StartPosition, Application.Current.RootVisual);
                foreach (FrameworkElement el in hits)
                {
                    bool hookable = (bool)el.GetValue(HookableProperty);
                    if (hookable)
                    {
                        _scrollertransformhelper.Delay = 0.5D;
                        ZoomToElement(el, _scrollertransformhelper.Scale / 0.5); //e.ScaleFactor);
                        break;
                    }
                }
            }
            */
        }

        void _scrollertransformhelper_AnimationComplete(object sender)
        {
            if (_gesturesinterpreter.Fingers.Count == 0)
            {
                _scrollbarsupdate();
                _scrollbarshide();
            }
            if (_gesturescale)
            {
                _gesturescale = false;
            }
            // reset animation delay settings to its default
//            _scrollertransformhelper.Delay = 0.25;
        }

        FrameworkElement _elementatlocation(Point pc)
        {
            Rect r = new Rect(pc.X - 10, pc.Y - 10, 20, 20);
            IEnumerable<UIElement> hits = VisualTreeHelper.FindElementsInHostCoordinates(r, Application.Current.RootVisual);
            FrameworkElement el = null;
            this.Clip = null;
            //
            foreach (FrameworkElement uiel in hits)
            {
                bool hookable = (bool)uiel.GetValue(HookableProperty);
                if (uiel != null && hookable)
                {
                    el = uiel;
                    break;
                }
            }
            //
            _clipview();
            //
            return el;
        }
        
        bool _elementfitview(FrameworkElement uiel)
        {
            Point sf = new Point(this.ActualWidth / (uiel.ActualWidth * _scrollertransformhelper.ScaleX), this.ActualHeight / (uiel.ActualHeight * _scrollertransformhelper.ScaleY));
            if (((sf.X > 0.70 && sf.X < 1.30) && uiel.ActualWidth >= uiel.ActualHeight)
                || ((sf.Y > 0.70 && sf.Y < 1.30) && uiel.ActualHeight >= uiel.ActualWidth)
                )
            {
                return true;
            }
            return false;
        }



        void _checkscrollerbound()
        {
            Point p1 = _scrollerpanel.TransformToVisual(this).Transform(new Point());
            Point p2 = _scrollerpanel.TransformToVisual(this).Transform(new Point(_scrollerpanel.ActualWidth, _scrollerpanel.ActualHeight));

            double szx = p2.X - p1.X;
            double szy = p2.Y - p1.Y;

            Point center = _scrollertransformhelper.CenterElement(_scrollerpanel, _scrollertransformhelper.Scale);

            if (szx <= this.ActualWidth && szy <= this.ActualHeight)
            {
//                _scrollertransformhelper.Delay = 0.25;
                ZoomToElement(_scrollerpanel);
            }
            else if (szx <= this.ActualWidth)
            {
//                _scrollertransformhelper.Delay = 0.25;
                _scrollertransformhelper.Translate = new Point(center.X, _scrollertransformhelper.Translate.Y);
            }
            else if (szy <= this.ActualHeight)
            {
//                _scrollertransformhelper.Delay = 0.25;
                _scrollertransformhelper.Translate = new Point(_scrollertransformhelper.Translate.X, center.Y);
            }
        }

        FrameworkElement _elementhook(FrameworkElement source, TouchGesture gesturedirection)
        {
            if (source == null) return null;

            Point cornertest = new Point();

            double hookdistance = 100;
            cornertest.X = source.ActualWidth / 2;
            cornertest.Y = source.ActualHeight / 2;
            if (gesturedirection == TouchGesture.MOVE_NORTH || gesturedirection == TouchGesture.MOVE_NORTHEAST || gesturedirection == TouchGesture.MOVE_NORTHWEST)
            {
                cornertest.Y = source.ActualHeight + hookdistance;
            }
            if (gesturedirection == TouchGesture.MOVE_SOUTH || gesturedirection == TouchGesture.MOVE_SOUTHEAST || gesturedirection == TouchGesture.MOVE_SOUTHWEST)
            {
                cornertest.Y = -hookdistance;
            }
            if (gesturedirection == TouchGesture.MOVE_EAST || gesturedirection == TouchGesture.MOVE_NORTHEAST || gesturedirection == TouchGesture.MOVE_SOUTHEAST)
            {
                cornertest.X = -hookdistance;
            }
            if (gesturedirection == TouchGesture.MOVE_WEST || gesturedirection == TouchGesture.MOVE_NORTHWEST || gesturedirection == TouchGesture.MOVE_SOUTHWEST)
            {
                cornertest.X = source.ActualWidth + hookdistance;
            }
            this.Clip = null;
            FrameworkElement hookedelement = null;
            Point ht = source.TransformToVisual(Application.Current.RootVisual).Transform(cornertest);
            Rect rht = new Rect() { X = ht.X - 10, Y = ht.Y - 10, Width = 20, Height = 20 };
            IEnumerable<UIElement> hitsn = VisualTreeHelper.FindElementsInHostCoordinates(rht, Application.Current.RootVisual);
            foreach (FrameworkElement el in hitsn)
            {
                bool hookable = (bool)el.GetValue(HookableProperty);
                if (el.Parent != null /* && !el.Equals(_scrollercanvas) */ && hookable)
                {
                    hookedelement = el;
                    break;
                }
            }
            //
            _clipview();
            //
            if (hookedelement != null)
            {
                ZoomToElement(hookedelement);
            }
            return hookedelement;
        }

        void _translate(Point c)
        {
            _scrollertransformhelper.Stop();

            this.Dispatcher.BeginInvoke(()=>{

            c = MIRIA.Utility.Vector2D.Rotate(c, new Point(0, 0), -_scrollertransformhelper.Angle * Math.PI / 180);

            double vxf = this.ActualWidth / _scrollerpanel.ActualWidth;
            double vyf = this.ActualHeight / _scrollerpanel.ActualHeight;
            if (_scrollbarhpressed)
            {
                c.X /= -vxf;
                c.Y = 0;
            }
            else if (_scrollbarvpressed)
            {
                c.X = 0;
                c.Y /= -vyf;
            }
            //
            Point lowerbound = this.TransformToVisual(_scrollerpanel).Transform(new Point());
            Point upperbound = this.TransformToVisual(_scrollerpanel).Transform(new Point(this.ActualWidth, this.ActualHeight));
            Point outerlimit = this.TransformToVisual(_scrollerpanel).Transform(new Point(this.ActualWidth / 2, this.ActualHeight / 2));
            outerlimit.X = 0;
            outerlimit.Y = 0;
            if (_gesturesinterpreter.Fingers.Count == 1 && !(_scrollbarvpressed || _scrollbarhpressed))
            {
                outerlimit.X = this.Width / 2;
                outerlimit.Y = this.Height / 2;
            }
            lowerbound.X += outerlimit.X;
            lowerbound.Y += outerlimit.Y;
            upperbound.X -= outerlimit.X;
            upperbound.Y -= outerlimit.Y;

            Point nc = new Point(c.X, c.Y);

            Point p1 = _scrollerpanel.TransformToVisual(this).Transform(new Point());
            Point p2 = _scrollerpanel.TransformToVisual(this).Transform(new Point(_scrollerpanel.ActualWidth, _scrollerpanel.ActualHeight));

            double szx = p2.X - p1.X;
            double szy = p2.Y - p1.Y;
            if (szx >= this.ActualWidth)
            {
                if (nc.X < -(_scrollerpanel.ActualWidth - upperbound.X))
                {
                    nc.X = -(_scrollerpanel.ActualWidth - upperbound.X);
                }
                else if (nc.X > lowerbound.X)
                {
                    nc.X = lowerbound.X;
                }
            }
            else
            {
                nc.X = 0;
            }
            if (szy >= this.ActualHeight)
            {
                if (nc.Y < -(_scrollerpanel.ActualHeight - upperbound.Y))
                {
                    nc.Y = -(_scrollerpanel.ActualHeight - upperbound.Y);
                }
                else if (nc.Y > lowerbound.Y)
                {
                    nc.Y = lowerbound.Y;
                }
            }
            else
            {
                nc.Y = 0;
            }

            double dc = MIRIA.Utility.Vector2D.Distance(new Point(), c);
            double dnc = MIRIA.Utility.Vector2D.Distance(new Point(), nc);

            if (dc - dnc != 0 && (c.X != 0 || c.Y != 0) && _scrollertransformhelper.Delay != 0)
            {
                _scrollertransformhelper.Delay = _scrollertransformhelper.Delay / Math.Abs(dc / dnc) / 2;
            }

            c = nc;
            c.X *= _scrollertransformhelper.ScaleX;
            c.Y *= _scrollertransformhelper.ScaleY;

            double nx = _scrollertransformhelper.Translate.X + c.X;
            double ny = _scrollertransformhelper.Translate.Y + c.Y;

            if (_direction == ScrollDirection.Horizontal)
                _scrollertransformhelper.Translate = new Point(nx, _scrollertransformhelper.Translate.Y);
            else if (_direction == ScrollDirection.Vertical)
                _scrollertransformhelper.Translate = new Point(_scrollertransformhelper.Translate.X, ny);
            else
                _scrollertransformhelper.Translate = new Point(nx, ny);

            _scrollbarsupdate();
            });
        }

        void _scrollbarshide()
        {
            if (_scrollbarv != null)
            {
                Animations.Animator.AnimateProperty(_scrollbarh, "Opacity", 0.25, 0.0, 3.0, null);
                Animations.Animator.AnimateProperty(_scrollbarv, "Opacity", 0.25, 0.0, 3.0, null);
            }
        }

        void _scrollbarsshow()
        {
            if (_scrollbarv != null)
            {
                Animations.Animator.AnimateProperty(_scrollbarh, "Opacity", 0.0, 0.25, 3.0, null);
                Animations.Animator.AnimateProperty(_scrollbarv, "Opacity", 0.0, 0.25, 3.0, null);
            }
        }

        void _scrollbarsupdate()
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                _scrollbarh.Visibility = Visibility.Collapsed;
                _scrollbarv.Visibility = Visibility.Collapsed;
                if (_scrollbarsenabled)
                {
                    double vf = this.ActualWidth / _scrollerpanel.ActualWidth;
                    if (vf < 1.0)
                    {
                        if (!double.IsInfinity(vf))
                        {
                            _scrollbarh.Width = this.ActualWidth * vf;
                            if (_scrollbarh.Width < 40) _scrollbarh.Width = 40;
                            if (_hbarinvert)
                            {
                                _scrollbarh.SetValue(Canvas.LeftProperty, -_scrollertransformhelper.Translate.X * vf);
                                _scrollbarh.SetValue(Canvas.TopProperty, 0);
                            }
                            else
                            {
                                _scrollbarh.SetValue(Canvas.LeftProperty, -_scrollertransformhelper.Translate.X * vf);
                                _scrollbarh.SetValue(Canvas.TopProperty, this.Height - _scrollbarh.Height);
                            }
                            _scrollbarh.Visibility = Visibility.Visible;
                        }
                    }
                    vf = this.ActualHeight / _scrollerpanel.ActualHeight;
                    if (vf < 1.0)
                    {
                        if (!double.IsInfinity(vf))
                        {
                            _scrollbarv.Height = this.ActualHeight * vf;
                            if (_scrollbarh.Height < 40) _scrollbarh.Height = 40;
                            if (_vbarinvert)
                            {
                                _scrollbarv.SetValue(Canvas.LeftProperty, 0);
                                _scrollbarv.SetValue(Canvas.TopProperty, -_scrollertransformhelper.Translate.Y * vf);
                            }
                            else
                            {
                                _scrollbarv.SetValue(Canvas.LeftProperty, this.Width - _scrollbarv.Width);
                                _scrollbarv.SetValue(Canvas.TopProperty, -_scrollertransformhelper.Translate.Y * vf);
                            }
                            _scrollbarv.Visibility = Visibility.Visible;
                        }
                    }
                }
            });
        }

        Point _getfocusshift(FrameworkElement element)
        {
            GeneralTransform trel = element.TransformToVisual(_scrollerpanel);
            Point tt = trel.Transform(new Point());
            Point t = tt;

            t.X = -t.X - _scrollertransformhelper.Translate.X;
            t.Y = -t.Y - _scrollertransformhelper.Translate.Y;
            
            return t;
        }


        #endregion

    }
}
