namespace MikouTools.String.Text
{
    public static class Utils
    {
        public static string NormalizeNewlines(string input, string newLine = "\r\n")
        {
            return input.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", newLine);
        }
    }
}
