using System;
using System.Collections.Generic;
using System.Text;

namespace QuickDesk.Domain.Common.ExceptionHandling
{
    public class BadRequestCustomException : Exception
    {
        public List<string> Errors { get; set; }

        public BadRequestCustomException(List<string> errors): base("Bad Request") 
        {
            Errors = errors;
        }
    }

    public class UnauthorizedCustomException : Exception
    {
        public List<string> Errors { get; set; }
        public UnauthorizedCustomException(List<string> errors) : base("Unauthorized")
        {
            Errors = errors;
        }
    }
}
