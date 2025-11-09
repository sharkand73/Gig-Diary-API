using Amazon.DynamoDBv2.Model;

namespace ApiLambda.Models;

public record GigStats
{
    public required int GigCount { get; init; }
    public required decimal EarningsThisYear { get; init; }
    public required List<KeyValuePair<int, decimal>> EarningsByMonth { get; init; }
    public required decimal AverageMonthly { get; init; }
    public required List<KeyValuePair<int, int>> MonthlyGigTally { get; init; }
    public required int PayerCount { get; init; }
    public required List<KeyValuePair<string, decimal>> EarningsByPayer { get; init; }
}