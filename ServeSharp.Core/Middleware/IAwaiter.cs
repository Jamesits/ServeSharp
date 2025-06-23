using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ServeSharp.Core.Middleware
{
    /// <summary>
    /// Interface for <c>await something</c> that returns nothing.
    /// </summary>
    public interface IAwaiter: INotifyCompletion
    {
        public bool IsCompleted { get; }
        public void GetResult();
    }
}
