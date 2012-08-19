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
    /// Resolution options.
    /// </summary>
    public enum ImageResolution : int {

        /// <summary>
        /// The image resolution is invalid.
        /// </summary>
        Invalid = -1,

        /// <summary>
        /// The image resolution is 80 × 60.
        /// </summary>
        Resolution80x60 = 0,

        /// <summary>
        /// The image resolution is 320 × 240.
        /// </summary>
        Resolution320x240,

        /// <summary>
        /// The image resolution is 640 x 480.
        /// </summary>
        Resolution640x480,

        /// <summary>
        /// The image resolution is 1280 x 1024.
        /// </summary>
        Resolution1280x1024
    }
}