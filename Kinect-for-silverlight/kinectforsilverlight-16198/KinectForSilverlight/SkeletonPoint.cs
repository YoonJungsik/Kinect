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
    /// Represents a vector of four 32-bit floating-point components that are optimally aligned.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SkeletonPoint {

        /// <summary>
        /// Gets or sets the X coordinate of the SkeletonPoint. 
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate of the SkeletonPoint. 
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Gets or sets the Z coordinate of the SkeletonPoint. 
        /// </summary>
        public float Z { get; set; }

        /// <summary>
        /// W coordinate. For the floor plane, the W value is the distance from the plane to the origin. For all other vectors, this value is 1.0.
        /// </summary>
        internal float W { get; private set; }

        internal SkeletonPoint(float x, float y, float z, float w) : this() {

            X = x; 
            Y = y; 
            Z = z; 
            W = w;
        }

        /// <summary>
        /// Determines whether two SkeletonPoint instances are equal. 
        /// </summary>
        /// <param name="skeletonPoint1">A SkeletonPoint to compare for equality. </param>
        /// <param name="skeletonPoint2">A SkeletonPoint to compare for equality. </param>
        /// <returns>true if the two SkeletonPoint instances are equal; otherwise, false.</returns>
        public static bool op_Equality(SkeletonPoint skeletonPoint1, SkeletonPoint skeletonPoint2) {

            if (skeletonPoint1.X == skeletonPoint2.X && skeletonPoint1.Y == skeletonPoint2.Y && skeletonPoint1.Z == skeletonPoint2.Z)
                return true;

            return false;
        }

        /// <summary>
        /// Determines whether two SkeletonPoint instances are not equal. 
        /// </summary>
        /// <param name="skeletonPoint1">A SkeletonPoint to compare for inequality. </param>
        /// <param name="skeletonPoint2">A SkeletonPoint to compare for inequality. </param>
        /// <returns>true if the two SkeletonPoint instances are not equal; otherwise, false.</returns>
        public static bool op_Inequality(SkeletonPoint skeletonPoint1, SkeletonPoint skeletonPoint2) {

            if (skeletonPoint1.X != skeletonPoint2.X || skeletonPoint1.Y != skeletonPoint2.Y || skeletonPoint1.Z != skeletonPoint2.Z)
                return true;

            return false;
        }
    }
}