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
using Mosallem.KinectSilverlightLibrary;
using System.Windows.Media.Imaging;
using System.IO;

namespace TestApp
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();
        }
        

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
           Runtime runtime = (App.Current as App).KinectRuntime;
           runtime.VideoStream.OpenStream(ImageType.Color, ImageResolution.Resolution640x480);
          runtime.DepthStream.OpenStream(ImageType.Depth, ImageResolution.Resolution320x240);
         runtime.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(runtime_VideoFrameReady);
           runtime.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(runtime_DepthFrameReady);
        }
    
        void runtime_VideoFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            Dispatcher.BeginInvoke(() => {
                image1.Source = e.ImageFrame.ToBitmapSource();
            });
        }
        void runtime_DepthFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                image2.Source = e.ImageFrame.ToBitmapSource();
            });

        }
    }
}
