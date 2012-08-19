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

using MIRIA.Gestures;
using MIRIA.Interaction.MultiTouch;

namespace MIRIA.Interaction.WiiRemote
{
    public class WiiRemoteListener
    {
        private MIG.Client.Devices.Wii.Remote _wiiremote;

        private Overlay _overlay;
        private FrameworkElement _targetelement;
        private Size _targetsize;
        private UIElement _currentelement;

        private bool _selectButtonPressed = false;
        private DateTime _selectDateTime;

        private Animations.TransformHelper _handtransform;

        public WiiRemoteListener(FrameworkElement element)
        {
            _wiiremote = new MIG.Client.Devices.Wii.Remote();

            _targetelement = element;
            
            _overlay = new Overlay();
            (_targetelement as Panel).Children.Add(_overlay);

            _handtransform = new Animations.TransformHelper(_overlay.Hand);
            _handtransform.Delay = 0.25;

            _targetelement.Dispatcher.BeginInvoke(() =>
            {
                _targetsize = new Size(_targetelement.ActualWidth, _targetelement.ActualHeight);
                //
                _wiiremote.AccelerationUpdate += new MIG.Client.Devices.Wii.Remote.AccelerationUpdateHandler(_wiiremote_AccelerationUpdate);
                _wiiremote.ButtonPressed += new MIG.Client.Devices.Wii.Remote.ButtonPressedHandler(_wiiremote_ButtonPressed);
                _wiiremote.ButtonReleased += new MIG.Client.Devices.Wii.Remote.ButtonReleasedHandler(_wiiremote_ButtonReleased);
                _wiiremote.InfraredUpdate += new MIG.Client.Devices.Wii.Remote.InfraredUpdateHandler(_wiiremote_InfraredUpdate);
                _wiiremote.RemoteConnected += new MIG.Client.Devices.Wii.Remote.RemoteConnectedHandler(_wiiremote_RemoteConnected);
                _wiiremote.RemoteDisconnected += new MIG.Client.Devices.Wii.Remote.RemoteDisconnectedHandler(_wiiremote_RemoteDisconnected);
                //
                try
                {
                    _wiiremote.RequestConnect();
                }
                catch { }
            });

        }

        private void _wiiremote_RemoteDisconnected(object sender)
        {
            _targetelement.Dispatcher.BeginInvoke(() =>
            {
                _overlay.Hand.Visibility = Visibility.Collapsed;
            }); 
        }

        private void _wiiremote_RemoteConnected(object sender)
        {
            _targetelement.Dispatcher.BeginInvoke(() =>
            {
                _overlay.Hand.Visibility = Visibility.Visible;
            });            
        }

        public MIG.Client.MIGListener MIGListener
        {
            get { return _wiiremote; }
        }

        void _wiiremote_ButtonReleased(object sender, MIG.Client.Devices.Wii.ButtonEventArgs args)
        {       
            _targetelement.Dispatcher.BeginInvoke(() =>
            {
                if (_currentelement == null) return;
                //
                lock (this)
                {
                    if (!_selectButtonPressed && _currentelement.GetType() == typeof(UIKit.ScrollView))
                    {
                        UIKit.ScrollView sv = (_currentelement as UIKit.ScrollView);
                        //if (!sv.ScrollerTransformHelper.IsRunning)
                        {
                            sv.ScrollerTransformHelper.Delay = 0.5;
                            switch (args.Button)
                            {
                                case MIG.Client.Devices.Wii.ButtonEventArgs.WiiRemoteButton.RIGHT:
                                    sv.SimulateGesture(TouchGesture.MOVE_WEST);
                                    break;
                                case MIG.Client.Devices.Wii.ButtonEventArgs.WiiRemoteButton.LEFT:
                                    sv.SimulateGesture(TouchGesture.MOVE_EAST);
                                    break;
                                case MIG.Client.Devices.Wii.ButtonEventArgs.WiiRemoteButton.UP:
                                    sv.SimulateGesture(TouchGesture.MOVE_SOUTH);
                                    break;
                                case MIG.Client.Devices.Wii.ButtonEventArgs.WiiRemoteButton.DOWN:
                                    sv.SimulateGesture(TouchGesture.MOVE_NORTH);
                                    break;
                                case MIG.Client.Devices.Wii.ButtonEventArgs.WiiRemoteButton.B:
                                    sv.ZoomToFullHeight();
                                    break;
                            }
                        }
                    }
                    else if (typeof(Touchable).IsAssignableFrom(_currentelement.GetType()) && args.Button == MIG.Client.Devices.Wii.ButtonEventArgs.WiiRemoteButton.A)
                    {
                        _overlay.Hand.Circle.Visibility = Visibility.Collapsed;
                        _selectButtonPressed = false;
                        ((Touchable)_currentelement).FingerUp(this, new FingerTouchEventArgs(new Finger("WHAND:0", _handtransform.Centroid)));
                    }
                }
            });

        }

        void _wiiremote_ButtonPressed(object sender, MIG.Client.Devices.Wii.ButtonEventArgs args)
        {
            _targetelement.Dispatcher.BeginInvoke(() =>
            {
                if (_currentelement == null) return;
                //
                if (typeof(Touchable).IsAssignableFrom(_currentelement.GetType()) && args.Button == MIG.Client.Devices.Wii.ButtonEventArgs.WiiRemoteButton.A)
                {
                    ((Touchable)_currentelement).FingerDown(this, new FingerTouchEventArgs(new Finger("WHAND:0", _handtransform.Centroid)));
                    _selectDateTime = DateTime.Now;
                    _selectButtonPressed = true;
                }
            });
        }

        void _wiiremote_AccelerationUpdate(object sender, MIG.Client.Devices.Wii.WiiAccelerationUpdateEventArgs args)
        {
            //throw new NotImplementedException();
        }

        void _wiiremote_InfraredUpdate(object sender, MIG.Client.Devices.Wii.WiiInfraredUpdateEventArgs args)
        {
            if (args.X == 0 && args.Y == 0) return;
            //
            _targetelement.Dispatcher.BeginInvoke(() =>
            {
                _handtransform.Translate = new Point(args.X * (Application.Current.RootVisual.RenderSize.Width / 560), args.Y * (Application.Current.RootVisual.RenderSize.Height / 420));


                if (_selectButtonPressed && _currentelement != null && typeof(Touchable).IsAssignableFrom(_currentelement.GetType()))
                {
                    TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - _selectDateTime.Ticks);
                    if (ts.TotalMilliseconds > 350)
                    {
                        _overlay.Hand.Circle.Visibility = Visibility.Visible;
                        ((Touchable)_currentelement).FingerMove(this, new FingerTouchEventArgs(new Finger("WHAND:0", _handtransform.Centroid)));
                    }
                }
                else
                {
                    UIElement newelement = null;
                    List<UIElement> cls = Utility.Transform2d.GetCollisionsAt(_targetelement, _handtransform.Translate, new Size(_overlay.Hand.RenderSize.Width, _overlay.Hand.RenderSize.Height));
                    foreach (UIElement uiel in cls)
                    {
                        if (typeof(Touchable).IsAssignableFrom(uiel.GetType()))
                        {
                            newelement = uiel;
                            break;
                        }
                    }
                    //
                    _currentelement = newelement;
                }
            });
        }
    }
}
