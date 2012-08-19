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
    /// Specifies the various skeleton joints.
    /// </summary>
    public enum JointType : int {

        /// <summary>
        /// Center, between hips.
        /// </summary>
        HipCenter = 0,

        /// <summary>
        /// Spine.
        /// </summary>
        Spine,

        /// <summary>
        /// Center, between shoulders.
        /// </summary>
        ShoulderCenter,

        /// <summary>
        /// Head.
        /// </summary>
        Head,

        /// <summary>
        /// Left shoulder.
        /// </summary>
        ShoulderLeft,

        /// <summary>
        /// Left elbow.
        /// </summary>
        ElbowLeft,

        /// <summary>
        /// Left wrist.
        /// </summary>
        WristLeft,

        /// <summary>
        /// Left hand.
        /// </summary>
        HandLeft,

        /// <summary>
        /// Right shoulder.
        /// </summary>
        ShoulderRight,

        /// <summary>
        /// Right elbow.
        /// </summary>
        ElbowRight,

        /// <summary>
        /// Right wrist.
        /// </summary>
        WristRight,

        /// <summary>
        /// Right hand.
        /// </summary>
        HandRight,

        /// <summary>
        /// Left hip.
        /// </summary>
        HipLeft,

        /// <summary>
        /// Left knee.
        /// </summary>
        KneeLeft,

        /// <summary>
        /// Left ankle.
        /// </summary>
        AnkleLeft,

        /// <summary>
        /// Left foot.
        /// </summary>
        FootLeft,

        /// <summary>
        /// Right hip.
        /// </summary>
        HipRight,

        /// <summary>
        /// Right knee.
        /// </summary>
        KneeRight,

        /// <summary>
        /// Right ankle.
        /// </summary>
        AnkleRight,

        /// <summary>
        /// Right foot.
        /// </summary>
        FootRight,

        /// <summary>
        /// Used as an index to terminate a loop. Not used as a position index.
        /// </summary>
        PositionCount
    }
}