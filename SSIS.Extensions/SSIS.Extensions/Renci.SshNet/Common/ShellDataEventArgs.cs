﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Provides data for Shell DataReceived event
    /// </summary>
    public class ShellDataEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the data.
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Gets the line data.
        /// </summary>
        public string Line { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellDataEventArgs"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public ShellDataEventArgs(byte[] data)
        {
            this.Data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellDataEventArgs"/> class.
        /// </summary>
        /// <param name="line">The line.</param>
        public ShellDataEventArgs(string line)
        {
            this.Line = line;
        }
    }
}
