using System;
using System.Collections.Generic;
using System.Linq;
using fluXis.Game.Map;
using JetBrains.Annotations;
using osu.Framework.Graphics.Textures;
using Realms;

namespace fluXis.Game.Database.Maps;

public class RealmMapSet : RealmObject
{
    [PrimaryKey]
    public Guid ID { get; set; }

    public int OnlineID { get; set; } = -1;
    public string Cover { get; set; } = "cover.png";
    public IList<RealmMap> Maps { get; } = null!;

    [Ignored]
    public RealmMapMetadata Metadata => Maps.FirstOrDefault()?.Metadata ?? new RealmMapMetadata();

    [Ignored]
    public bool Managed { get; set; }

    [Ignored]
    [CanBeNull]
    public MapResourceProvider Resources { get; set; }

    public RealmMapSet([CanBeNull] List<RealmMap> maps = null)
    {
        ID = Guid.NewGuid();
        Maps = maps ?? new List<RealmMap>();
    }

    [UsedImplicitly]
    public RealmMapSet()
    {
    }

    public virtual Texture GetCover()
    {
        var backgrounds = Resources?.BackgroundStore;
        if (backgrounds == null) return null;

        var texture = backgrounds.Get(GetPathForFile(Cover));
        return texture ?? backgrounds.Get(GetPathForFile(Metadata.Background));
    }

    public string GetPathForFile(string filename) => $"{ID.ToString()}/{filename}";
    public override string ToString() => ID.ToString();

    public void SetStatus(int status)
    {
        foreach (RealmMap map in Maps) map.Status = status;
    }
}
