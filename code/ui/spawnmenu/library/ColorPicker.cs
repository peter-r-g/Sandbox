using System;
using Sandbox;
using Sandbox.UI;
using SandboxGame.UI;

namespace SandboxGame.UI
{
	[UseTemplate]
	public class ColorPicker : Panel
	{
		private ColorHsv colorHSV = new(255, 1, 1);
		protected Slider HueSlider;

		private bool IsClicking;
		protected bool IsOpen;
		public Action<Color> OnFinalValue;
		public Action<Color> OnValueChanged;
		protected Slider TransSlider;

		public ColorPicker()
		{
			PickerCursor.BindClass( "active", () => IsClicking );
			BindClass( "open", () => IsOpen );

			HueSlider = SliderPanel.Add.Slider( 0, 360, true );
			HueSlider.AddClass( "hue" );

			HueSlider.OnValueChanged = hue =>
			{
				ColorHSV = ColorHSV.WithHue( hue );
				OnValueChanged?.Invoke( ColorHSV );
			};

			HueSlider.OnFinalValue = hue =>
			{
				ColorHSV = ColorHSV.WithHue( hue );
				OnFinalValue?.Invoke( ColorHSV );
			};

			TransSlider = SliderPanel.Add.Slider( 0, 1, true );
			TransSlider.AddClass( "trans" );

			TransSlider.OnValueChanged = trans =>
			{
				ColorHSV = ColorHSV.WithAlpha( 1 - trans );
				OnValueChanged?.Invoke( ColorHSV );
			};

			TransSlider.OnFinalValue = trans =>
			{
				ColorHSV = ColorHSV.WithAlpha( 1 - trans );
				OnFinalValue?.Invoke( ColorHSV );
			};

			OnValueChanged += clr => CreateEvent( "onValueChanged" );
			OnFinalValue += clr => CreateEvent( "onFinalValue" );
			ColorHSV = Color.White;
		}

		protected Panel ColorPreview { get; set; }
		protected Panel PickerCursor { get; set; }
		protected Panel PickerPanel { get; set; }
		protected Panel SliderPanel { get; set; }
		public string TabText { get; set; } = "⯇";

		public ColorHsv ColorHSV
		{
			get => colorHSV;
			set
			{
				colorHSV = value;
				UpdateUI();
			}
		}

		public void ToggleWindow()
		{
			IsOpen = !IsOpen;
			TabText = IsOpen ? "⯆" : "⯇";
		}

		public void UpdateUI()
		{
			PickerPanel.Style.BackgroundColor = ColorHSV.WithSaturation( 1 ).WithValue( 1 ).WithAlpha( 1 );
			ColorPreview.Style.BackgroundColor = ColorHSV;

			PickerCursor.Style.Left = Length.Percent( ColorHSV.Saturation * 100 );
			PickerCursor.Style.Top = Length.Percent( (1 - ColorHSV.Value) * 100 );
			PickerCursor.Style.BackgroundColor = ColorHSV.WithAlpha( 1 );

			TransSlider.Value = 1 - ColorHSV.Alpha;
			HueSlider.Value = ColorHSV.Hue;

			var hexColor = ColorHSV.WithAlpha( 1 ).ToColor().Hex;
			TransSlider.Style.Set( "background", $"linear-gradient(to top, {hexColor} 0%, rgba({hexColor}, 0) 100%)" );
		}

		public void PickerClicking( bool isClicking )
		{
			IsClicking = isClicking;

			if ( !isClicking )
			{
				OnFinalValue?.Invoke( ColorHSV );
			}
			else
			{
				PickerMove();
			}
		}

		public void PickerMove()
		{
			if ( !IsClicking )
			{
				return;
			}

			Vector2 pickerBounds = new(PickerPanel.Box.Right - PickerPanel.Box.Left,
				PickerPanel.Box.Bottom - PickerPanel.Box.Top);
			var localPos = PickerPanel.ScreenPositionToPanelPosition( Mouse.Position ) / pickerBounds;

			ColorHSV = ColorHSV.WithSaturation( Math.Clamp( localPos.x, 0, 1 ) )
				.WithValue( Math.Clamp( 1 - localPos.y, 0, 1 ) );
		}
	}
}

namespace Sandbox.UI.Construct
{
	public static class ColorPickerCreator
	{
		public static ColorPicker ColorPicker( this PanelCreator self, Action<Color> callback )
		{
			ColorPicker newColorPicker = new();
			newColorPicker.OnFinalValue = callback;
			self.panel.AddChild( newColorPicker );
			return newColorPicker;
		}
	}
}
