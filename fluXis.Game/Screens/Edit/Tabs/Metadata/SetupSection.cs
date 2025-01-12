using System;
using fluXis.Game.Graphics.Sprites;
using fluXis.Game.Graphics.UserInterface.Color;
using fluXis.Game.Graphics.UserInterface.Text;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace fluXis.Game.Screens.Edit.Tabs.Metadata;

public partial class SetupSection : FillFlowContainer
{
    public SetupSection(string title)
    {
        Direction = FillDirection.Vertical;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Padding = new MarginPadding { Horizontal = 160 };

        Add(new MetadataSectionHeader(title));
    }
}

public partial class MetadataSectionHeader : FluXisSpriteText
{
    public MetadataSectionHeader(string text)
    {
        Text = text;
        Shadow = true;
        FontSize = 32;
        Margin = new MarginPadding { Vertical = 10 };
    }
}

public partial class SetupTextBox : Container
{
    private readonly FluXisTextBox textBox;

    public string Text
    {
        get => textBox.Text;
        set => textBox.Text = value;
    }

    public Action OnTextChanged { set => textBox.OnTextChanged = value; }

    public SetupTextBox(string name)
    {
        RelativeSizeAxes = Axes.X;
        Height = 40;

        Children = new Drawable[]
        {
            new FluXisSpriteText
            {
                Text = name,
                FontSize = 24,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreRight,
                X = 90
            },
            new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = 30,
                Margin = new MarginPadding { Top = 5 },
                Child = textBox = new FluXisTextBox
                {
                    RelativeSizeAxes = Axes.Both,
                    BackgroundInactive = FluXisColors.Background3,
                    BackgroundActive = FluXisColors.Background4
                },
                Padding = new MarginPadding { Left = 100 }
            }
        };
    }
}
