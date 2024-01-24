namespace File_Public.Extensions
{
    public static class StringExtension
    {
        public static bool IsNullOrEmpty(this string value) {
            return value == null || value.Trim() == String.Empty;
        }
    }
}
