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

using System.Windows.Browser;

using System.Windows.Threading;

using MIRIA.Animations;

using MIG.Client;
using MIG.Client.Devices.Wii;

namespace BallGame
{
    public partial class Page : UserControl
    {
        private static string basePath = System.IO.Path.GetDirectoryName(HtmlPage.Document.DocumentUri.OriginalString).Replace("\\", "/").Replace("http:/", "http://");

        MIGClient _migclient;
        Remote _wiimotelistener;

        Canvas _cursor;
        TransformHelper _cursortr;

        public Page()
        {
            InitializeComponent();

            // set width / height
            this.Width = Double.Parse(System.Windows.Browser.HtmlPage.Document.Body.GetProperty("clientWidth").ToString());
            this.Height = Double.Parse(System.Windows.Browser.HtmlPage.Document.Body.GetProperty("clientHeight").ToString());

            // we get a reference to the main canvas and add the
            Canvas maincanvas = (Canvas)this.FindName("Main");
            //            myCanvas.Background = new SolidColorBrush(Colors.White);

            // and then we instantiate a new Touch Listener on our main Canvas
            // so that contained objects can receive events from
            // multitouch and/or mouse input
            _wiimotelistener = new Remote(); //maincanvas);
            _wiimotelistener.AccelerationUpdate += new Remote.AccelerationUpdateHandler(_wiimotelistener_AccelerationUpdate);
            //new TouchListener.AccelerationUpdateHandler(multiTouchInput_AccelerationUpdate);
            //
            _migclient = new MIGClient();
            _migclient.ServiceReady += new MIGClient.ServiceReadyHandler(_migclient_ServiceReady);
            _migclient.AddListener("WII0", _wiimotelistener);
            _migclient.Connect();
            // 
            _cursor = (Canvas)maincanvas.FindName("cursor");
            _cursortr = new TransformHelper(_cursor); // _cursor.TransformHelper;
            _cursortr.Delay = 2.0;
            _cursortr.Translate = new Point((this.Width / 2) - 100, (this.Height) - 50);


            Point _balldirection = new Point(3, 6);

            Ellipse _ball = (Ellipse)maincanvas.FindName("ball");
            TransformHelper _balltr = new TransformHelper(_ball);
            _balltr.Delay = 0.0;

            TextBlock _scoreText = (TextBlock)maincanvas.FindName("score");
            _scoreText.Width = this.Width;

            int _score = 0;

            DispatcherTimer _timer = new DispatcherTimer();
            _timer.Tick += (sender2, e2) =>
            {

                _balltr.Translate = new Point(_balltr.Translate.X + _balldirection.X, _balltr.Translate.Y + _balldirection.Y);

                if (_balltr.Translate.X <= 0)
                {
                    _balldirection.X = -_balldirection.X;
                }
                else if (_balltr.Translate.X + 50 >= this.Width)
                {
                    _balldirection.X = -_balldirection.X;
                }
                if (_balltr.Translate.Y <= 0)
                {
                    _balldirection.Y = -_balldirection.Y;
                }
                else if (_balltr.Translate.Y + 50 >= this.Height)
                {
                    //_balldirection.Y = -_balldirection.Y;
                    _balltr.Translate = new Point(_balltr.Translate.X, 1);
                    _score = 0;
                    _balldirection = new Point(5, 5);
                }

                IEnumerable<UIElement> hits = VisualTreeHelper.FindElementsInHostCoordinates(new Point(_balltr.Translate.X + 25, _balltr.Translate.Y + 50), this);
                foreach (UIElement uiel in hits)
                {
                    if (uiel.Equals(_cursor))
                    {
                        _balltr.Translate = new Point(_balltr.Translate.X + _balldirection.X, _balltr.Translate.Y - _balldirection.Y);
                        _balldirection = new Point(_balldirection.X * 1.025, _balldirection.Y * 1.025);
                        _balldirection.Y = -_balldirection.Y;
                        _score += 10;
                    }
                }

                _scoreText.Text = _score + "";

            };
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 5);
            _timer.Start();

        }

        void _migclient_ServiceReady(object sender)
        {
            _wiimotelistener.RequestConnect();
        }

        void _wiimotelistener_AccelerationUpdate(object sender, WiiAccelerationUpdateEventArgs args)
        {
            double x = args.Roll / 10;
            double y = args.Pitch;
            double z = args.Yaw;

            _cursortr.Delay = 0.1;
            
            this.Dispatcher.BeginInvoke(() =>
            {
                Point t = _cursortr.Translate;
                if (t.X + (x * 1.5) > 0 && t.X + (x * 1.5) < this.Width - 200)
                {
                    _cursortr.Translate = new Point(t.X + (x * 1.5), t.Y);
                }
                _cursortr.Angle = x;
            });
        }

    }

}
