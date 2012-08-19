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
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

using MIRIA.Animations;
using MIRIA.Interaction.MultiTouch;

using MIG.Client.Devices.Wii;

namespace MIRIA.Interaction.WiiRemote
{


	public partial class WiiRemoteControl : UserControl
	{
		Remote wiiremote;

		public WiiRemoteControl ()
		{
			this.InitializeComponent ();
		}
		
		public void SetWiiRemoteListener(Remote wremote)
		{
			wiiremote = wremote;
			wiiremote.AccelerationUpdate += HandleWiiremoteAccelerationUpdate;
			wiiremote.ButtonPressed += HandleWiiremoteButtonPressed;
			wiiremote.ButtonReleased += HandleWiiremoteButtonReleased;
			wiiremote.RemoteConnected += HandleWiiremoteRemoteConnected;
			wiiremote.RemoteDisconnected += HandleWiiremoteRemoteDisconnected;
		}


        private void Connect_Tapped(object sender, MIRIA.Interaction.MultiTouch.FingerTouchEventArgs e)
        {
            try
            {
                wiiremote.RequestConnect();
            }
            catch (Exception ex) { }
        }

		void HandleWiiremoteRemoteDisconnected (object sender)
		{
            LayoutRoot.Dispatcher.BeginInvoke(() =>
            {
                Connect.Visibility = System.Windows.Visibility.Visible;
                WiiRemoteStatus.Text = "Wii Remote Disconnected! =)";
                Animator.AnimateProperty(Connect, "Opacity", 0.0, 1.0, 1, (object s, EventArgs a) =>
                {
                });
            });
		}

		void HandleWiiremoteRemoteConnected (object sender)
		{
            LayoutRoot.Dispatcher.BeginInvoke(() =>
            {
                Connect.Visibility = System.Windows.Visibility.Collapsed;
                WiiRemoteStatus.Text = "Wii Remote Connected! =)";
                Animator.AnimateProperty(Connect, "Opacity", 1.0, 0.0, 1, (object s, EventArgs a) =>
                {
                });
            });
		}

		
		void HandleWiiremoteButtonReleased (object sender, ButtonEventArgs args)
		{
			LayoutRoot.Dispatcher.BeginInvoke(()=>{
                WiiKey.Text = "";
			});
		}

		void HandleWiiremoteButtonPressed (object sender, ButtonEventArgs args)
		{
			LayoutRoot.Dispatcher.BeginInvoke(()=>{
				WiiKey.Text = args.Button.ToString();
			});
		}

		void HandleWiiremoteAccelerationUpdate (object sender, WiiAccelerationUpdateEventArgs e)
		{
			LayoutRoot.Dispatcher.BeginInvoke(()=>{
				WiiPitchAngle.Angle = e.Pitch;
				WiiRollAngle.Angle = e.Roll;
			});
		}

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                wiiremote.RequestConnect();
            }
            catch (Exception ex) { }
        }


        //////////////////////////////////////////////////////////////////


        private void SwitchRumble_Tapped(object sender, FingerTouchEventArgs e)
        {
            bool inversestatus = (SwitchRumble.Tag.ToString() == "1" ? false : true);
            try
            {
                wiiremote.SetRumble(inversestatus);
                SwitchRumble.Tag = (inversestatus ? "1" : "0");
            }
            catch (Exception ex) { }
        }

        private void WiiLed1_Tapped(object sender, FingerTouchEventArgs e)
        {
            bool inversestatus = (WiiLed1.Tag.ToString() == "1" ? false : true);
            try
            {
                wiiremote.SetLed(0, inversestatus);
                WiiLed1.Tag = (inversestatus ? "1" : "0");
                WiiLed1.Fill = new SolidColorBrush(inversestatus ? Color.FromArgb(255, 0x33, 0x33, 0xFF) : Color.FromArgb(255, 0x00, 0x00, 0x44));
            }
            catch (Exception ex) { }
        }

        private void WiiLed2_Tapped(object sender, FingerTouchEventArgs e)
        {
            bool inversestatus = (WiiLed2.Tag.ToString() == "1" ? false : true);
            try
            {
                wiiremote.SetLed(1, inversestatus);
                WiiLed2.Tag = (inversestatus ? "1" : "0");
                WiiLed2.Fill = new SolidColorBrush(inversestatus ? Color.FromArgb(255, 0x33, 0x33, 0xFF) : Color.FromArgb(255, 0x00, 0x00, 0x44));
            }
            catch (Exception ex) { }
        }

        private void WiiLed3_Tapped(object sender, FingerTouchEventArgs e)
        {
            bool inversestatus = (WiiLed3.Tag.ToString() == "1" ? false : true);
            try
            {
                wiiremote.SetLed(2, inversestatus);
                WiiLed3.Tag = (inversestatus ? "1" : "0");
                WiiLed3.Fill = new SolidColorBrush(inversestatus ? Color.FromArgb(255, 0x33, 0x33, 0xFF) : Color.FromArgb(255, 0x00, 0x00, 0x44));
            }
            catch (Exception ex) { }
        }

        private void WiiLed4_Tapped(object sender, FingerTouchEventArgs e)
        {
            bool inversestatus = (WiiLed4.Tag.ToString() == "1" ? false : true);
            try
            {
                wiiremote.SetLed(3, inversestatus);
                WiiLed4.Tag = (inversestatus ? "1" : "0");
                WiiLed4.Fill = new SolidColorBrush(inversestatus ? Color.FromArgb(255, 0x33, 0x33, 0xFF) : Color.FromArgb(255, 0x00, 0x00, 0x44));
            }
            catch (Exception ex) { }
        }
		
	}
}

