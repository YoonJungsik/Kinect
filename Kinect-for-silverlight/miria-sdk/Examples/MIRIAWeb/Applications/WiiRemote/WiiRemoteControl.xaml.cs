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

using MIRIA.Animations;

namespace MIRIAWeb.Applications.WiiRemote
{
    public partial class WiiRemoteControl : UserControl
    {
        //private MIG.Client.Devices.Wii.Remote _wiiremote;

        public WiiRemoteControl()
        {
            InitializeComponent();
        }

        public void SetWiiRemoteListener(MIG.Client.Devices.Wii.Remote remote)
        {
            //_wiiremote = remote;
            //_wiiremote.RemoteConnected += new MIG.Client.Devices.Wii.Remote.RemoteConnectedHandler(_wiiremote_RemoteConnected);
            //_wiiremote.RemoteDisconnected += new MIG.Client.Devices.Wii.Remote.RemoteDisconnectedHandler(_wiiremote_RemoteDisconnected);
            WiiRemote.SetWiiRemoteListener(remote);
        }
    }
}
