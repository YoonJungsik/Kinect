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
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Browser;

using System.Windows.Threading;

using MIRIA.Animations;
using MIRIA.Interaction.MultiTouch;
using MIRIA.Interaction.NiteKinect;
using MIRIA.Interaction.WiiRemote;
using MIRIA.Gestures;

using MIG.Client;

namespace MIRIAWeb
{
    public partial class MainPage : UserControl
    {
        private MIGClient _migclient;

        private TouchListener _touchlistener;
        private KinectListener _kinectlistener;
        private WiiRemoteListener _wiiremotelistener;
        
        private TransformHelper _appmenutrhelper;
        private TransformHelper _devmenutrhelper;

        public MainPage()
        {
            InitializeComponent();
            //
            // Multitouch TUIO / Standard Win 7 / Silverlight3 Multitouch / Mouse mapped as a single touch
            //
            _touchlistener = new TouchListener(LayoutRoot);
            //
            // Micrososft Kinect / PrimeSense Sensor - based on OpenNI NITE middleware
            //
            _kinectlistener = new KinectListener(LayoutRoot);
            Kinect.SetKinectListener(_kinectlistener.MIGListener as MIG.Client.Devices.NiteKinect.Kinect);
            //
            // Nintendo Wii Remote - based on MonoWiiUse included in monoMIG project
            //
            _wiiremotelistener = new WiiRemoteListener(LayoutRoot);
            WiiRemote.SetWiiRemoteListener(_wiiremotelistener.MIGListener as MIG.Client.Devices.Wii.Remote);
            //
            _migclient = new MIGClient();
            _migclient.AddListener("MT0", _touchlistener.MIGListener);
            _migclient.AddListener("KN0", _kinectlistener.MIGListener);
            _migclient.AddListener("WI0", _wiiremotelistener.MIGListener);
            _migclient.Connect();
            //
            Gallery.PhotoList.ElementTapped += new MIRIA.UIKit.ScrollView.ElementTappedHandler(PhotoList_ElementTapped);
            //
            Gallery.ButtonClose.Tapped += new MIRIA.UIKit.TButton.TappedHandler(ApplicationButtonClose_Tapped);
            MultiScaleDemo.ButtonClose.Tapped += new MIRIA.UIKit.TButton.TappedHandler(ApplicationButtonClose_Tapped);
            VirtualEarth.ButtonClose.Tapped += new MIRIA.UIKit.TButton.TappedHandler(ApplicationButtonClose_Tapped);
            Kinect.ButtonClose.Tapped += new MIRIA.UIKit.TButton.TappedHandler(ApplicationButtonClose_Tapped);
            WiiRemote.ButtonClose.Tapped += new MIRIA.UIKit.TButton.TappedHandler(ApplicationButtonClose_Tapped);
            Leggimi.ButtonClose.Tapped += new MIRIA.UIKit.TButton.TappedHandler(LeggimiButtonClose_Tapped);
            ContactsAndLinks.ButtonClose.Tapped += new MIRIA.UIKit.TButton.TappedHandler(ContactsAndLinksClose_Tapped);
        }

        void ContactsAndLinksClose_Tapped(object sender, FingerTouchEventArgs e)
        {
            ButtonTappedSound.Stop();
            ButtonTappedSound.Play();
            //
            ContactsAndLinks.Visibility = Visibility.Collapsed;
            ButtonMenuNext.Visibility = System.Windows.Visibility.Visible;
            Animator.AnimateProperty(MainMenu, "Opacity", 0.25, 1.0, 0.5, null);
        }

        void LeggimiButtonClose_Tapped(object sender, FingerTouchEventArgs e)
        {
            ButtonTappedSound.Stop();
            ButtonTappedSound.Play();
            //
            Leggimi.Visibility = Visibility.Collapsed;
            ButtonMenuNext.Visibility = System.Windows.Visibility.Visible;
            Animator.AnimateProperty(MainMenu, "Opacity", 0.25, 1.0, 0.5, null);
        }

        void ApplicationButtonClose_Tapped(object sender, FingerTouchEventArgs e)
        {
            ButtonTappedSound.Stop();
            ButtonTappedSound.Play();
            //
            _hideapplications();
        }


