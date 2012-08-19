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

using System.Runtime.InteropServices;

namespace KinectForSilverlight {

    /// <summary>
    /// Contains data for a single tracked skeleton.
    /// </summary>
    public class Skeleton {

        /// <summary>
        /// Gets or sets the way a skeleton is tracked.
        /// </summary>
        public SkeletonTrackingState TrackingState { get; set; }

        /// <summary>
        /// Gets or sets an ID for tracking a skeleton.
        /// </summary>
        public int TrackingID { get; set; }

        /// <summary>
        /// Gets the player's enrollment index. A value of zero indicates that the player is not currently enrolled.
        /// </summary>
        public int EnrollmentIndex { get; private set; }

        /// <summary>
        /// Gets or sets the skeleton position.
        /// </summary>
        public SkeletonPoint Position { get; set; }

        /// <summary>
        /// Gets or sets a collection of joints.
        /// </summary>
        public JointCollection Joints { get; set; }

        /// <summary>
        /// Gets or sets the quality of the skeleton data.
        /// </summary>
        public int QualityFlags { get; set; }

        /// <summary>
        /// Initializes a new instance of the Skeleton class.
        /// </summary>
        public Skeleton() { }

        internal Skeleton(SkeletonTrackingState trackingState, int trackingID, int enrollmentIndex, SkeletonPoint position, JointCollection joints) {

            TrackingState = trackingState;
            TrackingID = trackingID;
            EnrollmentIndex = enrollmentIndex;
            Position = position;
            Joints = joints;
        }
    }

    [StructLayoutAttribute(LayoutKind.Sequential)]
    internal struct NuiSkeletonData {

        public SkeletonTrackingState eTrackingState;
        public uint dwTrackingID;
        public uint dwEnrollmentIndex;
        public uint dwUserIndex_NotUsed;
        public SkeletonPoint position;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 20, ArraySubType = UnmanagedType.Struct)]
        public SkeletonPoint[] skeletonPositions;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 20, ArraySubType = UnmanagedType.Struct)]
        public JointTrackingState[] eSkeletonPositionTrackingState;
        public uint dwQualityFlags;
    }

    /// <summary>
    /// Specifies a skeleton's tracking state. 
    /// </summary>
    public enum SkeletonTrackingState {

        /// <summary>
        /// Not tracked
        /// </summary>
        NotTracked = 0,

        /// <summary>
        /// The overall position is being tracked but not the individual joint positions.
        /// </summary>
        PositionOnly,

        /// <summary>
        /// All joint positions are being tracked.
        /// </summary>
        Tracked
    }
}