namespace TechStudio.MonoTouch.Controls
{
	using System;
	using System.Drawing;
	using System.Linq;
	using System.Collections.Generic;
	
	using MonoTouch.UIKit;
	using MonoTouch.UIKit.Extensions;
	using MonoTouch.CoreGraphics;
	
	public class BarPlot : UIView
	{
		private IDictionary<string, double> data;
		
		private int numberOfBars;
			
		private IList<Bar> bars;
		
		private float maxValue;
		
		private float gapWidth;
		
		private float topLine;
		
		private float baseLine;
		
		private float maxHeight;
		
		private float total;
		
		private float textHeight;
		
		private RectangleF bounds;
				
		public BarPlot (IDictionary<string, double> data, RectangleF bounds) 
		{
			this.data = data;
			
			this.data.ToList().ForEach(d => total += (float)d.Value);
			
			this.gapWidth = bounds.Width / 20;
			
			this.textHeight = this.gapWidth / 2;
			
			this.numberOfBars = data.Count;
			
			this.maxValue = (float)this.data.Select(x => x.Value).Max();
			
			this.bounds = bounds;
			
			this.baseLine = this.bounds.Bottom - this.gapWidth;
			
			this.topLine = this.bounds.Top + this.gapWidth;
			
			this.maxHeight = this.baseLine - this.topLine;
			
			this.DrawBackground ();
		
			this.DrawLines();
			
			this.CreateBars();
		}

		private void DrawBackground ()
		{
			var background = new UIView(bounds) { BackgroundColor = UIColor.FromWhiteAlpha(1, 1f) };
			
			background.Layer.BorderWidth = 1;
			
			background.Layer.BorderColor = UIColor.LightGray.CGColor;
			
			background.Layer.CornerRadius = 3;
			
			this.AddSubview(background);
		}
		
		private void CreateBars()
		{
			
			this.bars = new List<Bar>();
			
			int i = 0;
			
			this.data.ToList().ForEach(x => {
								
				var frame = new RectangleF(this.CalculateBarX(i), this.baseLine, this.CalculateBarWidth(), 0);
				
				var bar = new Bar(x.Value, x.Key, frame, this.total, this.textHeight, this.gapWidth);
				
				this.bars.Add(bar);
				
				this.AddSubview(bar); 
				
				this.AddSubview(bar.Label);
				
				this.AddSubview(bar.Percentage);
				
				i++;
			});		
		}
		
		private void DrawLines()
		{
			
			//base line
			this.AddSubview(new UIView(new RectangleF(this.bounds.Left + this.gapWidth / 2, this.baseLine - 1, this.bounds.Width - this.gapWidth, 2)) { BackgroundColor = UIColor.DarkGray });
						
			this.AddSubview(new UIView(new RectangleF(this.bounds.Left + this.gapWidth / 2, this.baseLine - this.maxHeight / 4, this.bounds.Width - this.gapWidth, 1)){ BackgroundColor = UIColor.LightGray });
		
			this.AddSubview(new UIView(new RectangleF(this.bounds.Left + this.gapWidth / 2, this.baseLine - this.maxHeight / 2, this.bounds.Width - this.gapWidth, 1)){ BackgroundColor = UIColor.LightGray });
		
			this.AddSubview(new UIView(new RectangleF(this.bounds.Left + this.gapWidth / 2, this.baseLine - this.maxHeight * 0.75f, this.bounds.Width - this.gapWidth, 1)){ BackgroundColor = UIColor.LightGray });

		}

