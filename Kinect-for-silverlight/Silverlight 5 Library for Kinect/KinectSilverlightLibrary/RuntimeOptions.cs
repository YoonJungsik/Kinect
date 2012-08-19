using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Mosallem.KinectSilverlightLibrary
{
    public enum RuntimeOptions : uint
    {
        UseDepthAndPlayerIndex = 0x00000001,
        UseColor = 0x00000002,
        UseSkeletalTracking = 0x00000008,
        UseDepth = 0x00000020,
    }
}
