namespace ApiLambda.Models;

public record Mappings(
    Dictionary<string, string> Postcodes,
    Dictionary<string, string> Contacts,
    Dictionary<string, string> Instruments
    );