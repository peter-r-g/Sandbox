namespace SandboxGame.UI;

public class CategorySplit : SplitBase
{
	public CategorySplit()
	{
		StyleSheet.Load( "/ui/spawnmenu/library/splitters/CategorySplit.scss" );
		BindClass( "bodyHidden", () => CurrentButton == null || CurrentButton.GetPanel() is null );
	}
}