        void PhotoList_ElementTapped(object sender, MIRIA.UIKit.ScrollView.ElementTappedEventArgs e)
        {
            Gallery.PhotoList.ScrollerTransformHelper.Delay = 1.0;
            Gallery.PhotoList.ZoomToElementHeight(e.TappedElement);
        }

   


        private void Maps_Tapped(object sender, FingerTouchEventArgs e)
        {
            //_hideapplications();
            ButtonTappedSound.Stop();
            ButtonTappedSound.Play();
            VirtualEarth.Visibility = Visibility.Visible;
            _fadetoapplication();
        }

        private void Gallery_Tapped(object sender, FingerTouchEventArgs e)
        {
            ButtonTappedSound.Stop();
            ButtonTappedSound.Play();
            //_hideapplications();
//            OpenImage.Visibility = Visibility.Visible;
            Gallery.Visibility = Visibility.Visible;
            _fadetoapplication();
//            Gallery.PhotoList.ZoomToElementHeight(Gallery.SectionA);
        }

        private void WiiRemoteDemo_Tapped(object sender, FingerTouchEventArgs e)
        {
            ButtonTappedSound.Stop();
            ButtonTappedSound.Play();
            //_hideapplications();
            WiiRemote.Visibility = Visibility.Visible;
            _fadetoapplication();
        }

        private void KinectDemo_Tapped(object sender, FingerTouchEventArgs e)
        {
            ButtonTappedSound.Stop();
            ButtonTappedSound.Play();
            //_hideapplications();
            Kinect.Visibility = Visibility.Visible;
            _fadetoapplication();
        }

        private void ScaleImage_Tapped(object sender, FingerTouchEventArgs e)
        {
            ButtonTappedSound.Stop();
            ButtonTappedSound.Play();
            //_hideapplications();
            MultiScaleDemo.Visibility = Visibility.Visible;
            _fadetoapplication();
        }

        private void MiriaDocumentation_Tapped(object sender, FingerTouchEventArgs e)
        {
            ButtonTappedSound.Stop();
            ButtonTappedSound.Play();
            HtmlPage.Window.Navigate(new Uri("http://miria.codeplex.com/documentation"), "_blank");
        }

        private void MiriaHome_Tapped(object sender, FingerTouchEventArgs e)
        {
            ButtonTappedSound.Stop();
            ButtonTappedSound.Play();
            HtmlPage.Window.Navigate(new Uri("http://miria.codeplex.com/"), "_blank");
        }

        private void BehaviorExample_Tapped(object sender, FingerTouchEventArgs e)
        {
            ButtonTappedSound.Stop();
            ButtonTappedSound.Play();

            //if (BehaviorRect.Visibility == Visibility.Visible)
            //{
            //    BehaviorRect.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
            //    BehaviorRect.Visibility = Visibility.Visible;
            //}
        }

        private void _hideapplications()
        {
            Animator.AnimateProperty(Applications, "Opacity", 1.0, 0.0, 1, (object s, EventArgs a) =>
            {
                foreach (FrameworkElement app in Applications.Children)
                {
                    app.Visibility = Visibility.Collapsed;
                }
            });
            //Home.Visibility = System.Windows.Visibility.Collapsed;
            //OpenImage.Visibility = Visibility.Collapsed;
            //
            MenuPages.Visibility = System.Windows.Visibility.Visible;
            Animator.AnimateProperty(MenuPages, "Opacity", 0.0, 1.0, 1, (object s, EventArgs a) =>
            {
            });
        }

        private void _fadetoapplication()
        {
            //Animator.AnimateProperty(Dashboard, "Opacity", 1.0, 0.0, 1, (object s, EventArgs a) =>
            //{
            //    Dashboard.Visibility = Visibility.Collapsed;
            //});
            Applications.Opacity = 0;
            Applications.Visibility = Visibility.Visible;
            Animator.AnimateProperty(Applications, "Opacity", 0.0, 1.0, 1, (object s, EventArgs a) =>
            {
            });
            Animator.AnimateProperty(MenuPages, "Opacity", 1.0, 0.0, 1, (object s, EventArgs a) =>
            {
                MenuPages.Visibility = System.Windows.Visibility.Collapsed;
                //Home.Visibility = System.Windows.Visibility.Visible;
            });
        }

