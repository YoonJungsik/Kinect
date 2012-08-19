
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Controls;

using MIG.Client.Devices.MultiTouch;

namespace MoonLightTest
{


	public partial class MultitouchControl : UserControl
	{
		private MultitouchTuio multitouch;
		private Dictionary<string, FrameworkElement> touches;
		
		
		double tuio_accelx = 0;
		double tuio_accely = 0;
		double tuio_accelz = 0;
		
		
		public MultitouchControl ()
		{
			this.InitializeComponent ();
		}
		
		
		public void SetMultitouchListener(MultitouchTuio mt)
		{
			touches = new Dictionary<string, FrameworkElement>();
			
			multitouch = mt;
			multitouch.FingerDown += HandleMultitouchFingerDown;
			multitouch.FingerMove += HandleMultitouchFingerMove;
			multitouch.FingerUp += HandleMultitouchFingerUp;
			multitouch.AccelerationUpdate += HandleMultitouchAccelerationUpdate;
		}
		
		
		void HandleMultitouchAccelerationUpdate (object sender, AccelerationUpdateEventArgs e)
		{
			tuio_accelx = e.X;
			tuio_accely = e.Y;
			tuio_accelz = e.Z;
			_updateAccelerometer();
		}


		void HandleMultitouchFingerUp (object sender, FingerInputEventArgs e)
		{
			FrameworkElement t = touches[e.Identifier];
			Surface.Dispatcher.BeginInvoke(delegate(){
				Surface.Children.Remove(t);
			});
			touches.Remove(e.Identifier);
		}

		void HandleMultitouchFingerMove (object sender, FingerInputEventArgs e)
		{
			Point p = new Point( (e.Position.Value.X * this.RenderSize.Width),
												(e.Position.Value.Y * this.RenderSize.Height));
			FrameworkElement t = touches[e.Identifier];
			Surface.Dispatcher.BeginInvoke(delegate(){
				t.SetValue(Canvas.LeftProperty, p.X);
				t.SetValue(Canvas.TopProperty, p.Y);
			});
		}
		
		void HandleMultitouchFingerDown (object sender, FingerInputEventArgs e)
		{
			Ellipse touchaura = new Ellipse();
			touches.Add(e.Identifier, touchaura);
			
			Surface.Dispatcher.BeginInvoke(()=>{
				touchaura.Width = 50D;
				touchaura.Height = 50D;
				touchaura.Stroke = new SolidColorBrush(Colors.White);
				touchaura.StrokeThickness = 4.0;
				touchaura.Fill = new SolidColorBrush(Colors.Yellow);
				touchaura.Opacity = 0.45;
				Surface.Children.Add(touchaura);
			});
		}
				
		private void _updateAccelerometer()
		{
		}
		
	}
}

