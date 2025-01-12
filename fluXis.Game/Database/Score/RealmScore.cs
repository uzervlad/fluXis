using System;
using System.Linq;
using fluXis.Game.Database.Maps;
using fluXis.Game.Online;
using fluXis.Game.Online.API.Users;
using fluXis.Game.Scoring;
using fluXis.Game.Scoring.Enums;
using JetBrains.Annotations;
using Realms;

namespace fluXis.Game.Database.Score;

public class RealmScore : RealmObject
{
    [PrimaryKey]
    public Guid ID { get; set; }

    public int OnlineID { get; set; } = -1;
    public float Accuracy { get; set; }
    public string Grade { get; set; }
    public int Score { get; set; }
    public int MaxCombo { get; set; }
    public int Flawless { get; set; }
    public int Perfect { get; set; }
    public int Great { get; set; }
    public int Alright { get; set; }
    public int Okay { get; set; }
    public int Miss { get; set; }
    public string Mods { get; set; } = string.Empty;
    public Guid MapID { get; set; }
    public DateTimeOffset Date { get; set; }
    public int PlayerID { get; set; }

    [Ignored]
    public ScoreRank Rank
    {
        get => Enum.Parse<ScoreRank>(Grade);
        set => Grade = value.ToString();
    }

    [Ignored]
    public APIUser Player => UserCache.GetUser(PlayerID);

    public RealmScore(RealmMap map)
    {
        ID = Guid.NewGuid();
        Date = DateTimeOffset.Now;
        MapID = map.ID;
    }

    [UsedImplicitly]
    private RealmScore()
    {
    }

    public ScoreInfo ToScoreInfo()
    {
        return new ScoreInfo
        {
            Accuracy = Accuracy,
            Rank = Rank,
            Score = Score,
            Combo = 0,
            MaxCombo = MaxCombo,
            Flawless = Flawless,
            Perfect = Perfect,
            Great = Great,
            Alright = Alright,
            Okay = Okay,
            Miss = Miss,
            HitResults = null,
            MapID = -1,
            MapHash = null,
            Mods = Mods.Split(' ').ToList()
        };
    }
}
