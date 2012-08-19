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
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using MIG.Client;

namespace MIRIA.Interaction.MultiTouch
{

    public class AccelerationUpdateEventArgs : EventArgs
    {
        private double _x;
        private double _y;
        private double _z;

        public AccelerationUpdateEventArgs(double x, double y, double z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public double X
        {
            get { return _x; }
        }
        public double Y
        {
            get { return _y; }
        }
        public double Z
        {
            get { return _z; }
        }
    }

    public class FingerInputEventArgs
    {
        private string _identifier;
        private Point? _position = null;

        private bool _handled = false;

        public FingerInputEventArgs(string identifier)
        {
            _identifier = identifier;
        }

        public FingerInputEventArgs(string identifier, Point position)
        {
            _identifier = identifier;
            _position = position;
        }


        public Point? Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public bool Handled
        {
            get { return _handled; }
            set { _handled = value; }
        }

        public string Identifier
        {
            get { return _identifier; }
        }
    }

    public class TouchListener
    {
        private MIG.Client.Devices.MultiTouch.MultitouchTuio _multitouchtuio;

        private bool _auraenabled = true;

        private List<MIG.Client.Devices.MultiTouch.Cursor> _cursors;
        private FrameworkElement _targetelement;
        private Size _targetsize;

        private Dictionary<string, FingerAura> _fingersaura = new Dictionary<string, FingerAura>();
        private Canvas _infolayer = new Canvas();

        private bool _processmouse = true;

        private bool _propagateinput;
        private bool mouseclicked = false;
        List<string> _tpids = new List<string>();
        private Dictionary<string, UIElement> hookShape = new Dictionary<string, UIElement>();

        #region TouchListener Events

        public delegate void AccelerationUpdateHandler(object sender, AccelerationUpdateEventArgs e);
        public event AccelerationUpdateHandler AccelerationUpdate;

        public delegate void FingerDownHandler(object sender, FingerInputEventArgs e);
        public event FingerDownHandler FingerDown;

        public delegate void FingerMoveHandler(object sender, FingerInputEventArgs e);
        public event FingerMoveHandler FingerMove;

        public delegate void FingerUpHandler(object sender, FingerInputEventArgs e);
        public event FingerUpHandler FingerUp;

        #endregion

        public TouchListener(FrameworkElement element)
        {
            _multitouchtuio = new MIG.Client.Devices.MultiTouch.MultitouchTuio();

            _propagateinput = true;
            _targetelement = element;

            (_targetelement as Panel).Children.Add(_infolayer);
            _cursors = new List<MIG.Client.Devices.MultiTouch.Cursor>();

            _targetelement.Dispatcher.BeginInvoke(() =>
            {
                _targetsize = new Size(_targetelement.ActualWidth, _targetelement.ActualHeight);
                //
                // Standard Mouse Input mapped to touch
                //
                _targetelement.MouseLeftButtonDown += new MouseButtonEventHandler(delegate(object o, MouseButtonEventArgs e) { _targetelement.CaptureMouse(); MouseCursorAdd(e); });
                _targetelement.MouseLeftButtonUp += new MouseButtonEventHandler(delegate { MouseCursorDel(); _targetelement.ReleaseMouseCapture(); });
                _targetelement.MouseMove += new MouseEventHandler(delegate(object o, MouseEventArgs e) { MouseCursorSet(e); });
                //
                // Multitouch TUIO input
                //
                _multitouchtuio.FingerDown += new MIG.Client.Devices.MultiTouch.MultitouchTuio.FingerDownHandler(MultiTouchListener_FingerDown);
                _multitouchtuio.FingerUp += new MIG.Client.Devices.MultiTouch.MultitouchTuio.FingerUpHandler(MultiTouchListener_FingerUp);
                _multitouchtuio.FingerMove += new MIG.Client.Devices.MultiTouch.MultitouchTuio.FingerMoveHandler(MultiTouchListener_FingerMove);
                _multitouchtuio.AccelerationUpdate += new MIG.Client.Devices.MultiTouch.MultitouchTuio.AccelerationUpdateHandler(MultiTouchListener_AccelerationUpdate);
                //
                // Windows 7 WM_TOUCH events fw
                //
                Touch.FrameReported += new TouchFrameEventHandler(Touch_FrameReported);
            });
        }

        public MIGListener MIGListener
        {
            get { return _multitouchtuio; }
        }

