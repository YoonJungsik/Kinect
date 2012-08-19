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
using System.Runtime.InteropServices;

namespace Mosallem.KinectSilverlightLibrary
{
    [StructLayout(LayoutKind.Sequential)]
    public class PlanarImage
    {
        public int Width;
        public int Height;
        public int BytesPerPixel;
        public byte[] Bits;

    }


    [StructLayout(LayoutKind.Sequential)]
    internal class NativePlanarImage
    {
        public int Width;
        public int Height;
        public int BytesPerPixel;
        public IntPtr pBitsNative;
        public int reserved1;
    }
    
}
