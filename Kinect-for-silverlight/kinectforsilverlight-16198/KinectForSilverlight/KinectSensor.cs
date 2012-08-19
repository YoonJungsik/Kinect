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
using System.Threading;
using System.Runtime.InteropServices;

namespace KinectForSilverlight {

    /// <summary>
    /// Represents a single Kinect sensor.
    /// </summary>
    public class KinectSensor {

        #region Function calls
        [DllImport("Kinect10.dll")]
        private static extern HRESULT NuiInitialize(uint dwFlags);

        [DllImport("Kinect10.dll")]
        private static extern void NuiShutdown();

        [DllImport("Kinect10.dll")]
        private static extern HRESULT NuiCameraElevationGetAngle(ref int angleDegrees);

        [DllImport("Kinect10.dll")]
        private static extern HRESULT NuiCameraElevationSetAngle(int angleDegrees);
        #endregion

        private static KinectSensor sensor;

        private Thread kinectWatcherThread;

        private SkeletonStream skeletonStream = new SkeletonStream();
        private ImageStream colorStream = new ImageStream();
        private ImageStream depthStream = new ImageStream();        

        /// <summary>
        /// Occurs when the next SkeletonStream is ready.
        /// </summary>
        public event EventHandler<SkeletonFrameReadyEventArgs> SkeletonFrameReady;

        /// <summary>
        /// Occurs when frame data for the ColorStream is ready.
        /// </summary>
        public event EventHandler<ImageFrameReadyEventArgs> ColorFrameReady;

        /// <summary>
        /// Occurs when frame data for the DepthStream is ready.
        /// </summary>
        public event EventHandler<ImageFrameReadyEventArgs> DepthFrameReady;

        /// <summary>
        /// Occurs when the Kinect has been initialized.
        /// </summary>
        public event EventHandler KinectInitialized;

        /// <summary>
        /// Occurs when the Kinect initialization has failed.
        /// </summary>
        public event EventHandler<KinectInitializeArgs> KinectInitializeFailed;

        /// <summary>
        /// The skeleton stream of the KinectSensor engine.
        /// </summary>
        public SkeletonStream SkeletonStream { get { return skeletonStream; } }

        /// <summary>
        /// Gets the color stream for the Kinect sensor.
        /// </summary>
        public ImageStream ColorStream { get { return colorStream; } }

        /// <summary>
        /// Gets the depth stream for the Kinect sensor.
        /// </summary>
        public ImageStream DepthStream { get { return depthStream; } }

        /// <summary>
        /// Minimum elevation angle.
        /// </summary>
        public int MaxElevationAngle { get { return 27; } }

        /// <summary>
        /// Maximum elevation angle.
        /// </summary>
        public int MinElevationAngle { get { return -27; } }

        /// <summary>
        /// Indicates whether the Kinect sensor is running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Gets or sets the elevation angle of the Kinect sensor.
        /// </summary>
        public int ElevationAngle {

            get {
                int angleDegrees = 0;
                var res = NuiCameraElevationGetAngle(ref angleDegrees);

                if (res != HRESULT.S_OK)
                    throw new KinectException("Failed to read the camera's elevation angle.");
                else
                    return angleDegrees;
            }

            set {
                if (value >= MinElevationAngle && value <= MaxElevationAngle) {
                    var res = NuiCameraElevationSetAngle(value);

                    if (res != HRESULT.S_OK)
                        throw new KinectException("Failed to set the camera's elevation angle.");
                }
            }
        }

        private KinectSensor() { }

        /// <summary>
        /// Gets a kinectSensor instance.
        /// </summary>
        /// <returns>A kinectSensor instance.</returns>
        public static KinectSensor GetKinectSensor() {
            
            if (sensor == null)
                sensor = new KinectSensor();
            
            return sensor;
        }