        private void BackgroundSoundLoop_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaElement ctrl = (MediaElement)sender;
            ctrl.Stop();
            ctrl.Play();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void LayoutRoot_LayoutUpdated(object sender, EventArgs e)
        {
            // TransformHelpers for animating menus
            if (_appmenutrhelper == null)
            {
                _appmenutrhelper = new TransformHelper(MainMenu);
                _devmenutrhelper = new TransformHelper(DevicesMenu);
                _devmenutrhelper.Delay = 0;
                _devmenutrhelper.Translate = new Point(this.ActualWidth + ((this.ActualWidth - DevicesMenu.ActualWidth) / 2), 0);
            }

        }

        private void ButtonMenuNext_Tapped(object sender, FingerTouchEventArgs e)
        {
            if (_appmenutrhelper.IsRunning) return;
            //
            _appmenutrhelper.Delay = 1.75;
            _appmenutrhelper.Translate = new Point(-this.ActualWidth, 0);
            Animator.AnimateProperty(MainMenu, "(UIElement.Projection).(PlaneProjection.RotationY)", 0D, -65D, 0.75, null);
            //
            _devmenutrhelper.Delay = 1.5;
            _devmenutrhelper.Translate = new Point(0, 0);
            Animator.AnimateProperty(DevicesMenu, "(UIElement.Projection).(PlaneProjection.RotationY)", 65D, 0D, 0.75, null);
            //
            ButtonMenuNext.Visibility = System.Windows.Visibility.Collapsed;
            ButtonMenuPrevious.Visibility = System.Windows.Visibility.Visible;
        }

        private void ButtonMenuPrevious_Tapped(object sender, FingerTouchEventArgs e)
        {
            if (_devmenutrhelper.IsRunning) return;
            //
            _devmenutrhelper.Delay = 1.75;
            _devmenutrhelper.Translate = new Point(this.ActualWidth + ((this.ActualWidth - DevicesMenu.ActualWidth) / 2), 0);
            Animator.AnimateProperty(DevicesMenu, "(UIElement.Projection).(PlaneProjection.RotationY)", 0D, 65D, 0.75, null);
            //
            _appmenutrhelper.Delay = 1.5;
            _appmenutrhelper.Translate = new Point();
            Animator.AnimateProperty(MainMenu, "(UIElement.Projection).(PlaneProjection.RotationY)", -65D, 0D, 0.75, null);
            //
            ButtonMenuNext.Visibility = System.Windows.Visibility.Visible;
            ButtonMenuPrevious.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ButtonMenuNext_DragLimitReached(object element, MIRIA.UIKit.DragLimitReachedEventArgs e)
        {
            ButtonMenuNext_Tapped(null, null);
        }

        private void ButtonMenuPrevious_DragLimitReached(object element, MIRIA.UIKit.DragLimitReachedEventArgs e)
        {
            ButtonMenuPrevious_Tapped(null, null);
        }

        private void About_Tapped(object sender, FingerTouchEventArgs e)
        {
            ButtonTappedSound.Stop();
            ButtonTappedSound.Play();
            //_hideapplications();
            ButtonMenuNext.Visibility = System.Windows.Visibility.Collapsed;
            Leggimi.Opacity = 0.0;
            Leggimi.Visibility = Visibility.Visible;
            Animator.AnimateProperty(Leggimi, "Opacity", 0.0, 1.0, 0.5, null);
            Animator.AnimateProperty(MainMenu, "Opacity", 1.0, 0.25, 0.5, null);

            //_fadetoapplication();
        }

        private void Contacts_Tapped(object sender, FingerTouchEventArgs e)
        {
            ButtonTappedSound.Stop();
            ButtonTappedSound.Play();
            //_hideapplications();
            ButtonMenuNext.Visibility = System.Windows.Visibility.Collapsed;
            ContactsAndLinks.Opacity = 0.0;
            ContactsAndLinks.Visibility = Visibility.Visible;
            Animator.AnimateProperty(ContactsAndLinks, "Opacity", 0.0, 1.0, 0.5, null);
            Animator.AnimateProperty(MainMenu, "Opacity", 1.0, 0.25, 0.5, null);

            //_fadetoapplication();
        }

    }
}
