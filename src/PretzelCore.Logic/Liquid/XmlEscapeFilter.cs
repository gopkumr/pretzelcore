using System.Security;

namespace PretzelCore.Services.Liquid
{
    public static class XmlEscapeFilter
    {
        public static string xml_escape(string input)
        {
            return SecurityElement.Escape(input);
        }
    }
}
