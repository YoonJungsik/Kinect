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
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;

namespace KinectForSilverlight {

    #region eventHandlers
    public delegate void MoveEventHandler(object sender, MoveEventArgs e);
    public delegate void ScaleEventHandler(object sender, ScaleEventArgs e);
    public delegate void RotateEventHandler(object sender, RotateEventArgs e);
    public delegate void TranslateEventHandler(object sender, TranslateEventArgs e);
    public delegate void TransformEventHandler(object sender, TransformEventArgs e);
    public delegate void PersonEventHandler(object sender, PersonEventArgs e);
    public delegate void KinectExceptionEventHandler(object sender, string message);
    public delegate void MoveGestureEventHandler(object sender, MoveGestureEventArgs e);
    #endregion

    /// <summary>
    /// Contains Move event for the Kinect.
    /// </summary>
    public class Move {

        #region events
        /// <summary>
        /// Occurs when the right hand is moving forward.
        /// </summary>
        public static event MoveEventHandler MoveRightHandDown;

        /// <summary>
        /// Occurs when the right hand is moving back after the MoveRightHandDown event.
        /// </summary>
        public static event MoveEventHandler MoveRightHandUp;

        /// <summary>
        /// Occurs when the right hand is moving in front of the body.
        /// </summary>
        public static event MoveEventHandler MoveRightHandNavigating;

        /// <summary>
        /// Occurs when the right hand is no longer moving in front of the body.
        /// </summary>
        public static event MoveEventHandler MoveRightHandStopNavigating;

        /// <summary>
        /// Occurs when the left hand is moving forward.
        /// </summary>
        public static event MoveEventHandler MoveLeftHandDown;

        /// <summary>
        /// Occurs when the left hand is moving back after the MoveLeftHandDown event.
        /// </summary>
        public static event MoveEventHandler MoveLeftHandUp;

        /// <summary>
        /// Occurs when the left hand is moving in front of the body.
        /// </summary>
        public static event MoveEventHandler MoveLeftHandNavigating;

        /// <summary>
        /// Occurs when the right hand is no longer moving in front of the body.
        /// </summary>
        public static event MoveEventHandler MoveLeftHandStopNavigating;

        /// <summary>
        /// Occurs while MoveLeftHandDown and MoveRightHandDown are active.
        /// </summary>
        public static event ScaleEventHandler Scale;

        /// <summary>
        /// Occurs while MoveLeftHandDown and MoveRightHandDown are active.
        /// </summary>
        public static event RotateEventHandler Rotate;

        /// <summary>
        /// Occurs while MoveLeftHandDown and MoveRightHandDown are active.
        /// </summary>
        public static event TranslateEventHandler Translate;

        /// <summary>
        /// Occurs while MoveLeftHandDown and MoveRightHandDown are active.
        /// </summary>
        public static event TransformEventHandler Transform;

        /// <summary>
        /// Occurs when a person is noticed by the Kinect sensor.
        /// </summary>
        public static event PersonEventHandler OnPersonEntered;

        /// <summary>
        /// Occurs when a person is no longer noticed by the Kinect sensor.
        /// </summary>
        public static event PersonEventHandler OnPersonLeft;

        /// <summary>
        /// Occurs when moving down the right hand while navigating with the left hand.
        /// </summary>
        public static event MoveEventHandler RightNavigatingClick;

        /// <summary>
        /// Occurs when moving down the left hand while navigating with the right hand.
        /// </summary>
        public static event MoveEventHandler LeftNavigatingClick;

        /// <summary>
        /// Occurs when an exception occurs in the Kinect library.
        /// </summary>
        public static event KinectExceptionEventHandler OnKinectException;
        #endregion

        /// <summary>
        /// Indicates whether the Kinect sensor is running.
        /// </summary>
        public static bool KinectRunning { get; private set; }

        internal static KinectSensor Sensor { get; private set; }

        private static TransformSmoothParameters smoothParameters = new TransformSmoothParameters() {
            Smoothing = 0.4f,
            Correction = 0.2f,
            Prediction = 0.4f,
            JitterRadius = 0.7f,
            MaxDeviationRadius = 0.4f
        };

        private static double screenWidth;
        private static double screenHeight;

        private static bool scaling = false;
        private static bool rotating = false;
        private static bool translating = false;

        private static double scaleSize = 0;
        private static double angle = 0;
        private static double translateX = 0;
        private static double translateY = 0;

        private static double previousRightHandDifference = 0;
        private static double rightHandDifference = 0;
        private static double previousLeftHandDifference = 0;
        private static double leftHandDifference = 0;

