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

using System.Collections.Generic;

using MIRIA.Interaction.MultiTouch;
using MIRIA.Utility;

using System.Windows.Threading;

namespace MIRIA.Gestures
{
    // (c) 2008-2010 by Generoso Martello - generoso@martello.com


    public enum TouchGesture
    {
        DISABLED = -1,
        NONE = 0,
        MOVE_WEST = 1,
        MOVE_NORTH,
        MOVE_EAST,
        MOVE_SOUTH,
        MOVE_NORTHWEST,
        MOVE_NORTHEAST,
        MOVE_SOUTHWEST,
        MOVE_SOUTHEAST,
        ROTATE,
        SCALE,
        SLIDE_WEST = 11,
        SLIDE_NORTH,
        SLIDE_EAST,
        SLIDE_SOUTH,
        SLIDE_NORTHWEST,
        SLIDE_NORTHEAST,
        SLIDE_SOUTHWEST,
        SLIDE_SOUTHEAST
    }
    // basic implementation for Translate,Rotate,Scale gestures =)
    public class GestureRotateEventArgs : EventArgs
    {
        private double _angleshift;
        private Finger _fingera; // clone it before passing to args
        private Finger _fingerb; // clone it before passing to args

        public GestureRotateEventArgs(Finger fingera, Finger fingerb, double angleshift)
        {
            _angleshift = angleshift;
            _fingera = fingera;
            _fingerb = fingerb;
        }

        public double AngleShift
        {
            get { return _angleshift; }
        }
        public Finger FingerA
        {
            get { return _fingera; }
        }
        public Finger FingerB
        {
            get { return _fingerb; }
        }
    }
    public class GestureTranslateEventArgs : EventArgs
    {
        private Finger _finger;
        private Point _translationshift;

        public GestureTranslateEventArgs(Finger finger, Point translationshift)
        {
            _finger = finger;
            _translationshift = translationshift;
        }

        public Finger Finger
        {
            get { return _finger; }
        }
        public Point TranslationShift
        {
            get { return _translationshift; }
        }
    }
    public class GestureScaleEventArgs : EventArgs
    {
        private Finger _fingera;
        private Finger _fingerb;
        private double _scalefactor;

        public GestureScaleEventArgs(Finger fingera, Finger fingerb, double scalefactor)
        {
            _fingera = fingera;
            _fingerb = fingerb;
            _scalefactor = scalefactor;
        }

        public Finger FingerA
        {
            get { return _fingera; }
        }
        public Finger FingerB
        {
            get { return _fingerb; }
        }
        public double ScaleFactor
        {
            get { return _scalefactor; }
        }
    }
    public class GestureTapEventArgs : EventArgs
    {
        private Finger _finger;
        private Point _taplocation;

        public GestureTapEventArgs(Finger finger, Point taplocation)
        {
            _finger = finger;
            _taplocation = taplocation;
        }

        public Finger Finger
        {
            get { return _finger; }
        }
        public Point TapLocation
        {
            get { return _taplocation; }
        }
    }
    public class GestureHoldEventArgs : EventArgs
    {
        private Finger _finger;
        private Point _holdlocation;

        public GestureHoldEventArgs(Finger finger, Point holdlocation)
        {
            _finger = finger;
            _holdlocation = holdlocation;
        }

        public Finger Finger
        {
            get { return _finger; }
        }
        public Point HoldLocation
        {
            get { return _holdlocation; }
        }
    }
    public class GestureDetectedEventArgs : EventArgs
    {
        private TouchGesture _gesture;
        private object _gestureparameter;

        public GestureDetectedEventArgs(TouchGesture gesture, object gestureparameter)
        {
            _gesture = gesture;
            _gestureparameter = gestureparameter;
        }

        public TouchGesture Gesture
        {
            get { return _gesture; }
        }
        public Object GestureParameters
        {
            get { return _gestureparameter; }
        }
    }

    public class TouchGestures
    {
        private FingersStartInfo _fingerstartinfoa;
        private FingersStartInfo _fingersstartinfob;
        private List<Finger> _fingers = new List<Finger>();

        #region Gestures Interpreter Events Handlers

        public delegate void GestureRotateHandler(object sender, GestureRotateEventArgs e);
        public event GestureRotateHandler Rotate;

