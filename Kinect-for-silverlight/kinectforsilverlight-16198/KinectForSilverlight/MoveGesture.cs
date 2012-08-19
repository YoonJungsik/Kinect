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
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Threading;

namespace KinectForSilverlight {

    /// <summary>
    /// Contains gestures for the Kinect.
    /// </summary>
    public class MoveGesture {

        #region events
        /// <summary>
        /// Occurs when a left swipe move has been detected
        /// </summary>
        public static event MoveGestureEventHandler SwipeLeft;

        /// <summary>
        /// Occurs when a right swipe move has been detected
        /// </summary>
        public static event MoveGestureEventHandler SwipeRight;

        /// <summary>
        /// Occurs when a up swipe move has been detected
        /// </summary>
        public static event MoveGestureEventHandler SwipeUp;

        /// <summary>
        /// Occurs when a down swipe move has been detected
        /// </summary>
        public static event MoveGestureEventHandler SwipeDown;
        #endregion

        /// <summary>
        /// Indicates whether the Kinect sensor is running.
        /// </summary>
        public static bool KinectRunning { get; private set; }

        /// <summary>
        /// Occurs when an exception occurs in the Kinect library.
        /// </summary>
        public static event KinectExceptionEventHandler OnKinectException;

        private static KinectSensor sensor;

        private static SwipeDirection horizontalDirection = SwipeDirection.None;
        private static SwipeDirection verticalDirection = SwipeDirection.None;

        private static SkeletonPoint? horizontalStartPoint = null;
        private static SkeletonPoint? verticalStartPoint = null;

        private static List<SkeletonPoint> leftHandPositions;
        private static List<SkeletonPoint> rightHandPositions;

        private static Joint leftHand;
        private static Joint rightHand;
        private static Joint shoulderCenter;

        private static bool leftHandActive;
        private static bool rightHandActive;

        private static int trackingId;

        private static DispatcherTimer horizontalTimer;
        private static DispatcherTimer verticalTimer;

        static MoveGesture() {

            leftHandPositions = new List<SkeletonPoint>();
            rightHandPositions = new List<SkeletonPoint>();

            // Initialize timers.
            // If there is no event fired after 250 milliseconds of setting a starting point,
            // the tick event will reset the parameters.
            Deployment.Current.Dispatcher.BeginInvoke(() => {

                horizontalTimer = new DispatcherTimer();
                horizontalTimer.Interval = new TimeSpan(0, 0, 0, 0, 250); // 250 Milliseconds 
                horizontalTimer.Tick += (o, e) => {
                    horizontalDirection = SwipeDirection.None;
                    horizontalStartPoint = null;
                    horizontalTimer.Stop();
                };

                verticalTimer = new DispatcherTimer();
                verticalTimer.Interval = new TimeSpan(0, 0, 0, 0, 250); // 250 Milliseconds 
                verticalTimer.Tick += (o, e) => {
                    verticalDirection = SwipeDirection.None;
                    verticalStartPoint = null;
                    verticalTimer.Stop();
                };
            });

            sensor = Move.Sensor;

            if (sensor.IsRunning) {
                KinectRunning = true;
                sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(sensor_SkeletonFrameReady);
            }
        }

        private static void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e) {
          
            try {
                if (e.SkeletonFrame == null)
                    return;

                // Get all tracked skeletons.
                var skeletons = (from s in e.SkeletonFrame.Skeletons
                                 where s.TrackingState == SkeletonTrackingState.Tracked
                                 select s).ToArray();

                // Check moves for each tracked skeleton.
                foreach (var skeleton in skeletons)
                    CheckMoves(skeleton);

            } catch (KinectException ex) {
                Deployment.Current.Dispatcher.BeginInvoke(() => {
                    OnKinectException(null, ex.Message);
                });
            }
        }

