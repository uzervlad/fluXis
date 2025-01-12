using fluXis.Game.Graphics.Containers;
using fluXis.Game.Graphics.UserInterface.Color;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osuTK;

namespace fluXis.Game.Graphics.UserInterface.Menu;

public partial class FluXisMenu : osu.Framework.Graphics.UserInterface.Menu
{
    public FluXisMenu(Direction direction, bool topLevelMenu = false)
        : base(direction, topLevelMenu)
    {
        BackgroundColour = FluXisColors.Background4;
        MaskingContainer.CornerRadius = 10;
        ItemsContainer.Padding = new MarginPadding(5);
    }

    protected override osu.Framework.Graphics.UserInterface.Menu CreateSubMenu() =>
        new FluXisMenu(Direction.Vertical) { Anchor = Direction == Direction.Horizontal ? Anchor.TopLeft : Anchor.TopRight };

    protected override void UpdateSize(Vector2 newSize)
    {
        if (Direction == Direction.Vertical)
        {
            Width = newSize.X + 30;
            this.ResizeHeightTo(newSize.Y, 400, Easing.OutQuint);
        }
        else
        {
            Height = newSize.Y;
            this.ResizeWidthTo(newSize.X + 30, 400, Easing.OutQuint);
        }
    }

    protected override void AnimateClose()
    {
        this.FadeOut(200);
    }

    protected override void AnimateOpen()
    {
        this.FadeIn(200);
    }

    protected override DrawableMenuItem CreateDrawableMenuItem(MenuItem item)
    {
        return new DrawableFluXisMenuItem(item);
    }

    protected override ScrollContainer<Drawable> CreateScrollContainer(Direction direction) => new FluXisScrollContainer(direction) { ScrollbarVisible = false };
}
