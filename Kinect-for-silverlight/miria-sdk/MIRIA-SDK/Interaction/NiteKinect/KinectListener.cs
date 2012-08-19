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
using System.Threading;
using System.Windows.Threading;

using MIG.Client.Devices.NiteKinect;

using MIRIA.UIKit;
using MIRIA.Gestures;
using MIRIA.Interaction.MultiTouch;

namespace MIRIA.Interaction.NiteKinect
{

    public enum UserInteractionContext
    {
        None = -1,
        Moving = 0,
        Tapping,
        Pushing,
        Scrolling,
        Dragging,
        ScaleOrRotate,
        Scaling,
        Rotating
    }

    public class HandManager
    {
        public UserHand HandShape;
        public Ellipse HandPushPoint;
        public Animations.TransformHelper TransformHelper;
        public UIElement CurrentElement;
        public UIElement PushedElement;
        public HandGestures HandGestures;
        public DateTime TappingTs;

        public HandManager()
        {
            HandShape = new UserHand();
            TransformHelper = new Animations.TransformHelper(HandShape);
            HandGestures = new HandGestures();
            TappingTs = new DateTime();
            //
            HandPushPoint = new Ellipse();
            HandPushPoint.Width = 100;
            HandPushPoint.Height = 100;
            HandPushPoint.Stroke = new SolidColorBrush(Colors.Cyan);
            HandPushPoint.StrokeThickness = 20;
            HandPushPoint.Opacity = 0.65;
            HandPushPoint.Visibility = Visibility.Collapsed;
        }
    }

    public class KinectListener
    {
        public delegate void HandAddedEventHandler(object sender, HandEventArgs args);
        public event HandAddedEventHandler HandAdded;

        public delegate void HandUpdateEventHandler(object sender, HandEventArgs args);
        public event HandUpdateEventHandler HandUpdate;

        private MIG.Client.Devices.NiteKinect.Kinect _nitekinect;

        private Overlay _overlay;
        private FrameworkElement _targetelement;
        private Size _targetsize;

        private Dictionary<string, HandManager> _hands;

        private ManualResetEvent _handupdatemutex = null;
        private double _overclickdelay = 2;

        private ScrollView _currentscrollview = null;

        public KinectListener(FrameworkElement element)
        {
            _nitekinect = new MIG.Client.Devices.NiteKinect.Kinect();

            _targetelement = element;

            _overlay = new Overlay();
            (_targetelement as Panel).Children.Add(_overlay);

            _targetelement.Dispatcher.BeginInvoke(() =>
            {
                _targetsize = new Size(_targetelement.ActualWidth, _targetelement.ActualHeight);
                //
                _hands = new Dictionary<string, HandManager>();
                 //
                _nitekinect.HandUpdate += new MIG.Client.Devices.NiteKinect.Kinect.HandUpdateHandler(_nitekinect_HandUpdate);
//                _nitekinect.UserUpdate += new MIG.Client.Devices.NiteKinect.Kinect.UserUpdateHandler(_nitekinect_UserUpdate);
                _nitekinect.SkeltonUpdate += new MIG.Client.Devices.NiteKinect.Kinect.SkeltonUpdateHandler(_nitekinect_SkeltonUpdate);
            });
        }




        /*

        private void _showarrows()
        {
            _targetelement.Dispatcher.BeginInvoke(() =>
            {
                _overlay.ScrollLeft.SetValue(Canvas.LeftProperty, 0D);
                _overlay.ScrollLeft.SetValue(Canvas.TopProperty, (_targetsize.Height / 2D) - (_overlay.ScrollLeft.Height / 2D));
                _overlay.ScrollLeft.Visibility = Visibility.Visible;
                _overlay.ScrollRight.SetValue(Canvas.LeftProperty, _targetsize.Width - _overlay.ScrollRight.Width);
                _overlay.ScrollRight.SetValue(Canvas.TopProperty, (_targetsize.Height / 2D) - (_overlay.ScrollRight.Height / 2D));
                _overlay.ScrollRight.Visibility = Visibility.Visible;
            });
        }

        private void _hidearrows()
        {
        }


        */

        public Dictionary<string, HandManager> Hands
        {
            get { return _hands; }
        }


