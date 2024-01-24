namespace File_Public.Extensions
{
    public static class ObjectExtension
    {
        public static bool IsNullOrEmpty(this object obj) {
            return obj == null;
        }
    }
}
