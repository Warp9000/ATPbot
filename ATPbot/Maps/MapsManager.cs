using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ATPbot.Filtering;
using QuaverWebApi;
using QuaverWebApi.v1.Structures;

namespace ATPbot.Maps;

public class MapsManager
{
    private Wrapper quaverWebApi;
    private List<int> mapsetIds;
    private List<Map> maps;

    public MapsManager(Wrapper quaverWebApi)
    {
        this.quaverWebApi = quaverWebApi;
        mapsetIds = DataManager.Get<List<int>>(this, "mapsetIds") ?? new List<int>();
        maps = DataManager.GetFiles(this, "maps").Select(x => DataManager.GetDirect<Map>(x)).ToList();
        UpdateInBackground();
    }

    ~MapsManager()
    {
        Save();
    }

    public void Save()
    {
        DataManager.Set(this, "mapsetIds", mapsetIds);
        lock (maps)
        {
            foreach (var map in maps)
            {
                DataManager.Set(this, map.Id.ToString(), map, "maps");
            }
        }
    }

    public void UpdateInBackground()
    {
        var t = Task.Run(Update);
        t.ContinueWith((x) => Save());
    }
    public void Update()
    {
        var allIds = quaverWebApi.Endpoints.GetRankedMapsets().Result;
        var missingIds = allIds.Where(x => !mapsetIds.Contains(x)).ToList();
        var newMaps = new List<Map>();
        if (missingIds.Count > 128)
        {
            Parallel.ForEach(missingIds, new ParallelOptions() { MaxDegreeOfParallelism = 32 }, (id) =>
            {
                var mapset = quaverWebApi.Endpoints.GetMapset(id).Result;
                foreach (var map in mapset.Maps)
                {
                    newMaps.Add(quaverWebApi.Endpoints.GetMap(map.Id).Result);
                }
            });
        }
        else
        {
            foreach (var id in missingIds)
            {
                var mapset = quaverWebApi.Endpoints.GetMapset(id).Result;
                foreach (var map in mapset.Maps)
                {
                    newMaps.Add(quaverWebApi.Endpoints.GetMap(map.Id).Result);
                }
            }
        }
        mapsetIds.AddRange(missingIds);
        maps.AddRange(newMaps);
        maps = maps.OrderByDescending(x => x.Id).ToList();
    }

    public Map GetMap(int id)
    {
        return maps.FirstOrDefault(x => x.Id == id);
    }

    public int[] GetMapIds()
    {
        return maps.Select(x => x.Id).ToArray();
    }

    public Map[] GetMaps(MapFilter filter)
    {
        return maps.Where(x => filter.Matches(x)).ToArray();
    }
}