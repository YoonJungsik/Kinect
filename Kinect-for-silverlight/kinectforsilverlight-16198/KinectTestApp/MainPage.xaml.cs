using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using KinectForSilverlight;

namespace KinectTestApp {

    public partial class MainPage : UserControl {

        private KinectSensor sensor;
        private Brush brush;

        public MainPage() {
            InitializeComponent();

            if (!App.Current.IsRunningOutOfBrowser) {
                notification.Text = "You need to run the application out of browser if you want to work with the Kinect!\nGo to the properties of the KinectTestApp and check 'Enable running application out of browser'. ";

                btnElevationangle.IsEnabled = false;
                start.IsEnabled = false;
                stop.IsEnabled = false;

            } else {

                if (!App.Current.HasElevatedPermissions) {
                    notification.Text = "You need to give elevated trust to the application if you want to work with the Kinect!\nGo to the properties of the KinectTestApp, \nclick on 'Out-of-Browser Settings ...' and check 'Require elevated trust when running outside the browser'.";

                    btnElevationangle.IsEnabled = false;
                    start.IsEnabled = false;
                    stop.IsEnabled = false;
                }

                if (App.Current.InstallState == InstallState.NotInstalled)
                    App.Current.Install();

                else App.Current.CheckAndDownloadUpdateAsync();
            }

            brush = new SolidColorBrush(Colors.Red);
        }

        private void start_Click(object sender, RoutedEventArgs e) {

            //1. Working with the Kinect sensor.
            sensor = KinectSensor.GetKinectSensor();

            sensor.KinectInitializeFailed += new EventHandler<KinectInitializeArgs>(sensor_KinectInitializeFailed);

            if (!sensor.IsRunning)
                sensor.Start(KinectSensorOptions.UseSkeleton | KinectSensorOptions.UseColor | KinectSensorOptions.UseDepth);

            // Not yet working.
            //
            //if (!sensor.ColorStream.IsEnabled)
            //    sensor.ColorStream.Enable(ImageType.Color, ImageResolution.Resolution640x480);
            //sensor.ColorFrameReady += new EventHandler<ImageFrameReadyEventArgs>(sensor_ColorFrameReady);

            //if (!sensor.DepthStream.IsEnabled)
            //    sensor.DepthStream.Enable(ImageType.Depth);
            //sensor.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(sensor_DepthFrameReady);


            if (!sensor.SkeletonStream.IsEnabled)
                sensor.SkeletonStream.Enable();
            sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(sensor_SkeletonFrameReady);


            // 2. Working with events
            if (!Move.KinectRunning) {
                MessageBox.Show("Not running");
                Application.Current.MainWindow.Close();
            }

            // HAND MOVES
            //Move.MoveLeftHandDown += new MoveEventHandler(Move_MoveLeftHandDown);
            //Move.MoveLeftHandUp += new MoveEventHandler(Move_MoveLeftHandUp);
            //Move.MoveLeftHandNavigating += new MoveEventHandler(Move_MoveLeftHandNavigating);
            //Move.MoveRightHandDown += new MoveEventHandler(Move_MoveRightHandDown);
            //Move.MoveRightHandUp += new MoveEventHandler(Move_MoveRightHandUp);
            //Move.MoveRightHandNavigating += new MoveEventHandler(Move_MoveRightHandNavigating);

            // TRANSFORMATIONS
            //Move.Scale += new ScaleEventHandler(Move_ScaleMove);
            //Move.Rotate += new RotateEventHandler(Move_RotateMove);
            //Move.Translate += new TranslateEventHandler(Move_TranslateMove);

            // ALL IN ONE TRANFORMATION
            Move.Transform += new TransformEventHandler(Move_TransformMove);

            // SWIPES
            //MoveGesture.SwipeDown += new MoveGestureEventHandler(MoveGesture_SwipeDown);
            //MoveGesture.SwipeLeft += new MoveGestureEventHandler(MoveGesture_SwipeLeft);
            //MoveGesture.SwipeRight += new MoveGestureEventHandler(MoveGesture_SwipeRight);
            //MoveGesture.SwipeUp += new MoveGestureEventHandler(MoveGesture_SwipeUp);
        }

        private void stop_Click(object sender, RoutedEventArgs e) {

            if (sensor.IsRunning && sensor != null)
                sensor.Stop();
        }

        private void sensor_ColorFrameReady(object sender, ImageFrameReadyEventArgs e) {

            throw new NotImplementedException();
        }

        private void sensor_DepthFrameReady(object sender, ImageFrameReadyEventArgs e) {

            throw new NotImplementedException();
        }

        private void Move_MoveRightHandNavigating(object sender, MoveEventArgs e) {

            Canvas.SetLeft(circle, e.X);
            Canvas.SetTop(circle, e.Y);
        }

        private void Move_MoveRightHandUp(object sender, MoveEventArgs e) {

            circle.Opacity = 0.5;
        }

        private void Move_MoveRightHandDown(object sender, MoveEventArgs e) {

            circle.Opacity = 1;
        }

        private void Move_MoveLeftHandNavigating(object sender, MoveEventArgs e) {

            Canvas.SetLeft(circle, e.X);
            Canvas.SetTop(circle, e.Y);
        }

        private void Move_MoveLeftHandUp(object sender, MoveEventArgs e) {

            circle.Opacity = 0.5;
        }

        private void Move_MoveLeftHandDown(object sender, MoveEventArgs e) {

            circle.Opacity = 1;
        }