        public MIG.Client.MIGListener MIGListener
        {
            get { return _nitekinect; }
        }



        void _handgestures_HandEnter(object sender, HandEventArgs e)
        {
            _targetelement.Dispatcher.BeginInvoke(() =>
            {
                HandManager hand = _hands["HAND_" + e.HandData.HandId];
                _overlay.LayoutRoot.Children.Add(hand.HandShape);
                _overlay.LayoutRoot.Children.Add(hand.HandPushPoint);
                hand.TransformHelper.Translate = e.HandLocation;
            });
            //
            if (HandAdded != null) HandAdded(this, e);
        }

        void _handgestures_HandMove(object sender, HandEventArgs e)
        {
            if (_handupdatemutex == null)
            {
                _handupdatemutex = new ManualResetEvent(false);
            }
            _handupdatemutex.Reset();
            //
            DispatcherOperation dope = _targetelement.Dispatcher.BeginInvoke(() =>
            {
                HandManager hand = _hands["HAND_" + e.HandData.HandId];
                Point p = new Point((int)(e.HandLocation.X / 7) * 7, (int)(e.HandLocation.Y / 5) * 5);
                hand.TransformHelper.Delay = 0.75;
                hand.TransformHelper.Translate = p;

                if (_handupdatemutex != null)
                {
                    HandUpdate(this, e);
                }

                if (hand.PushedElement != null)
                {

                    /*if (hand.PushedElement.GetType() == typeof(UIKit.ScrollView))
                    {

                        UIKit.ScrollView sv = (hand.PushedElement as UIKit.ScrollView);
                        //if (!sv.ScrollerTransformHelper.IsRunning)
                        {
                            sv.ScrollerTransformHelper.Delay = 60.0 / Utility.Vector2D.Distance(new Point(e.HandData.PushLocation.X, e.HandData.PushLocation.Y), e.HandLocation);
                            if (e.HandData.PushShift.Y < -20)
                            {
                                //sv.ScrollerTransformHelper.Translate = new Point(sv.ScrollerTransformHelper.Translate.X, sv.ScrollerTransformHelper.Translate.Y - (e.HandData.PushLocation.Y - e.HandLocation.Y) / 10);

                                sv.Move(new Point(0, e.HandData.PushShift.Y / 2D));
                                //sv._checkscrollerbound();

                                //sv._simulategesture(Gestures.TouchGesture.MOVE_NORTH);
                            }
                            else if (e.HandData.PushShift.Y > 20)
                            {
                                //sv.ScrollerTransformHelper.Translate = new Point(sv.ScrollerTransformHelper.Translate.X, sv.ScrollerTransformHelper.Translate.Y - (e.HandData.PushLocation.Y - e.HandLocation.Y) / 10);

                                sv.Move(new Point(0, e.HandData.PushShift.Y / 2D));
                                //sv._checkscrollerbound();

                                //sv._simulategesture(Gestures.TouchGesture.MOVE_SOUTH);
                            }
                            else if (e.HandData.PushShift.X < -20)
                            {
                                sv.Move(new Point(e.HandData.PushShift.X / 2D, 0));
                                //sv._simulategesture(Gestures.TouchGesture.MOVE_WEST);
                            }
                            else if (e.HandData.PushShift.X > 20)
                            {
                                sv.Move(new Point(e.HandData.PushShift.X / 2D, 0));
                                //sv._simulategesture(Gestures.TouchGesture.MOVE_EAST);
                            }
                        }

                    }
                    else if (hand.PushedElement.GetType() == typeof(UIKit.TButton) && !(hand.PushedElement as UIKit.TButton).IsPressed)
                    {
                    //    TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - e.HandData.PushDateTime.Ticks);
                    //    //if (ts.TotalMilliseconds > 750)
                    //    //{
                    //    //    ((UIKit.TButton)hand.PushedElement).TransformHelper.Delay = 0;
                    //    //    ((Touchable)hand.PushedElement).FingerMove(this, new FingerTouchEventArgs(new Finger("KINECTHAND:" + e.HandData.HandId, e.HandLocation)));
                    //    //}
                        hand.PushedElement = null;
                        //
                        _handupdatemutex.Set();
                        //
                        return;
                    }
                    else*/
                    {
                        ((Touchable)hand.PushedElement).FingerMove(this, new FingerTouchEventArgs(new Finger("KINECTHAND:" + e.HandData.HandId, e.HandLocation)));
                    }
                }
                else
                {
                    UIElement curelement = null;
                    List<UIElement> cls = Utility.Transform2d.GetCollisionsAt(_targetelement, hand.TransformHelper.Translate, new Size(hand.HandShape.Width, hand.HandShape.Height));
                    foreach (UIElement uiel in cls)
                    {
                        if (typeof(Touchable).IsAssignableFrom(uiel.GetType()))
                        {
                            curelement = uiel;
                            break;
                        }
                    }

                    // HAND JUST OVER A NEW ELEMENT
                    if ((hand.CurrentElement == null && curelement != null) || (hand.CurrentElement != null && !hand.CurrentElement.Equals(curelement)))
                    {
                        hand.TappingTs = DateTime.Now;
                        //
                        if (hand.CurrentElement != null) // && hand.CurrentElement.GetType() == typeof(UIKit.ScrollView) && _currentscrollview == null)
                        {
                            //_currentscrollview = hand.CurrentElement as ScrollView;
                            //_showarrows();
                        }
//                        else if (hand.CurrentElement != null && hand.CurrentElement.GetType() == typeof(UIKit.ScrollView)) //if (_currentscrollview != null && !_currentscrollview.ScrollerTransformHelper.IsRunning) //(hand.CurrentElement != null && hand.CurrentElement.GetType() == typeof(UIKit.TButton))
//                        {
                            /*
                            if (_currentscrollview != null && !_currentscrollview.ScrollerTransformHelper.IsRunning)
                            {
                                


                                TButton tb = hand.CurrentElement as TButton;
                                if (tb.Equals(_overlay.ScrollRight))
                                {
                                    _currentscrollview._simulategesture(TouchGesture.MOVE_WEST);
                                }
                                else if (tb.Equals(_overlay.ScrollLeft))
                                {
                                    _currentscrollview._simulategesture(TouchGesture.MOVE_EAST);
                                }
                            }
                            */

//System.Diagnostics.Debug.WriteLine(hand.TransformHelper.Translate.Y + " # " + _targetsize.Height);
/*                            if (hand.TransformHelper.Translate.X > (_targetsize.Width - (_targetsize.Width / 5D)))
                            {
                                _currentscrollview.ScrollerTransformHelper.Delay = 1.0;
                                _currentscrollview._simulategesture(TouchGesture.MOVE_WEST);
                            }
                            else if (hand.TransformHelper.Translate.X < (_targetsize.Width / 5D))
                            {
                                _currentscrollview.ScrollerTransformHelper.Delay = 1.0;
                                _currentscrollview._simulategesture(TouchGesture.MOVE_EAST);
                            }

                            if (hand.TransformHelper.Translate.Y < ((_targetsize.Height / 5D)))
                            {
                                _currentscrollview.ScrollerTransformHelper.Delay = 1.0;
                                _currentscrollview._simulategesture(TouchGesture.MOVE_NORTH);
                                //_currentscrollview.ScrollerTransformHelper.Translate = new Point(_currentscrollview.ScrollerTransformHelper.Translate.X, _currentscrollview.ScrollerTransformHelper.Translate.Y - 1000);
                            }
                            else if (hand.TransformHelper.Translate.Y > (_targetsize.Height - (_targetsize.Height / 5D)))
                            {
                                _currentscrollview.ScrollerTransformHelper.Delay = 1.0;
                                _currentscrollview._simulategesture(TouchGesture.MOVE_SOUTH);
                                //_currentscrollview.ScrollerTransformHelper.Translate = new Point(_currentscrollview.ScrollerTransformHelper.Translate.X, _currentscrollview.ScrollerTransformHelper.Translate.Y - 1000);
                            }*/
//                        }
                    }
                    /*
                    if (curelement != null)
                    {
                        if ((hand.CurrentElement == null || !hand.CurrentElement.Equals(curelement)) && curelement.GetType() == typeof(UIKit.TButton))
                        {
                            //((UIKit.TButton)curelement).TransformHelper.Delay = 0.5;
                            //((UIKit.TButton)curelement).TransformHelper.Scale = 1.5;
                        }
                        if (hand.CurrentElement != null && hand.CurrentElement.Equals(curelement) && curelement.GetType() == typeof(UIKit.TButton))
                        {
                            //((UIKit.TButton)curelement).TransformHelper.Delay = 0.5;
                            //((UIKit.TButton)curelement).TransformHelper.Scale = 1.5;
                            UIKit.TButton tb = curelement as UIKit.TButton;
                            if (!double.IsInfinity(tb.DragRadius))
                            {
                                hand.PushedElement = curelement;
                                tb.FingerDown(this, new FingerTouchEventArgs(new Finger("KINECTHAND:" + e.HandData.HandId, e.HandLocation)));
                                //
                                Point c = tb.TransformToVisual(_targetelement).Transform(new Point(tb.RenderSize.Width / 4, tb.RenderSize.Height / 4));
                                hand.TransformHelper.Translate = c;
                                //
                                _handupdatemutex.Set();
                                //
                                return;
                            }
                        }
                    }*/
                    //
                    //
                    if (hand.CurrentElement != null && !hand.CurrentElement.Equals(curelement))
                    {
                        /*if (hand.CurrentElement.GetType() == typeof(UIKit.TButton))
                        {
                            //((UIKit.TButton)hand.CurrentElement).TransformHelper.Delay = 0.5;
                            //((UIKit.TButton)hand.CurrentElement).TransformHelper.Scale = 1.0;
                            ((UIKit.TButton)hand.CurrentElement)._simulateleave();
                        }
                        else if (hand.CurrentElement.GetType() == typeof(UIKit.ScrollView) && (hand.CurrentElement as ScrollView).ScrollerTransformHelper.IsRunning == false)
                        {
                            if (hand.TransformHelper.Translate.Y > 200 && hand.TransformHelper.Translate.Y < (_targetsize.Height - 200))
                            {
                                if (hand.TransformHelper.Translate.X > (_targetsize.Width - (_targetsize.Width / 4D)))
                                {
                                    (hand.CurrentElement as ScrollView).ScrollerTransformHelper.Delay = 0.75;
                                    (hand.CurrentElement as ScrollView).SimulateGesture(TouchGesture.MOVE_WEST);
                                }
                                else if (hand.TransformHelper.Translate.X < (_targetsize.Width / 4D))
                                {
                                    (hand.CurrentElement as ScrollView).ScrollerTransformHelper.Delay = 0.75;
                                    (hand.CurrentElement as ScrollView).SimulateGesture(TouchGesture.MOVE_EAST);
                                }
                            }
                            //if (hand.TransformHelper.Translate.X > 200 && hand.TransformHelper.Translate.X < (_targetsize.Width - 200))
                            //{
                            //    if (hand.TransformHelper.Translate.Y > (_targetsize.Height - (_targetsize.Height / 4D)))
                            //    {
                            //        (hand.CurrentElement as ScrollView).ScrollerTransformHelper.Delay = 0.75;
                            //        (hand.CurrentElement as ScrollView)._simulategesture(TouchGesture.MOVE_NORTH);
                            //    }
                            //    else if (hand.TransformHelper.Translate.Y < (_targetsize.Height / 4D))
                            //    {
                            //        (hand.CurrentElement as ScrollView).ScrollerTransformHelper.Delay = 0.75;
                            //        (hand.CurrentElement as ScrollView)._simulategesture(TouchGesture.MOVE_SOUTH);
                            //    }
                            //}

                        }*/
                    }
                    //
                    /*
                    if (hand.CurrentElement != null && hand.CurrentElement.GetType() == typeof(UIKit.TButton))
                    {
                        TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - hand.TappingTs.Ticks);
                        if (ts.TotalMilliseconds % 300 < 150)
                        {
                            hand.HandShape.CircleSelectO.Visibility = Visibility.Visible;
                            hand.HandShape.CircleSelectI.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            hand.HandShape.CircleSelectO.Visibility = Visibility.Collapsed;
                            hand.HandShape.CircleSelectI.Visibility = Visibility.Visible;
                        }
                        if (ts.TotalSeconds > _overclickdelay || !hand.CurrentElement.Equals(curelement))
                        {
                            if (ts.TotalSeconds > _overclickdelay)
                            {
                                hand.TappingTs = DateTime.Now;
                                // simulate click over TButton
                                ((UIKit.TButton)hand.CurrentElement).FingerDown(this, new FingerTouchEventArgs(new Finger("KHAND:0", e.HandData.GetRelativeLocation2d(new Point()))));
                                ((UIKit.TButton)hand.CurrentElement).FingerUp(this, new FingerTouchEventArgs(new Finger("KHAND:0", e.HandData.GetRelativeLocation2d(new Point()))));
                            }
                            //
                            hand.HandShape.TextLabel.Text = "";
                            hand.HandShape.CircleSelectO.Visibility = Visibility.Collapsed;
                            hand.HandShape.CircleSelectI.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            hand.HandShape.TextLabel.Text = (_overclickdelay - Math.Round(ts.TotalSeconds, 0)).ToString();
                        }
                    }*/
                    //
                    hand.CurrentElement = curelement;
                }
                _handupdatemutex.Set();
            });
            //
            _handupdatemutex.WaitOne();
        }

