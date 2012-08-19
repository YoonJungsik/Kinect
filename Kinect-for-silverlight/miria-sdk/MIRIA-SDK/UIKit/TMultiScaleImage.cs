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

using MIRIA.Animations;
using MIRIA.Gestures;
using MIRIA.Interaction.MultiTouch;

namespace MIRIA.UIKit
{
    public class TMultiScaleImage : TCanvas //, MultiTouch.Touchable
    {
        private System.Windows.Controls.MultiScaleImage _msi;

        private double _zoomfactor;
        private Point _lastlocation = new Point();
        private MultiScaleTileSource _source;


        public TMultiScaleImage()
        {
            _init();
        }

        private void _init()
        {

            this.Translate += new TranslateHandler(MultiScaleImage_Translate);
            this.Rotate += new RotateHandler(MultiScaleImage_Rotate);
            this.Scale += new ScaleHandler(MultiScaleImage_Scale);
            this.Tap += new TapHandler(MultiScaleImage_Tap);
            this.FingerAdded += new FingerAdddedHandler(MultiScaleImage_FingerAdd);

        }

        void MultiScaleImage_Tap(object sender, GestureTapEventArgs e)
        {
            Point p = Application.Current.RootVisual.TransformToVisual(_msi).Transform(e.TapLocation);
            _zoomfactor *= 1.5;
            Zoom(1.5, new Point(p.X, p.Y));
        }

        void MultiScaleImage_Scale(object sender, GestureScaleEventArgs e)
        {
            if (e.ScaleFactor != 1)
            {
                _zoomfactor *= e.ScaleFactor;
                Point zoomcenter;
                if (GesturesInterpreter.Fingers.Count > 1)
                {
                    double d = Utility.Vector2D.Distance(e.FingerA.Position, e.FingerB.Position);
                    double a = Utility.Vector2D.GetAngle(e.FingerA.Position, e.FingerB.Position);
                    Point c = Utility.Vector2D.GetPoint(e.FingerA.Position, d / 2, a * Math.PI / 180);
                    zoomcenter = Application.Current.RootVisual.TransformToVisual(_msi).Transform(c);
                }
                else
                {
                    zoomcenter = new Point((this.ActualWidth / 2), (this.ActualHeight / 2));
                    zoomcenter = Application.Current.RootVisual.TransformToVisual(_msi).Transform(zoomcenter);
                }
                Zoom(e.ScaleFactor, zoomcenter);
            }
        }

        void MultiScaleImage_Rotate(object sender, GestureRotateEventArgs e)
        {
            //this.TransformHelper.Angle += e.AngleShift;
        }

        void MultiScaleImage_Translate(object sender, GestureTranslateEventArgs e)
        {
            if (GesturesInterpreter.Fingers.Count == 1 && GesturesInterpreter.CurrentGesture >= TouchGesture.MOVE_WEST && GesturesInterpreter.CurrentGesture <= TouchGesture.MOVE_SOUTHEAST)
            {
                Point p0 = e.Finger.Position; 
                p0 = _msi.ElementToLogicalPoint(p0);
                Point p1 = _lastlocation; 
                Point shift = new Point(p0.X - p1.X, p0.Y - p1.Y);
                _msi.ViewportOrigin = new Point(_msi.ViewportOrigin.X - shift.X * 2, _msi.ViewportOrigin.Y - shift.Y * 2);
            }
        }

        void MultiScaleImage_FingerAdd(object sender, FingerTouchEventArgs e)
        {
            if (GesturesInterpreter.Fingers.Count == 0)
            {
                _lastlocation = _msi.ElementToLogicalPoint(e.Finger.Position);
            }
        }

        public MultiScaleTileSource Source
        {
            get { return _source; }
            set { _source = value; }
        }



        public System.Windows.Controls.MultiScaleImage Image
        {
            get { return _msi; }
        }


        public void SetSize(double x, double y, double w, double h)
        {
            System.Windows.Controls.MultiScaleImage msifade = new System.Windows.Controls.MultiScaleImage();
            msifade.Source = _source;
            msifade.Loaded += new RoutedEventHandler(msi_Loaded);
            this.Children.Add(msifade);
            //
            _zoomfactor = ((w > h ? w : h) / 7);
            this.Width = w;
            this.Height = h;
            this.TransformHelper.Translate = new Point(x, y);
            //
            msifade.Width = w;
            msifade.Height = h;
            msifade.MinWidth = w;
            msifade.MinHeight = h;
            msifade.UseLayoutRounding = true;
            msifade.UseSprings = true;
        }

        void msi_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Children.Contains(_msi)) this.Children.Remove(_msi);
            _msi = (System.Windows.Controls.MultiScaleImage)sender;
            //_msi.CacheMode = new BitmapCache();
        }

        public void Zoom(double zoom, Point pointToZoom)
        {
            Point logicalPoint = this._msi.ElementToLogicalPoint(pointToZoom);
            this._msi.ZoomAboutLogicalPoint(zoom, logicalPoint.X, logicalPoint.Y);
        }
    }
}
