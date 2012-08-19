using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.ComponentModel;

using Microsoft.Kinect;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace KinectAlgorithm
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {


        #region field
        private KinectSensor kinectDevice;

        private WriteableBitmap _room_bitmap;
        private WriteableBitmap _human_image1_bitmap;

        private Int32Rect _screenImageRect;
        private short[] _depthPixelData;
        private byte[] _colorPixelData;


        #endregion


        private List<byte[]> byteList;
        private bool IsCaptcher;

        private bool ShowFrame = false;
        private long FrameIndex = 0;


        #region プロパティ


        private string _listSize;
        public string ListSize
        {
            get { return _listSize; }
            set
            {
                if (_listSize == value) return;
                _listSize = value;
                OnPropertyChanged("ListSize");
            }
        }



        public WriteableBitmap Human_image1_bitmap { get { return _human_image1_bitmap; } }
        public WriteableBitmap Room_Bitmap { get { return _room_bitmap; } }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }


        #endregion



        public MainWindow()
        {
            InitializeComponent();


            //kinect初期化
            if (init_kinect() == false) Close();


            //Bind用
            this.DataContext = this;

            IsCaptcher = false;
            ListSize = "init";
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            StopKinect();
        }


        /// <summary>
        /// Kinectの動作を停止する
        /// </summary>
        /// <param name="kinect"></param>
        private void StopKinect()
        {
            if (kinectDevice != null)
            {
                if (kinectDevice.IsRunning)
                {
                    // Kinectの停止と、ネイティブリソースを解放する
                    kinectDevice.Stop();
                    kinectDevice.Dispose();
                }
            }
        }

        
        /// <summary>
        /// レンダリング時にKinectデータを取得し描画
        /// </summary>
        private void compositionTarget_rendering(object sender, EventArgs e)
        {
            using (ColorImageFrame colorFrame = this.kinectDevice.ColorStream.OpenNextFrame(100))//100ミリ秒
            {
                using (DepthImageFrame depthFrame = this.kinectDevice.DepthStream.OpenNextFrame(100))
                {
                    RenderScreen(colorFrame, depthFrame);
                }
            }
        }


        /// <summary>
        /// kinect初期化
        /// </summary>
        private bool init_kinect()
        {
            try
            {
                // Kinectデバイスを取得する
                kinectDevice = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
                if (kinectDevice == null) return false;
                kinectDevice.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                kinectDevice.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                kinectDevice.SkeletonStream.Enable();

                //ビットマップデータ初期化
                var depthStream = kinectDevice.DepthStream;
                _depthPixelData = new short[kinectDevice.DepthStream.FramePixelDataLength];
                _colorPixelData = new byte[kinectDevice.ColorStream.FramePixelDataLength];


                //部屋の画層初期化
                _room_bitmap = new WriteableBitmap(depthStream.FrameWidth, depthStream.FrameHeight, 96, 96, PixelFormats.Bgra32, null);

                //人の画層初期化
                _screenImageRect = new Int32Rect(0, 0, (int)Math.Ceiling(_room_bitmap.Width), (int)Math.Ceiling(_room_bitmap.Height));
                kinectDevice.Start();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }



        /// <summary>
        /// キネクトの画像をビットマップデータに書き出す
        /// </summary>
        /// <param name="kinectDevice"></param>
        /// <param name="colorFrame"></param>
        /// <param name="depthFrame"></param>
        private void RenderScreen(ColorImageFrame colorFrame, DepthImageFrame depthFrame)
        {
            if (kinectDevice == null || depthFrame == null || colorFrame == null) return;

            ColorImageStream colorStream = kinectDevice.ColorStream;
            DepthImageStream depthStream = kinectDevice.DepthStream;


            int depth = 0;//深度計算用
            int depthPixelIndex; //深度ピクセル情報を得るためのインデックス
            int playerIndex = 0;//人ID
            int colorPixelIndex;//色ピクセル情報を得るためのインデックス
            //ColorImagePoint colorPoint;
            int colorStride = colorFrame.BytesPerPixel * colorFrame.Width; //4×画像幅
            int screenImageStride = kinectDevice.DepthStream.FrameWidth * colorFrame.BytesPerPixel;

            int ImageIndex = 0;
            byte[] bytePlayer1 = new byte[depthFrame.Height * screenImageStride];
            byte[] byteRoom = new byte[depthFrame.Height * screenImageStride];

            depthFrame.CopyPixelDataTo(_depthPixelData);
            colorFrame.CopyPixelDataTo(_colorPixelData);


            ColorImagePoint[] colorPoint =new ColorImagePoint[depthFrame.PixelDataLength];
            short[] depthPixel = new short[depthFrame.PixelDataLength];
            kinectDevice.MapDepthFrameToColorFrame(depthFrame.Format, depthPixel, colorFrame.Format, colorPoint);


            for (int depthY = 0; depthY < depthFrame.Height; depthY++)
            {
                for (int depthX = 0; depthX < depthFrame.Width; depthX++, ImageIndex += colorFrame.BytesPerPixel)
                {
                    depthPixelIndex = depthX + (depthY * depthFrame.Width);
                    playerIndex = _depthPixelData[depthPixelIndex] & DepthImageFrame.PlayerIndexBitmask; //人のID取得
                    //colorPoint = kinectDevice.MapDepthToColorImagePoint(depthFrame.Format, depthX, depthY, _depthPixelData[depthPixelIndex], colorFrame.Format);
                    int X = Math.Min(colorPoint[depthPixelIndex].X, colorStream.FrameWidth - 1);
                    int Y = Math.Min(colorPoint[depthPixelIndex].Y, colorStream.FrameHeight - 1);

                    colorPixelIndex = (X * colorFrame.BytesPerPixel) + (Y * colorStride);
                    if (playerIndex != 0)
                    {
                        //ピクセル深度を取得
                        depth = _depthPixelData[depthPixelIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                        bytePlayer1[ImageIndex] = _colorPixelData[colorPixelIndex];           //Blue    
                        bytePlayer1[ImageIndex + 1] = _colorPixelData[colorPixelIndex + 1];   //Green
                        bytePlayer1[ImageIndex + 2] = _colorPixelData[colorPixelIndex + 2];   //Red
                        bytePlayer1[ImageIndex + 3] = 0xFF;                             //Alpha
                    }
                    else
                    {
                        //人以外は背景イメージへ描画
                        byteRoom[ImageIndex] = _colorPixelData[colorPixelIndex];           //Blue    
                        byteRoom[ImageIndex + 1] = _colorPixelData[colorPixelIndex + 1];   //Green
                        byteRoom[ImageIndex + 2] = _colorPixelData[colorPixelIndex + 2];   //Red
                        byteRoom[ImageIndex + 3] = 0xFF;                             //Alpha
                    }
                }
            }



            //byteからビットマップへ書出し
            //_room_bitmap.WritePixels(_screenImageRect, byteRoom, screenImageStride, 0);


            if (IsCaptcher)
            {
                byteList.Add(bytePlayer1);
            //    ListSize = byteList.

                using (Stream stream = new MemoryStream())
                {
                    IFormatter formatter = new BinaryFormatter();

                    formatter.Serialize(stream, byteList);
                    ListSize = string.Format("SIZE={0:000,000,000,000}", stream.Length);
                    _screenImageStride = screenImageStride;
                }
            }


            //変更通知
            OnPropertyChanged("Room_bitmap");
        }


        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //レンダリング時にKinectデータを取得し描画
            CompositionTarget.Rendering += compositionTarget_rendering;

            byteList = new List<byte[]>();
            IsCaptcher = true;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            IsCaptcher = false;
        }



        private void button3_Click(object sender, RoutedEventArgs e)
        {
            ShowFrame = true;
            FrameIndex = 0;
            //レンダリング時にKinectデータを取得し描画
            CompositionTarget.Rendering -= compositionTarget_rendering;
            CompositionTarget.Rendering += compositionTarget_rendering2;
        }


        /// <summary>
        /// レンダリング時にKinectデータを取得し描画
        /// </summary>
        private void compositionTarget_rendering2(object sender, EventArgs e)
        {
            FrameIndex++;
            RenderScreen2();
        }


        int _screenImageStride;

        /// <summary>
        /// キネクトの画像をビットマップデータに書き出す
        /// </summary>
        /// <param name="kinectDevice"></param>
        /// <param name="colorFrame"></param>
        /// <param name="depthFrame"></param>
        private void RenderScreen2()
        {
            if (byteList.Count <= FrameIndex) return;

            var byteRoom = byteList[(int)FrameIndex];

            //byteからビットマップへ書出し
            _room_bitmap.WritePixels(_screenImageRect, byteRoom, _screenImageStride, 0);

            //変更通知
            OnPropertyChanged("Room_bitmap");
        }



    }
}
