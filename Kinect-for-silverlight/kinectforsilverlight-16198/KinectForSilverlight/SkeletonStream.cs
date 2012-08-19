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
using System.Collections.Generic;

namespace KinectForSilverlight {

    /// <summary>
    /// Represents a stream of SkeletonFrame objects.
    /// </summary>
    public class SkeletonStream {

        #region Function calls
        [DllImport("Kinect10.dll")]
        private static extern HRESULT NuiSkeletonTrackingEnable(IntPtr hNextFrameEvent, uint dwFlags);

        [DllImport("Kinect10.dll")]
        private static extern HRESULT NuiSkeletonTrackingDisable();

        [DllImport("Kinect10.dll")]
        private static extern HRESULT NuiSkeletonSetTrackedSkeletons(uint[] trackingIDs);

        [DllImport("Kinect10.dll")]
        private static extern HRESULT NuiSkeletonGetNextFrame(uint dwMillisecondsToWait, ref NuiSkeletonFrame pSkeletonFrame);

        [DllImport("Kinect10.dll")]
        private static extern HRESULT NuiTransformSmooth(ref NuiSkeletonFrame pSkeletonFrame, ref TransformSmoothParameters pSmoothingParams);



        #endregion

        //Maximum number of tracked skeletons.
        private const int MAX_TRACKED_COUNT = 6;
        private Skeleton[] skeletonData = new Skeleton[MAX_TRACKED_COUNT];

        /// <summary>
        /// Gets or sets a Boolean value that determines whether smoothing is applied to the skeleton data. 
        /// </summary>
        public bool TransformSmooth { get; set; }

        /// <summary>
        /// Gets or sets the transform smoothing parameters.
        /// </summary>
        public TransformSmoothParameters SmoothParameters { get { return smoothParameters; } set { smoothParameters = value; TransformSmooth = true; } }
        private TransformSmoothParameters smoothParameters;

        /// <summary>
        /// Gets a value indicating whether the skeleton stream is enabled.
        /// </summary>
        public bool IsEnabled { get; private set; }

        private ManualResetEvent nextFrameReadyEvent = new ManualResetEvent(false);
        internal ManualResetEvent NextFrameReadyEvent { get { return nextFrameReadyEvent; } }

        /// <summary>
        /// Enables skeleton tracking.
        /// </summary>
        public void Enable() {

            var res = NuiSkeletonTrackingEnable(IntPtr.Zero, 1);

            if (res != HRESULT.S_OK) throw new KinectException("Failed to open skeletonstream");

            for (int i = 0; i < skeletonData.Length; i++)
                skeletonData[i] = new Skeleton();

            IsEnabled = true;
        }

        /// <summary>
        /// Enables skeleton tracking.
        /// </summary>
        /// <param name="smoothParameters">Smoothing parameters.</param>
        public void Enable(TransformSmoothParameters smoothParameters) {

            Enable();

            SmoothParameters = smoothParameters;
            TransformSmooth = true;
        }

        /// <summary>
        /// Disables skeleton tracking.
        /// </summary>
        public void Disable() {

            var res = NuiSkeletonTrackingDisable();

            if (res != HRESULT.S_OK) throw new KinectException("Failed to close skeletonstream");

            IsEnabled = false;
            TransformSmooth = false;
        }

        internal SkeletonFrame OpenNextFrame() {

            var frame = new NuiSkeletonFrame();

            // Get the next NuiSkeletonFrame.
            var res = NuiSkeletonGetNextFrame(0, ref frame);

            if (res == HRESULT.S_FALSE) return null;
            if (res != HRESULT.S_OK) throw new KinectException("Failed to get next frame");

            // Apply TransformSmoothing.
            if (TransformSmooth)
                res = NuiTransformSmooth(ref frame, ref smoothParameters);

            if (res == HRESULT.S_FALSE) return null;
            if (res != HRESULT.S_OK) throw new KinectException("Failed to get next frame");

            // Read every native SkeletonData object into a Skeleton object.
            for (int index = 0; index < frame.skeletonData.Length; index++) {

                var nuiData = frame.skeletonData[index];

                if (nuiData.eTrackingState == SkeletonTrackingState.Tracked) {

                    var joints = new List<Joint>();

                    for (int i = 0; i < nuiData.skeletonPositions.Length; i++)
                        joints.Add(new Joint((JointType)i, nuiData.skeletonPositions[i], nuiData.eSkeletonPositionTrackingState[i]));

                    var collection = new JointCollection(joints);

                    skeletonData[index] = new Skeleton(nuiData.eTrackingState, (int)nuiData.dwTrackingID, (int)nuiData.dwEnrollmentIndex, nuiData.position, collection);

                } else skeletonData[index] = new Skeleton();
            }

            return new SkeletonFrame(frame.liTimeStamp, (int)frame.dwFrameNumber, frame.vFloorClipPlane, skeletonData);
        }

        internal void Close() {

            if (nextFrameReadyEvent != null) nextFrameReadyEvent.Close();
        }
    }
}