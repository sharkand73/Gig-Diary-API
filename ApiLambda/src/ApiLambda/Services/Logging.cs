using Microsoft.Extensions.Logging;

namespace ApiLambda.Services;

public static class Logging
{
    public static LambdaLoggerOptions Options { get; } = new()
    {
        IncludeLogLevel = true,
        IncludeCategory = true,
        IncludeException = true,
        IncludeNewline = true,
    };
}