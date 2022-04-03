using System;
using Sandbox.UI;

namespace SandboxGame.UI;

public class FilterTextEntry : TextEntry
{
	public Action<string> OnValueChangedFunc;

	public override void OnValueChanged()
	{
		base.OnValueChanged();

		OnValueChangedFunc?.Invoke( Text );
	}
}
