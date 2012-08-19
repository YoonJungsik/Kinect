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

using MIG.Client.Devices.NiteKinect;

namespace MIRIA.Interaction.NiteKinect
{
    public partial class UserSkelton : UserControl
    {
        public UserSkelton()
        {
            InitializeComponent();
        }

        public void SetKinectListener(Kinect kinect)
        {
            kinect.SkeltonUpdate += new Kinect.SkeltonUpdateHandler(kinect_SkeltonUpdate);
            kinect.UserUpdate += new Kinect.UserUpdateHandler(kinect_UserUpdate);
        }


        private void kinect_UserUpdate(object sender, MIG.Client.Devices.NiteKinect.KinectUserStateEventArgs args)
        {
            this.Dispatcher.BeginInvoke(() =>
            {
                this.UserState.Text = args.State.ToString();
                if (args.Location != null)
                {
                    this.UserTrack.SetValue(Canvas.LeftProperty, (double)args.Location.X);
                    this.UserTrack.SetValue(Canvas.TopProperty, (double)args.Location.Y);
                }
            });
        }


        private void kinect_SkeltonUpdate(object sender, KinectSkeltonUpdateEventArgs args)
        {
            UpdateSkelton(args.Skelton);
        }


        public void UpdateSkelton(Dictionary<SkeltonJoint, Point3d> skelton)
        {
            LayoutRoot.Dispatcher.BeginInvoke(() =>
            {
                Point h = new Point(skelton[SkeltonJoint.Head].X, skelton[SkeltonJoint.Head].Y);
                Head.SetValue(Canvas.LeftProperty, h.X - 35);
                Head.SetValue(Canvas.TopProperty, h.Y - 45);
                //
                updateSkeletonLine(skelton, Neck, SkeltonJoint.Head, SkeltonJoint.Neck);
                updateSkeletonLine(skelton, LeftShoulder, SkeltonJoint.Neck, SkeltonJoint.LeftShoulder);
                updateSkeletonLine(skelton, RightShoulder, SkeltonJoint.Neck, SkeltonJoint.RightShoulder);
                updateSkeletonLine(skelton, LeftElbow, SkeltonJoint.LeftShoulder, SkeltonJoint.LeftElbow);
                updateSkeletonLine(skelton, LeftHand, SkeltonJoint.LeftElbow, SkeltonJoint.LeftHand);
                updateSkeletonLine(skelton, RightElbow, SkeltonJoint.RightShoulder, SkeltonJoint.RightElbow);
                updateSkeletonLine(skelton, RightHand, SkeltonJoint.RightElbow, SkeltonJoint.RightHand);
                //
                updateSkeletonLine(skelton, LeftUpTorso, SkeltonJoint.LeftShoulder, SkeltonJoint.Torso);
                updateSkeletonLine(skelton, RightUpTorso, SkeltonJoint.RightShoulder, SkeltonJoint.Torso);
                updateSkeletonLine(skelton, LeftDownTorso, SkeltonJoint.Torso, SkeltonJoint.LeftHip);
                updateSkeletonLine(skelton, RightDownTorso, SkeltonJoint.Torso, SkeltonJoint.RightHip);
                updateSkeletonLine(skelton, Pelvis, SkeltonJoint.LeftHip, SkeltonJoint.RightHip);
                //
                updateSkeletonLine(skelton, LeftHip, SkeltonJoint.LeftHip, SkeltonJoint.LeftKnee);
                updateSkeletonLine(skelton, LeftFoot, SkeltonJoint.LeftKnee, SkeltonJoint.LeftFoot);
                updateSkeletonLine(skelton, RightHip, SkeltonJoint.RightHip, SkeltonJoint.RightKnee);
                updateSkeletonLine(skelton, RightFoot, SkeltonJoint.RightKnee, SkeltonJoint.RightFoot);
            });
        }

        private void updateSkeletonLine(Dictionary<SkeltonJoint, Point3d> skelton, Line skeltonline, SkeltonJoint joint1, SkeltonJoint joint2)
        {
            try
            {
                skeltonline.X1 = skelton[joint1].X;
                skeltonline.Y1 = skelton[joint1].Y;
                skeltonline.X2 = skelton[joint2].X;
                skeltonline.Y2 = skelton[joint2].Y;
            }
            catch (Exception e) { }
        }

    }
}
