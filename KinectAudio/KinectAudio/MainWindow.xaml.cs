using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;

using Microsoft.Kinect;

namespace KinectAudio
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        KinectSensor _kinect;
        bool isContinue = true;

        public MainWindow()
        {
            InitializeComponent();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Kinectが接続されているかどうかを確認する
                if (KinectSensor.KinectSensors.Count == 0)
                {
                    throw new Exception("Kinectを接続してください");
                }

                // Kinectの動作を開始する
                StartKinect(KinectSensor.KinectSensors[0]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Kinectの停止
            _kinect.Stop();
            _kinect.Dispose();
        }


        /// <summary>
        /// Kinectの動作を開始する
        /// </summary>
        /// <param name="kinect"></param>
        private void StartKinect( KinectSensor kinect )
        {
            _kinect = kinect;

            // Kinectの動作を開始する
            kinect.Start();


/*
            // 音声入出力スレッド
            Thread thread = new Thread(new ThreadStart(() =>
            {
                // ストリーミングプレイヤー
                StreamingWavePlayer player = new StreamingWavePlayer(16000, 16, 1, 100);
                // 音声入力用のバッファ
                byte[] buffer = new byte[1024];

                // エコーのキャンセルと抑制を有効にする
                kinect.AudioSource.EchoCancellationMode = EchoCancellationMode.None;

                // 音声の入力を開始する
                using (Stream stream = kinect.AudioSource.Start())
                {
                    while (isContinue)
                    {
                        // 音声を入力し、スピーカーに出力する
                        stream.Read(buffer, 0, buffer.Length);
                        player.Output(buffer);
                    }
                }
            }));

            // スレッドの動作を開始する
            thread.Start();

            */
        }



    }
}
