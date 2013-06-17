﻿using System;
using System.Threading.Tasks;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents SSH command that can be executed.
    /// </summary>
    public partial class SshCommand 
    {
        /// <exception cref="ArgumentNullException"><paramref name=" action"/> is null.</exception>
        partial void ExecuteThread(Action action)
        {
            Task.Factory.StartNew(action);
        }
    }
}
