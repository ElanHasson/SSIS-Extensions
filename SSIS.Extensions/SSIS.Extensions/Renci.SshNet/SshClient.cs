﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Collections.ObjectModel;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides client connection to SSH server.
    /// </summary>
    public class SshClient : BaseClient
    {
        /// <summary>
        /// Holds the list of forwarded ports
        /// </summary>
        private List<ForwardedPort> _forwardedPorts = new List<ForwardedPort>();

        /// <summary>
        /// If true, causes the connectionInfo object to be disposed.
        /// </summary>
        private bool _disposeConnectionInfo;

        private Stream _inputStream;

        /// <summary>
        /// Gets the list of forwarded ports.
        /// </summary>
        public IEnumerable<ForwardedPort> ForwardedPorts
        {
            get
            {
                return this._forwardedPorts.AsReadOnly();
            }
        }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SshClient" /> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <example>
        ///     <code source="..\..\Renci.SshNet.Tests\Classes\PasswordConnectionInfoTest.cs" region="Example PasswordConnectionInfo" language="C#" title="Connect using PasswordConnectionInfo object" />
        ///     <code source="..\..\Renci.SshNet.Tests\Classes\PasswordConnectionInfoTest.cs" region="Example PasswordConnectionInfo PasswordExpired" language="C#" title="Connect using PasswordConnectionInfo object with passwod change option" />
        ///     <code source="..\..\Renci.SshNet.Tests\Classes\PrivateKeyConnectionInfoTest.cs" region="Example PrivateKeyConnectionInfo PrivateKeyFile" language="C#" title="Connect using PrivateKeyConnectionInfo" />
        ///     <code source="..\..\Renci.SshNet.Tests\Classes\SshClientTest.cs" region="Example SshClient Connect Timeout" language="C#" title="Specify connection timeout when connecting" />
        /// </example>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is null.</exception>
        public SshClient(ConnectionInfo connectionInfo)
            : base(connectionInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is null or contains whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="F:System.Net.IPEndPoint.MinPort"/> and <see cref="System.Net.IPEndPoint.MaxPort"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "C2A000:DisposeObjectsBeforeLosingScope", Justification = "Disposed in Dispose(bool) method.")]
        public SshClient(string host, int port, string username, string password)
            : this(new PasswordConnectionInfo(host, port, username, password))
        {
            this._disposeConnectionInfo = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <example>
        ///     <code source="..\..\Renci.SshNet.Tests\Classes\SshClientTest.cs" region="Example SshClient(host, username) Connect" language="C#" title="Connect using username and password" />
        /// </example>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is null or contains whitespace characters.</exception>
        public SshClient(string host, string username, string password)
            : this(host, ConnectionInfo.DEFAULT_PORT, username, password)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        /// <example>
        ///     <code source="..\..\Renci.SshNet.Tests\Classes\SshClientTest.cs" region="Example SshClient(host, username) Connect PrivateKeyFile" language="C#" title="Connect using username and private key" />
        ///     <code source="..\..\Renci.SshNet.Tests\Classes\SshClientTest.cs" region="Example SshClient(host, username) Connect PrivateKeyFile PassPhrase" language="C#" title="Connect using username and private key and pass phrase" />
        /// </example>
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, -or- <paramref name="username"/> is null or contains whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="F:System.Net.IPEndPoint.MinPort"/> and <see cref="System.Net.IPEndPoint.MaxPort"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Disposed in Dispose(bool) method.")]
        public SshClient(string host, int port, string username, params PrivateKeyFile[] keyFiles)
            : this(new PrivateKeyConnectionInfo(host, port, username, keyFiles))
        {
            this._disposeConnectionInfo = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        /// <example>
        ///     <code source="..\..\Renci.SshNet.Tests\Classes\SshClientTest.cs" region="Example SshClient(host, username) Connect PrivateKeyFile" language="C#" title="Connect using private key" />
        ///     <code source="..\..\Renci.SshNet.Tests\Classes\SshClientTest.cs" region="Example SshClient(host, username) Connect PrivateKeyFile PassPhrase" language="C#" title="Connect using private key and pass phrase" />
        /// </example>
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, -or- <paramref name="username"/> is null or contains whitespace characters.</exception>
        public SshClient(string host, string username, params PrivateKeyFile[] keyFiles)
            : this(host, ConnectionInfo.DEFAULT_PORT, username, keyFiles)
        {
        }

        #endregion

        /// <summary>
        /// Called when client is disconnecting from the server.
        /// </summary>
        protected override void OnDisconnecting()
        {
            base.OnDisconnecting();

            foreach (var port in this._forwardedPorts)
            {
                port.Stop();
            }
        }

        /// <summary>
        /// Adds the forwarded port.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <example>
        ///     <code source="..\..\Renci.SshNet.Tests\Classes\ForwardedPortRemoteTest.cs" region="Example SshClient AddForwardedPort Start Stop ForwardedPortRemote" language="C#" title="Remote port forwarding" />
        ///     <code source="..\..\Renci.SshNet.Tests\Classes\ForwardedPortLocalTest.cs" region="Example SshClient AddForwardedPort Start Stop ForwardedPortLocal" language="C#" title="Local port forwarding" />
        /// </example>
        /// <exception cref="InvalidOperationException">Forwarded port is already added to a different client.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="port"/> is null.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        public void AddForwardedPort(ForwardedPort port)
        {
            if (port == null)
                throw new ArgumentNullException("port");

            if (port.Session != null && port.Session != this.Session)
                throw new InvalidOperationException("Forwarded port is already added to a different client.");

            port.Session = this.Session;

            this._forwardedPorts.Add(port);
        }

        /// <summary>
        /// Stops and removes the forwarded port from the list.
        /// </summary>
        /// <param name="port">Forwarded port.</param>
        /// <exception cref="ArgumentNullException"><paramref name="port"/> is null.</exception>
        public void RemoveForwardedPort(ForwardedPort port)
        {
            if (port == null)
                throw new ArgumentNullException("port");

            //  Stop port forwarding before removing it
            port.Stop();

            port.Session = null;

            this._forwardedPorts.Remove(port);
        }

        /// <summary>
        /// Creates the command to be executed.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns><see cref="SshCommand"/> object.</returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        public SshCommand CreateCommand(string commandText)
        {
            return this.CreateCommand(commandText, this.ConnectionInfo.Encoding);
        }

        /// <summary>
        /// Creates the command to be executed with specified encoding.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="encoding">The encoding to use for results.</param>
        /// <returns><see cref="SshCommand"/> object which uses specified encoding.</returns>
        /// <remarks>TThis method will change current default encoding.</remarks>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="commandText"/> or <paramref name="encoding"/> is null.</exception>
        public SshCommand CreateCommand(string commandText, Encoding encoding)
        {
            this.ConnectionInfo.Encoding = encoding;
            return new SshCommand(this.Session, commandText);
        }

        /// <summary>
        /// Creates and executes the command.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns>Returns an instance of <see cref="SshCommand"/> with execution results.</returns>
        /// <remarks>This method internally uses asynchronous calls.</remarks>
        /// <example>
        ///     <code source="..\..\Renci.SshNet.Tests\Classes\SshCommandTest.cs" region="Example SshCommand RunCommand Result" language="C#" title="Running simple command" />
        ///     <code source="..\..\Renci.SshNet.Tests\Classes\SshCommandTest.NET40.cs" region="Example SshCommand RunCommand Parallel" language="C#" title="Run many commands in parallel" />
        /// </example>
        /// <exception cref="ArgumentException">CommandText property is empty.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshException">Invalid Operation - An existing channel was used to execute this command.</exception>
        /// <exception cref="InvalidOperationException">Asynchronous operation is already in progress.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="commandText"/> is null.</exception>
        public SshCommand RunCommand(string commandText)
        {
            var cmd = this.CreateCommand(commandText);
            cmd.Execute();
            return cmd;
        }

        /// <summary>
        /// Creates the shell.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="extendedOutput">The extended output.</param>
        /// <param name="terminalName">Name of the terminal.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="terminalModes">The terminal mode.</param>
        /// <param name="bufferSize">Size of the internal read buffer.</param>
        /// <returns>
        /// Returns a representation of a <see cref="Shell" /> object.
        /// </returns>
        public Shell CreateShell(Stream input, Stream output, Stream extendedOutput, string terminalName, uint columns, uint rows, uint width, uint height, IDictionary<TerminalModes, uint> terminalModes, int bufferSize)
        {
            return new Shell(this.Session, input, output, extendedOutput, terminalName, columns, rows, width, height, terminalModes, bufferSize);
        }

        /// <summary>
        /// Creates the shell.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="extendedOutput">The extended output.</param>
        /// <param name="terminalName">Name of the terminal.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="terminalModes">The terminal mode.</param>
        /// <returns>
        /// Returns a representation of a <see cref="Shell" /> object.
        /// </returns>
        public Shell CreateShell(Stream input, Stream output, Stream extendedOutput, string terminalName, uint columns, uint rows, uint width, uint height, IDictionary<TerminalModes, uint> terminalModes)
        {
            return this.CreateShell(input, output, extendedOutput, terminalName, columns, rows, width, height, terminalModes, 1024);
        }

        /// <summary>
        /// Creates the shell.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="extendedOutput">The extended output.</param>
        /// <returns>
        /// Returns a representation of a <see cref="Shell" /> object.
        /// </returns>
        public Shell CreateShell(Stream input, Stream output, Stream extendedOutput)
        {
            return this.CreateShell(input, output, extendedOutput, string.Empty, 0, 0, 0, 0, null, 1024);
        }

        /// <summary>
        /// Creates the shell.
        /// </summary>
        /// <param name="encoding">The encoding to use to send the input.</param>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="extendedOutput">The extended output.</param>
        /// <param name="terminalName">Name of the terminal.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="terminalModes">The terminal mode.</param>
        /// <param name="bufferSize">Size of the internal read buffer.</param>
        /// <returns>
        /// Returns a representation of a <see cref="Shell" /> object.
        /// </returns>
        public Shell CreateShell(Encoding encoding, string input, Stream output, Stream extendedOutput, string terminalName, uint columns, uint rows, uint width, uint height, IDictionary<TerminalModes, uint> terminalModes, int bufferSize)
        {
            this._inputStream = new MemoryStream();
            var writer = new StreamWriter(this._inputStream, encoding);
            writer.Write(input);
            writer.Flush();
            this._inputStream.Seek(0, SeekOrigin.Begin);

            return this.CreateShell(this._inputStream, output, extendedOutput, terminalName, columns, rows, width, height, terminalModes, bufferSize);
        }

        /// <summary>
        /// Creates the shell.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="extendedOutput">The extended output.</param>
        /// <param name="terminalName">Name of the terminal.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="terminalModes">The terminal modes.</param>
        /// <returns>
        /// Returns a representation of a <see cref="Shell" /> object.
        /// </returns>
        public Shell CreateShell(Encoding encoding, string input, Stream output, Stream extendedOutput, string terminalName, uint columns, uint rows, uint width, uint height, IDictionary<TerminalModes, uint> terminalModes)
        {
            return this.CreateShell(encoding, input, output, extendedOutput, terminalName, columns, rows, width, height, terminalModes, 1024);
        }

        /// <summary>
        /// Creates the shell.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="extendedOutput">The extended output.</param>
        /// <returns>
        /// Returns a representation of a <see cref="Shell" /> object.
        /// </returns>
        public Shell CreateShell(Encoding encoding, string input, Stream output, Stream extendedOutput)
        {
            return this.CreateShell(encoding, input, output, extendedOutput, string.Empty, 0, 0, 0, 0, null, 1024);
        }

        /// <summary>
        /// Creates the shell stream.
        /// </summary>
        /// <param name="terminalName">Name of the terminal.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        /// <returns>
        /// Reference to Created ShellStream object.
        /// </returns>
        public ShellStream CreateShellStream(string terminalName, uint columns, uint rows, uint width, uint height, int bufferSize)
        {
            return this.CreateShellStream(terminalName, columns, rows, width, height, bufferSize, null);
        }

        /// <summary>
        /// Creates the shell stream.
        /// </summary>
        /// <param name="terminalName">Name of the terminal.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        /// <param name="terminalModeValues">The terminal mode values.</param>
        /// <returns>
        /// Reference to Created ShellStream object.
        /// </returns>
        public ShellStream CreateShellStream(string terminalName, uint columns, uint rows, uint width, uint height, int bufferSize, IDictionary<TerminalModes, uint> terminalModeValues)
        {
            return new ShellStream(this.Session, terminalName, columns, rows, width, height, bufferSize, terminalModeValues);
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();

            foreach (var forwardedPort in this._forwardedPorts.ToArray())
            {
                this.RemoveForwardedPort(forwardedPort);
            }

        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged ResourceMessages.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (this._disposeConnectionInfo)
                ((IDisposable)this.ConnectionInfo).Dispose();

            if (this._inputStream != null)
            {
                this._inputStream.Dispose();
                this._inputStream = null;
            }
        }
    }
}
