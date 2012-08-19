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

namespace MIRIAWeb.Applications.VirtualEarth
{
    public partial class VirtualEarth : UserControl
    {
        MIRIA.UIKit.TMultiScaleImage _vec = new MIRIA.UIKit.TMultiScaleImage();

        public VirtualEarth()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                //LayoutRoot.Children.Clear();

                _vec.Source = new VirtualEarthTileSource();

                LayoutRoot.Children.Insert(0, _vec);
            });
        }

        private void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _vec.SetSize(0, 0, this.ActualWidth, this.ActualHeight);
        }

        private void ButtonZoomIn_Tapped(object sender, MIRIA.Interaction.MultiTouch.FingerTouchEventArgs e)
        {
            _vec.Zoom(2.0, new Point(this.ActualWidth / 2, this.ActualHeight / 2));
        }

        private void ButtonZoomOut_Tapped(object sender, MIRIA.Interaction.MultiTouch.FingerTouchEventArgs e)
        {
            _vec.Zoom(0.5, new Point(this.ActualWidth / 2, this.ActualHeight / 2));
        }


    }
}
