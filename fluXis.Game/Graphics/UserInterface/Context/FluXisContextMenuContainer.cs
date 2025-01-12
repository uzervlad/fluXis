using osu.Framework.Graphics.Cursor;

namespace fluXis.Game.Graphics.UserInterface.Context;

public partial class FluXisContextMenuContainer : ContextMenuContainer
{
    protected override osu.Framework.Graphics.UserInterface.Menu CreateMenu() => new FluXisContextMenu();
}
