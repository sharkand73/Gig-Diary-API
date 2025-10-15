using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;

namespace ApiLambda.Api;

[DynamoDBTable("Gigs")]
public record Gig
{
    [DynamoDBHashKey("pk")]
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("act")]
    public required string Act { get; init; }

    [JsonPropertyName("fee")]
    public decimal Fee { get; init; }

    [JsonPropertyName("leaveDate")]
    public DateTime LeaveDate { get; init; }

    [JsonPropertyName("returnDate")]
    public DateTime ReturnDate { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("venue")]
    public required string Venue { get; init; }

    [JsonPropertyName("postcode")]
    public required string Postcode { get; init; }
    
    [JsonPropertyName("bookingDate")]
    public required DateOnly BookingDate { get; init; }
    
    [JsonPropertyName("contact")]
    public required string Contact { get; init; }

    [JsonPropertyName("instrument")]
    public required string Instrument { get; init; }

    [JsonPropertyName("calendarSync")]
    public required bool CalendarSync { get; init; }

    [JsonPropertyName("calendarId")]
    public string? CalendarId { get; init; }

    [JsonPropertyName("isCash")]
    public bool? IsCash { get; init; }

    [JsonPropertyName("datePaid")]
    public DateOnly? DatePaid { get; init; }

    [JsonPropertyName("expenses")]
    public decimal? Expenses { get; init; }

    [JsonPropertyName("mileage")]
    public decimal? Mileage { get; init; }

    [JsonPropertyName("isComplete")]
    public bool IsComplete { get; init; }

    [DynamoDBIgnore]
    [JsonPropertyName("gigDate")]
    public DateOnly GigDate => DateOnly.FromDateTime(LeaveDate);

    [DynamoDBIgnore]
    [JsonPropertyName("isFuture")] public bool? IsFuture => LeaveDate > DateTime.Now;

    [DynamoDBIgnore]
    [JsonPropertyName("isPaid")]
    public bool IsPaid => DatePaid.HasValue && DatePaid <= DateOnly.FromDateTime(DateTime.Today);
}