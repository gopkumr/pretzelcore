using System;

namespace PretzelCore.Services.Liquid
{
    public static class CgiEscapeFilter
    {
        public static string cgi_escape(string input)
        {
            return Uri.EscapeDataString(input);
        }
    }
}