        void _handgestures_HandExit(object sender, HandEventArgs e)
        {
            if (!_hands.ContainsKey("HAND_" + e.HandData.HandId)) return;
            //
            _targetelement.Dispatcher.BeginInvoke(() =>
            {
                HandManager hand = _hands["HAND_" + e.HandData.HandId];
                //
                hand.HandGestures.HandEnter -= new HandEnterEventHandler(_handgestures_HandEnter);
                hand.HandGestures.HandMove -= new HandMoveEventHandler(_handgestures_HandMove);
                hand.HandGestures.HandExit -= new HandExitEventHandler(_handgestures_HandExit);
                hand.HandGestures.GestureRecognized -= new GestureRecognizedEventHandler(_handgestures_GestureRecognized);
                //
                if (hand.PushedElement != null) // && hand.PushedElement.GetType() != typeof(UIKit.ScrollView))
                {
                    ((Touchable)hand.PushedElement).FingerUp(this, new FingerTouchEventArgs(new Finger("KINECTHAND:" + e.HandData.HandId, e.HandLocation)));
                }
                _overlay.LayoutRoot.Children.Remove(hand.HandShape);
                _overlay.LayoutRoot.Children.Remove(hand.HandPushPoint);
                _hands.Remove("HAND_" + e.HandData.HandId);
            });            
        }



