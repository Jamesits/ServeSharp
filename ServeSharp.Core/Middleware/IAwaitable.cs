using System;
using System.Collections.Generic;
using System.Text;

namespace ServeSharp.Core.Middleware
{
    public interface IAwaitable
    {
        public IAwaiter GetAwaiter();
    }
}
