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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using System.Collections.Generic;

namespace MIRIA.Animations
{
    public class Animator
    {
        public static void AnimatePropertyStop(FrameworkElement element, string dependencyproperty)
        {
            if (element.Resources.Contains("MIRIA_sb_AnimateProperty_" + dependencyproperty))
            {
                Storyboard storyboard = (Storyboard)element.Resources["MIRIA_sb_AnimateProperty_" + dependencyproperty];
                storyboard.SkipToFill();
                storyboard.Stop();
            }
        }
        public static void AnimateProperty(FrameworkElement element, string dependencyproperty, double valuefrom, double valueto, double durationsec, EventHandler completedCallback)
        {
            Storyboard storyboard;
            DoubleAnimation doubleanimation;
            Duration duration = new Duration(TimeSpan.FromSeconds(durationsec));

            if (!element.Resources.Contains("MIRIA_sb_AnimateProperty_" + dependencyproperty))
            {
                doubleanimation = new DoubleAnimation();
                storyboard = new Storyboard();
                storyboard.Duration = duration;
                storyboard.Children.Add(doubleanimation);
                element.Resources.Add("MIRIA_sb_AnimateProperty_" + dependencyproperty, storyboard);
                Storyboard.SetTargetProperty(doubleanimation, new PropertyPath(dependencyproperty));
                Storyboard.SetTarget(doubleanimation, element);
            }
            else
            {
                storyboard = (Storyboard)element.Resources["MIRIA_sb_AnimateProperty_" + dependencyproperty];
                doubleanimation = (DoubleAnimation)storyboard.Children[0];
                storyboard.SkipToFill();
                storyboard.Stop();
            }

            doubleanimation.From = valuefrom;
            doubleanimation.To = valueto;
            doubleanimation.Duration = duration;
            storyboard.FillBehavior = FillBehavior.HoldEnd;

                storyboard.Completed += new EventHandler((object sender, EventArgs args) =>
            {
                if (completedCallback != null)
                {
                    ////// TODO: write custom event with element passed into args not as sender
                    completedCallback(element, args);
                    completedCallback = null;
                }
                //element.Resources.Remove("MIRIA_sb_AnimateProperty_" + dependencyproperty);
            });

            storyboard.Begin();
        }
    
    }
}
