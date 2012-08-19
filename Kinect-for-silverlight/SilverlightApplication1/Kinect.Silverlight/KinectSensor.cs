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


namespace Kinect.Silverlight
{
    public class KinectSensor
    {
        [DllImport("Kinect10.DLL")]
        private static extern HRESULT NuiInitialize(uint dwFlags);

        [DllImport("Kinect10.DLL")]
        private static extern void NuiShutdown();


        //public KinectAudioSource AudioSource { get; }
        //public ColorImageStream ColorStream { get; }
        //public DepthImageStream DepthStream { get; }
        //public string DeviceConnectionId { get; }
        //public int ElevationAngle { get; set; }
        //public bool IsRunning { get; }
        //public static KinectSensorCollection KinectSensors { get; }
        //public int MaxElevationAngle { get; }
        //public int MinElevationAngle { get; }
        //public SkeletonStream SkeletonStream { get; }
        //public KinectStatus Status { get; }
        //public string UniqueKinectId { get; }

        //public event EventHandler<AllFramesReadyEventArgs> AllFramesReady;
        //public event EventHandler<ColorImageFrameReadyEventArgs> ColorFrameReady;
        //public event EventHandler<DepthImageFrameReadyEventArgs> DepthFrameReady;
        //public event EventHandler<SkeletonFrameReadyEventArgs> SkeletonFrameReady;

        //public void Dispose();
        //public void MapDepthFrameToColorFrame(DepthImageFormat depthImageFormat, short[] depthPixelData, ColorImageFormat colorImageFormat, ColorImagePoint[] colorCoordinates);
        //public ColorImagePoint MapDepthToColorImagePoint(DepthImageFormat depthImageFormat, int depthX, int depthY, short depthPixelValue, ColorImageFormat colorImageFormat);
        //public SkeletonPoint MapDepthToSkeletonPoint(DepthImageFormat depthImageFormat, int depthX, int depthY, short depthPixelValue);
        //public ColorImagePoint MapSkeletonPointToColor(SkeletonPoint skeletonPoint, ColorImageFormat colorImageFormat);
        //public DepthImagePoint MapSkeletonPointToDepth(SkeletonPoint skeletonPoint, DepthImageFormat depthImageFormat);
        //public void Start();
        //public void Stop();


    }
}


//public sealed class KinectSensor : IDisposable
//{
//    public KinectAudioSource AudioSource { get; }
//    public ColorImageStream ColorStream { get; }
//    public DepthImageStream DepthStream { get; }
//    public string DeviceConnectionId { get; }
//    public int ElevationAngle { get; set; }
//    public bool IsRunning { get; }
//    public static KinectSensorCollection KinectSensors { get; }
//    public int MaxElevationAngle { get; }
//    public int MinElevationAngle { get; }
//    public SkeletonStream SkeletonStream { get; }
//    public KinectStatus Status { get; }
//    public string UniqueKinectId { get; }

//    public event EventHandler<AllFramesReadyEventArgs> AllFramesReady;
//    public event EventHandler<ColorImageFrameReadyEventArgs> ColorFrameReady;
//    public event EventHandler<DepthImageFrameReadyEventArgs> DepthFrameReady;
//    public event EventHandler<SkeletonFrameReadyEventArgs> SkeletonFrameReady;

//    public void Dispose();
//    public void MapDepthFrameToColorFrame(DepthImageFormat depthImageFormat, short[] depthPixelData, ColorImageFormat colorImageFormat, ColorImagePoint[] colorCoordinates);
//    public ColorImagePoint MapDepthToColorImagePoint(DepthImageFormat depthImageFormat, int depthX, int depthY, short depthPixelValue, ColorImageFormat colorImageFormat);
//    public SkeletonPoint MapDepthToSkeletonPoint(DepthImageFormat depthImageFormat, int depthX, int depthY, short depthPixelValue);
//    public ColorImagePoint MapSkeletonPointToColor(SkeletonPoint skeletonPoint, ColorImageFormat colorImageFormat);
//    public DepthImagePoint MapSkeletonPointToDepth(SkeletonPoint skeletonPoint, DepthImageFormat depthImageFormat);
//    public void Start();
//    public void Stop();
//}
