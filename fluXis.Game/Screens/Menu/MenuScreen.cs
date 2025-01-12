using System.Linq;
using fluXis.Game.Activity;
using fluXis.Game.Audio;
using fluXis.Game.Database.Maps;
using fluXis.Game.Graphics.Background;
using fluXis.Game.Graphics.Containers;
using fluXis.Game.Graphics.Sprites;
using fluXis.Game.Graphics.UserInterface.Panel;
using fluXis.Game.Graphics.UserInterface.Text;
using fluXis.Game.Map;
using fluXis.Game.Online.Fluxel;
using fluXis.Game.Overlay.Login;
using fluXis.Game.Overlay.Settings;
using fluXis.Game.Screens.Browse;
using fluXis.Game.Screens.Edit;
using fluXis.Game.Screens.Menu.UI;
using fluXis.Game.Screens.Menu.UI.NowPlaying;
using fluXis.Game.Screens.Menu.UI.Visualizer;
using fluXis.Game.Screens.Multiplayer;
using fluXis.Game.Screens.Ranking;
using fluXis.Game.Screens.Select;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace fluXis.Game.Screens.Menu;

public partial class MenuScreen : FluXisScreen
{
    public override float Zoom => pressedStart ? 1f : 1.2f;
    public override float BackgroundDim => pressedStart ? base.BackgroundDim : 1f;
    public override bool ShowToolbar => pressedStart;

    [Resolved]
    private MapStore maps { get; set; }

    [Resolved]
    private BackgroundStack backgrounds { get; set; }

    [Resolved]
    private SettingsMenu settings { get; set; }

    [Resolved]
    private LoginOverlay login { get; set; }

    [Resolved]
    private AudioClock clock { get; set; }

    [Resolved]
    private Fluxel fluxel { get; set; }

    [Resolved]
    private ActivityManager activity { get; set; }

    private FluXisTextFlow splashText;
    private FluXisSpriteText pressAnyKeyText;
    private MenuVisualizer visualizer;

    private Container textContainer;
    private Container buttonContainer;
    private FillFlowContainer linkContainer;

    private FluXisSpriteText animationText;
    private CircularContainer animationCircle;

    private MenuPlayButton playButton;

    private bool pressedStart;
    private Sample menuStart;
    private double inactivityTime;
    private const double inactivity_timeout = 60 * 1000;

