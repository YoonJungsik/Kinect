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

namespace MIRIAWeb.Applications.FlickrGallery
{
    public partial class PanelFrame : UserControl
    {
        private string _imageurl = "";

        public PanelFrame()
        {
            InitializeComponent();
        }


        public string ImageUrl
        {
            get { return _imageurl; }
            set { _imageurl = value; }
        }
    }
}
