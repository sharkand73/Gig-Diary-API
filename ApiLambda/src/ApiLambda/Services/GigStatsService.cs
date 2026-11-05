using ApiLambda.Models;
using ApiLambda.Repositories;

namespace ApiLambda.Services;

public class GigStatsService(IGigRepository repository) : IGigStatsService
{
    private readonly string[] _months =
    [
        "apr", "may", "jun", "jul", "aug", "sep", 
        "oct", "nov", "dec", "jan", "feb", "mar"
    ];

    public async Task<GigStats> GetGigStats(int? year = null)
    {
        var currentFinancialYear = GetCurrentFinancialYear();
        var financialYear = GetFinancialYear(year);
        
        var gigsForTheYear = await repository.GetRange(financialYear.Start, financialYear.End);
        
        var gigsByNumMonth = gigsForTheYear
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
        var gigCount = gigsForTheYear.Count;
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
        var gigsByPayer = gigsForTheYear
            .GroupBy(g => g.Contact)
            .ToDictionary(group => group.Key, group => group.ToList());
        var payerCount = gigsByPayer.Count;
        var earningsByPayer = gigsByPayer
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Sum(g => g.Fee))
            .OrderByDescending(kvp => kvp.Value)
            .ToList();
        var averageFee = gigsForTheYear.Average(g => g.Fee);
        var averageHourlyRate = gigsForTheYear.Average(g => g.HourlyRate);
        
        var partialStats = new GigStats
        {
            GigCount = gigCount,
            MonthlyGigTally = monthlyGigTally,
            EarningsThisYear = earningsThisYear,
            EarningsByMonth = earningsByMonth,
            AverageMonthly = Math.Round(averageMonthly, 2),
            PayerCount = payerCount,
            EarningsByPayer = earningsByPayer,
            AverageFee = Math.Round(averageFee, 2),
            AverageHourlyRate = Math.Round(averageHourlyRate, 2)
        };

        var completedGigsBoundary = DateTime.UtcNow;
        var endOfDay = new TimeOnly(23, 59, 59);
        if (!financialYear.Equals(currentFinancialYear))
        {
            completedGigsBoundary = financialYear.End.ToDateTime(endOfDay);
        }
        
        var completedGigsInYear = gigsForTheYear
            .Where(g => g.LeaveDate < completedGigsBoundary)
            .ToList();
        if (completedGigsInYear.Count == 0)
        {
            return partialStats;
        }
        
        var averagePaymentTime = (decimal?) completedGigsInYear
            .Where(g => g.PaymentTime.HasValue)
            .Average(g => g.PaymentTime);
        var averageBookingToGigTime = (decimal?) completedGigsInYear.Average(g => g.BookingToGigTime);

        var gapsBetweenGigs = new List<double>();
        
        for (var i = 1; i < completedGigsInYear.Count; i++)
        {
            var currentGig = completedGigsInYear[i];
            var previousGig = completedGigsInYear[i - 1];
            gapsBetweenGigs.Add((currentGig.LeaveDate - previousGig.LeaveDate).TotalHours);
        }
        var averageGap = gapsBetweenGigs.Average();
        var longestGap = gapsBetweenGigs.Max();
        
        return partialStats with
        {
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

    private static DateRange GetFinancialYear(int? year)
    {
        if (!year.HasValue)
        {
            return GetCurrentFinancialYear();
        }
        return new DateRange(
            new DateOnly(year.Value - 1, 4, 6), 
            new DateOnly(year.Value, 4, 5)
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