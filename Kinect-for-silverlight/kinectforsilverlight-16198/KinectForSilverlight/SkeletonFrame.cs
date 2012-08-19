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

namespace KinectForSilverlight {

    /// <summary>
    /// Contains information about a frame of data from the skeleton pipeline.
    /// </summary>
    public class SkeletonFrame {

        /// <summary>
        /// Gets or sets the frame's time stamp.
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the frame number.
        /// </summary>
        public int FrameNumber { get; private set; }

        /// <summary>
        /// Gets or sets the direction of the clipping plane for the floor.
        /// </summary>
        public Tuple<float, float, float, float> FloorClipPlane { get; set; }

        /// <summary>
        /// Gets an array of Skeleton structures, each of which contains data for one Skeleton.
        /// </summary>
        public Skeleton[] Skeletons { get; private set; }

        internal SkeletonFrame(long timeStamp, int frameNumber, SkeletonPoint floorClipPlane, Skeleton[] skeleton) {

            Timestamp = timeStamp;
            FrameNumber = frameNumber;
            FloorClipPlane = new Tuple<float, float, float, float>(floorClipPlane.X, floorClipPlane.Y, floorClipPlane.Z, floorClipPlane.W);
            Skeletons = skeleton;
        }
    }

    internal struct NuiSkeletonFrame {

        public Int64 liTimeStamp;
        public uint dwFrameNumber;
        public uint dwFlags_NotUsed;
        public SkeletonPoint vFloorClipPlane;
        public SkeletonPoint vNormalToGravity_NotUsed;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 6, ArraySubType = UnmanagedType.Struct)]
        public NuiSkeletonData[] skeletonData;
    }

    /// <summary>
    /// Contains transform smoothing parameters. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TransformSmoothParameters {

        /// <summary>
        /// Gets or sets the amount of smoothing.
        /// </summary>
        public float Smoothing { get; set; }

        /// <summary>
        /// Gets or sets the amount of correction.
        /// </summary>
        public float Correction { get; set; }

        /// <summary>
        /// Gets or sets the number of predicted frames.
        /// </summary>
        public float Prediction { get; set; }

        /// <summary>
        /// Gets or sets the radius (in meters) for jitter reduction.
        /// </summary>
        public float JitterRadius { get; set; }

        /// <summary>
        /// Gets or sets the maximum radius that filtered positions can deviate from raw data.
        /// </summary>
        public float MaxDeviationRadius { get; set; }

        /// <summary>
        /// Determines whether two TransformSmoothParameters instances are equal.
        /// </summary>
        /// <param name="smoothParameters1">A TransformSmoothParameters to compare for equality.</param>
        /// <param name="smoothParameters2">A TransformSmoothParameters to compare for equality.</param>
        /// <returns>true if the two TransformSmoothParameters instances are equal; otherwise, false.</returns>
        public static bool op_Equality(TransformSmoothParameters smoothParameters1, TransformSmoothParameters smoothParameters2) {

            if (smoothParameters1.Correction == smoothParameters2.Correction && smoothParameters1.JitterRadius == smoothParameters2.JitterRadius && 
                smoothParameters1.MaxDeviationRadius == smoothParameters2.MaxDeviationRadius && smoothParameters1.Prediction == smoothParameters2.Prediction && 
                smoothParameters1.Smoothing == smoothParameters2.Smoothing)
                return true;

            return false;
        }

        /// <summary>
        /// Determines whether two TransformSmoothParameters instances are not equal.
        /// </summary>
        /// <param name="smoothParameters1">A TransformSmoothParameters to compare for inequality.</param>
        /// <param name="smoothParameters2">A TransformSmoothParameters to compare for inequality.</param>
        /// <returns>true if the two TransformSmoothParameters instances are not equal; otherwise, false.</returns>
        public static bool op_Inequality(TransformSmoothParameters smoothParameters1, TransformSmoothParameters smoothParameters2) {
            
            if (smoothParameters1.Correction != smoothParameters2.Correction && smoothParameters1.JitterRadius != smoothParameters2.JitterRadius &&
                smoothParameters1.MaxDeviationRadius != smoothParameters2.MaxDeviationRadius && smoothParameters1.Prediction != smoothParameters2.Prediction &&
                smoothParameters1.Smoothing != smoothParameters2.Smoothing)
                return true;

            return false;
        }
    }
}