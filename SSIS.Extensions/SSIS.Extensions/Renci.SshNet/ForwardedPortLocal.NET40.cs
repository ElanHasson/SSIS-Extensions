﻿using System;
using System.Threading.Tasks;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality for local port forwarding
    /// </summary>
    public partial class ForwardedPortLocal 
    {
        partial void ExecuteThread(Action action)
        {
            Task.Factory.StartNew(action);
        }
    }
}
