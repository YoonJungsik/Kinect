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

using System.Windows.Interactivity;

using MIRIA.Gestures;
using MIRIA.UIKit;

namespace MIRIA.Behaviors.MultiTouch
{
    public class TouchScale : Behavior<DependencyObject>
    {
        private TCanvas _tcanvas;
        private double _scale;

        protected override void OnAttached()
        {
            base.OnAttached();

            FrameworkElement targetelement = (AssociatedObject as FrameworkElement);
            targetelement.Loaded += new RoutedEventHandler(targetelement_Loaded);
            targetelement.LayoutUpdated += new EventHandler(targetelement_LayoutUpdated);
        }

        void targetelement_LayoutUpdated(object sender, EventArgs e)
        {
            if (_tcanvas != null)
            {
                FrameworkElement targetelement = (AssociatedObject as FrameworkElement);
                _tcanvas.Width = targetelement.ActualWidth;
                _tcanvas.Height = targetelement.ActualHeight;
            }
        }

        void targetelement_Loaded(object sender, RoutedEventArgs e)
        {
            if (_tcanvas == null)
            {
                _tcanvas = new TCanvas();
                FrameworkElement targetelement = (AssociatedObject as FrameworkElement);
                Panel parent = (Panel)targetelement.Parent;
                if (parent is TCanvas)
                {
                    _tcanvas = parent as TCanvas;
                }
                else
                {
                    int objindex = parent.Children.IndexOf(targetelement);
                    parent.Children.RemoveAt(objindex);
                    _tcanvas.Children.Add(targetelement);
                    parent.Children.Insert(objindex, _tcanvas);
                }
                _tcanvas.Scale += new TCanvas.ScaleHandler(_tcanvas_Scale);
                _scale = _tcanvas.TransformHelper.Scale;
            }
        }

        void _tcanvas_Scale(object sender, GestureScaleEventArgs e)
        {
            _scale *= e.ScaleFactor;
            _tcanvas.TransformHelper.Scale = _scale;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }

    }
}
