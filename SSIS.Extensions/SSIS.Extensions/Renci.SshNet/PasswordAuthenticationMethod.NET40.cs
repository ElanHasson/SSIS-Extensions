﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Messages;
using System.Threading.Tasks;

namespace Renci.SshNet
{
    public partial class PasswordAuthenticationMethod : AuthenticationMethod
    {
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
        partial void ExecuteThread(Action action)
        {
            Task.Factory.StartNew(action);
        }
    }
}