		public void RaiseBars ()
		{
			this.bars.ToList().ForEach(b =>
			{
				b.Percentage.Frame = new RectangleF(this.bounds.GetMidX(), this.topLine, 0, 0);
				
				b.Label.Frame = new RectangleF(this.bounds.GetMidX(), this.baseLine, 0, 0);
				
				UIView.BeginAnimations("RaiseBar");
				
				UIView.SetAnimationDuration(0.8);
				
				UIView.SetAnimationCurve(UIViewAnimationCurve.EaseInOut);
				
				var height = this.CalculateBarY(b.Value);
				
				b.BackgroundColor = new UIColor().MsdGreen();
				
				b.Frame = new RectangleF(b.Frame.X, height, b.Frame.Width, this.baseLine - height);
				
				b.Percentage.SizeToFit();
				
				b.Percentage.Alpha = 1;
				
				b.Percentage.Frame = new RectangleF(b.Frame.GetMidX() - b.Percentage.Frame.Width / 2, height - (b.Percentage.Frame.Height / 2) - (this.gapWidth / 2), b.Percentage.Frame.Width, b.Percentage.Frame.Height);
											
				b.Label.Alpha = 1;
				
				b.Label.Frame = b.GetFinalLabelFrame();
				
				UIView.CommitAnimations();
			});
		}
		
		private float CalculateBarY(double val)
		{
			
			if (val == 0)
			{
				return this.baseLine -1;
			}
			
			float Y = this.gapWidth + this.bounds.Top + (((this.maxValue - (float)val) / this.maxValue) * this.maxHeight);
				
			return Y;
		}
		
		private float CalculateBarX(int i)
		{
			
			return (i * this.CalculateBarWidth()) + ((i + 1) * this.gapWidth) + this.bounds.Left;					
		}
		
		private float CalculateBarWidth(){
			
			var width = (this.bounds.Width - (this.gapWidth * (this.numberOfBars + 1))) / this.numberOfBars;
			
			return width;
		}
		
		class Bar : UIView
		{
			private float textHeight;
			
			private float gap;
			
			public double Value {get; set;}
			
			public UILabel Label {get; private set;}
			
			public UILabel Percentage {get; private set;}
			
			public Bar(double val, string text, RectangleF frame, double dataSetTotal, float textHeight, float gap){
				
				this.textHeight = textHeight;
									
				this.Frame = frame;
				
				this.Layer.BorderWidth = 1;
				
				this.Layer.BorderColor = UIColor.DarkGray.CGColor;
				
				this.Layer.CornerRadius = 2;
				
				this.Value = val;
				
				CreateLabel (text);
				
				CreatePercentageLabel ((float)dataSetTotal);
				
				this.BackgroundColor = UIColor.LightGray;
				
				this.Alpha = 0.9f;
				
				this.gap = gap / 2;
			}
			
			public RectangleF GetFinalLabelFrame()
			{
				this.Label.SizeToFit();
				return new RectangleF((this.Frame.Right - this.Frame.Width / 2) - (this.Label.Frame.Width / 2), this.Frame.Bottom + this.gap - this.Label.Frame.Height / 2, this.Label.Frame.Width, this.Label.Frame.Height);
			
			}
			
			private void CreatePercentageLabel (float total)
			{
				this.Percentage = new UILabel() {
					Text = total > 0 ? string.Format("{0}%", (int)((this.Value / total) * 100)) : "0%",
					TextAlignment = UITextAlignment.Center, 
					TextColor = UIColor.DarkGray,
					Font = UIFont.SystemFontOfSize(this.textHeight),
					BackgroundColor = UIColor.Clear,
					ShadowColor = UIColor.White,
					ShadowOffset = new SizeF(0,1),
					Alpha = 0
				};
				
				this.Percentage.SizeToFit();
			}

			private void CreateLabel (string text)
			{
				this.Label = new UILabel {
					Font = UIFont.SystemFontOfSize(this.textHeight),
					Text = text, 
					TextAlignment = 
					UITextAlignment.Center, 
					TextColor = UIColor.DarkGray, 
					BackgroundColor = UIColor.Clear,
					ShadowColor = UIColor.White,
					ShadowOffset = new SizeF(0,1),
					Alpha  = 0
				};
				
				this.Label.SizeToFit();
			}
		}
	}
}