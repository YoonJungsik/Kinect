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

using MIRIA.Interaction.MultiTouch;

namespace MIRIA.Utility
{
    public class Transform2d
    {
        static public ScaleTransform GetScaleTransform(UIElement element)
        {
            // cerchiamo la ScaleTransform dell'oggetto
            ScaleTransform st = null;
            TransformGroup tg = (element.RenderTransform as TransformGroup);
            if (tg == null) return null;
            for (int i = 0; i < tg.Children.Count; i++)
            {
                if (tg.Children[i].GetType() == typeof(ScaleTransform))
                {
                    st = tg.Children[i] as ScaleTransform;
                    break;
                }
            }
            return st;
        }

        static public RotateTransform GetRotateTransform(UIElement element)
        {
            // cerchiamo la RotateTransform dell'oggetto
            RotateTransform rt = null;
            TransformGroup tg = (element.RenderTransform as TransformGroup);
            if (tg == null) return null;
            for (int i = 0; i < tg.Children.Count; i++)
            {
                if (tg.Children[i].GetType() == typeof(RotateTransform))
                {
                    rt = tg.Children[i] as RotateTransform;
                    break;
                }
            }
            return rt;
        }

        static public TranslateTransform GetTranslateTransform(UIElement element)
        {
            // cerchiamo la TranslateTransform dell'oggetto
            TranslateTransform tt = null;
            TransformGroup tg = (element.RenderTransform as TransformGroup);
            if (tg == null) return null;
            for (int i = 0; i < tg.Children.Count; i++)
            {
                if (tg.Children[i].GetType() == typeof(TranslateTransform))
                {
                    tt = tg.Children[i] as TranslateTransform;
                    break;
                }
            }
            return tt;
        }

        static public Point AdjustToParent(UIElement element, Point p)
        {
            // adjust translate to scaling factor
            // ...
            UIElement cs = element;
            while (cs != null && VisualTreeHelper.GetParent(cs) != null && VisualTreeHelper.GetParent(cs).GetType().IsSubclassOf(typeof(FrameworkElement)))
            {
                cs = (UIElement)VisualTreeHelper.GetParent(cs);
                ScaleTransform st = Transform2d.GetScaleTransform(cs);
                if (st != null)
                {
                    p.X = p.X / st.ScaleX;
                    p.Y = p.Y / st.ScaleY;
                }
                RotateTransform rt = Transform2d.GetRotateTransform(cs);
                if (rt != null)
                    p = MIRIA.Utility.Vector2D.Rotate(p, new Point(0, 0), -rt.Angle * Math.PI / 180);
            }
            return p;
        }


        static public List<UIElement> GetCollisionsAt(FrameworkElement targetelement, Point p, Size pointersize)
        {
            p = targetelement.TransformToVisual(Application.Current.RootVisual).Transform(p);
            Rect r = new Rect(p.X - (pointersize.Width / 2), p.Y - (pointersize.Height / 2), pointersize.Width, pointersize.Height);
            List<UIElement> collisions = new List<UIElement>();
            IEnumerable<UIElement> hits = VisualTreeHelper.FindElementsInHostCoordinates(r, Application.Current.RootVisual);
            foreach (UIElement uiel in hits)
            {
                // COLLISIONE INTERCETTATA ... =)";
                if (typeof(Touchable).IsAssignableFrom(uiel.GetType()))
                {
                    collisions.Add(uiel);
                }
            }
            return collisions;
        }


    }
}