        public delegate void GestureTranslateHandler(object sender, GestureTranslateEventArgs e);
        public event GestureTranslateHandler Translate;

        public delegate void GestureScaleHandler(object sender, GestureScaleEventArgs e);
        public event GestureScaleHandler Scale;
        
        public delegate void GestureTapHandler(object sender, GestureTapEventArgs e);
        public event GestureTapHandler Tap;

        public delegate void GestureHold(object sender, GestureHoldEventArgs e);
        public event GestureHold Hold;

        public delegate void GestureDetectedHandler(object sender, GestureDetectedEventArgs e);
        public event GestureDetectedHandler GestureDetected;


        public delegate void GestureRelease(object sender);
        public event GestureRelease Release;
        
        //public delegate void GestureComplete(Point translate, double angle, double scale);
        //public event GestureComplete DataChanged;

        #endregion

        private Point _centroid;
        private FrameworkElement _targetelement;
        private Speedometer _speedometer;

        private bool _singleaction = false;
        private bool _twofingersgesture = false;
        private double _maxtapdistance = double.PositiveInfinity;


        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        DateTime _gesturegap = DateTime.Now;

        private object _currentgestureparam = null;

        public TouchGestures(FrameworkElement element)
        {
            _targetelement = element;
            _speedometer = new Speedometer(_targetelement);
            //
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 750);
            dispatcherTimer.Tick += (sender2, e2) => {
                dispatcherTimer.Stop();
                if (_singleaction && Utility.Vector2D.Distance(_fingerstartinfoa.A , _fingers[0].Position) < 5 && Hold != null)
                {
                    _singleaction = false; // cancel it for tap
                    Hold(this, new GestureHoldEventArgs(_fingers[0].Clone(), _fingers[0].Position));
                }
            };
            //
            _gesturesobserver = new System.Threading.Timer((object target) =>
            {
                //_detectgesture();
                //_gesturegap = DateTime.Now;
            });
        }

        public List<Finger> Fingers
        {
            get { return _fingers; }
        }
        
        public Point Acceleration
        {
            get { return _speedometer.GetSpeed(); }
        }


        public TouchGesture CurrentGesture
        {
            get { return _currentgesture; }
        }




        private void _detectgesture()
        {
            //if (_targetelement.FindName("DEBUG") != null)
            //{
            //    _targetelement.Dispatcher.BeginInvoke(() =>
            //    {
            //        TextBlock debug = _targetelement.FindName("DEBUG") as TextBlock;
            //        debug.Text = "S " + _gscaleamount + " R " + _grotateamount + " L " + _gslideamount;
            //    });
            //}


            object param = null;
            if (!_twofingersgesture && _lastdetectedgesture >= TouchGesture.MOVE_WEST && _lastdetectedgesture <= TouchGesture.MOVE_SOUTHEAST)
            {
                _currentgesture = _lastdetectedgesture;
            } 
            else if ((Math.Abs(_gscaleamount) < 0.6 || Math.Abs(_gscaleamount) > 1.7) /* && Math.Abs(_grotateamount) < 35 */ && _gslideamount < 3)
            {
                _currentgesture = TouchGesture.SCALE;
                param = _gscaleamount;
            }
            else if (Math.Abs(_grotateamount) >= 35 && _gslideamount < 3)
            {
                _currentgesture = TouchGesture.ROTATE;
                param = _grotateamount;
            }
            else if (_gslideamount >= 2)
            {
                _currentgesture = _lastdetectedgesture;
            }
            else
            {
                _currentgesture = TouchGesture.NONE;
            }
            //if (_currentgesture != Gestures.NONE)
            //    if (GestureDetected != null) GestureDetected(_currentgesture, param);
            //                if (_gscaleamount > 40)
            //                    ... ;
            _currentgestureparam = param;

            _gscaleamount = 1;
            _grotateamount = 0;
            _gslideamount = 0;
            _lastdetectedgesture = TouchGesture.NONE;


        }





        private TouchGesture _currentgesture = TouchGesture.NONE;
        private TouchGesture _lastdetectedgesture = 0;
        private double _scalethreshold = 0.3;
        private double _rotatethreshold = 7.0;
        private double _slidesensitivity = 1;



