using System.Text.Json;
using System.Text.Json.Serialization;

#nullable disable

namespace Monarchs_Project;

public class Program
{
    private static string Url =
        "https://gist.githubusercontent.com/christianpanton/10d65ccef9f29de3acd49d97ed423736/raw/b09563bc0c4b318132c7a738e679d4f984ef0048/kings";

    public static void Main()
    {
        ShowResult();
        Console.ReadKey();
    }

    private static void ShowResult()
    {
        //Preferably this client should be injected but as im in static context ill use with new.
        var httpClient = new HttpClient();

        //using awaiter and result as im in static context. prefered way is using await in async Task
        var response = httpClient.GetAsync(Url).GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult();
        var monarchsData = JsonSerializer.Deserialize<List<Monarch>>(response);

        //For ruling out null reference im using pattern matching
        if (monarchsData is not { } monarchs) throw new Exception("Cannot deserialize monarchs data");

        var longestMonarch = GetLongestRuledMonarch(monarchs);
        var longestRulingHouse = GetLongestRuledHouse(monarchs);
        Console.WriteLine($"Total Number of Monarchs: {monarchs.Count}\n" +
                          $"Longest Monarch: {longestMonarch.name} For {longestMonarch.duration} Years\n" +
                          $"Longest Ruling House: {longestRulingHouse.House} For Years {longestRulingHouse.Duration}\n" +
                          $"Most Common First Name: {GetMostCommonFirstName(monarchs)}");

    }

    /// <summary>
    /// Get longest monarch
    /// </summary>
    /// <param name="monarchs">List of monarchs</param>
    /// <returns>Name and duration</returns>
    /// <exception cref="Exception"></exception>
    private static (string name, int duration) GetLongestRuledMonarch(IEnumerable<Monarch> monarchs)
    {
        var longestMonarchMax = monarchs.MaxBy(x => x.Duration);
        if (longestMonarchMax is not { } longestMonarch)
        {
            throw new Exception("Cannot get longest monarch");
        }
        return (longestMonarch.Name, longestMonarch.Duration);
    }

    /// <summary>
    /// Get longest ruling house
    /// </summary>
    /// <param name="monarchs">List of monarchs</param>
    /// <returns>House and duration</returns>
    private static (string House, int Duration) GetLongestRuledHouse(IEnumerable<Monarch> monarchs)
    {
        var result = monarchs.GroupBy(x => x.House)
            .OrderByDescending(x => x.Count());
        var (house, duration) = (string.Empty, -1);
        foreach (var item in result)
        {
            var totalDuration = item.Sum(x => x.Duration);
            if (totalDuration <= duration) continue;
            house = item.Key;
            duration = totalDuration;
        }
        return (house, duration);

    }

    /// <summary>
    /// Get most common first name from list
    /// </summary>
    /// <param name="monarchs">List of monarchs</param>
    /// <returns>Most common name</returns>
    private static string GetMostCommonFirstName(List<Monarch> monarchs)
    {
        var names = new List<string>();
        monarchs.ForEach(x =>
        {
            var name = x.Name.Split(" ");
            names.Add(name.First());
        });
        return names.GroupBy(x => x).OrderByDescending(x => x.Count()).First().Key;
    }
}

public class Monarch
{
    [JsonPropertyName("nm")]
    public string Name { get; init; }
    [JsonPropertyName("hse")]
    public string House { get; init; }
    [JsonPropertyName("yrs")]
    public string Years { get; init; }
    public int Duration
    {
        get
        {
            var row = Years.Split("-");
            return row.Length switch
            {
                1 => 0,
                2 when string.IsNullOrEmpty(row[1]) => DateTime.UtcNow.Year - Convert.ToInt32(row[0]),
                _ => Convert.ToInt32(row[1]) - Convert.ToInt32(row[0])
            };
        }
    }
}
