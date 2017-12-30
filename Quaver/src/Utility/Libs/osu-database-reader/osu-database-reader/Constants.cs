using System.Globalization;

namespace osu_database_reader
{
    internal static class Constants
    {
        //nfi so parsing works on all cultures
        public static readonly NumberFormatInfo NumberFormat = new CultureInfo(@"en-US", false).NumberFormat;
    }
}