        private System.Threading.Timer _gesturesobserver;
        private double _gscaleamount = 1;
        private double _grotateamount = 0;
        private double _gslideamount = 0;
        private double _gmoveamount = 0;
        private void _observeGesture()
        {

            if (_fingers.Count > 1)
            {
                double a0 = _fingers[0].PathAngle;
                double a1 = _fingers[1].PathAngle;
                double l0 = _fingers[0].PathLength;
                double l1 = _fingers[1].PathLength;
                if (l0 > _slidesensitivity && l1 > _slidesensitivity)
                {
                    if (((a0 > 340 && a0 <= 360) || (a0 >= 0 && a0 < 20)) && ((a1 > 340 && a1 <= 360) || (a1 >= 0 && a1 < 20)))
                    {
                        _lastdetectedgesture = TouchGesture.SLIDE_EAST;
                        _gslideamount++;
                    }
                    else if (a0 > 160 && a0 < 200 && a1 > 160 && a1 < 200)
                    {
                        _lastdetectedgesture = TouchGesture.SLIDE_WEST;
                        _gslideamount++;
                    }
                    else if (a0 > 250 && a0 < 290 && a1 > 250 && a1 < 290)
                    {
                        _lastdetectedgesture = TouchGesture.SLIDE_NORTH;
                        _gslideamount++;
                    }
                    else if (a0 > 70 && a0 < 110 && a1 > 70 && a1 < 110)
                    {
                        _lastdetectedgesture = TouchGesture.SLIDE_SOUTH;
                        _gslideamount++;
                    }
                    else if (a0 > 210 && a0 < 270 && a1 > 210 && a1 < 270)
                    {
                        _lastdetectedgesture = TouchGesture.SLIDE_NORTHWEST;
                        _gslideamount++;
                    }
                    else if (a0 > 290 && a0 < 340 && a1 > 290 && a1 < 340)
                    {
                        _lastdetectedgesture = TouchGesture.SLIDE_NORTHEAST;
                        _gslideamount++;
                    }
                    else if (a0 > 110 && a0 < 160 && a1 > 110 && a1 < 160)
                    {
                        _lastdetectedgesture = TouchGesture.SLIDE_SOUTHWEST;
                        _gslideamount++;
                    }
                    else if (a0 > 20 && a0 < 70 && a1 > 20 && a1 < 70)
                    {
                        _lastdetectedgesture = TouchGesture.SLIDE_SOUTHEAST;
                        _gslideamount++;
                    }
                }
                else
                {
                   // _lastdetectedgesture = Gestures.NONE;
                }
                
            }
            else if (!_twofingersgesture && _fingers.Count == 1)
            {
                double a0 = _fingers[0].PathAngle;
                double l0 = _fingers[0].PathLength;
                if (l0 > 5) //_slidesensitivity / 2)
                {
                    if (((a0 > 340 && a0 <= 360) || (a0 >= 0 && a0 < 20)))
                    {
                        _lastdetectedgesture = TouchGesture.MOVE_EAST;
                        _gmoveamount++;
                    }
                    else if (a0 > 160 && a0 < 200)
                    {
                        _lastdetectedgesture = TouchGesture.MOVE_WEST;
                        _gmoveamount++;
                    }
                    else if (a0 > 250 && a0 < 290)
                    {
                        _lastdetectedgesture = TouchGesture.MOVE_NORTH;
                        _gmoveamount++;
                    }
                    else if (a0 > 70 && a0 < 110)
                    {
                        _lastdetectedgesture = TouchGesture.MOVE_SOUTH;
                        _gmoveamount++;
                    }
                    else if (a0 > 210 && a0 < 270)
                    {
                        _lastdetectedgesture = TouchGesture.MOVE_NORTHWEST;
                        _gmoveamount++;
                    }
                    else if (a0 > 290 && a0 < 340)
                    {
                        _lastdetectedgesture = TouchGesture.MOVE_NORTHEAST;
                        _gmoveamount++;
                    }
                    else if (a0 > 110 && a0 < 160)
                    {
                        _lastdetectedgesture = TouchGesture.MOVE_SOUTHWEST;
                        _gmoveamount++;
                    }
                    else if (a0 > 20 && a0 < 70)
                    {
                        _lastdetectedgesture = TouchGesture.MOVE_SOUTHEAST;
                        _gmoveamount++;
                    }
                }
                else
                {
                   // _lastdetectedgesture = Gestures.NONE;
                }
            }
////////////////////////////
            if (!_twofingersgesture && _lastdetectedgesture >= TouchGesture.MOVE_WEST && _lastdetectedgesture <= TouchGesture.MOVE_SOUTHEAST)
            {
                _currentgesture = _lastdetectedgesture;
            }
            else if ((Math.Abs(_gscaleamount) < 0.5 || Math.Abs(_gscaleamount) > 1.7) /*&& Math.Abs(_grotateamount) < 35*/ && _gslideamount < 3)
            {
                _currentgesture = TouchGesture.SCALE;
//                param = _gscaleamount;
            }
            else if (Math.Abs(_grotateamount) >= 35 && _gslideamount < 8)
            {
                _currentgesture = TouchGesture.ROTATE;
//                param = _grotateamount;
            }
            else if (_gslideamount >= 3)
            {
                _currentgesture = _lastdetectedgesture;
            }
            //else
            //{
            //    _currentgesture = Gestures.NONE;
            //}

        }



