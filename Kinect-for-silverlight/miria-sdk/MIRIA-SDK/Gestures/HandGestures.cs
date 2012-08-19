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

using System.Windows.Threading;

using MIG.Client.Devices.NiteKinect;

namespace MIRIA.Gestures
{
    public delegate void HandEnterEventHandler(object sender, HandEventArgs e);
    public delegate void HandMoveEventHandler(object sender, HandEventArgs e);
    public delegate void HandExitEventHandler(object sender, HandEventArgs e);

    public class HandEventArgs : EventArgs
    {
        public readonly HandDetail HandData;
        public readonly Point HandLocation;

        public HandEventArgs(HandDetail handdata, Point location)
        {
            this.HandData = handdata;
            this.HandLocation = location;
        }
    }


    public delegate void GestureRecognizedEventHandler(object sender, GestureRecognizedEventArgs e);

    public class GestureRecognizedEventArgs
    {
        public HandDetail HandDetail;
        public HandGesture Gesture;
        public double AverageValue;
        public double MinimumValue;
        public double MaximumValue;

        public GestureRecognizedEventArgs(HandDetail detail, HandGesture state, double avg, double min, double max)
        {
            this.HandDetail = detail;
            this.Gesture = state;
            this.AverageValue = avg;
            this.MinimumValue = min;
            this.MaximumValue = max;
        }
    }

    public enum HandGesture
    {
        None = 0,
        Push = 1,
        Pull = -1
    }



    public class HandDetail
    {
        private float _speedratio = ((float)Application.Current.RootVisual.RenderSize.Width / 200f);
        private Point3d _previouslocation;
        private Point3d _currentlocation;

        public int HandId { get; set; }

        public Point3d EnterLocation { get; set; }
        public Point3d CurrentLocation
        {
            get { return _currentlocation; }
            set { _previouslocation = _currentlocation; _currentlocation = value; }
        }
        public Point3d PushLocation { get; set; }
        public DateTime PushDateTime { get; set; }
        public Point3d GetRelativeLocation(Point offset)
        {
            Point3d p = new Point3d((float)(offset.X) + MoveShift.X, (float)(offset.Y) + MoveShift.Y, _currentlocation.Z);
            return p;
        }

        public Point GetRelativeLocation2d(Point offset)
        {
            Point3d p = GetRelativeLocation(offset);
            return new Point(p.X, p.Y);
        }

        public Point3d PushShift
        {
            get { return new Point3d(_currentlocation.X - PushLocation.X, _currentlocation.Y - PushLocation.Y, _currentlocation.Z - PushLocation.Z); }
        }

        public Point3d MoveIncrement
        {
            get
            {
                if (_previouslocation == null) _previouslocation = _currentlocation;
                Point3d p = new Point3d(_currentlocation.X - _previouslocation.X, _currentlocation.Y - _previouslocation.Y, _currentlocation.Z - _previouslocation.Z);
                p.X *= _speedratio;
                p.Y *= _speedratio;
                p.Z *= _speedratio;
                return p;
            }
        }

        public Point3d MoveShift
        {
            get
            {
                if (_previouslocation == null) _previouslocation = _currentlocation;
                if (EnterLocation == null) EnterLocation = _currentlocation;
                Point3d p = new Point3d(_currentlocation.X - EnterLocation.X, _currentlocation.Y - EnterLocation.Y, _currentlocation.Z - EnterLocation.Z);
                p.X *= _speedratio;
                p.Y *= _speedratio;
                p.Z *= _speedratio;
                return p;
            }
        }

        public double CurrentGap { get; set; }
        public double AverageDepth { get; set; }

    }

    
    public class  HandGestures
    {
        public event GestureRecognizedEventHandler GestureRecognized;
        public event HandMoveEventHandler HandMove;
        public event HandEnterEventHandler HandEnter;
        public event HandExitEventHandler HandExit;

        private HandDetail _handdetail = new HandDetail();

        //private Panel _targetelement;
        //private Size _targetsize;
        private Point _offset;

