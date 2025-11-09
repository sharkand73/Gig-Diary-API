using ApiLambda.Models;
using ApiLambda.Repositories;

namespace ApiLambda.Services;

public class GigStatsService(IGigRepository repository) : IGigStatsService
{
    private readonly DateRange _financialYear = GetCurrentFinancialYear();
    
    public async Task<GigStats> GetGigStats()
    {
        var gigsThisYear = await repository.GetRange(_financialYear.Start, _financialYear.End);
        var gigsByMonth = gigsThisYear
            .GroupBy(GetGigMonth)
            .ToDictionary(group => group.Key, group => group.ToList());
        var monthlyGigTally = gigsByMonth
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count)
            .OrderBy(kvp => kvp.Key)
            .ToList();
        var gigCount = gigsThisYear.Count;
        var earningsByMonth = gigsByMonth
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value
                .Sum(g => g.Fee))
            .OrderBy(kvp => kvp.Key)
            .ToList();
        var averageMonthly = earningsByMonth
            .Average(kvp => kvp.Value);
        var earningsThisYear = earningsByMonth
            .Sum(kvp => kvp.Value);
        var gigsByPayer = gigsThisYear
            .GroupBy(g => g.Contact)
            .ToDictionary(group => group.Key, group => group.ToList());
        var payerCount = gigsByPayer.Count;
        var earningsByPayer = gigsByPayer
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Sum(g => g.Fee))
            .OrderByDescending(kvp => kvp.Value)
            .ToList();

        return new GigStats
        {
            GigCount = gigCount,
            MonthlyGigTally = monthlyGigTally,
            EarningsThisYear = earningsThisYear,
            EarningsByMonth = earningsByMonth,
            AverageMonthly = Math.Round(averageMonthly, 2),
            PayerCount = payerCount,
            EarningsByPayer = earningsByPayer
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