        private Point _getcentroid()
        {
            //System.Windows.Media.MatrixTransform trs = (System.Windows.Media.MatrixTransform)_targetelement.TransformToVisual(Application.Current.RootVisual);
            double w = _targetelement.ActualWidth; if (w == 0) w = _targetelement.RenderSize.Width;
            double h = _targetelement.ActualHeight; if (h == 0) w = _targetelement.RenderSize.Height;
            //Point t = trs.Transform(new Point((w / 2), (h / 2)));
            Point t = new Point((w / 2), (h / 2));
            t = _targetelement.TransformToVisual(Application.Current.RootVisual).Transform(t);
            if (_fingers.Count == 2)
            {
                Point p1 = _fingers[0].Position;
                Point p2 = _fingers[1].Position;
                t = new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
            }
            return t;
        }
        private PointCollection _getvertexes()
        {
            Point p1, p2, p3, p4;
            double w = _targetelement.ActualWidth; if (w == 0) w = _targetelement.RenderSize.Width;
            double h = _targetelement.ActualHeight; if (h == 0) w = _targetelement.RenderSize.Height;

            GeneralTransform tr = _targetelement.TransformToVisual(Application.Current.RootVisual);
            p1 = tr.Transform(new Point());
            p2 = tr.Transform(new Point(0, h));
            p3 = tr.Transform(new Point(w, 0));
            p4 = tr.Transform(new Point(w, h));

            PointCollection pc = new PointCollection();
            pc.Add(p1);
            pc.Add(p2);
            pc.Add(p3);
            pc.Add(p4);

            return pc;
        }


        public Point Centroid
        {
            get { return _getcentroid(); }
        }