        private void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e) {

            Dispatcher.BeginInvoke(() => {
                var skeletons = (from s in e.SkeletonFrame.Skeletons
                                 where s.TrackingState == SkeletonTrackingState.Tracked
                                 select s).ToArray();

                if (skeletons.Length > 0) {

                    skeletonCanvas.Children.Clear();

                    var joints = skeletons[0].Joints;

                    Draw(joints[JointType.Head], joints[JointType.ShoulderCenter]);
                    Draw(joints[JointType.ShoulderCenter], joints[JointType.Spine]);
                    Draw(joints[JointType.Spine], joints[JointType.HipCenter]);
                    Draw(joints[JointType.HipCenter], joints[JointType.HipLeft]);
                    Draw(joints[JointType.HipLeft], joints[JointType.KneeLeft]);
                    Draw(joints[JointType.KneeLeft], joints[JointType.AnkleLeft]);
                    Draw(joints[JointType.AnkleLeft], joints[JointType.FootLeft]);
                    Draw(joints[JointType.HipCenter], joints[JointType.HipRight]);
                    Draw(joints[JointType.HipRight], joints[JointType.KneeRight]);
                    Draw(joints[JointType.KneeRight], joints[JointType.AnkleRight]);
                    Draw(joints[JointType.AnkleRight], joints[JointType.FootRight]);
                    Draw(joints[JointType.ShoulderCenter], joints[JointType.ShoulderLeft]);
                    Draw(joints[JointType.ShoulderLeft], joints[JointType.ElbowLeft]);
                    Draw(joints[JointType.ElbowLeft], joints[JointType.WristLeft]);
                    Draw(joints[JointType.WristLeft], joints[JointType.HandLeft]);
                    Draw(joints[JointType.ShoulderCenter], joints[JointType.ShoulderRight]);
                    Draw(joints[JointType.ShoulderRight], joints[JointType.ElbowRight]);
                    Draw(joints[JointType.ElbowRight], joints[JointType.WristRight]);
                    Draw(joints[JointType.WristRight], joints[JointType.HandRight]);
                }
            });
        }

        private void Draw(Joint joint, Joint joint2) {

            var line = new Line();
            line.X1 = (((((float)skeletonCanvas.ActualWidth) / (skeletonCanvas.ActualWidth / 1000)) / 2f) * joint.Position.X) + (skeletonCanvas.ActualWidth / 2);
            line.Y1 = (((((float)skeletonCanvas.ActualHeight) / (skeletonCanvas.ActualWidth / 1000)) / 2f) * -joint.Position.Y) + (skeletonCanvas.ActualHeight / 2);
            line.X2 = (((((float)skeletonCanvas.ActualWidth) / (skeletonCanvas.ActualWidth / 1000)) / 2f) * joint2.Position.X) + (skeletonCanvas.ActualWidth / 2);
            line.Y2 = (((((float)skeletonCanvas.ActualHeight) / (skeletonCanvas.ActualWidth / 1000)) / 2f) * -joint2.Position.Y) + (skeletonCanvas.ActualHeight / 2);

            line.Stroke = brush;
            line.StrokeThickness = 5;

            skeletonCanvas.Children.Add(line);
        }

        private void sensor_KinectInitializeFailed(object sender, KinectInitializeArgs e) {

            MessageBox.Show(e.Message);
            Application.Current.MainWindow.Close();
        }

        private void MoveGesture_SwipeUp(object sender, MoveGestureEventArgs e) {

            Canvas.SetTop(rectangle, 0);
        }

        private void MoveGesture_SwipeRight(object sender, MoveGestureEventArgs e) {

            Canvas.SetLeft(rectangle, 1800);
        }

        private void MoveGesture_SwipeLeft(object sender, MoveGestureEventArgs e) {

            Canvas.SetLeft(rectangle, 0);
        }

        private void MoveGesture_SwipeDown(object sender, MoveGestureEventArgs e) {

            Canvas.SetTop(rectangle, 900);
        }

        private void Move_TransformMove(object sender, TransformEventArgs e) {

            var matrix = e.GetMatrix(rectangle.ActualWidth, rectangle.ActualHeight);

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(rectangle.RenderTransform);
            transformGroup.Children.Add(new MatrixTransform { Matrix = matrix });

            rectangle.RenderTransform = transformGroup;
        }

        private void Move_TranslateMove(object sender, TranslateEventArgs e) {

            var translateTransform = new TranslateTransform {
                X = e.TranslateX,
                Y = e.TranslateY
            };

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(rectangle.RenderTransform);
            transformGroup.Children.Add(translateTransform);

            rectangle.RenderTransform = transformGroup;
        }

        private void Move_RotateMove(object sender, RotateEventArgs e) {

            var rotateTransform = new RotateTransform {
                Angle = e.Angle,
                CenterX = rectangle.ActualWidth / 2,
                CenterY = rectangle.ActualHeight / 2
            };

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(rectangle.RenderTransform);
            transformGroup.Children.Add(rotateTransform);

            rectangle.RenderTransform = transformGroup;
        }

        private void Move_ScaleMove(object sender, ScaleEventArgs e) {

            var scaleTransform = new ScaleTransform {
                ScaleX = e.Scale,
                ScaleY = e.Scale,
                CenterX = rectangle.ActualWidth / 2,
                CenterY = rectangle.ActualHeight / 2
            };

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(rectangle.RenderTransform);
            transformGroup.Children.Add(scaleTransform);

            rectangle.RenderTransform = transformGroup;
        }

        private void btnElevationangle_Click(object sender, RoutedEventArgs e) {

            if (sensor != null && sensor.IsRunning) {

                int angle = int.Parse(elevationangle.Text);

                if (angle > 27) angle = 27;
                else if (angle < -27) angle = -27;

                sensor.ElevationAngle = angle;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
        }
    }
}