        public bool PropagateInput
        {
            get { return _propagateinput; }
            set { _propagateinput = value; }
        }

        public FrameworkElement TargetElement
        {
            get { return _targetelement; }
        }

        public bool ShowAuras
        {
            get { return _auraenabled; }
            set { _auraenabled = value; }
        }

        public bool ProcessMouseEvents
        {
            get { return _processmouse; }
            set { _processmouse = value; }
        }


        // Mouse input
        void MouseCursorAdd(MouseEventArgs args)
        {
            if (_processmouse)
            {
                Point me = args.GetPosition(_targetelement);
                //
                _cursoradd("MOUSE:0");
                _cursorupd("MOUSE:0", me);
                //
                mouseclicked = true;
                MouseCursorSet(args);
            }
        }
        void MouseCursorSet(MouseEventArgs args)
        {
            if (_processmouse)
            {
                if (mouseclicked)
                {
                    Point me = args.GetPosition(_targetelement);
                    //
                    _cursorupd("MOUSE:0", me);
                }
            }
        }
        void MouseCursorDel()
        {
            if (_processmouse)
            {
                mouseclicked = false;
                //
                _cursordel("MOUSE:0");
            }
        }

        // Win 7 / Silverlight 3 standard WM_TOUCH messages MultiTouch device input
        void Touch_FrameReported(object sender, TouchFrameEventArgs e)
        {
            TouchPointCollection touchcollection = e.GetTouchPoints(_targetelement);
            TouchPoint primarytouch = e.GetPrimaryTouchPoint(_targetelement);
            if (_tpids.Count == 0 && primarytouch != null && primarytouch.Action == TouchAction.Down)
            {
                try
                {
                    IEnumerable<UIElement> hits = VisualTreeHelper.FindElementsInHostCoordinates(_targetelement.TransformToVisual(Application.Current.RootVisual).Transform(primarytouch.Position), Application.Current.RootVisual);
                    foreach (UIElement uiel in hits)
                    {
                        if (typeof(Touchable).IsAssignableFrom(uiel.GetType()) || _hastouchableparent(uiel))
                        {
                            e.SuspendMousePromotionUntilTouchUp();
                            break;
                        }
                        else if (typeof(Control).IsAssignableFrom(uiel.GetType()))
                        {
                            return;
                        }
                    }
                }
                catch (Exception ex) { }
            }

            lock (this)
            {
                List<int> removed = new List<int>();
                for (int i = 0; i < touchcollection.Count; i++)
                {
                    if (i + 1 > _tpids.Count)
                    {
                        int max = 1;
                        for (int c = 0; c < _tpids.Count; c++)
                            max = Math.Max(max, int.Parse(_tpids[c]) + 1000);
                        _tpids.Add((max).ToString());
                        _cursoradd("WM_TOUCH:" + (max).ToString());
                    }

                    TouchPoint tp = touchcollection[i];
                    if (tp.Action == TouchAction.Up)
                    {
                        _cursordel("WM_TOUCH:" + _tpids[i]);
                        removed.Add(i);
                    }
                    else
                    {
                        _cursorupd("WM_TOUCH:" + _tpids[i], touchcollection[i].Position);
                    }
                }
                removed.Reverse();
                foreach (int r in removed)
                    _tpids.RemoveAt(r);
            }
        }

        // MIG MultiTouch TUIO input
        void MultiTouchListener_AccelerationUpdate(object sender, MIG.Client.Devices.MultiTouch.AccelerationUpdateEventArgs e)
        {
            if (AccelerationUpdate != null) AccelerationUpdate(this, new AccelerationUpdateEventArgs(e.X, e.Y, e.Z));
        }
        void MultiTouchListener_FingerMove(object sender, MIG.Client.Devices.MultiTouch.FingerInputEventArgs e)
        {
            Point p = new Point(_targetsize.Width * e.Position.Value.X, _targetsize.Height * e.Position.Value.Y);
            _cursorupd("TUIO:" + e.Identifier, p);
            e.Handled = true;
        }
        void MultiTouchListener_FingerUp(object sender, MIG.Client.Devices.MultiTouch.FingerInputEventArgs e)
        {
            _cursordel("TUIO:" + e.Identifier);
            e.Handled = true;
        }
        void MultiTouchListener_FingerDown(object sender, MIG.Client.Devices.MultiTouch.FingerInputEventArgs e)
        {
            _cursoradd("TUIO:" + e.Identifier);
            e.Handled = true;
        }