    [BackgroundDependencyLoader]
    private void load(GameHost host, ISampleStore samples)
    {
        menuStart = samples.Get("UI/accept");

        // load a random map
        if (maps.MapSets.Count > 0)
        {
            maps.CurrentMapSet = maps.GetRandom();

            RealmMap map = maps.CurrentMapSet.Maps.First();
            clock.LoadMap(map, true, true);
        }

        backgrounds.AddBackgroundFromMap(maps.CurrentMapSet?.Maps.First());

        InternalChildren = new Drawable[]
        {
            new ParallaxContainer
            {
                Child = visualizer = new MenuVisualizer(),
                RelativeSizeAxes = Axes.Both,
                Strength = 5
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Padding = new MarginPadding(40),
                Children = new Drawable[]
                {
                    textContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        Children = new Drawable[]
                        {
                            new FluXisSpriteText
                            {
                                Text = "fluXis",
                                FontSize = 100,
                                Shadow = true,
                                ShadowOffset = new Vector2(0, 0.04f)
                            },
                            splashText = new FluXisTextFlow
                            {
                                FontSize = 32,
                                RelativeSizeAxes = Axes.X,
                                Margin = new MarginPadding { Top = 80 },
                                Shadow = true
                            }
                        }
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            animationCircle = new CircularContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Masking = true,
                                BorderColour = Color4.White,
                                BorderThickness = 20,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        AlwaysPresent = true,
                                        Alpha = 0
                                    }
                                }
                            },
                            animationText = new FluXisSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "fluXis",
                                FontSize = 100,
                                Shadow = true,
                                ShadowOffset = new Vector2(0, 0.04f),
                                Alpha = 0
                            }
                        }
                    },
                    pressAnyKeyText = new FluXisSpriteText
                    {
                        Text = "Press any key.",
                        FontSize = 32,
                        Shadow = true,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre
                    },
                    new MenuNowPlaying(),
                    buttonContainer = new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Alpha = 0,
                        Shear = new Vector2(-.2f, 0),
                        Margin = new MarginPadding { Left = 40 },
                        X = -200,
                        Children = new Drawable[]
                        {
                            new MenuButtonBackground { Y = 30 },
                            new MenuButtonBackground { Y = 110 },
                            new MenuButtonBackground { Y = 190 },
                            playButton = new MenuPlayButton
                            {
                                Description = $"{maps.MapSets.Count} maps loaded",
                                Action = continueToPlay,
                                Width = 700
                            },
                            new SmallMenuButton
                            {
                                Icon = FontAwesome.Solid.Cog,
                                Action = settings.ToggleVisibility,
                                Width = 90,
                                Y = 80
                            },
                            new MenuButton
                            {
                                Text = "Multiplayer",
                                Description = "Play against other players",
                                Icon = FontAwesome.Solid.Users,
                                Action = continueToMultiplayer,
                                Width = 290,
                                X = 110,
                                Y = 80
                            },
                            new MenuButton
                            {
                                Text = "Ranking",
                                Description = "Check online leaderboards",
                                Icon = FontAwesome.Solid.Trophy,
                                Action = continueToRankings,
                                Width = 280,
                                X = 420,
                                Y = 80
                            },
                            new SmallMenuButton
                            {
                                Icon = FontAwesome.Solid.Times,
                                Action = Game.Exit,
                                Width = 90,
                                Y = 160
                            },
                            new MenuButton
                            {
                                Text = "Browse",
                                Description = "Download community-made maps",
                                Icon = FontAwesome.Solid.Download,
                                Width = 330,
                                X = 110,
                                Y = 160,
                                Action = continueToBrowse
                            },
                            new MenuButton
                            {
                                Text = "Edit",
                                Description = "Create your own maps",
                                Icon = FontAwesome.Solid.Pen,
                                Action = () => this.Push(new Editor()),
                                Width = 240,
                                X = 460,
                                Y = 160
                            }
                        }
                    },
                    new MenuGamepadTooltips
                    {
                        ButtonContainer = buttonContainer
                    },
                    linkContainer = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(10),
                        Alpha = 0,
                        X = 200,
                        Children = new Drawable[]
                        {
                            new MenuIconButton
                            {
                                Icon = FontAwesome.Brands.Discord,
                                Action = () => host.OpenUrlExternally("https://discord.gg/29hMftpNq9"),
                                Text = "Discord"
                            },
                            new MenuIconButton
                            {
                                Icon = FontAwesome.Brands.Github,
                                Action = () => host.OpenUrlExternally("https://github.com/TeamFluXis/fluXis"),
                                Text = "GitHub"
                            },
                            new MenuIconButton
                            {
                                Icon = FontAwesome.Solid.Globe,
                                Action = () => host.OpenUrlExternally(fluxel.Endpoint.WebsiteRootUrl),
                                Text = "Website"
                            }
                        }
                    }
                }
            }
        };

        maps.MapSetAdded += _ => playButton.Description = $"{maps.MapSets.Count} maps loaded";
    }

    private void continueToPlay() => this.Push(new SelectScreen());
    private void continueToMultiplayer() => this.Push(new MultiplayerScreen());
    private void continueToRankings() => this.Push(new Rankings());
    private void continueToBrowse() => this.Push(new MapBrowser());

    private bool canPlayAnimation()
    {
        if (pressedStart) return false;

        playStartAnimation();
        return true;
    }

    private void playStartAnimation()
    {
        pressedStart = true;
        inactivityTime = 0;
        menuStart?.Play();
        randomizeSplash();
        backgrounds.Zoom = 1f;

        animationText.ScaleTo(1.2f, 800, Easing.OutQuint).FadeOut(600);
        animationCircle.TransformTo(nameof(animationCircle.BorderThickness), 20f).ResizeTo(0)
                       .TransformTo(nameof(animationCircle.BorderThickness), 0f, 1200, Easing.OutQuint).ResizeTo(400, 1000, Easing.OutQuint);

        this.Delay(800).FadeIn().OnComplete(_ =>
        {
            Game.Toolbar.ShowToolbar.Value = true;
            showMenu(1000);
            login.Show();
        });

        pressAnyKeyText.FadeOut(600).MoveToY(200, 800, Easing.InQuint);
    }

    private void revertStartAnimation()
    {
        Game.Toolbar.ShowToolbar.Value = false;
        backgrounds.Zoom = 1.2f;
        hideMenu();

        animationText.Delay(800).ScaleTo(.9f).ScaleTo(1f, 800, Easing.OutQuint).FadeIn(400);
        this.Delay(800).FadeIn().OnComplete(_ => pressedStart = false);

        pressAnyKeyText.Delay(800).MoveToY(0, 800, Easing.OutQuint);
        pressAnyKeyText.FadeInFromZero(1400).Then().FadeOut(1400).Loop();
    }

    protected override bool OnKeyDown(KeyDownEvent e)
    {
        if (e.Key == Key.Escape)
        {
            Game.Overlay ??= new ConfirmExitPanel();
            return true;
        }

        return canPlayAnimation();
    }

    protected override bool OnMouseDown(MouseDownEvent e) => canPlayAnimation();
    protected override bool OnTouchDown(TouchDownEvent e) => canPlayAnimation();
    protected override bool OnMidiDown(MidiDownEvent e) => canPlayAnimation();

    private void showMenu(int duration = 400)
    {
        textContainer.MoveToX(0, duration, Easing.OutQuint).FadeIn(duration / 2f);
        buttonContainer.MoveToX(0, duration, Easing.OutQuint).FadeIn(duration / 2f);
        linkContainer.MoveToX(0, duration, Easing.OutQuint).FadeIn(duration / 2f);
    }

    private void hideMenu(int duration = 400)
    {
        textContainer.MoveToX(-200, duration, Easing.OutQuint).FadeOut(duration / 2f);
        buttonContainer.MoveToX(-200, duration, Easing.OutQuint).FadeOut(duration / 2f);
        linkContainer.MoveToX(200, duration, Easing.OutQuint).FadeOut(duration / 2f);
    }

    private void randomizeSplash() => splashText.Text = MenuSplashes.RandomSplash;

    public override void OnEntering(ScreenTransitionEvent e)
    {
        activity.Update("In the menus", "Idle", "menu");

        pressAnyKeyText.FadeInFromZero(1400).Then().FadeOut(1400).Loop();
        visualizer.FadeInFromZero(2000);
        animationText.FadeInFromZero(500);
        backgrounds.SetDim(1f, 0);
        backgrounds.SetDim(base.BackgroundDim, 2000);
        inactivityTime = 0;
    }

    public override void OnSuspending(ScreenTransitionEvent e)
    {
        this.FadeOut(200);
        hideMenu();
    }

    public override void OnResuming(ScreenTransitionEvent e)
    {
        randomizeSplash();
        showMenu();
        this.FadeIn(200);
        activity.Update("In the menus", "Idle", "menu");
        inactivityTime = 0;
    }

    protected override void Update()
    {
        if (clock.Finished) Game.NextSong();

        inactivityTime += Time.Elapsed;

        if (inactivityTime > inactivity_timeout && pressedStart)
        {
            inactivityTime = 0;
            revertStartAnimation();
        }
    }
}
