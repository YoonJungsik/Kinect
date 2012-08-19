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
using System.Windows.Resources;      // StreamResourceInfo
using System.Windows.Media.Imaging;  // BitmapImage

using System.Collections.Generic;

using MIRIA.Interaction.MultiTouch;
using MIRIA.Gestures;
using MIRIA.Utility;

namespace MIRIA.UIKit
{

    public class DragLimitReachedEventArgs
    {
        public Point DragOffset;

        public DragLimitReachedEventArgs(Point dragoffset)
        {
            DragOffset = dragoffset;
        }
    
    }


    public enum TButtonDragDirection
    {
        Any,
        Vertical,
        Horizontal
    }

    public enum TButtonDragValues
    {
        PositiveOrNegative,
        Positive,
        Negative
    }

    public class TButton : Grid, Touchable
    {
        public delegate void TappedHandler(object sender, FingerTouchEventArgs e);
        public event TappedHandler Tapped;
        public delegate void PressedHandler(object sender, FingerTouchEventArgs e);
        public event PressedHandler Pressed;
        public delegate void DropHandler(object sender, FingerTouchEventArgs e);
        public event DropHandler Drop;
        public delegate void DragHandler(object element, FingerTouchEventArgs e);
        public event DragHandler Drag;
        public delegate void DragLimitReachedHandler(object element, DragLimitReachedEventArgs e);
        public event DragLimitReachedHandler DragLimitReached;
        public delegate void LeaveHandler(object sender, FingerTouchEventArgs e);
        public event LeaveHandler Leave;
        public delegate void EnterHandler(object sender, FingerTouchEventArgs e);
        public event EnterHandler Enter;

        private bool _enabled = true;
        private bool _pressed = false;
        private bool _isdragenabled = true;
        private TButtonDragDirection _dragdirection = TButtonDragDirection.Any;
        private double _dragradius = double.PositiveInfinity;
        private Finger _myfinger;

        private Point _homelocation = new Point();
        private Point pz = new Point();

        private MediaElement sfx;

        private Animations.TransformHelper _transformhelper;
        private Point _starttranslate = new Point();
        private Point _dragstart = new Point();
        private bool _draglimitreached = false;
        private TButtonDragValues _dragvalues = TButtonDragValues.PositiveOrNegative;
        private int _myzindex = 0;
        private double _startscale = 1.0;

        public TButton()
        {
            _init();
        }

        public bool DisableAnimation { get; set; }

        public Point HomeLocation
        {
            get { return _homelocation; }
            set {
                _homelocation = value;
                if (!_pressed)
                {
                    _transformhelper.Delay = 1.0;
                    _transformhelper.Translate = _homelocation;
                    _transformhelper.Delay = 0.0;
                }
            }
        }


        public TButtonDragDirection DragDirection
        {
            get { return _dragdirection; }
            set { _dragdirection = value; }
        }

        public double DragRadius
        {
            get { return _dragradius; }
            set { _dragradius = value; }
        }

        
        public bool IsDragEnabled
        {
            get { return _isdragenabled; }
            set { _isdragenabled = value; }
        }

        public Animations.TransformHelper TransformHelper
        {
            get { return _transformhelper; }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }
        public bool IsPressed
        {
            get { return _pressed; }
        }

        public TButtonDragValues DragValidValues
        {
            get { return _dragvalues; }
            set { _dragvalues = value; }
        }

        public void FingerDown(object sender, FingerTouchEventArgs e)
        {
            if (_myfinger != null) return;

            _starttranslate = _transformhelper.Translate;
            _dragstart = e.Finger.Position;
            _draglimitreached = false;
            _startscale = _transformhelper.Scale;
            _myzindex = (int)this.GetValue(Canvas.ZIndexProperty);
            //
            _transformhelper.Delay = 0.25;
            //
            if (!DisableAnimation)
            {
                if (double.IsInfinity(_dragradius))
                {
                    _transformhelper.Scale = _startscale + .10;
                }
                else
                {
                    _transformhelper.Scale = _startscale - .05;
                }
            }
            this.SetValue(Canvas.ZIndexProperty, 100);
            //
            _myfinger = new Finger(e.Finger.Identifier, e.Finger.Position);

            pz = this.TransformToVisual(this.Parent as UIElement).Transform(new Point());

            _pressed = true;
            if (Pressed != null && _enabled) Pressed(this, new FingerTouchEventArgs( _myfinger.Clone() ));
        }

