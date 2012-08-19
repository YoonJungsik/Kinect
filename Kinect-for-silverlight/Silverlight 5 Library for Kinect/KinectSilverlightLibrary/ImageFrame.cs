using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.IO;

namespace Mosallem.KinectSilverlightLibrary
{
    [StructLayout(LayoutKind.Sequential)]
    internal class NativeImageFrame
    {
        public UInt64 liTimeStamp;
        public uint dwFrameNumber;
        public ImageType eImageType;
        public ImageResolution eResolution;
        public IntPtr pFrameTexture;
        public uint dwFrameFlags_NotUsed;
        public ImageViewArea ViewArea_NotUsed;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class ImageFrame
    {
        public long Timestamp;
        public int FrameNumber;
        public ImageType Type;
        public ImageResolution Resolution;
        public PlanarImage Image;
        private uint Flags;
        public ImageViewArea ViewArea;

        public ImageFrame()
        {
            this.Image = new PlanarImage();
        }

        public BitmapSource ToBitmapSource()
        {
            
            switch (this.Type)
            {
                case ImageType.Color:
                    return ToBitmap(this.Image.Bits,this.Image.Width, this.Image.Height);

                case ImageType.Depth:
                    byte[] bitmap = ConvertDepthFrameDataToBitmapData(this.Image.Bits,this.Image.Width,this.Image.Height);
                    return ToBitmap(bitmap, this.Image.Width, this.Image.Height);
            }
            return null;
        }
      
        internal BitmapSource ToBitmap(  byte[] pixels, int width, int height)
        {
            //X8R8G8B8
            int index = 0;
            WriteableBitmap bb = new WriteableBitmap(width, height);
            
            for (int y = 0; y < bb.PixelHeight; y++)
            {
                for (int x = 0; x < bb.PixelWidth; x++)
                {
                    bb.SetPixel(x, y, Color.FromArgb(255, pixels[index], pixels[index + 1], pixels[index + 2]));
                    index += 4;
                }
            }
           return  bb;
        }

        internal byte[] ConvertDepthFrameDataToBitmapData(byte[] pixels, int width, int height)
        {
            byte[] colorFrame = new byte[(height * width) * 4];
            int greyIndex = 0;
            for (int y = 0; y < height; y++)
            {
                int heightOffset = y * width;
                for (int x = 0; x < width; x++)
                {
                    byte intensity = CalculateIntensityFromDepth(pixels[greyIndex], pixels[greyIndex + 1]);
                    int index = (((width - x) - 1) + heightOffset) * 4;
                    colorFrame[index + 2] = intensity;
                    colorFrame[index + 1] = intensity;
                    colorFrame[index] = intensity;
                    greyIndex += 2;
                }
            }
            return colorFrame;
        }
        internal byte CalculateIntensityFromDepth(byte firstFrame, byte secondFrame)
        {
            return (byte)(255f - ((255f * Math.Max((float)((firstFrame | (secondFrame << 8)) - 800f), (float)0f)) / 3200f));
        }
   
    }
}