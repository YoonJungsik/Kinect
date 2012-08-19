//Copyright (c) 2012 Zentrick BVBA

//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and 
//associated documentation files (the "Software"), to deal in the Software without restriction, including without 
//limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the 
//Software, and to permit persons to whom the Software is furnished to do so, subject to the following 
//conditions:

//The above copyright notice and this permission notice shall be included in all copies or substantial portions 
//of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
//PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
//LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace KinectForSilverlight {

    /// <summary>
    /// Represents the class for streams of Kinect sensor data.
    /// </summary>
    public class ImageStream {

        [DllImport("Kinect10.dll")]
        private static extern HRESULT NuiImageStreamOpen(ImageType eImageType, ImageResolution eResolution, uint dwImageFrameFlags, uint dwFrameLimit, IntPtr hNextFrameEvent, ref IntPtr phStreamHandle);

        [DllImport("Kinect10.dll")]
        private static extern HRESULT NuiImageStreamGetNextFrame(IntPtr hStream, uint dwMillisecondsToWait, ref IntPtr ppcImageFrame);

        [DllImport("Kinect10.dll")]
        private static extern HRESULT NuiImageStreamReleaseFrame(IntPtr hStream, IntPtr pImageFrame);

        [DllImport("Kinect10.dll")]
        private static extern HRESULT LockRect(uint Level, NuiLockedRect pLockedRect, uint pRect, uint Flags);

        private ManualResetEvent nextFrameReadyEvent = new ManualResetEvent(false);
        internal ManualResetEvent NextFrameReadyEvent { get { return nextFrameReadyEvent; } }

        private IntPtr streamHandle = IntPtr.Zero;
        private IntPtr imageFramePtr = IntPtr.Zero;

        /// <summary>
        /// Gets or sets a Boolean value that determines whether the image stream is enabled for use.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// /// Enables data from the sensor with the default resolution (640x480).
        /// </summary>
        /// <param name="type">Image type to use</param>
        public void Enable(ImageType type) {

            Enable(type, ImageResolution.Resolution640x480);
        }

        /// <summary>
        /// Enables data from the sensor with the specified format.
        /// </summary>
        /// <param name="type">Image type to use</param>
        /// <param name="resolution">Image resolution to use. The default value is ImageResolution.Resolution640x480.</param>
        public void Enable(ImageType type, ImageResolution resolution) {

            var res = NuiImageStreamOpen(type, resolution, 0, 2, IntPtr.Zero, ref streamHandle);

            if (res != HRESULT.S_OK) throw new KinectException("Failed to open the imagestream.");

            IsEnabled = true;
        }

        /// <summary>
        /// Opens the next frame in the stream.
        /// </summary>
        /// <param name="millisecondsWait">The timeout (in milliseconds) before returning without a new frame.</param>
        /// <returns>The next frame in the stream.</returns>
        public ImageFrame OpenNextFrame() {

            if (streamHandle == IntPtr.Zero)
                throw new KinectException("Open the imagestream first");

            var res = NuiImageStreamGetNextFrame(streamHandle, 100, ref imageFramePtr);

            if (res != HRESULT.S_OK) return null;

            var frame = new NuiImageFrame();
            Marshal.PtrToStructure(imageFramePtr, frame);

            //TODO
            var texture = Marshal.GetDelegateForFunctionPointer(frame.pFrameTexture, typeof(INuiFrameTexture));// = (INuiFrameTexture)Marshal.GetObjectForIUnknown(frame.pFrameTexture);

            var rect = new NuiLockedRect();
            //res = texture.LockRect(0, ref rect, IntPtr.Zero, 0);

            byte[] data = new byte[rect.size];
            Marshal.Copy(rect.pBits, data, 0, (int)rect.size);

            int width = 0;
            int height = 0;

            switch (frame.eResolution) {

                case ImageResolution.Resolution80x60:
                    width = 80;
                    height = 60;
                    break;

                case ImageResolution.Resolution640x480:
                    width = 640;
                    height = 480;
                    break;

                case ImageResolution.Resolution320x240:
                    width = 320;
                    height = 240;
                    break;

                case ImageResolution.Resolution1280x1024:
                    width = 1280;
                    height = 1024;
                    break;

                default:
                    width = 640;
                    height = 480;
                    break;
            }

            ImageFrame imageFrame = new ImageFrame(frame.eImageType, frame.eResolution, height, width, (int)frame.dwFrameNumber, (int)rect.size, (int)frame.liTimeStamp, data);

            res = NuiImageStreamReleaseFrame(streamHandle, imageFramePtr);

            if (res != HRESULT.S_FALSE && res != HRESULT.S_OK)
                throw new KinectException("Failed to release stream.");

            return imageFrame;
        }

        internal void Close() {

            if (nextFrameReadyEvent != null) nextFrameReadyEvent.Close();
        }
    }
}