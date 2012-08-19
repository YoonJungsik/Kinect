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

using System.Windows.Threading;

namespace MIRIA.Animations
{
    public class TransformHelper
    {
        /// <summary>
        /// Summary description 
        /// </summary>

        public delegate void AnimationStartingHandler(object sender);
        public event AnimationStartingHandler AnimationStarting;
        public delegate void AnimationCompleteHandler(object sender);
        public event AnimationCompleteHandler AnimationComplete;

        // private members to store 
        // current rotation angle, 
        // current translation and scale
        TransformGroup _transformgroup;
        RotateTransform _rotatetransform;
        ScaleTransform _scaletransform;
        TranslateTransform _translatetransform;
        PlaneProjection _projectiontransform;

        Storyboard _storyboard;
        DoubleAnimationUsingKeyFrames _dkanimscalex, _dkanimscaley, _dkanimrotateangle, _dkanimtranslatex, _dkanimtranslatey, _dkanimperspectivex,_dkanimperspectivey, _dkanimperspectivez;

        private double _delay = 1.0;

        private Point _location = new Point(0, 0);
        private Point _scalexy = new Point(1.0, 1.0);
        private double _angle = 0;        

        private bool _isrunning = false;

        private FrameworkElement _targetelement;


        public TransformHelper(FrameworkElement element)
        {
            _targetelement = element;
            element.Projection = new PlaneProjection();

            _transformgroup = new TransformGroup();
            _rotatetransform = new RotateTransform();
            _scaletransform = new ScaleTransform();
            _translatetransform = new TranslateTransform();
            _projectiontransform = new PlaneProjection();

            _storyboard = new Storyboard();
            _dkanimscalex = new DoubleAnimationUsingKeyFrames();
            _dkanimscaley = new DoubleAnimationUsingKeyFrames();
            _dkanimrotateangle = new DoubleAnimationUsingKeyFrames();
            _dkanimtranslatex = new DoubleAnimationUsingKeyFrames();
            _dkanimtranslatey = new DoubleAnimationUsingKeyFrames();
            _dkanimperspectivex = new DoubleAnimationUsingKeyFrames();
            _dkanimperspectivey = new DoubleAnimationUsingKeyFrames();
            _dkanimperspectivez = new DoubleAnimationUsingKeyFrames();

            _delay = 0.25;

            _location = new Point(0, 0);
            _scalexy = new Point(1.0, 1.0);
            _angle = 0;


            if (!_targetelement.Resources.Contains("MIRIA_sb_TransformHelper"))
            {

                _transformgroup.Children.Add(_translatetransform);
                _transformgroup.Children.Add(_rotatetransform);
                _transformgroup.Children.Add(_scaletransform);

                _targetelement.RenderTransform = _transformgroup;

                _targetelement.Projection = _projectiontransform;

                Storyboard.SetTargetProperty(_dkanimscalex, new PropertyPath("ScaleX"));
                Storyboard.SetTargetProperty(_dkanimscaley, new PropertyPath("ScaleY"));
                Storyboard.SetTargetProperty(_dkanimrotateangle, new PropertyPath("Angle"));
                Storyboard.SetTargetProperty(_dkanimtranslatex, new PropertyPath("X"));
                Storyboard.SetTargetProperty(_dkanimtranslatey, new PropertyPath("Y"));
                Storyboard.SetTargetProperty(_dkanimperspectivex, new PropertyPath("RotationX"));
                Storyboard.SetTargetProperty(_dkanimperspectivey, new PropertyPath("RotationY"));
                Storyboard.SetTargetProperty(_dkanimperspectivez, new PropertyPath("RotationZ"));

                Storyboard.SetTarget(_dkanimscalex, _scaletransform);
                Storyboard.SetTarget(_dkanimscaley, _scaletransform);
                Storyboard.SetTarget(_dkanimrotateangle, _rotatetransform);
                Storyboard.SetTarget(_dkanimtranslatex, _translatetransform);
                Storyboard.SetTarget(_dkanimtranslatey, _translatetransform);
                Storyboard.SetTarget(_dkanimperspectivex, _projectiontransform);
                Storyboard.SetTarget(_dkanimperspectivey, _projectiontransform);
                Storyboard.SetTarget(_dkanimperspectivez, _projectiontransform);

                _storyboard.FillBehavior = FillBehavior.HoldEnd;
                _storyboard.Children.Add(_dkanimscalex);
                _storyboard.Children.Add(_dkanimscaley);
                _storyboard.Children.Add(_dkanimrotateangle);
                _storyboard.Children.Add(_dkanimtranslatex);
                _storyboard.Children.Add(_dkanimtranslatey);
                _storyboard.Children.Add(_dkanimperspectivex);
                _storyboard.Children.Add(_dkanimperspectivey);
                _storyboard.Children.Add(_dkanimperspectivez);
                _storyboard.Completed += new EventHandler(delegate(object sender, EventArgs e)
                {
                    _isrunning = false;
                    if (AnimationComplete != null) AnimationComplete(this);
                });
                _targetelement.Resources.Add("MIRIA_sb_TransformHelper", _storyboard);

            }
            else
            {

                _translatetransform = Utility.Transform2d.GetTranslateTransform(_targetelement);
                _rotatetransform = Utility.Transform2d.GetRotateTransform(_targetelement);
                _scaletransform = Utility.Transform2d.GetScaleTransform(_targetelement);

                _angle = _rotatetransform.Angle;
                _scalexy.X = _scaletransform.ScaleX;
                _scalexy.Y = _scaletransform.ScaleY;
                _location = new Point(_translatetransform.X, _translatetransform.Y);

                _storyboard = (Storyboard)(_targetelement.Resources["MIRIA_sb_TransformHelper"]);

            }
            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);
        }

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            try
            {
                if (_isrunning || _storyboard.GetCurrentState() == ClockState.Active)
                {
                    this._angle = _rotatetransform.Angle;
                    this._location.X = _translatetransform.X;
                    this._location.Y = _translatetransform.Y;
                    this._scalexy.X = _scaletransform.ScaleX;
                    this._scalexy.Y = _scaletransform.ScaleY;
                    _centertransform();
                }
            }
            catch (Exception ex) { }
        }

        public bool IsRunning
        {
            get
            {
                return _isrunning;
            }
        }

        public void Stop()
        {
            _storyboard.Pause();
            //
            if (AnimationComplete != null) AnimationComplete(this);
            //
//////////////            CompositionTarget_Rendering(null, null);
            //
            _isrunning = false;
        }

        public PlaneProjection PlaneProjection
        {
            get { return _projectiontransform; }
        }

        public double Angle
        {
            get { return _angle; }
            set
            {
                if (_angle > 360) _angle = _angle - 360;
                _angle = value;
                this.TransformEx();
            }
        }
        public double Scale
        {
            get { return (_scalexy.X + _scalexy.Y) / 2; }
            set
            {
                _scalexy.X = value;
                _scalexy.Y = value;
                //
                this.TransformEx();
            }
        }

        public double ScaleX
        {
            get { return _scalexy.X; }
            set
            {
                _scalexy.X = value;
                this.TransformEx();
            }
        }

        public double ScaleY
        {
            get { return _scalexy.Y; }
            set
            {
                _scalexy.Y = value;
                this.TransformEx();
            }
        }

        public Point Translate
        {
            get {
                return _location;
            }
            set
            {
                _location = new Point(value.X, value.Y);
                this.TransformEx();
            }
        }
        public Point Centroid
        {
            get
            {
                GeneralTransform trs = _targetelement.TransformToVisual(Application.Current.RootVisual);// _targetelement.Parent as FrameworkElement);
                Point t = trs.Transform(new Point((_targetelement.ActualWidth / 2), (_targetelement.ActualHeight / 2)));
                return t;
            }
        }


        public Point CentroidRelative
        {
            get
            {
                GeneralTransform trs = _targetelement.TransformToVisual(_targetelement.Parent as FrameworkElement);
                Point t = trs.Transform(new Point((_targetelement.ActualWidth / 2), (_targetelement.ActualHeight / 2)));
                return t;
            }
        }

        private void TransformEx()
        {
            try
            {
                    Animate(_angle, _scalexy, _location, _delay);
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }


        public double Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }

        

        public Point CenterElement(FrameworkElement element, double scale)
        {
            Point c = new Point();
            try
            {
                c = element.TransformToVisual(_targetelement).Transform(new Point(/* element.RenderSize.Width / 2, element.RenderSize.Height / 2 */));
            }
            catch { }

            Point cn = this.CentroidRelative;
            cn = new Point(cn.X - _location.X, cn.Y - _location.Y);

            /*
            Point nc = new Point(cn.X - c.X, cn.Y - c.Y);
            return nc;
            */


            Point uielC = new Point(c.X + element.RenderSize.Width / 2, c.Y + element.RenderSize.Height / 2);
            Point bs = new Point(cn.X - uielC.X, cn.Y - uielC.Y);
            bs = new Point(((cn.X - uielC.X) * scale) - bs.X, ((cn.Y - uielC.Y) * scale) - bs.Y);

            c.X = -c.X;
            c.Y = -c.Y;
            c.X += ((_targetelement.Parent as FrameworkElement).RenderSize.Width - element.RenderSize.Width) / 2;
            c.Y += ((_targetelement.Parent as FrameworkElement).RenderSize.Height - element.RenderSize.Height) / 2;

            return new Point(c.X + bs.X, c.Y + bs.Y);
        }


        private void _centertransform()
        {
            Point delta = new Point( _translatetransform.X, _translatetransform.Y);
            //Point delta = _targetelement.TransformToVisual(_targetelement.Parent as UIElement).Transform(new Point(_targetelement.ActualWidth / 2D, _targetelement.ActualHeight / 2D));
            _scaletransform.CenterX = delta.X + (_targetelement.ActualWidth / 2);
            _scaletransform.CenterY = delta.Y + (_targetelement.ActualHeight / 2);
            _rotatetransform.CenterX = delta.X + (_targetelement.ActualWidth / 2);
            _rotatetransform.CenterY = delta.Y + (_targetelement.ActualHeight / 2);
        }



        SplineDoubleKeyFrame _skfscalex = new SplineDoubleKeyFrame();
        SplineDoubleKeyFrame _skfscaley = new SplineDoubleKeyFrame();
        SplineDoubleKeyFrame _skfrotate = new SplineDoubleKeyFrame();
        SplineDoubleKeyFrame _skftranslatex = new SplineDoubleKeyFrame();
        SplineDoubleKeyFrame _skftranslatey = new SplineDoubleKeyFrame();



        public void Animate(double angle, Point scale, Point translate, double dur)
        {
            if (_storyboard.GetCurrentState() == ClockState.Filling)
                _storyboard.SkipToFill();

            if (dur == 0)
            {
                _rotatetransform.Angle = angle;
                _translatetransform.X = translate.X;
                _translatetransform.Y = translate.Y;
                _scaletransform.ScaleX = scale.X;
                _scaletransform.ScaleY = scale.Y;
                // TODO: verify if the next line is really needed, though if needed then AnimationStarting should be fired as well
                if (AnimationComplete != null) AnimationComplete(this);
                return;
            }

            if (_dkanimscalex.KeyFrames.Count == 0)
            {

                Point cp1 = new Point(0.3, 0.7);
                Point cp2 = new Point(0.5, 1.0);

                _skfscalex.KeySpline = new KeySpline();
                _skfscalex.KeySpline.ControlPoint1 = cp1;
                _skfscalex.KeySpline.ControlPoint2 = cp2;
                _dkanimscalex.KeyFrames.Add(_skfscalex);

                _skfscaley.KeySpline = new KeySpline();
                _skfscaley.KeySpline.ControlPoint1 = cp1;
                _skfscaley.KeySpline.ControlPoint2 = cp2;
                _dkanimscaley.KeyFrames.Add(_skfscaley);

                _skfrotate.KeySpline = new KeySpline();
                _skfrotate.KeySpline.ControlPoint1 = cp1;
                _skfrotate.KeySpline.ControlPoint2 = cp2;
                _dkanimrotateangle.KeyFrames.Add(_skfrotate);

                _skftranslatex.KeySpline = new KeySpline();
                _skftranslatex.KeySpline.ControlPoint1 = cp1;
                _skftranslatex.KeySpline.ControlPoint2 = cp2;
                _dkanimtranslatex.KeyFrames.Add(_skftranslatex);

                _skftranslatey.KeySpline = new KeySpline();
                _skftranslatey.KeySpline.ControlPoint1 = cp1;
                _skftranslatey.KeySpline.ControlPoint2 = cp2;
                _dkanimtranslatey.KeyFrames.Add(_skftranslatey);

            }

            try
            {
                _skfscalex.Value = scale.X;
                _skfscaley.Value = scale.Y;
                _skfrotate.Value = angle;
                _skftranslatex.Value = translate.X;
                _skftranslatey.Value = translate.Y;
            }
            catch (Exception ex) { }

            _skfscalex.KeyTime = new TimeSpan(0, 0, 0, 0, (int)(dur * 1000) / 4 * 3);
            _skfscaley.KeyTime = new TimeSpan(0, 0, 0, 0, (int)(dur * 1000) / 4 * 3);
            _skfrotate.KeyTime = new TimeSpan(0, 0, 0, 0, (int)(dur * 1000) / 4 * 3);
            _skftranslatex.KeyTime = new TimeSpan(0, 0, 0, 0, (int)(dur * 1000) / 4 * 3);
            _skftranslatey.KeyTime = new TimeSpan(0, 0, 0, 0, (int)(dur * 1000) / 4 * 3);

            Duration duration = new Duration(TimeSpan.FromSeconds(dur));

            _dkanimscalex.Duration = duration;
            _dkanimscaley.Duration = duration;
            _dkanimrotateangle.Duration = duration;
            _dkanimtranslatex.Duration = duration;
            _dkanimtranslatey.Duration = duration;

            _storyboard.Duration = duration;

            if (AnimationStarting != null) AnimationStarting(this);

            _storyboard.Begin();

            _isrunning = true;

        }

        public void Animate(double angle, Point scale, Point translate, double dur, EventHandler callback)
        {
            _storyboard.Completed += new EventHandler((object sender, EventArgs args) =>
            {
                if (callback != null)
                {
                    callback(this, args);
                    callback = null;
                }
            });
            Animate(angle, scale, translate, dur);
        }

    }
}