        public void FingerMove(object sender, FingerTouchEventArgs e)
        {
            if (_myfinger == null || (_myfinger != null && _myfinger.Identifier != e.Finger.Identifier)) return;
            _myfinger.Position = e.Finger.Position;
            if (_pressed)   
            {

                if (_isdragenabled)
                {
                    Point l = new Point(_starttranslate.X + (e.Finger.Position.X - _dragstart.X), _starttranslate.Y + (e.Finger.Position.Y - _dragstart.Y));
                    //l.X += Utility.Transform2d.AdjustToParent(this, _myfinger.Shift).X;
                    //l.Y += Utility.Transform2d.AdjustToParent(this, _myfinger.Shift).Y;
                    //
                    if (_dragvalues == TButtonDragValues.Positive && l.X < 0) 
                    { 
                        l.X = 0; 
                    }
                    else if (_dragvalues == TButtonDragValues.Negative && l.X > 0)
                    {
                        l.X = 0;
                    }
                    if (_dragvalues == TButtonDragValues.Positive && l.Y < 0)
                    {
                        l.Y = 0;
                    }
                    else if (_dragvalues == TButtonDragValues.Negative && l.Y > 0)
                    {
                        l.Y = 0;
                    }
                    //
                    if (!_draglimitreached)
                    {
                        if (Math.Abs(l.X) > _dragradius)
                        {
                            l.X = _dragradius * Math.Sign(l.X);
                            _draglimitreached = true;
                        }
                        if (Math.Abs(l.Y) > _dragradius)
                        {
                            l.Y = _dragradius * Math.Sign(l.Y);
                            _draglimitreached = true;
                        }
                        //
                        if (_draglimitreached && DragLimitReached != null)
                        {
                            DragLimitReached(this, new DragLimitReachedEventArgs(l));
                            //FingerUp(this, new FingerTouchEventArgs(_myfinger));
                        }
                    }
                    //
                    if (_dragdirection == TButtonDragDirection.Horizontal)
                    {
                        l.Y = 0;
                    }
                    else if (_dragdirection == TButtonDragDirection.Vertical)
                    {
                        l.X = 0;
                    }
                    //
                    //if (!DisableAnimation)
                    {
                        _transformhelper.Translate = l;
                    }
                    //
                    if (Drag != null && _enabled) Drag(this, new FingerTouchEventArgs(_myfinger.Clone()));
                    //
                    IEnumerable<UIElement> hits = VisualTreeHelper.FindElementsInHostCoordinates(e.Finger.Position, Application.Current.RootVisual);
                    foreach (FrameworkElement uiel in hits)
                    {
                        bool tbutton = uiel is TButton && !uiel.Equals(this);
                        if (uiel != null && tbutton && !((uiel as TButton).TransformHelper.IsRunning) && uiel.Parent.Equals(this.Parent))
                        {
                            Point pp = (uiel as TButton).HomeLocation;
                            (uiel as TButton).HomeLocation = this.HomeLocation;
                            this.HomeLocation = pp;
                            break;
                        }
                    }
                }
            }
        }
        public void FingerUp(object sender, FingerTouchEventArgs e)
        {
            if (_myfinger != null && _myfinger.Identifier != e.Finger.Identifier) return;
//            if (double.IsInfinity(_dragradius))
            if (!DisableAnimation)
            {
                _transformhelper.Delay = 0.50;
                _transformhelper.Scale = _startscale;
            }
            this.SetValue(Canvas.ZIndexProperty, _myzindex);
            //
            if (Tapped != null && _enabled && _myfinger != null && Utility.Vector2D.Distance(_myfinger.StartPosition, _myfinger.Position) < 10)
            {
                sfx.Play();
                Tapped(this, new FingerTouchEventArgs(_myfinger.Clone()));
            }
            _pressed = false;
            if (Drop != null && _enabled) Drop(this, new FingerTouchEventArgs( _myfinger.Clone() ));
            _myfinger = null;
            //
            if (Drop == null || !_draglimitreached)
            {
                _transformhelper.Translate = _homelocation;
            }
            _transformhelper.Delay = 0.0;
        }


        internal void _simulateleave()
        {
            if (Leave != null) Leave(this, null);
        }
        internal void _simulateenter()
        {
            if (Enter != null) Enter(this, null);
        }


        void _init()
        {
            _transformhelper = new Animations.TransformHelper(this);
            _transformhelper.Scale = _startscale;
            //
            sfx = new MediaElement();
            sfx.Volume = 1;
            // TODO: Convert it to embedded resource
            sfx.Source = new Uri("http://www.flashkit.com/downloads/soundfx/mp3/8411/Beep%20signal%20from%20NEO Sounds.mp3", UriKind.Absolute);
            //
            if (!UriParser.IsKnownScheme("pack"))
                UriParser.Register(new GenericUriParser(GenericUriParserOptions.GenericAuthority), "pack", -1);

            ResourceDictionary dict = new ResourceDictionary();
            Uri uri = new Uri("/MIRIA;component/Resources/Styles.xaml", UriKind.Relative);
            dict.Source = uri;
            Application.Current.Resources.MergedDictionaries.Add(dict);
            //
            //this.Style = (Style)Application.Current.Resources["GlassButton"];
        }
    }
}
