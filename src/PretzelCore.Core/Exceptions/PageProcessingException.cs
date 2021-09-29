using System;
using System.Runtime.Serialization;

namespace PretzelCore.Core.Exceptions
{
    [Serializable]
    public class PageProcessingException : Exception
    {
        public PageProcessingException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
