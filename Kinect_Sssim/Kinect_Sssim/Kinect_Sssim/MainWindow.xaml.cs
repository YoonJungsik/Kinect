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


                kinect.ElevationAngle = 5;
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
                //MessageBox.Show("太陽系シミュレータースタジオを起動してください");
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
        private Skeleton tracking_skeleton;

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

            if (haveSkeletonData == false) return;
            using (DepthImageFrame depthImageFrame = e.OpenDepthImageFrame())
            {
                if (depthImageFrame != null)
                {
                    //Skeleton skeleton = this.skeletonData.Where(p => p.TrackingState == SkeletonTrackingState.Tracked).FirstOrDefault();
                    //tracking(skeleton);

                    if (tracking_skeleton != null)
                    {
                        //tracking(tracking_skeleton);
                        get_postion2(tracking_skeleton);
                    }
                    if (tracking_skeleton == null)
                    {
                        foreach (Skeleton skeleton in this.skeletonData)
                        {
                            if (IsReadyHnad(skeleton))
                            {
                                tracking_skeleton = skeleton;
                                init_tracking(tracking_skeleton);
                                label4.Content = "開始";
                            }
                        }
                    }

                    if (tracking_skeleton == null) return;

                    //解除のため固定しているか調べる
                    if (IsReadyHnad(tracking_skeleton))
                    {
                        tracking_skeleton = null;
                        label4.Content = "解除";
                    }
                }
            }
        }



        #region 両手の準備ができているか調べる
        Microsoft.Xna.Framework.Vector3 left_v_old;
        Microsoft.Xna.Framework.Vector3 right_v_old;
        private DateTime starttime;
        private int kotei_time =2; //2秒固定したらトラッキング開始・解除


        //両手の準備ができているか調べる
        private bool IsReadyHnad(Skeleton skeleton)
        {


            if (skeleton == null) return false;
            if (skeleton.TrackingState == SkeletonTrackingState.NotTracked) return false;

            var left_joint = skeleton.Joints[JointType.HandLeft]; //右手位置取得
            var right_joint = skeleton.Joints[JointType.HandRight];//左手位置取得


            // 骨格の座標をカラー座標に変換する
            var left_v = new Microsoft.Xna.Framework.Vector3(left_joint.Position.X, left_joint.Position.Y, left_joint.Position.Z);
            var right_v = new Microsoft.Xna.Framework.Vector3(right_joint.Position.X, right_joint.Position.Y, right_joint.Position.Z);

            var 左右間隔 = right_v - left_v;
            label2.Content = string.Format("左右間隔 ={0}", 左右間隔);


            //古い座標がなかったら作成
            if (left_v_old.Length()==0 && right_v_old.Length()==0)
            {
                label1.Content = string.Format("サーチ中!!");
                left_v_old = left_v;
                right_v_old = right_v;
                starttime = DateTime.Now;
                return false;//今回は何も処理しない
            }

            //手の移動誤差を左右の手の幅を基準とするので、基準値を求める
            float 誤差 = 左右間隔.Length()/20;

            //移動量を求める
            float 移動量_left = (left_v - left_v_old).Length();
            float 移動量_right = (right_v - right_v_old).Length();

            left_v_old = left_v;
            right_v_old = right_v;

            label3.Content = string.Format("左移動量 ={0:0.00000} 左移動量 ={1:0.00000}", 移動量_left, 移動量_right);

            //移動しているか、固定しているか調べる
            if (移動量_left > 誤差 ||  移動量_right > 誤差) {
                starttime = DateTime.Now;
                return false;
            }


            label1.Content = string.Format("SPAN ={0}", DateTime.Now.Subtract(starttime).Seconds);
            //固定して○秒経っていたら固定されたと認識する
            if ( DateTime.Now.Subtract(starttime).Seconds > kotei_time)
            {
                label1.Content = string.Format("Ready!!");
                starttime = DateTime.Now; //リセット
                return true;
            }


            return false;

        }


        #endregion


        #region 初期手の位置を記憶

        Microsoft.Xna.Framework.Vector3 left_v_start;
        Microsoft.Xna.Framework.Vector3 right_v_start;
        Microsoft.Xna.Framework.Vector3 right_left_start; //左→右手方向ベクトル

        private void init_tracking(Skeleton skeleton)
        {
            if (skeleton == null) return;
            var left_joint = skeleton.Joints[JointType.HandLeft]; //右手位置取得
            var right_joint = skeleton.Joints[JointType.HandRight];//左手位置取得

            left_v_start = new Microsoft.Xna.Framework.Vector3(left_joint.Position.X, left_joint.Position.Y, left_joint.Position.Z);
            right_v_start = new Microsoft.Xna.Framework.Vector3(right_joint.Position.X, right_joint.Position.Y, right_joint.Position.Z);
            right_left_start = right_v_start - left_v_start; //右手位置-中点;
        }

        #endregion



        private void tracking(Skeleton skeleton)
        {

            if (skeleton == null) return;

            if (skeleton.TrackingState == SkeletonTrackingState.NotTracked) return;

            var left_joint = skeleton.Joints.Where(p => p.JointType == JointType.HandLeft).Single();
            var right_joint = skeleton.Joints.Where(p => p.JointType == JointType.HandRight).Single();
            //var right_joint2 = skeleton.Joints.Where(p => p.JointType == JointType.WristRight).Single();
            var right_Wrist = skeleton.Joints.Where(p => p.JointType == JointType.WristLeft).Single();

            var left_v = new Microsoft.Xna.Framework.Vector3(left_joint.Position.X, left_joint.Position.Y, left_joint.Position.Z);
            var right_v = new Microsoft.Xna.Framework.Vector3(right_joint.Position.X, right_joint.Position.Y, right_joint.Position.Z);
            var wrist_v = new Microsoft.Xna.Framework.Vector3(right_Wrist.Position.X, right_Wrist.Position.Y, right_Wrist.Position.Z);
            if (left_v.Length() > 0 && right_v.Length() > 0 && wrist_v.Length() > 0)
            {

                //zooming(left_v, right_v, wrist_v);
                //label1.Content = string.Format("X={0} Y={1} Z={2}", right_p.X, right_p.Y, right_p.Z);
                //label2.Content = string.Format("X={0} Y={1} Z={2}", right_p.X, right_p.Y, right_p.Z);
                //label3.Content = string.Format("X={0} Y={1} Z={2}", wrist_p.X, wrist_p.Y, wrist_p.Z);
                rotation(left_v, right_v, wrist_v);
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
            step = 7;
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



        /// <summary>
        /// 回転
        /// </summary>
        /// <param name="left_v"></param>
        /// <param name="right_v"></param>
        /// <param name="right_Elbow_v"></param>
        private void rotation(Microsoft.Xna.Framework.Vector3 left_v, Microsoft.Xna.Framework.Vector3 right_v, Microsoft.Xna.Framework.Vector3 right_Elbow_v)
        {

            var viewer = new Vector2(0);
            viewer.X = 300;
            viewer.Y = 300;


            Microsoft.Xna.Framework.Vector3 mid = (right_v + left_v) / 2; //中点
            Microsoft.Xna.Framework.Vector3 v1 = right_v - left_v; //左→右手方向ベクトル
            


            //Y軸回転（マウス移動方向＝X軸）
            //移動した角度をviewerで割り、今回の移動量を導き出す

            Microsoft.Xna.Framework.Vector3 移動量1 = right_left_start - v1; //前回

            double angle_x = Math.Atan2((double)移動量1.X, (double)移動量1.Z);
            if (angle_x < 1) angle_x = 0;

            double mouse_move_X = viewer.X * (angle_x / Math.PI);


            //X軸回転（マウス移動方向＝Y軸）
            double angle_y = Math.Atan2((double)移動量1.Y, (double)移動量1.Z);
            if (angle_y < 1) angle_y = 0;
            double mouse_move_Y = viewer.Y * (angle_y / Math.PI);


            MouseWinAPI.SendLeftButtonDown(SssimWnd, 300, 300);

            //マウスの移動量に変換する
            int step = 1;
            int x = (int)(mouse_move_X) * step;
            int y = (int)(mouse_move_Y) * step;
            MouseWinAPI.SendMouseMove(SssimWnd, 300 + x, 300 + y);
            MouseWinAPI.SendLeftButtonUp(SssimWnd, 300 + x, 300 + y);



            label2.Content = string.Format("angle_x={0} angle_y={1}", angle_x, angle_y);
            label3.Content = string.Format("移動量1.X={0} 移動量1.Y={1}", 移動量1.X, 移動量1.Y);


            /*
            //基準となるマウス移動量（180度回転させるのに必要となる移動量）
            float rad180 = MathHelper.ToRadians(180);

            var viewer = new Vector2(0);
            viewer.X = 300;
            viewer.Y = 300;

            Microsoft.Xna.Framework.Vector3 mid = (right_v + left_v) / 2; //中点
            Microsoft.Xna.Framework.Vector3 v1 = right_v - mid; //右手位置-中点
            if (v3_old.Length() == 0) { v3_old = v1; return; }

            Microsoft.Xna.Framework.Vector3 移動量1 = v3_old - v1; //前回
            移動量1.Normalize();


            //v1.Normalize();
            var dot = Microsoft.Xna.Framework.Vector3.Dot(v1, v3_old);
            label1.Content = string.Format("dot={0}", dot);
            if (Math.Abs(dot) < 0.08) return;

            var mouse = new Vector2(0);
            var mouse_move = new Vector2(0);

            //var angle = new Microsoft.Xna.Framework.Vector3(0);
            //angle.X = Vector3.

            //Y軸回転（マウス移動方向＝X軸）
            //移動した角度をviewerで割り、今回の移動量を導き出す
            double angle_x = Math.Atan2((double)移動量1.X, (double)移動量1.X);
            double mouse_move_X = viewer.X * (angle_x / Math.PI);


            ////X軸回転（マウス移動方向＝Y軸）
            double angle_y = Math.Atan2((double)移動量1.Y, (double)移動量1.Z);
            double mouse_move_Y = viewer.Y * (angle_y / Math.PI);


            label2.Content = string.Format("angle_x={0} angle_y={1}", angle_x, angle_y);
            label3.Content = string.Format("移動量1.X={0} 移動量1.Y={1}", 移動量1.X, 移動量1.Y);


            ////Z軸回転
            //Microsoft.Xna.Framework.Vector3 v2 = right_Elbow_v - left_v; //右ひじ置-左手位置


            MouseWinAPI.SendLeftButtonDown(SssimWnd, 300, 300);

            //マウスの移動量に変換する
            int step = 1;
            int x = (int)(mouse_move_X) * step;
            int y = (int)(mouse_move_Y ) * step;
            MouseWinAPI.SendMouseMove(SssimWnd, 300 + x, 300 + y);
            MouseWinAPI.SendLeftButtonUp(SssimWnd, 300 + x, 300 + y);


            v3_old = v1;

            */
        }


        private void get_postion2(Skeleton skeleton)
        {
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
