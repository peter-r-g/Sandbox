using System;
using Sandbox.UI;
using SandboxGame.UI;

namespace SandboxGame.UI
{
	[UseTemplate]
	public class SliderLabeled : Panel
	{
		public Action<float> OnFinalValue;
		public Action<float> OnValueChanged;
		public Slider Slider = new();
		private float SliderStep;

		public SliderLabeled()
		{
			Slider = AddChild<Slider>();

			Slider.OnValueChanged = val =>
			{
				ValueEntry.Text = Value.ToString();
				OnValueChanged?.Invoke( Value );
			};

			Slider.OnFinalValue = val =>
			{
				ValueEntry.Text = Value.ToString();
				OnFinalValue?.Invoke( Value );
			};

			ValueEntry.BindClass( "active", () => ValueEntry.HasActive );
			ValueEntry.Numeric = true;

			ValueEntry.AddEventListener( "onchange", () =>
			{
				if ( float.TryParse( ValueEntry.Text, out var val ) )
				{
					Value = val;
					OnFinalValue?.Invoke( Value );
				}
			} );

			OnFinalValue += val => CreateEvent( "onFinalValue" );
			OnValueChanged += val => CreateEvent( "onValueChanged" );
		}

		public SliderLabeled( string title, float min, float max, float step ) : this()
		{
			SliderStep = step;
			TextName = title;
			Slider.Min = min;
			Slider.Max = max;

			ValueEntry.Text = min.ToString();
			Value = min;
		}

		public TextEntry ValueEntry { get; set; }
		public Panel SliderSpot { get; set; }
		public string TextName { get; set; }

		public float Value
		{
			get => MathF.Floor( Slider.Value / SliderStep ) * SliderStep;
			set
			{
				Slider.Value = value;
				UpdateUI();
			}
		}

		public override void SetProperty( string name, string value )
		{
			switch ( name )
			{
				case "step":
					SliderStep = float.Parse( value );
					ValueEntry.Text = Slider.Min.ToString();
					Value = Slider.Min;
					return;
				case "title":
					TextName = value;
					return;
			}

			Slider.SetProperty( name, value );
		}

		protected void UpdateUI()
		{
			ValueEntry.Text = Value.ToString();
		}
	}
}

namespace Sandbox.UI.Construct
{
	public static class LabeledSliderCreator
	{
		public static SliderLabeled SliderLabeled( this PanelCreator self, string title, float min, float max,
			float step = .1f )
		{
			SliderLabeled nSlider = new(title, min, max, step);
			self.panel.AddChild( nSlider );
			return nSlider;
		}
	}
}
