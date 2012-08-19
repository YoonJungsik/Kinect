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
    /// Represents an image that contains one frame of data. 
    /// </summary>
    public class ImageFrame {

        /// <summary>
        /// Gets or sets the number of bytes per pixel.
        /// </summary>
        public int BytesPerPixel { get; set; }

        /// <summary>
        /// Gets the frame number.
        /// </summary>
        public int FrameNumber { get; private set; }

        /// <summary>
        /// Gets the frame height.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Gets the ImageType of the frame.
        /// </summary>
        public ImageType ImageType { get; private set; }

        /// <summary>
        /// Gets the size of the pixel data.
        /// </summary>
        public int PixelDataLength { get; private set; }

        /// <summary>
        /// Gets the frame resolution.
        /// </summary>
        public ImageResolution Resolution { get; private set; }

        /// <summary>
        /// Gets the timestamp of the frame.
        /// </summary>
        public long Timestamp { get; private set; }

        /// <summary>
        /// Gets the frame width.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the image frame data.
        /// </summary>
        public byte[] ImageData { get; private set; }

        internal ImageFrame(ImageType imageType, ImageResolution imageResolution, int height, int width, int framenumber, int pixelDataLength, int timestamp, byte[] data) {

            ImageType = imageType;
            Resolution = imageResolution;
            Height = height;
            Width = width;
            FrameNumber = framenumber;
            PixelDataLength = pixelDataLength;
            Timestamp = timestamp;
            ImageData = data;
            BytesPerPixel = pixelDataLength / (Width * Height);
        }

        /// <summary>
        /// Converts the depth data to bitmap data.
        /// </summary>
        /// <returns>the image frame data</returns>
        public byte[] ConvertDepthData() {

            byte[] frameData = new byte[(Width * Height) * 4];
            BytesPerPixel = 4;

            int greyIndex = 0;

            for (int y = 0; y < Height; y++) {

                int heightOffset = y * Width;

                for (int x = Width - 1; x >= 0; x--) {

                    byte intensity = (byte)(255f - ((255f * Math.Max((float)((ImageData[greyIndex] | (ImageData[greyIndex + 1] << 8)) - 800f), (float)0f)) / 12000f));

                    int index = (((Width - x) - 1) + heightOffset) * 4;

                    frameData[index + 2] = intensity;
                    frameData[index + 1] = intensity;
                    frameData[index] = intensity;

                    greyIndex += 2;
                }
            }

            return frameData;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal class NuiImageFrame {

        public UInt64 liTimeStamp;
        public uint dwFrameNumber;
        public ImageType eImageType;
        public ImageResolution eResolution;
        public IntPtr pFrameTexture;
        public uint dwFrameFlags_NotUsed;
        public ImageViewArea ViewArea_NotUsed;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ImageViewArea {

        public int eDigitalZoom;
        public int lCenterX;
        public int lCenterY;
    }
}