        private static void CheckMoves(Skeleton skeleton) {

            leftHand = skeleton.Joints[JointType.HandLeft];
            rightHand = skeleton.Joints[JointType.HandRight];
            shoulderCenter = skeleton.Joints[JointType.ShoulderCenter];

            trackingId = skeleton.TrackingID;

            // Check if left hand is active.
            if (shoulderCenter.Position.Z - leftHand.Position.Z < 0.25) {
                leftHandActive = false;
                leftHandPositions.Clear();

            } else if (!rightHandActive) {
                leftHandActive = true;
                leftHandPositions.Add(leftHand.Position);
            }

            // Check if right hand is active.
            if (shoulderCenter.Position.Z - rightHand.Position.Z < 0.25) {
                rightHandActive = false;
                rightHandPositions.Clear();

            } else if (!leftHandActive) {
                rightHandActive = true;
                rightHandPositions.Add(rightHand.Position);
            }

            // When no hands are active, reset all starting points and reset swiping directions.
            if (!leftHandActive && !rightHandActive) {
                horizontalStartPoint = null;
                verticalStartPoint = null;
                horizontalDirection = SwipeDirection.None;
                verticalDirection = SwipeDirection.None;

            } else {

                if (rightHandActive) {
                    // Check for horizontal swipes if the right hand is higher than the spine.
                    if (rightHand.Position.Y > skeleton.Joints[JointType.Spine].Position.Y)
                        CheckHorizontalSwipes(Hand.Right);

                    // Check for vertical swipes.
                    if (SwipeUp != null || SwipeDown != null)
                        CheckVerticalSwipes(Hand.Right);
                }

                if (leftHandActive) {
                    // Check for horizontal swipes if the left hand is higher than the spine.
                    if (leftHand.Position.Y > skeleton.Joints[JointType.Spine].Position.Y)
                        CheckHorizontalSwipes(Hand.Left);

                    // Check for vertical swipes.
                    if (SwipeUp != null || SwipeDown != null)
                        CheckVerticalSwipes(Hand.Left);
                }
            }
        }

        private static void CheckHorizontalSwipes(Hand hand) {

            var lastPoint = hand == Hand.Right ? rightHandPositions.Last() : leftHandPositions.Last();

            // If no starting point is set, set the current hand position.
            if (!horizontalStartPoint.HasValue) {
                horizontalStartPoint = lastPoint;

                Deployment.Current.Dispatcher.BeginInvoke(() => {
                    horizontalTimer.Start();
                });
                return;
            }

            // If the hand moved too much in the vertical direction, reset horizontal parameters.
            if (Math.Abs(lastPoint.Y - horizontalStartPoint.Value.Y) > 0.2) {
                horizontalStartPoint = null;
                horizontalDirection = SwipeDirection.None;
                return;
            }

            // When the current hand position is on the right side of the starting position.
            if (lastPoint.X > horizontalStartPoint.Value.X) {

                // If the current position is already set to the right.
                if (horizontalDirection == SwipeDirection.Right) {

                    // If there is 30 cm between the starting point and the current point, fire event and reset.
                    if (lastPoint.X - horizontalStartPoint.Value.X > 0.3 && SwipeRight != null) {

                        Deployment.Current.Dispatcher.BeginInvoke(() => {
                            SwipeRight(null, new MoveGestureEventArgs(hand, trackingId));
                        });

                        horizontalStartPoint = null;
                        horizontalDirection = SwipeDirection.None;
                    }
                } else {

                    // Initialize for right swipe.
                    horizontalStartPoint = lastPoint;
                    horizontalDirection = SwipeDirection.Right;

                    Deployment.Current.Dispatcher.BeginInvoke(() => {
                        horizontalTimer.Start();
                    });
                }
            } else {

                // Same as the right swipe check, but for the left swipe.
                if (horizontalDirection == SwipeDirection.Left) {

                    if (horizontalStartPoint.Value.X - lastPoint.X > 0.3 && SwipeLeft != null) {

                        Deployment.Current.Dispatcher.BeginInvoke(() => {
                            SwipeLeft(null, new MoveGestureEventArgs(hand, trackingId));
                        });

                        horizontalStartPoint = null;
                        horizontalDirection = SwipeDirection.None;
                    }
                } else {

                    horizontalStartPoint = lastPoint;
                    horizontalDirection = SwipeDirection.Left;

                    Deployment.Current.Dispatcher.BeginInvoke(() => {
                        horizontalTimer.Start();
                    });
                }
            }
        }