        private static double xTranslate = 0;
        private static double yTranslate = 0;
        private static int rotationAngle = 0;
        private static double scale = 1;

        private static bool rightActive = false;
        private static bool leftActive = false;

        private static int trackingID;

        private static Skeleton[] trackedSkeletons = new Skeleton[0];

        private static SkeletonPoint shoulderCenter;
        private static SkeletonPoint hipCenter;
        private static SkeletonPoint handRight;
        private static SkeletonPoint handLeft;

        private static MoveEventArgs leftArgs;
        private static MoveEventArgs rightArgs;

        static Move() {

            // Scale screen measurements.
            screenWidth = Application.Current.Host.Content.ActualWidth / 0.6f;
            screenHeight = Application.Current.Host.Content.ActualHeight / 0.65f;

            Application.Current.Host.Content.FullScreenChanged += (sender, e) => {
                screenWidth = Application.Current.Host.Content.ActualWidth / 0.6f;
                screenHeight = Application.Current.Host.Content.ActualHeight / 0.65f;
            };

            Application.Current.Host.Content.Resized += (sender, e) => {
                screenWidth = Application.Current.Host.Content.ActualWidth / 0.6f;
                screenHeight = Application.Current.Host.Content.ActualHeight / 0.65f;
            };

            Application.Current.Exit += new EventHandler(Exit);

            try {
                // Start the kinect sensor.
                if (Sensor == null) {
                    Sensor = KinectSensor.GetKinectSensor();
                    
                    if (!Sensor.IsRunning)
                        Sensor.Start(KinectSensorOptions.UseSkeleton);
                }

                //Start the skeleton stream.
                if (Sensor.IsRunning) {
                    KinectRunning = true;

                    if (!Sensor.SkeletonStream.IsEnabled)
                        Sensor.SkeletonStream.Enable();

                    Sensor.SkeletonStream.SmoothParameters = smoothParameters;

                    Sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);
                }
            } catch (KinectException ex) {

                if (OnKinectException != null)
                    Deployment.Current.Dispatcher.BeginInvoke(() => {
                        OnKinectException(null, ex.Message);
                    });
            }

        }

        private static void Exit(object sender, EventArgs e) {

            if (Sensor.IsRunning)
                Sensor.Stop();
        }

