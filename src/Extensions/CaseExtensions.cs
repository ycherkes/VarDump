namespace VarDumpExtended.Extensions;

internal static class CaseExtensions
{
    public static string ToPascalCase(this string input)
    {
        var length = input?.Length ?? 0;

        if (length == 0)
            return input;

        return input.Substring(0, 1).ToUpperInvariant() + input.Substring(1);
    }

    public static string ToCamelCase(this string input)
    {
        var length = input?.Length ?? 0;

        if (length == 0)
            return input;


        return input.Substring(0, 1).ToLowerInvariant() + input.Substring(1);
    }
}