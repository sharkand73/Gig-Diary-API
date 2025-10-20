using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;

namespace ApiLambda.Models;

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

    [DynamoDBIgnore]
    [JsonIgnore]
    public DateTimeOffset LeaveDate { get; private init; }

    // For DynamoDB storage and JSON serialization
    [DynamoDBProperty("leaveDate")]
    [JsonPropertyName("leaveDate")]
    public string LeaveDateString 
    { 
        get => LeaveDate.ToString("O"); // ISO 8601 format
        init => LeaveDate = DateTimeOffset.Parse(value);
    }
    
    [DynamoDBIgnore]
    [JsonIgnore]
    public DateTimeOffset ReturnDate { get; private init;  }

    [DynamoDBProperty("returnDate")]
    [JsonPropertyName("returnDate")]
    public string ReturnDateString 
    { 
        get => ReturnDate.ToString("O");
        init => ReturnDate = DateTimeOffset.Parse(value);
    }

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
    public string? CalendarId { get; set; }

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
    public DateOnly GigDate => DateOnly.FromDateTime(LeaveDate.DateTime);

    [DynamoDBIgnore]
    [JsonPropertyName("isFuture")] public bool? IsFuture => LeaveDate > DateTimeOffset.Now;

    [DynamoDBIgnore]
    [JsonPropertyName("isPaid")]
    public bool IsPaid => DatePaid.HasValue && DatePaid <= DateOnly.FromDateTime(DateTime.Today);

    [DynamoDBIgnore] [JsonPropertyName("isNextGig")]
    public bool IsNextGig { get; set; }
}