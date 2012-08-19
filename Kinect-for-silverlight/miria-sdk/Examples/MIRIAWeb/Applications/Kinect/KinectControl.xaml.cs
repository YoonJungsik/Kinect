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

namespace MIRIAWeb.Applications.Kinect
{
    public partial class KinectControl : UserControl
    {
        //private MIG.Client.Devices.NiteKinect.Kinect _kinect;

        public KinectControl()
        {
            InitializeComponent();
        }


        public void SetKinectListener(MIG.Client.Devices.NiteKinect.Kinect kinect)
        {
            //_kinect = kinect;
            //_kinect.UserUpdate += new MIG.Client.Devices.NiteKinect.Kinect.UserUpdateHandler(_kinect_UserUpdate);
            Skelton.SetKinectListener(kinect);
        }

        //private void _kinect_UserUpdate(object sender, MIG.Client.Devices.NiteKinect.KinectUserStateEventArgs args)
        //{
        //}



    }
}
