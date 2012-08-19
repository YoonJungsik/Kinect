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
using Microsoft.Win32.SafeHandles;

namespace Mosallem.KinectSilverlightLibrary
{
    public class ImageStream
    {
        [DllImport("Kinect10.dll")]
        private static extern HRESULT NuiImageStreamOpen(ImageType eImageType, ImageResolution eResolution,
            uint dwImageFrameFlags_NotUsed, uint dwFrameLimit, IntPtr hNextFrameEvent, ref IntPtr phStreamHandle);

        [DllImport("Kinect10.dll")]
        private static extern HRESULT NuiImageStreamGetNextFrame(IntPtr hStream,
   uint dwMillisecondsToWait, ref IntPtr ppcImageFrame);

        [DllImport("Kinect10.dll")]
        private static extern HRESULT NuiImageStreamReleaseFrame(IntPtr hStream,
   IntPtr pImageFrame);


        ManualResetEvent nextFrameReadyEvent = new ManualResetEvent(false);
        internal  ManualResetEvent NextFrameReadyEvent
        {
            get { return nextFrameReadyEvent; }
        }


        private IntPtr streamHandle = IntPtr.Zero;

        public void OpenStream(ImageType imageType, ImageResolution resolution)
        {
            HRESULT res = NuiImageStreamOpen(imageType, resolution, 0, 2, IntPtr.Zero, ref streamHandle);

         if (res != HRESULT.S_OK)
             throw new Exception("Failed to open stream, return value:" + res.ToString());

        }

        public void Close()
        {
            if (nextFrameReadyEvent != null)
            {
                nextFrameReadyEvent.Close();
            }
        }

        internal ImageFrame GetNextFrame()
        {
            if (streamHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Stream Handle is null, make sure to open the stream first");
            }
            
            IntPtr nativeImageFramePtr=IntPtr.Zero;
            HRESULT res = NuiImageStreamGetNextFrame(streamHandle, 0, ref nativeImageFramePtr);
            if (res != HRESULT.S_OK)
            {
                throw new InvalidOperationException("Failed to get next frame, HRESULT: " + res.ToString());
            }
            if (res == HRESULT.S_FALSE)
            {
                return null;
            }
            if (nativeImageFramePtr == IntPtr.Zero)
            {
                return null;
            }
         
            NativeImageFrame nativeImageFrame=new NativeImageFrame() ;
            Marshal.PtrToStructure(nativeImageFramePtr, nativeImageFrame);

            int nativeimageframe = Marshal.SizeOf(typeof(NativeImageFrame));


            NativePlanarImage nativePlanarImage=new NativePlanarImage();
            Marshal.PtrToStructure(nativeImageFrame.pFrameTexture, nativePlanarImage);

            ImageFrame frame = new ImageFrame();
            //frame.Type = nativeImageFrame.eImageType;
            //frame.Resolution = nativeImageFrame.eResolution;
            //frame.FrameNumber = (int)nativeImageFrame.liTimeStamp;
            //frame.Timestamp =(long) nativeImageFrame.liTimeStamp;
            //frame.ViewArea = nativeImageFrame.ViewArea_NotUsed;
                        
            //frame.Image.Width = nativePlanarImage.Width;
            //frame.Image.Height = nativePlanarImage.Height;
            //frame.Image.BytesPerPixel = nativePlanarImage.BytesPerPixel;
            //frame.Image.Bits = new byte[(nativePlanarImage.Width * nativePlanarImage.Height) * nativePlanarImage.BytesPerPixel];
            //Marshal.Copy(nativePlanarImage.pBitsNative, frame.Image.Bits, 0, frame.Image.Bits.Length);
            
            res = NuiImageStreamReleaseFrame(streamHandle, nativeImageFramePtr);
            if (res!= HRESULT.S_FALSE && res!=HRESULT.S_OK)
            {
                throw new InvalidOperationException("Failed to release stream, HRESULT: "+ res.ToString());
            }
            return frame;
        }
    }
}
