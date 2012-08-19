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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace KinectForSilverlight {

    /// <summary>
    /// Represents an indexed collection of Joint structures.
    /// </summary>
    public class JointCollection : IEnumerable<Joint>, IEnumerable {

        private List<Joint> joints;

        /// <summary>
        /// Gets the number of joints in the collection.
        /// </summary>
        public int Count { get { return joints.Count; } }

        /// <summary>
        /// The joint identifier.
        /// </summary>
        /// <param name="jointType">Specifies a skeleton joint.</param>
        public Joint this[JointType jointType] {

            get {
                return joints.Single(j => j.JointType == jointType);
            }
        }

        internal JointCollection(List<Joint> joints) {

            this.joints = joints;
        }

        /// <summary>
        /// Gets the object's enumerator.
        /// </summary>
        /// <returns>An IEnumerator interface for enumerating the joints in the collection.</returns>
        public IEnumerator<Joint> GetEnumerator() {

            return joints.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {

            return joints.GetEnumerator();
        }
    }
}