        // UTILITY METHODS

        private bool _hastouchableparent(UIElement control) 
        {
            UIElement uiel = VisualTreeHelper.GetParent(control) as UIElement;
            if (uiel != null)
            {
                if (typeof(Touchable).IsAssignableFrom(uiel.GetType()))
                    return true;
                else
                    return _hastouchableparent(uiel);
            }
            return false;
        }


        private int _getfingerbyid(string id)
        {
            int found = -1;
            for (int x = 0; x < _cursors.Count; x++)
            {
                if (_cursors[x].Identifier.Equals(id))
                {
                    found = x;
                    break;
                }
            }
            return found;
        }

        private void _removechildren(string id)
        {
            if (_fingersaura.ContainsKey(id))
            {
                _infolayer.Children.Remove(_fingersaura[id]);
                _fingersaura.Remove(id);
            }

            if (hookShape.ContainsKey(id))
            {
                if (typeof(Touchable).IsAssignableFrom(hookShape[id].GetType()))
                {
                    ((Touchable)hookShape[id]).FingerUp(this, new FingerTouchEventArgs(new Finger(id, new Point())));
                    hookShape.Remove(id);
                }
            }
        }
        private void _updatechildren(string id, Point p)
        {
            if (_auraenabled)
            {
                if (_fingersaura.ContainsKey(id))
                {
                    _fingersaura[id].SetValue(Canvas.LeftProperty, p.X - (_fingersaura[id].Width / 2));
                    _fingersaura[id].SetValue(Canvas.TopProperty, p.Y - (_fingersaura[id].Height / 2));
                }
                else
                {
                    FingerAura aura = new FingerAura();
                    aura.SetValue(Canvas.LeftProperty, p.X - (aura.Width / 2));
                    aura.SetValue(Canvas.TopProperty, p.Y - (aura.Height / 2));
                    _fingersaura.Add(id, aura);
                    _infolayer.Children.Add(aura);
                }
            }

            if (!_propagateinput) return;
            // controlliamo collisioni
            if (!hookShape.ContainsKey(id))
            {
                try
                {
                    IEnumerable<UIElement> hits = VisualTreeHelper.FindElementsInHostCoordinates(_targetelement.TransformToVisual(Application.Current.RootVisual).Transform(p), Application.Current.RootVisual);
                    foreach (UIElement uiel in hits)
                    {
                        // COLLISIONE INTERCETTATA ... =)";
                        if (typeof(Touchable).IsAssignableFrom(uiel.GetType()))
                        {
                            hookShape.Add(id, uiel);
                            FingerTouchEventArgs e = new FingerTouchEventArgs(new Finger(id, p));
                            ((Touchable)uiel).FingerDown(this, e); //myUIElement.TransformToVisual(uiel).Transform(p));
                            if (e.Handled) break;
                        }
                    }
                }
                catch (Exception ex) { }
            }
            else
            {
                try
                {
                    if (typeof(Touchable).IsAssignableFrom(hookShape[id].GetType()))
                        ((Touchable)hookShape[id]).FingerMove(this, new FingerTouchEventArgs(new Finger(id, p))); //myUIElement.TransformToVisual(hookShape[id]).Transform(p));
                }
                catch (Exception ex) { }
            }
        }



        void _cursoradd(string cid)
        {
            _cursors.Add(new MIG.Client.Devices.MultiTouch.Cursor(cid));
            if (FingerDown != null) FingerDown(this, new FingerInputEventArgs(cid));
        }

        void _cursorupd(string cid, Point position)
        {
            _targetelement.Dispatcher.BeginInvoke(() =>
            {
                _updatechildren(cid, position);
            });
            int x = _getfingerbyid(cid);
            _cursors[x].Update(position);
            if (FingerMove != null) FingerMove(this, new FingerInputEventArgs(cid, position));
        }

        void _cursordel(string cid)
        {
            int x = _getfingerbyid(cid);
            //
            if (x == -1) return;
            //
            _targetelement.Dispatcher.BeginInvoke(() =>
            {
                _removechildren(cid);
            });
            _cursors.Remove(_cursors[x]);
            if (FingerUp != null) FingerUp(this, new FingerInputEventArgs(cid));
        }

    }
}
