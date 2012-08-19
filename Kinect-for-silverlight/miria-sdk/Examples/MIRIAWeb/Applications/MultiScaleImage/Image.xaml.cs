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

using System.Windows.Resources;      // StreamResourceInfo
using System.Windows.Media.Imaging;  // BitmapImage
using System.Xml;
using System.Runtime.Serialization;
using System.Text;
using System.IO;
using System.Xml.Serialization;

using System.Windows.Browser;

using MIG.Client.Devices.MultiTouch;

namespace MIRIAWeb.Applications.MultiScaleImage
{
    public partial class Image : UserControl
    {
        private string _basepath = "http://win.generoso.info/";
        //System.IO.Path.GetDirectoryName(HtmlPage.Document.DocumentUri.OriginalString).Replace("\\", "/").Replace("http:/", "http://");
        private string _imagepath = "Gallery/IMG_3544.jpg";

        public Image()
        {
            InitializeComponent();        
        }



        private void RespCallback(IAsyncResult result)
        {
            HttpWebRequest request = result.AsyncState as HttpWebRequest;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(result))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Stream responseStream = response.GetResponseStream();
                        using (responseStream)
                        {
                            StreamReader readStream = new StreamReader(responseStream); //, Encoding.UTF8 );
                            using (readStream)
                            {
                                string _info = readStream.ReadToEnd();
                                string[] picInfo = _info.Split('x');
                                int _realWidth = int.Parse(picInfo[0]);
                                int _realHeight = int.Parse(picInfo[1]);
                                //
                                this.Dispatcher.BeginInvoke(delegate()
                                {
                                    ImageTileSource mts;
                                    mts = new ImageTileSource(_imagepath, _realWidth, _realHeight, 512, 512);
                                    TouchScaleImage.Source = mts;
                                    TouchScaleImage.SetSize(0, 0, this.RenderSize.Width, this.RenderSize.Height);
                                });
                            }
                        }
                    }
                    else
                    {
                        // "remote service error";
                    }
                }
            }
            catch (Exception ex)
            {
                // "error fetching image info";
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(_basepath + "/MIRIA-Web/Handler.ashx?fn=info&path=" + _imagepath, UriKind.Absolute));
            request.BeginGetResponse(new AsyncCallback(this.RespCallback), request);

        }


        private void ButtonZoomIn_Tapped(object sender, MIRIA.Interaction.MultiTouch.FingerTouchEventArgs e)
        {
            TouchScaleImage.Zoom(2.0, new Point(this.ActualWidth / 2, this.ActualHeight / 2));
        }

        private void ButtonZoomOut_Tapped(object sender, MIRIA.Interaction.MultiTouch.FingerTouchEventArgs e)
        {
            TouchScaleImage.Zoom(0.5, new Point(this.ActualWidth / 2, this.ActualHeight / 2));
        }
    }
}