        private static void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e) {

            try {
                if (e.SkeletonFrame == null)
                    return;

                // Get all tracked skeletons.
                var newSkeletons = (from s in e.SkeletonFrame.Skeletons
                                    where s.TrackingState == SkeletonTrackingState.Tracked
                                    select s).ToArray();

                // Get all skeletons that left the frame.
                var leftSkeletons = new List<Skeleton>();
                foreach (var trackedSkeleton in trackedSkeletons) {
                    if (!newSkeletons.Any(n => n.TrackingID == trackedSkeleton.TrackingID))
                        leftSkeletons.Add(trackedSkeleton);
                }

                // Get all skeletons that entered the frame.
                var enteredSkeletons = new List<Skeleton>();
                foreach (var newSkeleton in newSkeletons) {
                    if (!trackedSkeletons.Any(t => t.TrackingID == newSkeleton.TrackingID))
                        enteredSkeletons.Add(newSkeleton);
                }

                // Fire OnPersonLeft event for each skeleton that left the frame.
                if (OnPersonLeft != null)
                    foreach (var skeleton in leftSkeletons)
                        Deployment.Current.Dispatcher.BeginInvoke(() => {
                            OnPersonLeft(Sensor, new PersonEventArgs(skeleton.TrackingID));
                        });

                // Fire OnPersonEntered event for each skeleton that entered the frame.
                if (OnPersonEntered != null)
                    foreach (var skeleton in enteredSkeletons)
                        Deployment.Current.Dispatcher.BeginInvoke(() => {
                            OnPersonEntered(Sensor, new PersonEventArgs(skeleton.TrackingID));
                        });

                trackedSkeletons = newSkeletons;

                // Check moves for each tracked skeleton.
                foreach (var skeleton in newSkeletons)
                    CheckMoves(skeleton);

            } catch (KinectException ex) {

                if (OnKinectException != null)

                    Deployment.Current.Dispatcher.BeginInvoke(() => {
                        OnKinectException(null, ex.Message);
                    });
            }
        }

        private static void CheckMoves(Skeleton skeleton) {

            var positions = skeleton.Joints.Select(j => j.Position).ToArray();
            var newShoulderCenter = positions[(int)JointType.ShoulderCenter];

            if (Math.Abs(newShoulderCenter.X - shoulderCenter.X) > 0.05)
                shoulderCenter = newShoulderCenter;

            hipCenter = positions[(int)JointType.HipCenter];
            handRight = positions[(int)JointType.HandRight];
            handLeft = positions[(int)JointType.HandLeft];

            trackingID = skeleton.TrackingID;

            rightHandDifference = shoulderCenter.Z - handRight.Z;
            leftHandDifference = shoulderCenter.Z - handLeft.Z;

            // Convert the X and Y positions to a position on the screen.
            var leftX = Math.Round(ConvertX(handLeft.X + 0.45f), 0);
            var leftY = Math.Round(ConvertY(handLeft.Y), 0);
            var leftZ = handLeft.Z;

            if (leftX < 0) leftX = 0;
            if (leftY < 0) leftY = 0;

            leftArgs = new MoveEventArgs(trackingID, leftX, leftY, handLeft.Z);

            var rightX = Math.Round(ConvertX(handRight.X + 0.15f), 0);
            var rightY = Math.Round(ConvertY(handRight.Y), 0);
            var rightZ = handRight.Z;

            if (rightX < 0) rightX = 0;
            if (rightY < 0) rightY = 0;

            rightArgs = new MoveEventArgs(trackingID, rightX, rightY, handRight.Z);

            // Check all moves.
            CheckRightHandMove();
            CheckLeftHandMove();

            CheckScale();
            CheckRotate();
            CheckTranslate();

            if (Transform != null)
                CheckTransform();
        }

        private static void CheckTransform() {

            // Fire when both hands are active.
            if (leftHandDifference > 0.3 && rightHandDifference > 0.3)

                Deployment.Current.Dispatcher.BeginInvoke(() => {
                    Transform(null, new TransformEventArgs(trackingID, xTranslate, yTranslate, rotationAngle, scale));
                });
        }

        private static void CheckTranslate() {

            // If both hands are active.
            if (leftHandDifference > 0.3 && rightHandDifference > 0.3) {

                // Initialize translate if needed.
                if (!translating) {
                    translating = true;

                    // Set average X and Y of both hands as reference.
                    translateX = (handLeft.X + handRight.X) / 2;
                    translateY = (handLeft.Y + handRight.Y) / 2;

                } else {

                    // Get the average X and Y of both hands.
                    var newTranslateX = (handLeft.X + handRight.X) / 2;
                    var newTranslateY = (handLeft.Y + handRight.Y) / 2;

                    // Calculate translation.
                    xTranslate = Math.Round((newTranslateX - translateX) * screenWidth, 3);
                    yTranslate = Math.Round((translateY - newTranslateY) * screenHeight, 3);

                    // Fire Translate event.
                    if (Translate != null)
                        Deployment.Current.Dispatcher.BeginInvoke(() => {
                            Translate(null, new TranslateEventArgs(trackingID, xTranslate, yTranslate));
                        });

                    // Set reference.
                    translateX = newTranslateX;
                    translateY = newTranslateY;
                }
            } else if (translating)

                // Stop translating.
                translating = false;
        }

        private static void CheckRotate() {

            // If both hands are active.
            if (leftHandDifference > 0.3 && rightHandDifference > 0.3) {

                // Initialize rotate if needed.
                if (!rotating) {
                    rotating = true;

                    // Set angle reference.
                    angle = Math.Atan2(handRight.Y - handLeft.Y, handRight.X - handLeft.X) * 180 / Math.PI;

                } else {

                    // Get angle.
                    var newAngle = Math.Atan2(handRight.Y - handLeft.Y, handRight.X - handLeft.X) * 180 / Math.PI;

                    // Calculate angle difference.
                    rotationAngle = Convert.ToInt32(angle - newAngle);

                    // Fire Rotate event.
                    if (Rotate != null)
                        Deployment.Current.Dispatcher.BeginInvoke(() => {
                            Rotate(null, new RotateEventArgs(trackingID, rotationAngle));
                        });

                    // Set angle reference.
                    angle = newAngle;
                }
            } else if (rotating)

                // Stop rotating.
                rotating = false;
        }

        private static void CheckScale() {

            // If both hands are active.
            if (leftHandDifference > 0.3 && rightHandDifference > 0.3) {

                // Initialize scale if needed.
                if (!scaling) {
                    scaling = true;

                    // Set scale size as reference.
                    scaleSize = Math.Sqrt((handLeft.X - handRight.X) * (handLeft.X - handRight.X) + (handLeft.Y - handRight.Y) * (handLeft.Y - handRight.Y));

                } else {

                    // Get new scale size.
                    var newScaleSize = Math.Sqrt((handLeft.X - handRight.X) * (handLeft.X - handRight.X) + (handLeft.Y - handRight.Y) * (handLeft.Y - handRight.Y));

                    // Calculate scaling factor.
                    scale = Math.Round(newScaleSize / scaleSize, 2);

                    // Fire Scale event.
                    if (Scale != null)
                        Deployment.Current.Dispatcher.BeginInvoke(() => {
                            Scale(null, new ScaleEventArgs(trackingID, scale));
                        });

                    // Set scale size as reference.
                    scaleSize = newScaleSize;
                }

            } else if (scaling)

                // Stop scaling.
                scaling = false;
        }

        private static void CheckRightHandMove() {

            // If right hand is active.
            if (rightHandDifference > 0.1 && handRight.Y > hipCenter.Y) {

                rightActive = true;

                // Fire MoveRightHandNavigating event.
                if (MoveRightHandNavigating != null)
                    Deployment.Current.Dispatcher.BeginInvoke(() => {
                        MoveRightHandNavigating(null, rightArgs);
                    });

                // If the arm is being stretched.
                if (rightHandDifference > 0.45 && previousRightHandDifference <= 0.45) {

                    // Fire MoveRightHandDown event.
                    if (MoveRightHandDown != null) {
                        Deployment.Current.Dispatcher.BeginInvoke(() => {
                            MoveRightHandDown(null, rightArgs);
                        });
                    }

                    // If the left hand is active.
                    if (leftActive && RightNavigatingClick != null) {

                        // Fire rechtseklik event.
                        Deployment.Current.Dispatcher.BeginInvoke(() => {
                            RightNavigatingClick(null, leftArgs);
                        });
                    }

                } else

                    // If the arm is being pulled back.
                    if (rightHandDifference <= 0.45 && previousRightHandDifference > 0.45)

                        // Fire MoveRightHandUp event.
                        if (MoveRightHandUp != null)
                            Deployment.Current.Dispatcher.BeginInvoke(() => {
                                MoveRightHandUp(null, rightArgs);
                            });

                previousRightHandDifference = shoulderCenter.Z - handRight.Z;

            } else if (rightActive) {

                // Fire MoveRightHandStopNavigating event.
                if (MoveRightHandStopNavigating != null)
                    Deployment.Current.Dispatcher.BeginInvoke(() => {
                        MoveRightHandStopNavigating(null, rightArgs);
                    });

                // Deactivate right hand.
                rightActive = false;
            }
        }

        // Same as CheckRightHandMove.
        private static void CheckLeftHandMove() {

            if (leftHandDifference > 0.1 && handLeft.Y > hipCenter.Y) {

                leftActive = true;

                if (MoveLeftHandNavigating != null)
                    Deployment.Current.Dispatcher.BeginInvoke(() => {
                        MoveLeftHandNavigating(null, leftArgs);
                    });

                if (leftHandDifference > 0.45 && previousLeftHandDifference <= 0.45) {

                    if (MoveLeftHandDown != null)
                        Deployment.Current.Dispatcher.BeginInvoke(() => {
                            MoveLeftHandDown(null, leftArgs);
                        });

                    if (rightActive && LeftNavigatingClick != null)
                        Deployment.Current.Dispatcher.BeginInvoke(() => {
                            LeftNavigatingClick(null, rightArgs);
                        });

                } else if (leftHandDifference <= 0.45 && previousLeftHandDifference > 0.45)

                    if (MoveLeftHandUp != null)
                        Deployment.Current.Dispatcher.BeginInvoke(() => {
                            MoveLeftHandUp(null, leftArgs);
                        });

                previousLeftHandDifference = shoulderCenter.Z - handLeft.Z;

            } else if (leftActive) {

                if (MoveLeftHandStopNavigating != null)
                    Deployment.Current.Dispatcher.BeginInvoke(() => {
                        MoveLeftHandStopNavigating(null, leftArgs);
                    });

                leftActive = false;
            }
        }

        // Convert the X position to a position on the screen.
        private static double ConvertX(float x) {

            var diff = x - shoulderCenter.X;

            if (diff < -0f) diff = 0f;
            else if (diff > 0.6f) diff = 0.6f;

            return screenWidth * diff;
        }

        // Convert the Y position to a position on the screen.
        private static double ConvertY(float y) {

            var diff = y - shoulderCenter.Y - 0.25f;

            if (diff > 0) diff = 0;
            else if (diff < -0.65) diff = -0.65f;

            return Math.Abs(diff * screenHeight);
        }
    }

    /// <summary>
    /// Provides data for the Pinch routed event.
    /// </summary>
    public class ScaleEventArgs : EventArgs {

        /// <summary>
        /// The scaled coordinate.
        /// </summary>
        public double Scale { get; private set; }

        /// <summary>
        /// The tracking ID of the skeleton.
        /// </summary>
        public int TrackingID { get; private set; }

        internal ScaleEventArgs(int trackingID, double scale) {
            Scale = scale;
            TrackingID = trackingID;
        }
    }

    /// <summary>
    /// Provides data for the Move routed event.
    /// </summary>
    public class MoveEventArgs : EventArgs {

        /// <summary>
        /// The X coordinate on the screen.
        /// </summary>
        public double X { get; private set; }

        /// <summary>
        /// The Y coordinate on the screen.
        /// </summary>
        public double Y { get; private set; }

        /// <summary>
        /// The Z coordinate of the joint.
        /// </summary>
        public double Z { get; private set; }

        /// <summary>
        /// The tracking ID of the skeleton.
        /// </summary>
        public int TrackingID { get; private set; }

        internal MoveEventArgs(int trackingID, double x, double y, double z) {
            TrackingID = trackingID;
            X = x;
            Y = y;
            Z = z;
        }
    }

    /// <summary>
    /// Provides data for the Rotate routed event.
    /// </summary>
    public class RotateEventArgs : EventArgs {

        /// <summary>
        /// The number of degrees rotated.
        /// </summary>
        public int Angle { get; private set; }

        /// <summary>
        /// The tracking ID of the skeleton.
        /// </summary>
        public int TrackingID { get; private set; }

        internal RotateEventArgs(int trackingID, int angle) {
            Angle = angle;
            TrackingID = trackingID;
        }
    }

    /// <summary>
    /// Provides data for the Translate routed event.
    /// </summary>
    public class TranslateEventArgs : EventArgs {

        /// <summary>
        /// Distance to translate along the x-axis.
        /// </summary>
        public double TranslateX { get; private set; }

        /// <summary>
        /// Distance to translate along the y-axis.
        /// </summary>
        public double TranslateY { get; private set; }

        /// <summary>
        /// The tracking ID of the skeleton.
        /// </summary>
        public int TrackingID { get; private set; }

        internal TranslateEventArgs(int trackingID, double translateX, double translateY) {
            TranslateX = translateX;
            TranslateY = translateY;
            TrackingID = trackingID;
        }
    }

    /// <summary>
    /// Provides data for the Transform routed event.
    /// </summary>
    public class TransformEventArgs : EventArgs {

        private double transX;
        private double transY;
        private double rotAngle;
        private double scaleX;
        private double scaleY;

        internal TransformEventArgs(int trackingID, double transX, double transY, double rotAngle, double scale) {
            this.transX = transX;
            this.transY = transY;
            this.rotAngle = rotAngle;
            this.scaleX = scale;
            this.scaleY = scale;
            TrackingID = trackingID;
        }

        /// <summary>
        /// Return a Matrix containing all the transformations for the element.
        /// </summary>
        /// <param name="width">The element's width.</param>
        /// <param name="height">The element's height.</param>
        /// <returns>A matrix used for transformations in two-dimensional space.</returns>
        public Matrix GetMatrix(double width, double height) {
            TranslateTransform translate = new TranslateTransform();
            translate.X = transX;
            translate.Y = transY;

            RotateTransform rotate = new RotateTransform();
            rotate.Angle = rotAngle;
            rotate.CenterX = width / 2;
            rotate.CenterY = height / 2;

            ScaleTransform scale = new ScaleTransform();
            scale.ScaleX = scaleX;
            scale.ScaleY = scaleY;
            scale.CenterX = width / 2;
            scale.CenterY = height / 2;

            TransformGroup transformGroup = new TransformGroup();

            transformGroup.Children.Add(scale);
            transformGroup.Children.Add(translate);
            transformGroup.Children.Add(rotate);

            return transformGroup.Value;
        }

        /// <summary>
        /// The tracking ID of the skeleton.
        /// </summary>
        public int TrackingID { get; private set; }
    }

    /// <summary>
    /// Provides data for the OnPersonEntered and OnPersonLeft events.
    /// </summary>
    public class PersonEventArgs : EventArgs {

        /// <summary>
        /// The tracking ID of the skeleton.
        /// </summary>
        public int TrackingID { get; private set; }

        internal PersonEventArgs(int trackingID) {
            TrackingID = trackingID;
        }
    }
}