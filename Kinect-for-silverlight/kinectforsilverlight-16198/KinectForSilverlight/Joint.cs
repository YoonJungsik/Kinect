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

namespace KinectForSilverlight {

    /// <summary>
    /// Represents a skeleton joint.
    /// </summary>
    public class Joint {

        /// <summary>
        /// Gets or sets the type of joint.
        /// </summary>
        public JointType JointType { get; set; }

        /// <summary>
        /// Gets or sets the position of the joint.
        /// </summary>
        public SkeletonPoint Position { get; set; }

        /// <summary>
        /// Gets or sets the tracking state of the joint.
        /// </summary>
        public JointTrackingState TrackingState { get; set; }

        internal Joint(JointType jointType, SkeletonPoint skeletonPoint, JointTrackingState jointTrackingState) {

            JointType = jointType;
            Position = skeletonPoint;
            TrackingState = jointTrackingState;
        }

        /// <summary>
        /// Determines whether two Joint instances are equal.
        /// </summary>
        /// <param name="joint1">A Joint to compare for equality.</param>
        /// <param name="joint2">A Joint to compare for equality.</param>
        /// <returns>true if the two Joint instances are equal; otherwise, false.</returns>
        public static bool op_Equality(Joint joint1, Joint joint2) {

            if (joint1.JointType == joint2.JointType && joint1.TrackingState == joint2.TrackingState && SkeletonPoint.op_Equality(joint1.Position, joint2.Position))
                return true;

            return false;
        }

        /// <summary>
        /// Determines whether two Joint instances are not equal.
        /// </summary>
        /// <param name="joint1">A Joint to compare for inequality.</param>
        /// <param name="joint2">A Joint to compare for inequality.</param>
        /// <returns>true if the two Joint instances are not equal; otherwise, false.</returns>
        public static bool op_Inequality(Joint joint1, Joint joint2) {

            if (joint1.JointType != joint2.JointType && joint1.TrackingState != joint2.TrackingState && SkeletonPoint.op_Inequality(joint1.Position, joint2.Position))
                return true;

            return false;
        }
    }

    /// <summary>
    /// Specifies the joint tracking state. 
    /// </summary>
    public enum JointTrackingState {

        /// <summary>
        /// Not tracked.
        /// </summary>
        NotTracked = 0,

        /// <summary>
        /// Tracking is inferred.
        /// </summary>
        Inferred,

        /// <summary>
        /// tracked.
        /// </summary>
        Tracked
    }
}