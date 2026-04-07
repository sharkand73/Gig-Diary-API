using ApiLambda.Models;
using ApiLambda.Repositories;

namespace ApiLambda.Services;

public class GigStatsService(IGigRepository repository) : IGigStatsService
{
    private readonly DateRange _financialYear = GetCurrentFinancialYear();

    private readonly string[] _months =
    [
        "apr", "may", "jun", "jul", "aug", "sep", 
        "oct", "nov", "dec", "jan", "feb", "mar"
    ];

    public async Task<GigStats> GetGigStats()
    {
        var gigsThisYear = await repository.GetRange(_financialYear.Start, _financialYear.End);
        var gigsByNumMonth = gigsThisYear
            .GroupBy(GetGigMonth)
            .ToDictionary(group => group.Key, group => group.ToList());
        var gigTallyByNumMonth = gigsByNumMonth
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
        var monthlyGigTally = new List<KeyValuePair<string, int>>();
        foreach (var i in Enumerable.Range(1, 12))
        {
            if (gigTallyByNumMonth.TryGetValue(i, out var value))
            {
                monthlyGigTally.Add(new KeyValuePair<string, int>(_months[i-1], value));
                continue;
            }
            monthlyGigTally.Add(new KeyValuePair<string, int>(_months[i-1], 0));
        }
        var gigCount = gigsThisYear.Count;
        var earningsByNumMonth = gigsByNumMonth
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value
                .Sum(g => g.Fee));
        var earningsByMonth = new List<KeyValuePair<string, decimal>>();

        foreach (var i in Enumerable.Range(1, 12))
        {
            if (earningsByNumMonth.TryGetValue(i, out var value))
            {
                earningsByMonth.Add(new KeyValuePair<string, decimal>(_months[i-1], value));
                continue;
            }
            earningsByMonth.Add(new KeyValuePair<string, decimal>(_months[i-1], 0));
        }
        var averageMonthly = earningsByNumMonth
            .Average(kvp => kvp.Value);
        var earningsThisYear = earningsByNumMonth
            .Sum(kvp => kvp.Value);
        var gigsByPayer = gigsThisYear
            .GroupBy(g => g.Contact)
            .ToDictionary(group => group.Key, group => group.ToList());
        var payerCount = gigsByPayer.Count;
        var earningsByPayer = gigsByPayer
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Sum(g => g.Fee))
            .OrderByDescending(kvp => kvp.Value)
            .ToList();
        var averageFee = gigsThisYear.Average(g => g.Fee);
        var averageHourlyRate = gigsThisYear.Average(g => g.HourlyRate);
        
        var pastGigsThisYear = gigsThisYear
            .Where(g => g.LeaveDate < DateTime.Now)
            .ToList();
        var averagePaymentTime = (decimal?) pastGigsThisYear
            .Where(g => g.PaymentTime.HasValue)
            .Average(g => g.PaymentTime);
        var averageBookingToGigTime = (decimal?) pastGigsThisYear.Average(g => g.BookingToGigTime);

        var gapsBetweenGigs = new List<double>();
        
        for (var i = 1; i < pastGigsThisYear.Count; i++)
        {
            var currentGig = pastGigsThisYear[i];
            var previousGig = pastGigsThisYear[i - 1];
            gapsBetweenGigs.Add((currentGig.LeaveDate - previousGig.LeaveDate).TotalHours);
        }
        var averageGap = gapsBetweenGigs.Average();
        var longestGap = gapsBetweenGigs.Max();
        
        return new GigStats
        {
            GigCount = gigCount,
            MonthlyGigTally = monthlyGigTally,
            EarningsThisYear = earningsThisYear,
            EarningsByMonth = earningsByMonth,
            AverageMonthly = Math.Round(averageMonthly, 2),
            PayerCount = payerCount,
            EarningsByPayer = earningsByPayer,
            AverageFee = Math.Round(averageFee, 2),
            AverageHourlyRate = Math.Round(averageHourlyRate, 2),
            AveragePaymentTime = averagePaymentTime.HasValue 
                ? Math.Round((decimal)averagePaymentTime, 1)
                : null,
            AverageBookingToGigTime = Math.Round(averageBookingToGigTime?? 0,1),
            AverageTimeBetweenGigs = (decimal) Math.Round(averageGap / 24, 1),
            LongestTimeBetweenGigs = (decimal) Math.Round(longestGap / 24, 1),
        };
    }

    private static DateRange GetCurrentFinancialYear()
    {
        var (year, month, day) = DateOnly.FromDateTime(DateTime.Today);

        var startYear = year;
        if (month < 4 || month == 4 && day < 6)
        {
            startYear = year - 1;
        }
        return new DateRange(
            new DateOnly(startYear, 4, 6), 
            new DateOnly(startYear + 1, 4, 5)
            );
    }

    private static int GetGigMonth(Gig gig)
    {
        var month = gig.LeaveDate.Month;
        var day = gig.LeaveDate.Day;
        if (month != 4 || day >= 6)
        {
            return ((month - 4 + 12) % 12) + 1;
        }
        // 1 - 5 April count as month 12 (March)
        return 12;
    }
}