using System;
using fluXis.Game.Map;

namespace fluXis.Game.Screens.Edit.Tabs.Charting.Placement;

public partial class NotePlacementBlueprint : PlacementBlueprint
{
    protected NotePlacementBlueprint()
        : base(new HitObjectInfo())
    {
    }

    public override void UpdatePlacement(float time, int lane)
    {
        base.UpdatePlacement(time, lane);
        ((HitObjectInfo)Object).Lane = Math.Clamp(lane, 1, EditorValues.MapInfo.KeyCount);
    }

    public override void OnPlacementFinished(bool commit)
    {
        if (commit) EditorValues.MapInfo.Add((HitObjectInfo)Object);
    }
}