        /// <summary>
        /// Initializes the KinectSensor engine.
        /// </summary>
        /// <param name="options">Multiple KinectSensorOptions.</param>
        public void Start(KinectSensorOptions options) {
            
            // Initialize the kinect.
            var res = NuiInitialize((uint)options);

            if (res != HRESULT.S_OK) {
                string message = "";

                switch (res) {

                    case HRESULT.E_FAIL:
                        message = "Unspecified failure";
                        break;

                    case HRESULT.E_ABORT:
                        message = "Operation aborted";
                        break;

                    case HRESULT.E_ACCESSDENIED:
                        message = "General access denied error";
                        break;

                    case HRESULT.E_HANDLE:
                        message = "Invalid handle";
                        break;

                    case HRESULT.E_INVALIDARG:
                        message = "One or more arguments are not valid";
                        break;

                    case HRESULT.E_NOINTERFACE:
                        message = "No such interface supported";
                        break;

                    case HRESULT.E_NOTIMPL:
                        message = "Not implemented";
                        break;

                    case HRESULT.E_OUTOFMEMORY:
                        message = "Failed to allocate necessary memory";
                        break;

                    case HRESULT.E_POINTER:
                        message = "Invalid Pointer";
                        break;

                    case HRESULT.E_UNEXPECTED:
                        message = "Unexpected failure";
                        break;

                    default:
                        message = "Failed to initialize the kinect";
                        break;
                }

                if (KinectInitializeFailed != null)
                    KinectInitializeFailed(this, new KinectInitializeArgs(message));

            } else {

                // Start new thread.
                kinectWatcherThread = new Thread(new ParameterizedThreadStart(KinectWatcherThread));
                kinectWatcherThread.IsBackground = true;
                kinectWatcherThread.Start();

                IsRunning = true;

                if (KinectInitialized != null)
                    KinectInitialized(this, null);
            }
        }

        /// <summary>
        /// Uninitializes the KinectSensor engine.
        /// </summary>
        public void Stop() {

            if (kinectWatcherThread != null) {
                kinectWatcherThread.Abort();
                kinectWatcherThread.Join();
            }

            colorStream.Close();
            depthStream.Close();
            skeletonStream.Close();

            NuiShutdown();

            IsRunning = false;
        }

        private void KinectWatcherThread(object data) {

            while (true) {

                WaitHandle.WaitAll(new WaitHandle[] { SkeletonStream.NextFrameReadyEvent, ColorStream.NextFrameReadyEvent }, 0);

                if (this.ColorStream != null && this.ColorFrameReady != null && ColorFrameReady.GetInvocationList().Length > 0) {
                    var args = new ImageFrameReadyEventArgs();

                    // Get the next colorframe.
                    var frame = this.ColorStream.OpenNextFrame();

                    if (frame != null) {
                        args.ImageFrame = frame;

                        // Fire ColorFrameReady event.
                        ColorFrameReady(this, args);
                    }
                }

                if (this.DepthStream != null && this.DepthFrameReady != null && DepthFrameReady.GetInvocationList().Length > 0) {

                    var args = new ImageFrameReadyEventArgs();

                    // Get the next depthframe.
                    var frame = this.DepthStream.OpenNextFrame();

                    if (frame != null) {
                        args.ImageFrame = frame;

                        // Fire DepthFrameReady event.
                        DepthFrameReady(this, args);
                    }
                }

                if (this.SkeletonStream != null && this.SkeletonFrameReady != null && SkeletonFrameReady.GetInvocationList().Length > 0) {

                    var args = new SkeletonFrameReadyEventArgs();

                    // Get the next frame of skeletons.
                    var frame = this.SkeletonStream.OpenNextFrame();

                    if (frame != null) {
                        args.SkeletonFrame = frame;

                        // Fire SkeletonFrameReady event.
                        SkeletonFrameReady(this, args);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Provides the error message when the Kinect initialization has failed.
    /// </summary>
    public class KinectInitializeArgs : EventArgs {

        /// <summary>
        /// The specific message for the initialization failure.
        /// </summary>
        public string Message { get; set; }

        internal KinectInitializeArgs(string message) {
            this.Message = message;
        }
    }

    internal enum HRESULT : uint {

        S_FALSE = 0x0001,
        S_OK = 0x00000000,
        E_INVALIDARG = 0x80070057,
        E_OUTOFMEMORY = 0x8007000E,
        E_ABORT = 0x80004004,
        E_ACCESSDENIED = 0x80070005,
        E_FAIL = 0x80004005,
        E_HANDLE = 0x80070006,
        E_NOINTERFACE = 0x80004002,
        E_NOTIMPL = 0x80004001,
        E_POINTER = 0x80004003,
        E_UNEXPECTED = 0x8000FFFF
    }
}