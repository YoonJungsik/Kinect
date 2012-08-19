using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

using System.Runtime.InteropServices;


namespace SilverlightApplication1
{



    public partial class MainPage : UserControl
    {

        #region DllImport

        [DllImport("Kinect10.dll")]
        private static extern HRESULT NuiInitialize(uint dwFlags);


        [DllImport("Kinect10.dll")]
        private static extern void NuiShutdown();


        [DllImport("Kinect10.DLL")]
        private static extern HRESULT NuiImageStreamOpen(
            /* [in] */ NUI_IMAGE_TYPE eImageType,
            /* [in] */ NUI_IMAGE_RESOLUTION eResolution,
            /* [in] */ uint dwImageFrameFlags,
            /* [in] */ uint dwFrameLimit,
            /* [in] */ IntPtr hNextFrameEvent,
            /* [out] */ ref IntPtr phStreamHandle);


        [DllImport("Kinect10.DLL")]
        private static extern HRESULT NuiImageStreamGetNextFrame(IntPtr hStream,
           uint dwMillisecondsToWait, ref IntPtr ppcImageFrame);
           //uint dwMillisecondsToWait, ref NUI_IMAGE_FRAME ppcImageFrame);

        [DllImport("Kinect10.DLL")]
        private static extern HRESULT NuiImageStreamReleaseFrame(IntPtr hStream,
           IntPtr pImageFrame);

        #endregion

        private IntPtr streamHandle = IntPtr.Zero;
        private IntPtr imageFramePtr = IntPtr.Zero;
        //private NUI_IMAGE_FRAME imageFrame;




        public MainPage()
        {
            InitializeComponent();
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            HRESULT res = NuiInitialize((uint)NUI_INITIALIZE_FLAG.USES_COLOR);
            if (res != HRESULT.S_OK)
                throw new InvalidOperationException("Failed to initialize the kinect runtime, return value:" + res.ToString());


            OpenStream(NUI_IMAGE_TYPE.NUI_IMAGE_TYPE_COLOR, NUI_IMAGE_RESOLUTION.NUI_IMAGE_RESOLUTION_640x480);


            //レンダリング時にKinectデータを取得し描画
            CompositionTarget.Rendering += compositionTarget_rendering;

        }


        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            NuiShutdown();
        }


        private void OpenStream(NUI_IMAGE_TYPE imageType, NUI_IMAGE_RESOLUTION resolution)
        {
            HRESULT res = NuiImageStreamOpen(imageType, resolution, 0, 2, IntPtr.Zero, ref streamHandle);

            if (res != HRESULT.S_OK)
                throw new Exception("Failed to open stream, return value:" + res.ToString());

        }



        /// <summary>
        /// レンダリング時にKinectデータを取得し描画
        /// </summary>
        private void compositionTarget_rendering(object sender, EventArgs e)
        {

            //フレームを読み取る
            HRESULT hr = NuiImageStreamGetNextFrame(streamHandle, 0, ref imageFramePtr);
            if (hr != HRESULT.S_OK)
            {
                return;
            }

            //フレームポインタからNUI_IMAGE_FRAMEへ変換
            var nativeImageFrame = new NUI_IMAGE_FRAME();
            Marshal.PtrToStructure(imageFramePtr, nativeImageFrame);



            //pFrameTextureからINuiFrameTextureへ変換
            //var pTexture = new NativePlanarImage();
            //Marshal.PtrToStructure(nativeImageFrame.pFrameTexture, pTexture);


            //dynamic pTexture = nativeImageFrame.pFrameTexture;
            //Marshal.PtrToStructure(nativeImageFrame.pFrameTexture, pTexture);


            ////
            IntPtr pLockedRect = IntPtr.Zero;
            IntPtr pRect = IntPtr.Zero;
            pTexture.LockRect(0, ref pLockedRect, ref pRect, 0);

            //var LockedRect = new NUI_LOCKED_RECT();




    //if ( LockedRect.Pitch != 0 )
    //{
    //    m_pDrawColor->Draw( static_cast<BYTE *>(LockedRect.pBits), LockedRect.size );
    //}
    //else
    //{
    //    OutputDebugString( L"Buffer length of received texture is bogus\r\n" );
    //}

    //pTexture->UnlockRect( 0 );

    //m_pNuiSensor->NuiImageStreamReleaseFrame( m_pVideoStreamHandle, &imageFrame );







            //INuiFrameTexture* pTexture = imageFrame.pFrameTexture;

            //image1.Source = ToBitmapSource();

            NuiShutdown();
        }




        private WriteableBitmap ToBitmap(byte[] pixels, int width, int height)
        {
            int index = 0;
            WriteableBitmap bb = new WriteableBitmap(width, height);

            for (int y = 0; y < bb.PixelHeight; y++)
            {
                for (int x = 0; x < bb.PixelWidth; x++)
                {
                    bb.Pixels[index] = pixels[index]; index++;
                    bb.Pixels[index] = pixels[index]; index++;
                    bb.Pixels[index] = pixels[index]; index++;
                    bb.Pixels[index] = pixels[index]; index++;
                }
            }

            return bb;
        }




    }
}
