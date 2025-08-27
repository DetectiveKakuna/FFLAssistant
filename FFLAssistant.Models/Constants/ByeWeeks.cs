using FFLAssistant.Models.Enums;

namespace FFLAssistant.Models.Constants;

public static class ByeWeeks
{
    public static readonly IReadOnlyDictionary<int, IReadOnlyList<Team>> Schedule =
        new Dictionary<int, IReadOnlyList<Team>>
        {
            [5] =
            [
                Team.ATL,
                Team.CHI,
                Team.GB,
                Team.PIT
            ],
            [6] =
            [
                Team.HOU,
                Team.MIN
            ],
            [7] =
            [
                Team.BAL,
                Team.BUF
            ],
            [8] =
            [
                Team.ARI,
                Team.DET,
                Team.JAX,
                Team.LV,
                Team.LAR,
                Team.SEA
            ],
            [9] =
            [
                Team.CLE,
                Team.NYJ,
                Team.PHI,
                Team.TB
            ],
            [10] =
            [
                Team.CIN,
                Team.DAL,
                Team.KC,
                Team.TEN
            ],
            [11] = 
            [
                Team.IND, 
                Team.NO
            ],
            [12] =
            [
                Team.DEN,
                Team.LAC,
                Team.MIA,
                Team.WAS
            ],
            [14] =
            [
                Team.CAR,
                Team.NE,
                Team.NYG,
                Team.SF
            ]
        };
}