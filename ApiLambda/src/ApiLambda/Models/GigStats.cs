using Amazon.DynamoDBv2.Model;

namespace ApiLambda.Models;

public record GigStats
{
    public required int GigCount { get; init; }
    public required decimal EarningsThisYear { get; init; }
    public required List<KeyValuePair<string, decimal>> EarningsByMonth { get; init; }
    public required decimal AverageMonthly { get; init; }
    public required List<KeyValuePair<string, int>> MonthlyGigTally { get; init; }
    public required int PayerCount { get; init; }
    public required List<KeyValuePair<string, decimal>> EarningsByPayer { get; init; }
    
    public required decimal AverageFee { get; init; }
    public required decimal AverageHourlyRate { get; init; }
    public required decimal? AveragePaymentTime { get; init; }
    public required decimal AverageBookingToGigTime { get; init; }
    public required decimal AverageTimeBetweenGigs { get; init; }
    public required decimal LongestTimeBetweenGigs { get; init; }
}