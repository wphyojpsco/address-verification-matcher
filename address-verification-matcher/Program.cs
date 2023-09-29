using System;
using System.Collections.Generic;
using FuzzySharp;
using Newtonsoft.Json;

public class AddressMatcher
{
    private const double addressLnWeight = 0.6;
    private const double addressCityWeight = 0.2;
    private const double addressParishWeight = 0.2;
    private const double cutoff = 0.8;

    public static List<MatchResult> GetMatches(AddressInput input, List<AddressChoice> addressChoices)
    {
        var matchResults = new List<MatchResult>();

        foreach (var choice in addressChoices)
        {
            var lnScore = Process.ExtractOne(input.address_ln_1, new[] { choice.address_ln }).Score / 100.0;
            var cityScore = Process.ExtractOne(input.address_city, new[] { choice.address_city }).Score / 100.0;
            var parishScore = Process.ExtractOne(input.address_parish, new[] { choice.address_parish }).Score / 100.0;

            var weightedScore = lnScore * addressLnWeight + cityScore * addressCityWeight + parishScore * addressParishWeight;

            if (weightedScore >= cutoff)
            {
                matchResults.Add(new MatchResult
                {
                    Address = $"{choice.address_ln}/{choice.address_city}/{choice.address_parish}",
                    TotalScore = weightedScore,
                    LnScore = lnScore,
                    CityScore = cityScore,
                    ParishScore = parishScore
                });
            }
        }

        return matchResults;
    }
}

public class MatchResult
{
    public string Address { get; set; }
    public double TotalScore { get; set; }
    public double LnScore { get; set; }
    public double CityScore { get; set; }
    public double ParishScore { get; set; }

    public override string ToString()
    {
        return $"{Address} - Total Score: {TotalScore * 100}% (Ln: {LnScore * 100}%, City: {CityScore * 100}%, Parish: {ParishScore * 100}%)";
    }
}

public class AddressInput
{
    public string address_ln_1 { get; set; }
    public string address_city { get; set; }
    public string address_parish { get; set; }
}

public class AddressChoice
{
    public string address_ln { get; set; }
    public string address_city { get; set; }
    public string address_parish { get; set; }
}

public class Program
{
    public static void Main()
    {
        string inputJson = @"{
            ""address_ln_1"": ""garden close"",
            ""address_city"": ""barbican kingston"",
            ""address_parish"": ""standrew""
        }";

        string addressChoicesJson = @"[
            {
                ""address_ln"": ""11 Gorden Close"",
                ""address_city"": ""Roundhill, Montego Bay"",
                ""address_parish"": ""Saint James""
            },
            {
                ""address_ln"": ""Garden Close"",
                ""address_city"": ""Barbican, Kingston"",
                ""address_parish"": ""Saint Andrew""
            },
            {
                ""address_ln"": ""Gordon Ave"",
                ""address_city"": ""Hill City, Kingston"",
                ""address_parish"": ""Saint Andrew""
            },
            {
                ""address_ln"": ""25 Gordon Cl"",
                ""address_city"": ""Portmore"",
                ""address_parish"": ""Saint Catherine""
            },
            {
                ""address_ln"": ""88 Gordon Town"",
                ""address_city"": ""Barbican, Kingston"",
                ""address_parish"": ""Kingston""
            }
        ]";

        var input = JsonConvert.DeserializeObject<AddressInput>(inputJson);
        var addressChoices = JsonConvert.DeserializeObject<List<AddressChoice>>(addressChoicesJson);

        var matchResults = AddressMatcher.GetMatches(input, addressChoices);
        foreach (var result in matchResults)
        {
            Console.WriteLine(result);
        }
    }
}
