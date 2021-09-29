using System;

namespace PretzelCore.Services.Liquid
{
    public static class UriEscapeFilter
    {
        public static string uri_escape(string input)
        {
            return Uri.EscapeUriString(input);
        }
    }
}