        private double _averagedepth;
        private double mindepth;
        private double maxdepth;
        private double _depth;
        private double engagethreshold = 50;

        private HandGesture currentgesture;

        private DispatcherTimer dispatcherTimer;

        public HandGestures()
        {
            mindepth = double.PositiveInfinity;
            maxdepth = double.NegativeInfinity;

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 150);
            dispatcherTimer.Tick += (sender, e) =>
            {
                _averagedepth = (_averagedepth + _depth) / 2D; // TODO: get rid of _averagedepth and use _handdetail.AverageDepth only
                //
                _handdetail.AverageDepth = _averagedepth;
                //
                double gap = maxdepth - mindepth;
                //
                _handdetail.CurrentGap = gap;
                //
                if (gap > engagethreshold / 1.5 && maxdepth <= _depth)
                {
                    dispatcherTimer.Stop();
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 150);
                    dispatcherTimer.Start();
                    currentgesture = HandGesture.Pull;
                    //
                    if (GestureRecognized != null) GestureRecognized(this, new GestureRecognizedEventArgs(_handdetail, currentgesture, _averagedepth, mindepth, maxdepth));
                    //
                    mindepth = maxdepth = _averagedepth;
                }
                else if (gap > engagethreshold && mindepth >= _depth)
                {
                    dispatcherTimer.Stop();
                    dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 75);
                    dispatcherTimer.Start();
                    currentgesture = HandGesture.Push;
                    _handdetail.PushLocation = _handdetail.CurrentLocation;
                    _handdetail.PushDateTime = DateTime.Now;
                    //
                    if (GestureRecognized != null) GestureRecognized(this, new GestureRecognizedEventArgs(_handdetail, currentgesture, _averagedepth, mindepth, maxdepth));
                    //
                    mindepth = maxdepth = _averagedepth;
                }
                else
                {
                    // clear gesture...
                    currentgesture = HandGesture.None;
                }
                //
                if (mindepth > _averagedepth) mindepth = _averagedepth;
                if (maxdepth < _averagedepth) maxdepth = _averagedepth;
            };
            dispatcherTimer.Start();
        }

        public HandGesture GestureState
        {
            get { return currentgesture; }
        }

        public double Depth
        {
            get { return _depth; }
        }

        public void SetTargetElement(FrameworkElement t, Point offset)
        {
            _offset = offset;
        }


        public bool HandUpdate(object sender, MIG.Client.Devices.NiteKinect.KinectHandUpdateEventArgs args)
        {
            if (_handdetail.HandId != 0 && args.HandId != _handdetail.HandId || (_handdetail.HandId == 0 && args.State != KinectHandState.Created)) return false;
            //
            if (args.State == MIG.Client.Devices.NiteKinect.KinectHandState.Created && _handdetail.HandId == 0)
            {
                _handdetail.HandId = args.HandId;
                _handdetail.EnterLocation = args.Location;
                _handdetail.CurrentLocation = args.Location;
                //
                _depth = args.Location.Z;
                //
                if (HandEnter != null) HandEnter(this, new HandEventArgs(_handdetail, new Point(_handdetail.GetRelativeLocation(_offset).X, _handdetail.GetRelativeLocation(_offset).Y)));
                //
                return true;
            }
            else if (args.State == MIG.Client.Devices.NiteKinect.KinectHandState.Destroyed)
            {
                //
                if (HandExit != null) HandExit(this, new HandEventArgs(_handdetail, new Point(_handdetail.GetRelativeLocation(_offset).X, _handdetail.GetRelativeLocation(_offset).Y)));
                //
                //_handdetail.HandId = 0;
                return true;
            }
            //
            _handdetail.CurrentLocation = args.Location;
            //
            if (_handdetail.HandId != 0)
            {
                _depth = args.Location.Z;
                //
                if (HandMove != null) HandMove(this, new HandEventArgs(_handdetail, new Point(_handdetail.GetRelativeLocation(_offset).X, _handdetail.GetRelativeLocation(_offset).Y)));
            }
            //
            return true;
        }

    }
}
