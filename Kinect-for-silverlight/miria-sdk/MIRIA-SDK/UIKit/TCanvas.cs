/*
|| 
|| MIRIA - http://miria.codeplex.com
|| Copyright (C) 2008-2011 Generoso Martello <generoso@martello.com>
||
|| This program is free software: you can redistribute it and/or modify
|| it under the terms of the GNU General Public License as published by
|| the Free Software Foundation, either version 3 of the License, or
|| (at your option) any later version.
||  
|| This program is distributed in the hope that it will be useful,
|| but WITHOUT ANY WARRANTY; without even the implied warranty of
|| MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
|| GNU General Public License for more details.
|| 
|| You should have received a copy of the GNU General Public License
|| along with this program. If not, see
|| <http://www.gnu.org/licenses/>.
|| 
*/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Interop;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading;
using System.Runtime.CompilerServices;

using System.Collections.Generic;

using MIRIA.Interaction.MultiTouch;
using MIRIA.Gestures;
using MIRIA.Utility;


namespace MIRIA.UIKit
{
    public class TCanvas : System.Windows.Controls.Canvas, Touchable
    {
        public delegate void RotateHandler(object sender, GestureRotateEventArgs e);
        public event RotateHandler Rotate;
        public delegate void TranslateHandler(object sender, GestureTranslateEventArgs e);
        public event TranslateHandler Translate;
        public delegate void ScaleHandler(object sender, GestureScaleEventArgs e);
        public event ScaleHandler Scale;
        public delegate void ReleaseHandler(object sender);
        public event ReleaseHandler Release;
        public delegate void HoldHandler(object sender, GestureHoldEventArgs e);
        public event HoldHandler Hold;
        public delegate void TapHandler(object sender, GestureTapEventArgs e);
        public event TapHandler Tap;
        public delegate void GestureDetectedHandler(object sender, GestureDetectedEventArgs e);
        public event GestureDetectedHandler GestureDetected;

        public delegate void FingerAdddedHandler(object sender, FingerTouchEventArgs e);
        public event FingerAdddedHandler FingerAdded;
        public delegate void FingerRemovedHandler(object sender, FingerTouchEventArgs e);
        public event FingerRemovedHandler FingerRemoved;
        public delegate void FingerUpdatedHandler(object sender, FingerTouchEventArgs e);
        public event FingerUpdatedHandler FingerUpdated;

        private TouchGestures _gesturesinterpreter;
        private Animations.TransformHelper _transformhelper; 

        public TCanvas()
        {
            _gesturesinterpreter = new TouchGestures(this);
            _gesturesinterpreter.Translate += new TouchGestures.GestureTranslateHandler(_gesturesinterpreter_Translate);
            _gesturesinterpreter.Rotate += new TouchGestures.GestureRotateHandler(_gesturesinterpreter_Rotate);
            _gesturesinterpreter.Scale += new TouchGestures.GestureScaleHandler(_gesturesinterpreter_Scale);
            _gesturesinterpreter.Release += new TouchGestures.GestureRelease(_gesturesinterpreter_Release);
            _gesturesinterpreter.Hold += new TouchGestures.GestureHold(_gesturesinterpreter_Hold);
            _gesturesinterpreter.Tap += new TouchGestures.GestureTapHandler(_gesturesinterpreter_Tap);
            _gesturesinterpreter.GestureDetected += new TouchGestures.GestureDetectedHandler(_gesturesinterpreter_GestureDetected);
            _transformhelper = new Animations.TransformHelper(this);
            _transformhelper.Delay = 2.5;
        }

        public Animations.TransformHelper TransformHelper
        {
            get { return _transformhelper; }
        }

        public TouchGestures GesturesInterpreter
        {
            get { return _gesturesinterpreter; }
        }


        public void FingerDown(object sender, FingerTouchEventArgs e)
        {
            //_gesturesinterpreter.FingerUpdate(hid, p);
            if (FingerAdded != null) FingerAdded(this, e);
        }
        public void FingerMove(object sender, FingerTouchEventArgs e)
        {
            int fc = _gesturesinterpreter.Fingers.Count;
            _gesturesinterpreter.FingerUpdate(e.Finger.Identifier, e.Finger.Position);
            if (FingerUpdated != null) FingerUpdated(this, e);
        }
        public void FingerUp(object sender, FingerTouchEventArgs e)
        {
            if (FingerRemoved != null) FingerRemoved(this, e);
            _gesturesinterpreter.FingerRemove(e.Finger.Identifier);
        }

        void _gesturesinterpreter_GestureDetected(object sender, GestureDetectedEventArgs e)
        {
            if (GestureDetected != null) GestureDetected(this, e);
        }

        void _gesturesinterpreter_Tap(object sender, GestureTapEventArgs e)
        {
            if (Tap != null) Tap(this, e);
        }

        void _gesturesinterpreter_Release(object sender)
        {
            if (Release != null) Release(this);
        }

        void _gesturesinterpreter_Hold(object sender, GestureHoldEventArgs e)
        {
            if (Hold != null) Hold(this, e);
        }

        void _gesturesinterpreter_Scale(object sender, GestureScaleEventArgs e)
        {
            if (Scale != null) Scale(this, e);
        }

        void _gesturesinterpreter_Rotate(object sender, GestureRotateEventArgs e)
        {
            if (Rotate != null) Rotate(this, e);
        }

        void _gesturesinterpreter_Translate(object sender, GestureTranslateEventArgs e)
        {
            if (Translate != null) Translate(this, e);
        }


    }
}
