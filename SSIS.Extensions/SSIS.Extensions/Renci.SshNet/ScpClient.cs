﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Channels;
using System.IO;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides SCP client functionality.
    /// </summary>
    public partial class ScpClient : BaseClient
    {
        private static Regex _fileInfoRe = new Regex(@"C(?<mode>\d{4}) (?<length>\d+) (?<filename>.+)");

        private static Regex _directoryInfoRe = new Regex(@"D(?<mode>\d{4}) (?<length>\d+) (?<filename>.+)");

        private static Regex _timestampRe = new Regex(@"T(?<mtime>\d+) 0 (?<atime>\d+) 0");

        private static char[] _byteToChar;

        private bool _disposeConnectionInfo;

        /// <summary>
        /// Gets or sets the operation timeout.
        /// </summary>
        /// <value>The operation timeout.</value>
        public TimeSpan OperationTimeout { get; set; }

        /// <summary>
        /// Gets or sets the size of the buffer.
        /// </summary>
        /// <value>The size of the buffer.</value>
        public uint BufferSize { get; set; }

        /// <summary>
        /// Occurs when downloading file.
        /// </summary>
        public event EventHandler<ScpDownloadEventArgs> Downloading;

        /// <summary>
        /// Occurs when uploading file.
        /// </summary>
        public event EventHandler<ScpUploadEventArgs> Uploading;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is null.</exception>
        public ScpClient(ConnectionInfo connectionInfo)
            : base(connectionInfo)
        {
            this.OperationTimeout = new TimeSpan(0, 0, 0, 0, -1);
            this.BufferSize = 1024 * 16;

            if (_byteToChar == null)
            {
                _byteToChar = new char[128];
                var ch = '\0';
                for (int i = 0; i < 128; i++)
                {
                    _byteToChar[i] = ch++;
                }
            }

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is null or contains whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="F:System.Net.IPEndPoint.MinPort"/> and <see cref="System.Net.IPEndPoint.MaxPort"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Disposed in Dispose(bool) method.")]
        public ScpClient(string host, int port, string username, string password)
            : this(new PasswordConnectionInfo(host, port, username, password))
        {
            this._disposeConnectionInfo = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is null or contains whitespace characters.</exception>
        public ScpClient(string host, string username, string password)
            : this(host, ConnectionInfo.DEFAULT_PORT, username, password)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, -or- <paramref name="username"/> is null or contains whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="F:System.Net.IPEndPoint.MinPort"/> and <see cref="System.Net.IPEndPoint.MaxPort"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Disposed in Dispose(bool) method.")]
        public ScpClient(string host, int port, string username, params PrivateKeyFile[] keyFiles)
            : this(new PrivateKeyConnectionInfo(host, port, username, keyFiles))
        {
            this._disposeConnectionInfo = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, -or- <paramref name="username"/> is null or contains whitespace characters.</exception>
        public ScpClient(string host, string username, params PrivateKeyFile[] keyFiles)
            : this(host, ConnectionInfo.DEFAULT_PORT, username, keyFiles)
        {
        }

        #endregion

        /// <summary>
        /// Uploads the specified stream to the remote host.
        /// </summary>
        /// <param name="source">Stream to upload.</param>
        /// <param name="filename">Remote host file name.</param>
        public void Upload(Stream source, string path)
        {
            using (var input = new PipeStream())
            using (var channel = this.Session.CreateChannel<ChannelSession>())
            {
                channel.DataReceived += delegate(object sender, Common.ChannelDataEventArgs e)
                {
                    input.Write(e.Data, 0, e.Data.Length);
                    input.Flush();
                };

                channel.Open();

                var pathParts = path.Split('\\', '/');

                //  Send channel command request
                channel.SendExecRequest(string.Format("scp -rt \"{0}\"", pathParts[0]));
                this.CheckReturnCode(input);

                //  Prepare directory structure
                for (int i = 0; i < pathParts.Length - 1; i++)
                {
                    this.InternalSetTimestamp(channel, input, DateTime.UtcNow, DateTime.UtcNow);
                    this.SendData(channel, string.Format("D0755 0 {0}\n", pathParts[i]));
                    this.CheckReturnCode(input);
                }

                this.InternalUpload(channel, input, source, pathParts.Last());

                //  Finish directory structure
                for (int i = 0; i < pathParts.Length - 1; i++)
                {
                    this.SendData(channel, "E\n");
                    this.CheckReturnCode(input);
                }

                channel.Close();
            }
        }

        /// <summary>
        /// Downloads the specified file from the remote host to the stream.
        /// </summary>
        /// <param name="filename">Remote host file name.</param>
        /// <param name="destination">The stream where to download remote file.</param>
        /// <exception cref="ArgumentException"><paramref name="filename"/> is null or contains whitespace characters.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="destination"/> is null.</exception>
        /// <remarks>Method calls made by this method to <paramref name="destination"/>, may under certain conditions result in exceptions thrown by the stream.</remarks>
        public void Download(string filename, Stream destination)
        {
            if (filename.IsNullOrWhiteSpace())
                throw new ArgumentException("filename");

            if (destination == null)
                throw new ArgumentNullException("destination");

            using (var input = new PipeStream())
            using (var channel = this.Session.CreateChannel<ChannelSession>())
            {
                channel.DataReceived += delegate(object sender, Common.ChannelDataEventArgs e)
                {
                    input.Write(e.Data, 0, e.Data.Length);
                    input.Flush();
                };

                channel.Open();

                //  Send channel command request
                channel.SendExecRequest(string.Format("scp -f \"{0}\"", filename));
                this.SendConfirmation(channel); //  Send reply

                var message = ReadString(input);
                var match = _fileInfoRe.Match(message);

                if (match.Success)
                {
                    //  Read file
                    this.SendConfirmation(channel); //  Send reply

                    var mode = match.Result("${mode}");
                    var length = long.Parse(match.Result("${length}"));
                    var fileName = match.Result("${filename}");

                    this.InternalDownload(channel, input, destination, fileName, length);
                }
                else
                {
                    this.SendConfirmation(channel, 1, string.Format("\"{0}\" is not valid protocol message.", message));
                }

                channel.Close();
            }
        }

        private void InternalSetTimestamp(ChannelSession channel, Stream input, DateTime lastWriteTime, DateTime lastAccessime)
        {
            var zeroTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var modificationSeconds = (long)(lastWriteTime - zeroTime).TotalSeconds;
            var accessSeconds = (long)(lastAccessime - zeroTime).TotalSeconds;
            this.SendData(channel, string.Format("T{0} 0 {1} 0\n", modificationSeconds, accessSeconds));
            this.CheckReturnCode(input);
        }

        private void InternalUpload(ChannelSession channel, Stream input, Stream source, string filename)
        {
            var length = source.Length;

            this.SendData(channel, string.Format("C0644 {0} {1}\n", length, Path.GetFileName(filename)));

            var buffer = new byte[this.BufferSize];

            var read = source.Read(buffer, 0, buffer.Length);

            long totalRead = 0;

            while (read > 0)
            {
                this.SendData(channel, buffer, read);

                totalRead += read;

                this.RaiseUploadingEvent(filename, length, totalRead);

                read = source.Read(buffer, 0, buffer.Length);
            }

            this.SendConfirmation(channel);
            this.CheckReturnCode(input);
        }

        private void InternalDownload(ChannelSession channel, Stream input, Stream output, string filename, long length)
        {
            var buffer = new byte[Math.Min(length, this.BufferSize)];
            var needToRead = length;

            do
            {
                var read = input.Read(buffer, 0, (int)Math.Min(needToRead, this.BufferSize));

                output.Write(buffer, 0, read);

                this.RaiseDownloadingEvent(filename, length, length - needToRead);

                needToRead -= read;
            }
            while (needToRead > 0);

            output.Flush();

            //  Raise one more time when file downloaded
            this.RaiseDownloadingEvent(filename, length, length - needToRead);

            //  Send confirmation byte after last data byte was read
            this.SendConfirmation(channel);

            this.CheckReturnCode(input);
        }

        private void RaiseDownloadingEvent(string filename, long size, long downloaded)
        {
            if (this.Downloading != null)
            {
                this.Downloading(this, new ScpDownloadEventArgs(filename, size, downloaded));
            }
        }

        private void RaiseUploadingEvent(string filename, long size, long uploaded)
        {
            if (this.Uploading != null)
            {
                this.Uploading(this, new ScpUploadEventArgs(filename, size, uploaded));
            }
        }

        private void SendConfirmation(ChannelSession channel)
        {
            this.SendData(channel, new byte[] { 0 });
        }

        private void SendConfirmation(ChannelSession channel, byte errorCode, string message)
        {
            this.SendData(channel, new byte[] { errorCode });
            this.SendData(channel, string.Format("{0}\n", message));
        }

        /// <summary>
        /// Checks the return code.
        /// </summary>
        /// <param name="input">The output stream.</param>
        private void CheckReturnCode(Stream input)
        {
            var b = ReadByte(input);

            if (b > 0)
            {
                var errorText = ReadString(input);

                throw new ScpException(errorText);
            }
        }

        partial void SendData(ChannelSession channel, string command);

        private void SendData(ChannelSession channel, byte[] buffer, int length)
        {
            if (length == buffer.Length)
            {
                channel.SendData(buffer);
            }
            else
            {
                channel.SendData(buffer.Take(length).ToArray());
            }
        }

        private void SendData(ChannelSession channel, byte[] buffer)
        {
            channel.SendData(buffer);
        }

        private static int ReadByte(Stream stream)
        {
            var b = stream.ReadByte();

            while (b < 0)
            {
                Thread.Sleep(100);
                b = stream.ReadByte();
            }

            return b;
        }

        private static string ReadString(Stream stream)
        {
            var hasError = false;

            StringBuilder sb = new StringBuilder();

            var b = ReadByte(stream);

            if (b == 1 || b == 2)
            {
                hasError = true;
                b = ReadByte(stream);
            }

            var ch = _byteToChar[b];

            while (ch != '\n')
            {
                sb.Append(ch);

                b = ReadByte(stream);

                ch = _byteToChar[b];
            }

            if (hasError)
                throw new ScpException(sb.ToString());

            return sb.ToString();
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
        }
    }
}
