using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace SandboxGame.UI;

[UseTemplate]
public class Slider : Panel
{
	private bool IsHolding;
	public float Min, Max;
	public Action<float> OnFinalValue;
	public Action<float> OnValueChanged;

	private float sliderValue;
	private bool Vertical;

	public Slider()
	{
		OnValueChanged += val => CreateEvent( "onValueChanged" );
		OnFinalValue += val => CreateEvent( "onFinalValue" );
		BindClass( "active", () => IsHolding || HasHovered );
		BindClass( "vertical", () => Vertical );
	}

	public Slider( float min, float max, bool vertical = false ) : this()
	{
		Vertical = vertical;
		Max = max;
		Min = min;
	}

	public Panel Grabber { get; set; }


	public float Value
	{
		get => sliderValue;
		set
		{
			sliderValue = value;
			UpdateUI();
		}
	}

	public override void SetProperty( string name, string value )
	{
		switch ( name )
		{
			case "min":
				Min = float.Parse( value );
				return;
			case "max":
				Max = float.Parse( value );
				return;
			case "vertical":
				Vertical = bool.Parse( value );
				return;
		}

		base.SetProperty( name, value );
	}

	protected void UpdateUI()
	{
		var percentage = (Value - Min) / (Max - Min);
		if ( Vertical )
		{
			Grabber.Style.Top = Length.Percent( percentage * 100 );
		}
		else
		{
			Grabber.Style.Left = Length.Percent( percentage * 100 );
		}
	}

	public void MouseClicked( bool isClicking )
	{
		IsHolding = isClicking;

		if ( !isClicking )
		{
			OnFinalValue?.Invoke( Value );
		}
		else
		{
			MouseMoved();
		}
	}

	public void MouseMoved()
	{
		if ( !IsHolding )
		{
			return;
		}

		Vector2 pickerBounds = new(Box.Right - Box.Left, Box.Bottom - Box.Top);
		var localPos = ScreenPositionToPanelPosition( Mouse.Position ) / pickerBounds;
		if ( Vertical )
		{
			Value = Math.Clamp( Min + localPos.y * (Max - Min), Min, Max );
		}
		else
		{
			Value = Math.Clamp( Min + localPos.x * (Max - Min), Min, Max );
		}

		OnValueChanged?.Invoke( Value );
	}
}

public static class SliderCreator
{
	public static Slider Slider( this PanelCreator self, float min, float max, bool vertical = false )
	{
		var nSlider = new Slider( min, max, vertical );
		self.panel.AddChild( nSlider );

		return nSlider;
	}
}