        void _nitekinect_SkeltonUpdate(object sender, MIG.Client.Devices.NiteKinect.KinectSkeltonUpdateEventArgs args)
        {
        }


        // MAIN UIKIT CONTROLS KINECT INTERACTION LOGIC
        //
        //bool _already_adding = false;
        void _nitekinect_HandUpdate(object sender, MIG.Client.Devices.NiteKinect.KinectHandUpdateEventArgs args)
        {
            
            if (args.State == MIG.Client.Devices.NiteKinect.KinectHandState.GestureClick)
            {
                // TODO: ....
            }
            else if (args.State == MIG.Client.Devices.NiteKinect.KinectHandState.GestureRaiseHand)
            {
                // TODO: ....
            }
            

            if (_hands.ContainsKey("HAND_" + args.HandId) && _hands["HAND_" + args.HandId] != null)
            {
                _hands["HAND_" + args.HandId].HandGestures.HandUpdate(this, args);
            }
            else
            {
                //if (_already_adding) return;
                //_already_adding = true;
                _targetelement.Dispatcher.BeginInvoke(() =>
                {
                    HandManager hand = new HandManager();
/*                    if (_hands.Count > 0 && _hands.Count % 2 != 0)
                    {
                        // left hand
                        hand.HandGestures.SetTargetElement(_targetelement, new Point(_targetsize.Width / 4D, _targetsize.Height / 2));
                    }
                    else
                    {
                        // right hand
                        hand.HandGestures.SetTargetElement(_targetelement, new Point(_targetsize.Width / 4D * 3D, _targetsize.Height / 2));
                    }*/
                    hand.HandGestures.SetTargetElement(_targetelement, new Point(_targetsize.Width / 2D, _targetsize.Height / 2D));
                    //
                    hand.HandGestures.HandEnter += new HandEnterEventHandler(_handgestures_HandEnter);
                    hand.HandGestures.HandMove += new HandMoveEventHandler(_handgestures_HandMove);
                    hand.HandGestures.HandExit += new HandExitEventHandler(_handgestures_HandExit);
                    hand.HandGestures.GestureRecognized += new GestureRecognizedEventHandler(_handgestures_GestureRecognized);
                    //
                    _hands["HAND_" + args.HandId] = hand;
                    //
                    hand.HandGestures.HandUpdate(this, args);
                    //
                    //_already_adding = false;
                });
            }
            return;

        }