        private static void CheckVerticalSwipes(Hand hand) {
            var lastPoint = hand == Hand.Right ? rightHandPositions.Last() : leftHandPositions.Last();

            // If no starting point is set, set the current hand position.
            if (!verticalStartPoint.HasValue) {
                verticalStartPoint = lastPoint;

                Deployment.Current.Dispatcher.BeginInvoke(() => {
                    verticalTimer.Start();
                });

                return;
            }

            // If the hand moved too much in the horizontal direction, reset vertical parameters.
            if (Math.Abs(lastPoint.X - verticalStartPoint.Value.X) > 0.2) {
                verticalStartPoint = null;
                verticalDirection = SwipeDirection.None;
                return;
            }

            // When the current hand position is higher than the starting position.
            if (lastPoint.Y > verticalStartPoint.Value.Y) {

                // If the current position is already set to 'up'.
                if (verticalDirection == SwipeDirection.Up) {

                    // If there is 35 cm between the starting point and the current point, fire event and reset.
                    if (lastPoint.Y - verticalStartPoint.Value.Y > 0.35 && SwipeUp != null) {

                        Deployment.Current.Dispatcher.BeginInvoke(() => {
                            SwipeUp(null, new MoveGestureEventArgs(hand, trackingId));
                        });

                        verticalStartPoint = null;
                        verticalDirection = SwipeDirection.None;
                    }
                } else {

                    // Initialize for right swipe.
                    verticalStartPoint = lastPoint;
                    verticalDirection = SwipeDirection.Up;

                    Deployment.Current.Dispatcher.BeginInvoke(() => {
                        verticalTimer.Start();
                    });
                }
            } else {

                // Same as the 'up' swipe check, but for the 'down' swipe.
                if (verticalDirection == SwipeDirection.Down) {

                    if (verticalStartPoint.Value.Y - lastPoint.Y > 0.35 && SwipeDown != null) {

                        Deployment.Current.Dispatcher.BeginInvoke(() => {
                            SwipeDown(null, new MoveGestureEventArgs(hand, trackingId));
                        });

                        verticalStartPoint = null;
                        verticalDirection = SwipeDirection.None;
                    }
                } else {

                    verticalStartPoint = lastPoint;
                    verticalDirection = SwipeDirection.Down;

                    Deployment.Current.Dispatcher.BeginInvoke(() => {
                        verticalTimer.Start();
                    });
                }
            }
        }
    }

    //Specifies the swiping direction.
    public enum SwipeDirection {

        /// <summary>
        /// Swipe left.
        /// </summary>
        Left,

        /// <summary>
        /// Swipe right.
        /// </summary>
        Right,

        /// <summary>
        /// Swipe up.
        /// </summary>
        Up,

        /// <summary>
        /// Swipe down.
        /// </summary>
        Down,

        /// <summary>
        /// No swiping.
        /// </summary>
        None
    }

    /// <summary>
    /// Specifies the hands.
    /// </summary>
    public enum Hand {

        /// <summary>
        /// Left hand.
        /// </summary>
        Left,

        /// <summary>
        /// Right hand.
        /// </summary>
        Right
    }

    /// <summary>
    /// Provides data for the MoveGesture routed event.
    /// </summary>
    public class MoveGestureEventArgs : EventArgs {

        /// <summary>
        /// The JointType of the the Joint that fired the event.
        /// </summary>
        public Hand Hand { get; private set; }

        /// <summary>
        /// The tracking ID of the skeleton.
        /// </summary>
        public int TrackingID { get; private set; }

        internal MoveGestureEventArgs(Hand hand, int trackingId) {
            Hand = hand;
            TrackingID = trackingId;
        }
    }
}