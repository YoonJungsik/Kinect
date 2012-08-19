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

using System.Windows.Media.Imaging;

using FlickrSearch;
using MIRIA.UIKit;
using MIRIA.Gestures;

namespace MIRIAWeb.Applications.FlickrGallery
{
    public partial class Gallery : UserControl
    {
        private FlickrSearch.FlickrSearch _flickrsearch;
        private FlickrClient _flickrclient;
        private const int _resultsperpage = 10;
        private int _resultssofar = 0;
        private int _resultstoget = 42;
        private string _flickrapikey = "2e47b9a1154be9b3bf482979458afea5";

        public Gallery()
        {
            InitializeComponent();


            _flickrclient = new FlickrClient();
            _flickrclient.SearchCompleted += new EventHandler<SearchCompletedEventArgs>(flickrClient_SearchCompleted);
            _flickrsearch = new FlickrSearch.FlickrSearch()
            {
                ApiKey = _flickrapikey,
                Page = 1,
                PerPage = _resultsperpage,
                Text = "landscape"
            };

    //        _flickrclient.SearchAsync(_flickrsearch);

            PhotoList.GestureDetected += new ScrollView.GestureDetectedHandler(PhotoList_GestureDetected);
        }

        void PhotoList_GestureDetected(object sender, GestureDetectedEventArgs e)
        {
            if (PhotoList.ScrollerTransformHelper.IsRunning) return;
            //
            if (e.Gesture == TouchGesture.SCALE)
            {
                double scale = (double)e.GestureParameters;
                if (scale < 0.85)
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        PhotoList.ZoomToFullHeight();
                    });
                }
                else if (scale > 1.15 && PhotoList.FirstTouchedElement != null)
                {
                    this.Dispatcher.BeginInvoke(() =>
                    {
                        PhotoList.ZoomToElementHeight(PhotoList.FirstTouchedElement);
                    });
                }
            }
        }

        void _dosearch()
        {
            _resultssofar = 0;
//            _flickrsearch.Text = "";
            _flickrsearch.Page = 1;
            _flickrclient.SearchAsync(_flickrsearch);
        }

        void flickrClient_SearchCompleted(object sender, SearchCompletedEventArgs e)
        {
            try
            {
                for (int x = 0; x < e.Results.Count; x++)
                {
                    int cx = x;
                    Pictures.Dispatcher.BeginInvoke(() =>
                    {
                        Canvas c = new Canvas();
                        c.Width = 580;
                        c.Height = 580;
                        PanelFrame img = new PanelFrame();
                        img.Image.Source = new BitmapImage(e.Results[cx].ImageUri);
                        img.ImageUrl = e.Results[cx].ImageUri.ToString();
                        img.Image.ImageOpened += delegate(object s, RoutedEventArgs args)
                        {
                            BitmapImage bi = img.Image.Source as BitmapImage;
                            c.Dispatcher.BeginInvoke(() =>
                            {
                                //c.Width = bi.PixelWidth * (580D / bi.PixelHeight);
                                //img.Width = c.Width;
                                //img.Image.Width = c.Width;
                            });
                        };
                        c.Children.Add(img);
                        c.SetValue(MIRIA.UIKit.ScrollView.HookableProperty, true);
                        c.Tag = img.ImageUrl;
                        if (Pictures.Children.Count > 21)
                            Pictures2.Children.Add(c);
                        else
                            Pictures.Children.Add(c);
                    });
                    //
                    _resultssofar++;
                    if (_resultssofar == _resultstoget)
                        break;
                }
            }
            catch (Exception ex) { }

            if (_resultssofar < _resultstoget)
            {
                _flickrsearch.Page++;
                _flickrclient.SearchAsync(_flickrsearch);
            }

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _dosearch();
        }

        private void ButtonNext_Tapped(object sender, MIRIA.Interaction.MultiTouch.FingerTouchEventArgs e)
        {
            PhotoList.ScrollerTransformHelper.Delay = 0.5;
            PhotoList.SimulateGesture(TouchGesture.MOVE_WEST);
        }

        private void ButtonPrevious_Tapped(object sender, MIRIA.Interaction.MultiTouch.FingerTouchEventArgs e)
        {
            PhotoList.ScrollerTransformHelper.Delay = 0.5;
            PhotoList.SimulateGesture(TouchGesture.MOVE_EAST);
        }

        private void ButtonUnZoom_Tapped(object sender, MIRIA.Interaction.MultiTouch.FingerTouchEventArgs e)
        {
            PhotoList.ScrollerTransformHelper.Delay = 0.25;
            PhotoList.ZoomToFullHeight();
        }
    
    }
}