        private void _handgestures_GestureRecognized(object sender, GestureRecognizedEventArgs e)
        {
            //return;
            _targetelement.Dispatcher.BeginInvoke(() =>
            {
                if (!_hands.ContainsKey("HAND_" + e.HandDetail.HandId)) return;
                //
                HandManager hand = _hands["HAND_" + e.HandDetail.HandId];
                if (e.Gesture == HandGesture.Push)
                {
                    UIElement curelement = null;
                    List<UIElement> cls = Utility.Transform2d.GetCollisionsAt(_targetelement, hand.TransformHelper.Translate, new Size(hand.HandShape.Width, hand.HandShape.Height));
                    foreach (UIElement uiel in cls)
                    {
                        if (typeof(Touchable).IsAssignableFrom(uiel.GetType()))
                        {
                            curelement = uiel;
                            break;
                        }
                    }

                    if (curelement != null)
                    {
                        hand.PushedElement = curelement;
                        //if (hand.PushedElement.GetType() != typeof(UIKit.ScrollView))
                        {
                            ((Touchable)hand.PushedElement).FingerDown(this, new FingerTouchEventArgs(new Finger("KINECTHAND:" + e.HandDetail.HandId, hand.TransformHelper.Translate)));
                        }/*
                        else
                        {
                            UIKit.ScrollView t = hand.PushedElement as UIKit.ScrollView;
                            if (false && t.Direction != ScrollDirection.Both)
                            {
                                t.ScrollerTransformHelper.Delay = 0.10;
                                t.ZoomToElementHeight(t.GetElementInView());
                                t.ScrollerTransformHelper.Delay = 1.0D;
                                t.ScrollerTransformHelper.Animate(0, new Point(10D, 10D), t.ScrollerTransformHelper.Translate, 1D);
                                //t.ScrollerTransformHelper.Scale = 10D;
                                Animations.Animator.AnimateProperty(t, "Opacity", 1.0, 0.0, 1.0, _opencurrentslide);
                            }
                            //
                            hand.HandPushPoint.SetValue(Canvas.LeftProperty, hand.TransformHelper.Translate.X);
                            hand.HandPushPoint.SetValue(Canvas.TopProperty, hand.TransformHelper.Translate.Y);
                            hand.HandPushPoint.Visibility = Visibility.Visible;
                        }*/
                    }
                    //
                    hand.HandShape.Circle.Visibility = Visibility.Visible;
                }
                else if (e.Gesture == HandGesture.Pull)
                {
                    if (hand.PushedElement != null) // && hand.PushedElement.GetType() != typeof(UIKit.ScrollView))
                    {
                        ((Touchable)hand.PushedElement).FingerUp(this, new FingerTouchEventArgs(new Finger("KINECTHAND:" + e.HandDetail.HandId, hand.TransformHelper.Translate)));
                    }/*
                    else if (hand.PushedElement != null && hand.PushedElement.GetType() == typeof(UIKit.ScrollView))
                    {
                        //UIKit.ScrollView t = hand.PushedElement as UIKit.ScrollView;
                        //t.ScrollerTransformHelper.Delay = 3.0;
                        //t.ZoomToElementWidth(t.GetElementInView());
                        //
                        hand.HandPushPoint.Visibility = Visibility.Collapsed;
                    }*/
                    hand.PushedElement = null;
                    //
                    hand.HandShape.Circle.Visibility = Visibility.Collapsed;
                }
            });
        }
        /*
        private void _opencurrentslide(object sender, EventArgs args)
        {
            //////
            // -> open current hookable element into a new view with pan/zoom
            //////
            ScrollView sv = sender as ScrollView;
            sv.ScrollerTransformHelper.Scale = 1.0;
            sv.Opacity = 1.0;
        }
        */
    }
}
