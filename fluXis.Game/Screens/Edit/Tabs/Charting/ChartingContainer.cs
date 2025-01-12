using System;
using System.Collections.Generic;
using System.Linq;
using fluXis.Game.Map;
using fluXis.Game.Overlay.Notifications;
using fluXis.Game.Screens.Edit.Tabs.Charting.Blueprints;
using fluXis.Game.Screens.Edit.Tabs.Charting.Playfield;
using fluXis.Game.Screens.Edit.Tabs.Charting.Tools;
using fluXis.Game.Screens.Edit.Tabs.Charting.Tools.Effects;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osuTK.Input;

namespace fluXis.Game.Screens.Edit.Tabs.Charting;

public partial class ChartingContainer : Container, IKeyBindingHandler<PlatformAction>
{
    public IReadOnlyList<ChartingTool> Tools { get; } = new ChartingTool[]
    {
        new SelectTool(),
        new SingleNoteTool(),
        new LongNoteTool()
    };

    public IReadOnlyList<EffectTool> EffectTools { get; } = new EffectTool[]
    {
        new LaneSwitchTool(),
        new FlashTool()
    };

    public static readonly int[] SNAP_DIVISORS = { 1, 2, 3, 4, 6, 8, 12, 16 };

    [Resolved]
    private EditorClock clock { get; set; }

    [Resolved]
    private EditorValues values { get; set; }

    [Resolved]
    private EditorChangeHandler changeHandler { get; set; }

    [Resolved]
    private Clipboard clipboard { get; set; }

    [Resolved]
    private NotificationManager notifications { get; set; }

    private DependencyContainer dependencies;
    private InputManager inputManager;
    private double scrollAccumulation;

    public EditorPlayfield Playfield { get; private set; }
    public BlueprintContainer BlueprintContainer { get; private set; }
    public IEnumerable<EditorHitObject> HitObjects => Playfield.HitObjectContainer.HitObjects;
    public bool CursorInPlacementArea => Playfield.ReceivePositionalInputAt(inputManager.CurrentState.Mouse.Position);

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        values.Editor.ChartingContainer = this;

        dependencies.Cache(this);
        dependencies.CacheAs(Playfield = new EditorPlayfield());

        InternalChildren = new Drawable[]
        {
            new Container
            {
                Name = "Playfield",
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    Playfield,
                    BlueprintContainer = new BlueprintContainer { ChartingContainer = this }
                }
            },
            new Toolbox.Toolbox()
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        inputManager = GetContainingInputManager();
    }

    protected override bool OnKeyDown(KeyDownEvent e)
    {
        switch (e.Key)
        {
            case >= Key.Number1 and <= Key.Number9 when !e.ControlPressed:
            {
                var index = e.Key - Key.Number1;

                if (index >= Tools.Count) return false;

                BlueprintContainer.CurrentTool = Tools[index];
                return true;
            }

            case >= Key.A and <= Key.Z when !e.ControlPressed:
            {
                var letter = e.Key.ToString();
                var tool = EffectTools.FirstOrDefault(t => t.Letter == letter);

                if (tool == null) return false;

                BlueprintContainer.CurrentTool = tool;
                return true;
            }

            case Key.Space:
            {
                if (clock.IsRunning)
                    clock.Stop();
                else
                    clock.Start();

                return true;
            }

            default:
                return false;
        }
    }

    protected override bool OnScroll(ScrollEvent e)
    {
        var scroll = e.ShiftPressed ? e.ScrollDelta.X : e.ScrollDelta.Y;
        int delta = scroll > 0 ? 1 : -1;

        if (e.ControlPressed)
        {
            values.Zoom += delta * .1f;
            values.Zoom = Math.Clamp(values.Zoom, .5f, 5f);
        }
        else if (e.ShiftPressed)
        {
            var snaps = SNAP_DIVISORS;
            var index = Array.IndexOf(snaps, values.SnapDivisor);
            index += delta;

            if (index < 0)
                index = snaps.Length - 1;
            else if (index >= snaps.Length)
                index = 0;

            values.SnapDivisor = snaps[index];
            changeHandler.SnapDivisorChanged?.Invoke();
        }
        else
        {
            if (scrollAccumulation != 0 && Math.Sign(scrollAccumulation) != delta)
                scrollAccumulation = delta * (1 - Math.Abs(scrollAccumulation));

            scrollAccumulation += e.ScrollDelta.Y;

            while (Math.Abs(scrollAccumulation) >= 1)
            {
                seek(scrollAccumulation < 0 ? 1 : -1);
                scrollAccumulation = scrollAccumulation < 0 ? Math.Min(0, scrollAccumulation + 1) : Math.Max(0, scrollAccumulation - 1);
            }
        }

        return true;
    }

    private void seek(int direction)
    {
        double amount = 1;

        if (clock.IsRunning)
        {
            var tp = values.MapInfo.GetTimingPoint(clock.CurrentTime);
            amount *= 4 * (tp.BPM / 120);
        }

        if (direction < 1)
            clock.SeekBackward(amount);
        else
            clock.SeekForward(amount);
    }

    protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
    {
        return dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
    }

    public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
    {
        switch (e.Action)
        {
            case PlatformAction.Copy:
                Copy();
                return true;

            case PlatformAction.Paste:
                Paste();
                return true;

            case PlatformAction.Cut:
                Copy(true);
                return true;

            case PlatformAction.SelectAll:
                BlueprintContainer.SelectAll();
                return true;
        }

        return false;
    }

    public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e) { }

    public void Copy(bool deleteAfter = false)
    {
        var hits = BlueprintContainer.SelectionHandler.SelectedHitObjects.Select(x => new HitObjectInfo
        {
            Time = x.Time,
            Lane = x.Lane,
            HoldTime = x.HoldTime
        }).ToList();

        if (!hits.Any())
        {
            notifications.SendSmallText("Nothing selected.", FontAwesome.Solid.Times);
            return;
        }

        var minTime = hits.Min(x => x.Time);

        foreach (var hit in hits)
            hit.Time -= minTime;

        var content = new EditorClipboardContent { HitObjects = hits };
        clipboard.SetText(content.ToString() ?? string.Empty);

        if (deleteAfter)
        {
            notifications.SendSmallText($"Cut {content.HitObjects.Count} hit objects.", FontAwesome.Solid.Check);
            BlueprintContainer.SelectionHandler.DeleteSelected();
        }
        else
            notifications.SendSmallText($"Copied {content.HitObjects.Count} hit objects.", FontAwesome.Solid.Check);
    }

    public void Paste()
    {
        var content = EditorClipboardContent.Deserialize(clipboard.GetText());

        if (content == null)
        {
            notifications.SendSmallText("Clipboard is empty.", FontAwesome.Solid.Times);
            return;
        }

        BlueprintContainer.SelectionHandler.DeselectAll();

        foreach (var hitObject in content.HitObjects)
        {
            hitObject.Time += (float)clock.CurrentTime;
            values.MapInfo.Add(hitObject);
        }

        notifications.SendSmallText($"Pasted {content.HitObjects.Count} hit objects.", FontAwesome.Solid.Check);
    }
}
