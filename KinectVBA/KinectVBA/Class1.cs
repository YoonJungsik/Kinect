using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using Microsoft.Kinect;


namespace KinectVBA
{

    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class KinectVBA
    {

        private KinectSensor _kinect = null;

        private int _framerate=60;

        public KinectVBA()
        {

            if (KinectSensor.KinectSensors.Count == 0)
            {
                throw new Exception("Kinectが接続されていません");
            }

            // Kinectインスタンスを取得する
            _kinect = KinectSensor.KinectSensors[0];
        }

        
        public int KinectSensor_Count()
        {
            return KinectSensor.KinectSensors.Count;
        }

        public void KinectSensor_ElevationAngle(int angle)
        {
            if (_kinect == null) return;
            _kinect.ElevationAngle = angle;
        }


        #region Stop&Go


        public delegate void CallBackFunction();
        public delegate void CallBackFunction2(
            [MarshalAs(UnmanagedType.SafeArray)] 
            ref byte[] colorPixel);
        public CallBackFunction _callback;
        public CallBackFunction2 _callback2;



        public string KinectSensor_Start(
            [MarshalAs(UnmanagedType.FunctionPtr)] 
            ref CallBackFunction callback,
            [MarshalAs(UnmanagedType.FunctionPtr)] 
            ref CallBackFunction2 callback2
)
        {
            // Kinectインスタンスを取得する
            _kinect = KinectSensor.KinectSensors[0];

            _callback = callback;
            _callback2 = callback2;
            //_framerate = framerate;

            try
            {
                if (_kinect == null) return "nothing kinect";

                _kinect.ColorStream.Enable();
                _kinect.ColorFrameReady += kinect_ColorFrameReady;


                _kinect.Start();
                _kinect.ElevationAngle = 0;

                return "success!";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public void KinectSensor_Stop()
        {
            if (_kinect == null) return;
            _kinect.ColorFrameReady -= kinect_ColorFrameReady;
            _kinect.Stop();
            _kinect.Dispose();
            _kinect = null;
        }

        #endregion


        #region StreamEnable
        public void KinectSensor_ColorStreamEnable()
        {
            if (_kinect == null) return;
            _kinect.ColorStream.Enable();
        }

        public void KinectSensor_DepthStreamEnable()
        {
            if (_kinect == null) return;
            _kinect.DepthStream.Enable();
        }

        public void KinectSensor_SkeletonStreamEnable()
        {
            if (_kinect == null) return;
            _kinect.SkeletonStream.Enable();
        }

        #endregion



        /// <summary>
        /// RGBカメラのフレーム更新イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void kinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            try
            {
                // RGBカメラのフレームデータを取得する
                using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
                {
                    //if (colorFrame.FrameNumber % 60 != 0) return;//3フレームに1回にする

                    if (colorFrame != null)
                    {
                        _kinect.ColorFrameReady -= kinect_ColorFrameReady;

                        // RGBカメラのピクセルデータを取得する
                        byte[] colorPixel = new byte[colorFrame.PixelDataLength];
                        colorFrame.CopyPixelDataTo(colorPixel);

                        if (_callback != null) _callback();
                        if (_callback2 != null) _callback2(ref colorPixel);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public byte[] Get_RGBFrame_CopyPixelDataTo()
        {
            using (ColorImageFrame colorFrame = _kinect.ColorStream.OpenNextFrame(100))//100ミリ秒
            {
                if (colorFrame != null) return null;

                // RGBカメラのピクセルデータを取得する
                var colorPixel = new byte[colorFrame.PixelDataLength];
                colorFrame.CopyPixelDataTo(colorPixel);

                return colorPixel;
            }
        }


    }
}
