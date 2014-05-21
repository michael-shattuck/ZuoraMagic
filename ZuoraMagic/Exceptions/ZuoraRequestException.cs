using System;

namespace ZuoraMagic.Exceptions
{
    public class ZuoraRequestException : Exception
    {
        public ZuoraRequestException()
        {
        }

        public ZuoraRequestException(string message) : base(message)
        {
        }

        public ZuoraRequestException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }
    }
}