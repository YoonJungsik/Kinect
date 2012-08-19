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

namespace KinectForSilverlight {

    /// <summary>
    /// The exception that is thrown when an error occurs in the Kinect library.
    /// </summary>
    public class KinectException : Exception {

        /// <summary>
        /// Initializes a new instance of the KinectException class.
        /// </summary>
        public KinectException() : base() { }

        /// <summary>
        /// Initializes a new instance of the KinectException class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public KinectException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the KinectException class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="e">The exception that is the cause of the current exception. 
        /// If the innerException parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
        public KinectException(string message, Exception e) : base(message, e) { }
    }
}