using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Coding4Fun.Kinect.Wpf;
using Microsoft.Kinect;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Kinect_Sssim
{


    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {


        private IntPtr SssimWnd;
        KinectSensor kinect;


        Microsoft.Xna.Framework.Vector3 left_joint_old;
        Microsoft.Xna.Framework.Vector3 right_joint_old;
        Microsoft.Xna.Framework.Vector3 v3_old; //前回ベクトル外積
      
        float len_back;

        public MainWindow()
        {
            InitializeComponent();


            #region キネクト初期化
            try
            {
                if (KinectSensor.KinectSensors.Count == 0)
                {
                    throw new Exception("Kinectが接続されていません");
                }

                // Kinectインスタンスを取得する
                kinect = KinectSensor.KinectSensors[0];

                //// Colorを有効にする
                //kinect.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(kinect_ColorFrameReady);
                //kinect.ColorStream.Enable();


                //// Depthを有効にする
                //kinect.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(kinect_DepthFrameReady);
                //kinect.DepthStream.Enable();

                //// Skeletonを有効にするとプレーヤーが取得できる
                //kinect.SkeletonStream.Enable();

                // すべてのフレーム更新通知をもらう
                kinect.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(kinect_AllFramesReady);


                // Color,Depth,Skeletonを有効にする
                kinect.ColorStream.Enable();
                kinect.DepthStream.Enable();
                kinect.SkeletonStream.Enable();


                // Kinectの動作を開始する
                kinect.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Close();
            }

            #endregion



            //太陽系シミュレータースタジオのウィンドウハンドル取得
            SssimWnd = SssimWindow.get_Sssim_hWnd();
            if (SssimWnd == IntPtr.Zero)
            {
                MessageBox.Show("太陽系シミュレータースタジオを起動してください");
                return;
            }



        }


        void kinect_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            imageRgbCamera.Source = e.OpenColorImageFrame().ToBitmapSource();
        }


        void kinect_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            imageDepthCamera.Source = e.OpenDepthImageFrame().ToBitmapSource();
        }



        // すべてのデータの更新通知を受け取る
        void kinect_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            //imageRgbCamera.Source = e.OpenColorImageFrame().ToBitmapSource();
            imageDepthCamera.Source = e.OpenDepthImageFrame().ToBitmapSource();

            // 骨格位置の表示
            //ShowSkeleton(e);

            get_postion(e);
        }


        private Skeleton[] skeletonData;

        private void get_postion(AllFramesReadyEventArgs e)
        {
            bool haveSkeletonData = false;

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if ((this.skeletonData == null) || (this.skeletonData.Length != skeletonFrame.SkeletonArrayLength))
                    {
                        this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    skeletonFrame.CopySkeletonDataTo(this.skeletonData);
                    haveSkeletonData = true;
                }
            }


            using (DepthImageFrame depthImageFrame = e.OpenDepthImageFrame())
            {
                if (depthImageFrame != null)
                {

                    foreach (Skeleton skeleton in this.skeletonData)
                    {

                        var left_joint = skeleton.Joints.Where(p => p.JointType == JointType.HandLeft).Single();
                        var right_joint = skeleton.Joints.Where(p => p.JointType == JointType.HandRight).Single();
                        //var right_joint2 = skeleton.Joints.Where(p => p.JointType == JointType.WristRight).Single();
                        var right_Wrist = skeleton.Joints.Where(p => p.JointType == JointType.WristLeft).Single();

                        var left_p = new Microsoft.Xna.Framework.Vector3(left_joint.Position.X, left_joint.Position.Y, left_joint.Position.Z);
                        var right_p = new Microsoft.Xna.Framework.Vector3(right_joint.Position.X, right_joint.Position.Y, right_joint.Position.Z);
                        var wrist_p = new Microsoft.Xna.Framework.Vector3(right_Wrist.Position.X, right_Wrist.Position.Y, right_Wrist.Position.Z);
                        if (left_p.Length() > 0 && right_p.Length() > 0 && wrist_p.Length() > 0)
                        {

                            zooming(left_p, right_p, wrist_p);

                            //label1.Content = string.Format("X={0} Y={1} Z={2}", right_p.X, right_p.Y, right_p.Z);
                            //label2.Content = string.Format("X={0} Y={1} Z={2}", right_p.X, right_p.Y, right_p.Z);
                            //label3.Content = string.Format("X={0} Y={1} Z={2}", wrist_p.X, wrist_p.Y, wrist_p.Z);

                            rotation(left_p, right_p, wrist_p);
                        }

                    }
                }
            }
        }


        private void zooming(Microsoft.Xna.Framework.Vector3 left_v, Microsoft.Xna.Framework.Vector3 right_v, Microsoft.Xna.Framework.Vector3 right_Elbow_v)
        {

            //両手の距離
            float len = Vector3.Distance(left_v,right_v);
            if (len_back == 0) { len_back = len; return; }

            //label2.Content = string.Format("len={0} len_back={1}", len, len_back);
            //label3.Content = "";
            if (Math.Abs(len_back - len) < 0.001) return;
            //label3.Content = "Calc";

            int step;
            #region 拡大・縮小
            step = 10;
            if (len_back > len)
            {
                MouseWinAPI.SendRightButtonDown(SssimWnd, 1, 1);
                MouseWinAPI.SendMouseMove(SssimWnd, 1, step);
                MouseWinAPI.SendRightButtonUp(SssimWnd, 1, step);
            }
            else
            {
                MouseWinAPI.SendRightButtonDown(SssimWnd, 1, step);
                MouseWinAPI.SendMouseMove(SssimWnd, 1, 1);
                MouseWinAPI.SendRightButtonUp(SssimWnd, 1, step);
            }
            len_back = len;
            #endregion

        }

        private void rotation(Microsoft.Xna.Framework.Vector3 left_v, Microsoft.Xna.Framework.Vector3 right_v, Microsoft.Xna.Framework.Vector3 right_Elbow_v)
        {
            //両手の距離
            float len = Vector3.Distance(left_v, right_v);
            if (len_back == 0) { len_back = len; return; }

            //前回計測時よりあまり動いてなかったら今回は何もしない
            //label2.Content = string.Format("len={0} len_back={1}", len, len_back);
            //label3.Content = "";
            if (Math.Abs(len_back - len) < 0.01) return;
            //label3.Content = "Calc";


            #region 回転

            var mouse = new Vector2(0);
            var mouse_move = new Vector2(0);
            

            //Y軸回転
            mouse_move.X += 10;


            //X軸回転
            mouse_move.Y += 10;


            //Z軸回転
            mouse_move.X += 10;
            mouse_move.Y += 10;



            //左手、右手、右ひじの、外積を求める
            Microsoft.Xna.Framework.Vector3 v1 = right_v - left_v; //右手位置-左手位置
            Microsoft.Xna.Framework.Vector3 v2 = right_Elbow_v - left_v; //右ひじ置-左手位置
            Microsoft.Xna.Framework.Vector3 v3 = Vector3.Cross(v1, v2);

            //v3.Normalize();
            //label2.Content = string.Format("外積 X={0} Y={1} Z={2}", v3.X, v3.Y, v3.Z);

            //前回計測時よりあまり動いてなかったら今回は何もしない
            //v3.Normalize();
            //v3_old.Normalize();
            //var move_dot = Vector3.Dot(v3, v3_old);
            //label2.Content = move_dot;
            //label3.Content = "";
            //if (Math.Abs(move_dot) < 0.01) return; //
            //label3.Content = "Calc";


            //前回ベクトルと、現在ベクトルの差
            Microsoft.Xna.Framework.Vector3 now_vec = v3_old - v3;
            now_vec.Normalize();//単位ベクトル化

            MouseWinAPI.SendLeftButtonDown(SssimWnd, 300, 300);

            //マウスの移動量に変換する
            int step =10;
            int x = (int)(now_vec.X * 10) * step;
            int y = (int)(now_vec.Y * 10) * step;
            MouseWinAPI.SendMouseMove(SssimWnd, 300 + x, 300 + y);
            MouseWinAPI.SendLeftButtonUp(SssimWnd, 300 + x, 300 + y);


            label1.Content = string.Format("X={0} Y={1}", x, y);
            //label2.Content = string.Format("X={0} Y={1} Z={2}", right_p.X, right_p.Y, right_p.Z);
            //label3.Content = string.Format("X={0} Y={1} Z={2}", wrist_p.X, wrist_p.Y, wrist_p.Z);

            v3_old = v3;

            #endregion
        }


        private void get_postion2(AllFramesReadyEventArgs e)
        {
            // スケルトンフレームを取得する
            SkeletonFrame skeletonFrame = e.OpenSkeletonFrame();
            if (skeletonFrame == null) return;


            // スケルトンデータを取得する
            Skeleton[] skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
            skeletonFrame.CopySkeletonDataTo(skeletonData);

            //1プレーヤースケルトン
            var skeleton = skeletonData.First();
            if (skeleton.TrackingState != SkeletonTrackingState.Tracked && skeleton.TrackingState != SkeletonTrackingState.PositionOnly) return;


            //両手のポジション取得
            var left_joint = skeleton.Joints.Where(p => p.JointType == JointType.HandLeft).Single();
            var right_joint = skeleton.Joints.Where(p => p.JointType == JointType.HandRight).Single();
            //var right_joint2 = skeleton.Joints.Where(p => p.JointType == JointType.WristRight).Single();
            var right_Elbow = skeleton.Joints.Where(p => p.JointType == JointType.ElbowRight).Single();

            var left_v = new Microsoft.Xna.Framework.Vector3(left_joint.Position.X, left_joint.Position.Y, left_joint.Position.Z);
            var right_v = new Microsoft.Xna.Framework.Vector3(right_joint.Position.X, right_joint.Position.Y, right_joint.Position.Z);
            var right_Elbow_v = new Microsoft.Xna.Framework.Vector3(right_Elbow.Position.X, right_Elbow.Position.Y, right_Elbow.Position.Z);

            //両手の距離
            Microsoft.Xna.Framework.Vector3 vec1 = left_v - right_v;
            float len = vec1.Length();

            if (len_back == 0) { len_back = len; return; }

            int step;
            #region 拡大・縮小
            step = 10;
            if (len_back > len)
            {
                MouseWinAPI.SendRightButtonDown(SssimWnd, 1, 1);
                MouseWinAPI.SendMouseMove(SssimWnd, 1, step);
                MouseWinAPI.SendRightButtonUp(SssimWnd, 1, step);
            }
            else
            {
                MouseWinAPI.SendRightButtonDown(SssimWnd, 1, step);
                MouseWinAPI.SendMouseMove(SssimWnd, 1, 1);
                MouseWinAPI.SendRightButtonUp(SssimWnd, 1, step);
            }
            len_back = len;
            #endregion


            #region 回転
            step = 2;

            //左手、右手、右ひじの、外積を求める
            Microsoft.Xna.Framework.Vector3 v1 = right_v - left_v; //右手位置-左手位置
            Microsoft.Xna.Framework.Vector3 v2 = right_Elbow_v - left_v; //右ひじ置-左手位置
            Microsoft.Xna.Framework.Vector3 v3 = Vector3.Cross(v1, v2);


            //前回ベクトルと、現在ベクトルの差
            Microsoft.Xna.Framework.Vector3 now_vec = v3_old - v3;


            MouseWinAPI.SendLeftButtonDown(SssimWnd, 100, 100);

            int x = (int)(now_vec.X * 1000) * step;
            int y = (int)(now_vec.Y * 1000) * step;
            MouseWinAPI.SendMouseMove(SssimWnd, 100 + x, 100 + y);
            MouseWinAPI.SendLeftButtonUp(SssimWnd, 100 + x, 100 + y);

            label1.Content = "X=" + x;
            label2.Content = "Y=" + y;

            v3_old = v3;

            #endregion

        }




        private void ShowSkeleton(AllFramesReadyEventArgs e)
        {
            // キャンバスをクリアする
            canvas1.Children.Clear();

            // スケルトンフレームを取得する
            SkeletonFrame skeletonFrame = e.OpenSkeletonFrame();
            if (skeletonFrame != null)
            {
                // スケルトンデータを取得する
                Skeleton[] skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                skeletonFrame.CopySkeletonDataTo(skeletonData);

                // プレーヤーごとのスケルトンを描画する
                foreach (var skeleton in skeletonData)
                {
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                    {


                        // 骨格を描画する
                        foreach (Joint joint in skeleton.Joints)
                        {

                            var col = System.Windows.Media.Colors.Black;
                            if (joint.JointType == JointType.HandLeft || joint.JointType == JointType.HandRight) col = Colors.Red;


                            // 骨格の座標をカラー座標に変換する
                            ColorImagePoint point = kinect.MapSkeletonPointToColor(joint.Position, kinect.ColorStream.Format);

                            // 円を書く
                            canvas1.Children.Add(new Ellipse()
                            {
                                Margin = new Thickness(point.X, point.Y, 0, 0),
                                Fill = new SolidColorBrush(),
                                Width = 20,
                                Height = 20,
                            });
                        }
                    }
                }
            }
        }




        private void button3_Click(object sender, RoutedEventArgs e)
        {

            SssimWindow.Test_Move(SssimWnd);
        }


    }
}