        public void FingerUpdate(String identifier, Point p)
        {
            PointCollection vertexes = _getvertexes();
            Point cn = _getcentroid();

            //_detectgesture();
            _observeGesture();
            
            _centroid = cn;
            if (_fingers.Count == 2)
            {
                // effettuiamo traslazione e rotazione
                // sullo shsape in base alla nuova
                // posizione degli fingers
                Point p1 = _fingers[0].Position;
                Point p2 = _fingers[1].Position;

                // E ORA DOBBIAMO CALCOLARE LA NUOVA 
                // POSIZIONE DEL punto C
                double aaab = Vector2D.GetAngle(_fingerstartinfoa.A, _fingerstartinfoa.C);
                double seg_ab = Vector2D.Distance(_fingerstartinfoa.A, _fingerstartinfoa.B);
                double seg_ac = Vector2D.Distance(_fingerstartinfoa.A, _fingerstartinfoa.C);
                double seg_ppab = Vector2D.Distance(p1, p2);
                double seg_ppac = seg_ac * (seg_ppab / seg_ab);
                Point c = Vector2D.GetPoint(p1, seg_ppac, aaab * Math.PI / 180);
                double papb = Vector2D.GetAngle(p1, p2);
                double angolo = papb - Vector2D.GetAngle(_fingerstartinfoa.A, _fingerstartinfoa.B);
                c = Vector2D.Rotate(c, p1, angolo * Math.PI / 180);
                papb = Vector2D.GetAngle(p1, c);

                _fingersstartinfob.A = p1;
                _fingersstartinfob.B = p2;
                _fingersstartinfob.C = c;

                double newAngle = papb - aaab;
                double newScale = (seg_ppab / seg_ab);
                Point newTranslate = _adjusttoparentscale( new Point(c.X - _fingerstartinfoa.C.X, c.Y - _fingerstartinfoa.C.Y) );

                ///
                _grotateamount += newAngle;
                if (newScale != 0) _gscaleamount *= newScale;
                ///

                if (Rotate != null && newAngle != 0)
                {
                    Rotate(this, new GestureRotateEventArgs(_fingers[0].Clone(), _fingers[1].Clone(), newAngle));
                }
                if (Scale != null && newScale != 0)
                {
                    Scale(this, new GestureScaleEventArgs(_fingers[0].Clone(), _fingers[1].Clone(), newScale));
                }
                if (Translate != null && (newTranslate.X != 0 || newTranslate.Y != 0))
                {
                    Translate(this, new GestureTranslateEventArgs(_fingers[0].Clone(), newTranslate));
                }
                //if (DataChanged != null)
                //    DataChanged(newTranslate, newAngle, newScale);

                _fingerstartinfoa = new FingersStartInfo(p1, p2, c);

                _observeGesture();

//                _gesturesobserver.Change(350, 0);


            }
            else if (_fingers.Count == 1)
            {
                if (identifier == _fingers[0].Identifier)
                {
                    Point pc = _fingers[0].Position;
                    Point newTranslate = _adjusttoparentscale(new Point((p.X - pc.X), (p.Y - pc.Y)));
                    if (Translate != null && (newTranslate.X != 0 || newTranslate.Y != 0))
                    {
                        Translate(this, new GestureTranslateEventArgs(_fingers[0].Clone(), newTranslate));
                    }
                }

            }
            else
            {

            }


            int h = GetFingerById(identifier);
            if (h != -1)
            {
                _fingers[h].Position = p;
                //if (h == 0 && _fingers.Count > 1 && _fingers[1].Id == "H")
                //    _fingers[1].Update(Vector2D.Rotate(p, centroid, 45 * Math.PI / 180));
            }
            else if (_fingers.Count < 2)
            {
                _currentgesture = TouchGesture.NONE;
                
                if (_fingers.Count == 0)
                    _twofingersgesture = false;
                else if (_fingers.Count == 1)
                    _twofingersgesture = true;

                _fingers.Add(new Finger(identifier, p));

                _gesturegap = DateTime.Now;

                if (_fingers.Count == 1)
                {
                    _singleaction = true;
                    _lastdetectedgesture = TouchGesture.NONE;

                    _observeGesture();

                    //_gesturesobserver.Change(250, 0);

                    //
                    // controllo coordinate se collidono con uno dei angoli
                    // in fururo generalizzare per <n> vertexes (non solo x 4)
                    // 
                    if (NearBy(vertexes[0], p) ||
                        NearBy(vertexes[1], p) ||
                        NearBy(vertexes[2], p) ||
                        NearBy(vertexes[3], p)
                        )
                    {
                        _singleaction = false;
                        _fingers.Add(new Finger("H", _centroid));
                    }
                    else
                    {
                        _fingerstartinfoa = new FingersStartInfo(p, new Point(0, 0), new Point(0, 0));
                    }
                    // GestureHold Timer
                    dispatcherTimer.Stop();
                    dispatcherTimer.Start();

                }

                if (_fingers.Count == 2)
                {

                    dispatcherTimer.Stop();
                    _singleaction = false;
                    _currentgesture = _lastdetectedgesture = TouchGesture.NONE;
                    // E STATO APPENA AGGIUNTO IL SECONDO FINGER
                    // QUINDI SALVIAMO LA CONFIGURAZIONE INIZIALE DEL
                    // TRIANGOLO A-B-C 
                    // DOVE A (p1) e B (p2) SONO I DUE FINGER
                    // MENTRE C (center) è IL CENTRO DELLO SHAPE
                    Point p1 = _fingers[0].Position;
                    Point p2 = _fingers[1].Position;
                    _fingerstartinfoa = new FingersStartInfo(p1, p2, _centroid);
                    //
                    _observeGesture();

                    //_gesturesobserver.Change(250, 0);

                }
            }

            if (_fingers.Count > 0 && !_speedometer.IsEnabled)
            {
                _speedometer.Start();
            }

            if (_fingers.Count == 1)
            {
                double d = Utility.Vector2D.Distance(_fingerstartinfoa.A, _fingers[0].Position);
                if (_maxtapdistance < d || _maxtapdistance == double.PositiveInfinity)
                {
                    _maxtapdistance = d;
                }
            }
        }

