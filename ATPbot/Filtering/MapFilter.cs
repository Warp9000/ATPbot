using System.Linq;
using QuaverWebApi.v1.Structures;
using QuaverWebApi.v1.Structures.Enums;

namespace ATPbot.Filtering;

public class MapFilter
{
    [FilterParam("--tags", "Space seperated tags", "-t")]
    public string? tags { get; set; }

    [FilterParam("--mode", "Keys4 or Keys7", "-m")]
    public GameMode? mode { get; set; }

    [FilterParam("--mindiff", "Number with decimals", "-md")]
    public double? mindiff { get; set; }

    [FilterParam("--maxdiff", "Number with decimals", "-xd")]
    public double? maxdiff { get; set; }

    [FilterParam("--minlength", "Seconds", "-ml")]
    public int? minlength { get; set; }

    [FilterParam("--maxlength", "Seconds", "-xl")]
    public int? maxlength { get; set; }

    [FilterParam("--minlns", "Integer 0-100% LNs", "-mln")]
    public int? minlns { get; set; }

    [FilterParam("--maxlns", "Integer 0-100% LNs", "-xln")]
    public int? maxlns { get; set; }

    public bool Matches(Map map)
    {
        if (tags != null)
        {
            var querys = tags.Split(' ');
            foreach (var q in querys)
            {
                if (!map.Tags.Split(',').Any(x => x.ToLower().Contains(q.ToLower())))
                {
                    return false;
                }
            }
        }
        if (mode != null && map.GameMode != mode)
            return false;
        if (mindiff != null && map.DifficultyRating < mindiff)
            return false;
        if (maxdiff != null && map.DifficultyRating > maxdiff)
            return false;
        if (minlength != null && map.Length / 1000 < minlength)
            return false;
        if (maxlength != null && map.Length / 1000 > maxlength)
            return false;
        if (minlns != null && map.CountHitobjectLong / (float)(map.CountHitobjectNormal + map.CountHitobjectLong) * 100 < minlns)
            return false;
        if (maxlns != null && map.CountHitobjectLong / (float)(map.CountHitobjectNormal + map.CountHitobjectLong) * 100 > maxlns)
            return false;
        return true;
    }
}