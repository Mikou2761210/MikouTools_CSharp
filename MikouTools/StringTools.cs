namespace MikouTools
{
    public static class StringTools
    {
        public static string NormalizeNewlines(string input, string newLine = "\r\n")
        {
            return input.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
        }
    }
}