        public void FingerRemove(string identifier)
        {
            dispatcherTimer.Stop();

            TimeSpan diff = DateTime.Now - _gesturegap;
            if (_fingers.Count == 1)
            {
                _detectgesture();
                if (GestureDetected != null && diff.TotalMilliseconds < 350) GestureDetected(this, new GestureDetectedEventArgs(_currentgesture, _currentgestureparam));
            }

            int h = GetFingerById(identifier);
            if (h != -1)
            {
                if (h == 0 && _fingers.Count > 1 && _fingers[1].Identifier == "H")
                {
                    _fingers.Remove(_fingers[1]);
                }
                else if (_fingers.Count == 1)
                {
                    _speedometer.Stop();
                }
                if (_fingers.Count == 1 && _maxtapdistance < 5 && _singleaction && diff.TotalMilliseconds < 150)
                {
                    if (Tap != null)
                    {
                        Point p = _fingers[0].Position;
                        //// adjust translate to scaling factor
                        //// ...
                        //FrameworkElement cs = canvas;
                        //while (cs != null && cs.Parent != null && cs.Parent.GetType().IsSubclassOf(typeof(FrameworkElement)))
                        //{
                        //    cs = (FrameworkElement)cs.Parent;
                        //    TranslateTransform tt = Animations.TransformHelper.GetTranslateTransform(cs);
                        //    if (tt != null)
                        //    {
                        //        p.X = p.X - tt.X;
                        //        p.Y = p.Y - tt.Y;
                        //    }
                        //}
                        Tap(this, new GestureTapEventArgs(_fingers[0].Clone(),  p));
                    }
                }
                else if (_fingers.Count == 1)
                {
                    _maxtapdistance = double.PositiveInfinity;
                    _twofingersgesture = false;
                    // Release
                    if (Release != null) Release(this);

                    _currentgesture = _lastdetectedgesture = TouchGesture.NONE;
                }
                _fingers.Remove(_fingers[h]);
                _singleaction = false;
            }
        }
        private int GetFingerById(string hid)
        {
            int found = -1;
            for (int x = 0; x < _fingers.Count; x++)
            {
                if (_fingers[x].Identifier.Equals(hid))
                {
                    found = x;
                    break;
                }
            }
            return found;
        }
        
        
        /* TEMPORARY METHODS UTILI PER FARE UN PO' DI DEBUG A VIDEO =D */
        private PointCollection GetSI()
        {
            PointCollection pc = new PointCollection();
            pc.Add(_fingerstartinfoa.A);
            pc.Add(_fingerstartinfoa.B);
            pc.Add(_fingerstartinfoa.C);
            pc.Add(_fingerstartinfoa.A);
            return pc;
        }
        private PointCollection GetSI2()
        {
            PointCollection pc = new PointCollection();
            pc.Add(_fingersstartinfob.A);
            pc.Add(_fingersstartinfob.B);
            pc.Add(_fingersstartinfob.C);
            pc.Add(_fingersstartinfob.A);
            return pc;
        }
        
        private bool NearBy(Point a, Point b)
        {
            return (b.X > a.X - 30 && b.X < a.X + 30 && b.Y > a.Y - 30 && b.Y < a.Y + 30);
        }


        private Point _adjusttoparentscale(Point p)
        {
			// TODO: fix that please =D
			//return p;
            return Utility.Transform2d.AdjustToParent(_targetelement, p);
        }


    }




}
