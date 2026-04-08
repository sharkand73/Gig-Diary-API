using Amazon.DynamoDBv2.Model;

namespace ApiLambda.Models;

public record GigStats
{
    public int GigCount { get; init; } = 0;
    public decimal EarningsThisYear { get; init; }
    public required List<KeyValuePair<string, decimal>> EarningsByMonth { get; init; }
    public decimal AverageMonthly { get; init; }
    public required List<KeyValuePair<string, int>> MonthlyGigTally { get; init; }
    public int PayerCount { get; init; }
    public required List<KeyValuePair<string, decimal>> EarningsByPayer { get; init; }

    public decimal AverageFee { get; init; }
    public decimal AverageHourlyRate { get; init; }
    public decimal? AveragePaymentTime { get; init; }
    public decimal AverageBookingToGigTime { get; init; }
    public decimal AverageTimeBetweenGigs { get; init; }
    public decimal LongestTimeBetweenGigs { get; init; }  
}