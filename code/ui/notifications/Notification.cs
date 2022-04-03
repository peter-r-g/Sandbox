using Sandbox;
using Sandbox.UI;

namespace SandboxGame.UI;

[UseTemplate]
public class Notification : Panel
{
	public readonly string Type;
	public object Data;
	public TimeSince Jiggle;
	public TimeSince Lifetime;

	public Notification( string text, float lifetime, string type = "generic", object data = null )
	{
		BindClass( "jiggle", () => Jiggle < 0 );
		Lifetime = -lifetime;
		Text = text;
		Type = type;
		Data = data;
	}

	public string Text { get; set; }

	public override void Tick()
	{
		base.Tick();

		if ( Lifetime >= 0 && !IsDeleting )
		{
			Delete();
		}
	}
}
