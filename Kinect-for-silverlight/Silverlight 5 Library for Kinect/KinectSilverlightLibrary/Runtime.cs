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
using System.Threading;

namespace Mosallem.KinectSilverlightLibrary
{
    public class Runtime
    {
        [DllImport("Kinect10.dll")]
        private static extern HRESULT NuiInitialize(uint dwFlags);
        [DllImport("Kinect10.dll")]
        private static extern void NuiShutdown();


        Thread kinectWatcherThread;
        
        ImageStream videoStream = new ImageStream();
        public ImageStream VideoStream
        { get { return videoStream; } }
        public event EventHandler<ImageFrameReadyEventArgs> VideoFrameReady;

        ImageStream depthStream = new ImageStream();
        public ImageStream DepthStream
        { get { return depthStream; } }
        public event EventHandler<ImageFrameReadyEventArgs> DepthFrameReady;
        
        public void Initialize(RuntimeOptions options)
        {
            HRESULT res = NuiInitialize((uint)options);
            if (res != HRESULT.S_OK)
                throw new InvalidOperationException("Failed to initialize the kinect runtime, return value:" + res.ToString());

            kinectWatcherThread = new Thread(new ParameterizedThreadStart(this.KinectWatcherThreadProc));
            kinectWatcherThread.IsBackground = true;
            kinectWatcherThread.Start();
        }
        public void Uninitialize()
        {
            if (kinectWatcherThread != null)
            {
                kinectWatcherThread.Abort();
                kinectWatcherThread.Join();
            }
            videoStream.Close();
            depthStream.Close();
            NuiShutdown();
        }
        private void KinectWatcherThreadProc(object data)
        {
            while (true)
            {
                WaitHandle.WaitAll(new WaitHandle[] { VideoStream.NextFrameReadyEvent, DepthStream.NextFrameReadyEvent }, 250);
                if (this.VideoStream != null && this.VideoFrameReady != null && VideoFrameReady.GetInvocationList().Length > 0)
                {
                    ImageFrameReadyEventArgs args = new ImageFrameReadyEventArgs();
                    ImageFrame frame = this.VideoStream.GetNextFrame();
                    if (frame != null)
                    {
                        args.ImageFrame = frame;
                        if (VideoFrameReady != null)
                        {
                            VideoFrameReady(this, args);
                        }
                    }
                }


                if (this.DepthStream != null && this.DepthFrameReady != null && DepthFrameReady.GetInvocationList().Length > 0)
                {
                    ImageFrameReadyEventArgs args = new ImageFrameReadyEventArgs();
                    ImageFrame frame = this.DepthStream.GetNextFrame();
                    if (frame != null)
                    {
                        args.ImageFrame = frame;
                        if (DepthFrameReady != null)
                        {
                            DepthFrameReady(this, args);
                        }
                    }
                }
            }
        }
    
    }


    enum HRESULT : uint
    {
        S_FALSE = 0x0001,
        S_OK = 0x00000000,
        E_INVALIDARG = 0x80070057,
        E_OUTOFMEMORY = 0x8007000E
    }

}
