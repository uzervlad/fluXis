using fluXis.Game.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace fluXis.Game.Screens.Gameplay.UI;

public partial class AutoPlayDisplay : Container
{
    public GameplayScreen Screen { get; set; }

    private FluXisSpriteText text;
    private readonly Bindable<bool> autoPlay = new();

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        Y = 80;
        Height = 64;

        Add(text = new FluXisSpriteText
        {
            FontSize = 32,
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            Text = "AutoPlay",
            Alpha = 0
        });

        autoPlay.ValueChanged += onAutoPlayChanged;
    }

    protected override void Update()
    {
        if (Screen.Playfield.Manager.AutoPlay != autoPlay.Value)
            autoPlay.Value = Screen.Playfield.Manager.AutoPlay;
    }

    private void onAutoPlayChanged(ValueChangedEvent<bool> v)
    {
        if (v.NewValue)
            text.FadeIn(400).Then(400).FadeOut(400).Loop();
        else
            text.FadeOut(100);
    }
}
