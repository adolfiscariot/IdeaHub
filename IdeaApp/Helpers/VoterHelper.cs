using System;

namespace IdeaApp.Helpers;

public static class VoterHelper
{
    //convert voters from string to list
    public static List<string> ToList(string? voters)
    {
        return string.IsNullOrEmpty(voters)
            ? new List<string>()
            : voters.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    //convert from list to string
    public static string? ToCommaSeparatedString(List<string> voters)
    {
        return voters == null || voters.Count == 0
            ? null
            : string.Join(",", voters);
    }
}
