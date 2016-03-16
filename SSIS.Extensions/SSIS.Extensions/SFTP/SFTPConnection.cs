using Microsoft.SqlServer.Dts.Runtime;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SSIS.Extensions.SFTP
{
    /// <summary>
    /// This class is the SFTP interface to ssh net library.
    /// </summary>
    public class SFTPConnection
    {
        #region Properties

        public string hostName { get; set; }
        public string userName { get; set; }
        public string passWord { get; set; }
        public int portNumber { get; set; }
        //private int reTries { get; set; }
        private bool stopOnFailure { get; set; }
        public LogLevel logLevel { get; set; }
        public IDTSComponentEvents componentEvents { get; set; }
        
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SFTPConnection"/> class.
        /// </summary>
        /// <param name="hostName">Name of the host.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="passWord">The pass word.</param>
        /// <param name="portNumber">The port number.</param>
        /// <param name="reTries">The re tries.</param>
        /// <param name="componentEvents">The component events.</param>
        /// <param name="logLevel">The log level.</param>
        public SFTPConnection(string hostName, string userName, string passWord, int portNumber, bool stopOnFailure, IDTSComponentEvents componentEvents, LogLevel logLevel)
        {
            this.hostName = hostName;
            this.userName = userName;
            this.passWord = passWord;
            this.portNumber = portNumber;
            //this.reTries = reTries;
            this.componentEvents = componentEvents;
            this.logLevel = logLevel;
            this.stopOnFailure = stopOnFailure;
        }

        #endregion

        #region Private Members
          
        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="Message">The message.</param>
        /// <param name="minLogLevel">The min log level.</param>
        private void Log(string Message, LogLevel minLogLevel)
        {
            if (this.logLevel >= minLogLevel)
                Common.FireInfo(Message, componentEvents);
        }

        /// <summary>
        /// Throws the exception.
        /// </summary>
        /// <param name="Message">The message.</param>
        /// <param name="ex">The ex.</param>
        /// <exception cref="System.Exception">
        /// </exception>
        private void ThrowException(string Message, Exception ex)
        {
            string stackTrace = ex.StackTrace;
            
            if (ex.InnerException != null)
            {
                Message += ex.InnerException.Message;
                stackTrace += ex.InnerException.StackTrace;
            }

            Message += ex.Message + Environment.NewLine;
            stackTrace += Environment.NewLine;

            if (logLevel == LogLevel.Verbose)
                throw new Exception(String.Format("Error: {0}StackTrace: {1}", Message, stackTrace), ex);
            else
                throw new Exception(String.Format("Error: {0}", Message), ex);
        }

        #endregion

        #region Upload Files

        /// <summary>
        /// Uploads the files.
        /// </summary>
        /// <param name="fileList">The file list.</param>
        /// <exception cref="System.Exception">Remote File Already Exists.</exception>
        public void UploadFiles(List<ISFTPFileInfo> fileList)
        {
            try
            {
                this.Log(String.Format("Connecting to Host: [{0}].", this.hostName), LogLevel.Minimal);

                using (SftpClient sftp = new SftpClient(this.hostName, this.portNumber, this.userName, this.passWord))
                {
                    sftp.Connect();

                    this.Log(String.Format("Connected to Host: [{0}].", this.hostName), LogLevel.Verbose);

                    // Upload each file
                    foreach (SFTPFileInfo sftpFile in fileList)
                    {
                        FileInfo fileInfo = new FileInfo(sftpFile.LocalPath);
                        if (sftpFile.RemotePath.EndsWith("/"))
                            sftpFile.RemotePath = Path.Combine(sftpFile.RemotePath, fileInfo.Name).Replace(@"\", "/");

                        // if file exists can we overwrite it.
                        if (sftp.Exists(sftpFile.RemotePath))
                        {
                            if (sftpFile.OverwriteDestination)
                            {
                                this.Log(String.Format("Removing File: [{0}].", sftpFile.RemotePath), LogLevel.Verbose);
                                sftp.Delete(sftpFile.RemotePath);
                                this.Log(String.Format("Removed File: [{0}].", sftpFile.RemotePath), LogLevel.Verbose);
                            }
                            else
                            {
                                if (this.stopOnFailure)
                                    throw new Exception("Remote File Already Exists.");
                            }
                        }

                        using (FileStream file = File.OpenRead(sftpFile.LocalPath))
                        {
                            this.Log(String.Format("Uploading File: [{0}] -> [{1}].", fileInfo.FullName, sftpFile.RemotePath), LogLevel.Minimal);
                            sftp.UploadFile(file, sftpFile.RemotePath);
                            this.Log(String.Format("Uploaded File: [{0}] -> [{1}].", fileInfo.FullName, sftpFile.RemotePath), LogLevel.Verbose);
                        }
                    }
                }
                this.Log(String.Format("Disconnected from Host: [{0}].", this.hostName), LogLevel.Minimal);
            }
            catch (Exception ex)
            {
                this.Log(String.Format("Disconnected from Host: [{0}].", this.hostName), LogLevel.Minimal);
                this.ThrowException("Unable to Upload: ", ex);
            }
        }

        #endregion

        #region Download Files

        /// <summary>
        /// Downloads the files.
        /// </summary>
        /// <param name="fileList">The file list.</param>
        /// <param name="failRemoteNotExists">if set to <c>true</c> [fail remote not exists].</param>
        /// <exception cref="System.Exception">
        /// Local File Already Exists.
        /// </exception>
        public void DownloadFiles(List<ISFTPFileInfo> fileList)
        {
            try
            {
                this.Log(String.Format("Connecting to Host: [{0}].", this.hostName), LogLevel.Minimal);

                using (SftpClient sftp = new SftpClient(this.hostName, this.portNumber, this.userName, this.passWord))
                {
                    sftp.Connect();

                    this.Log(String.Format("Connected to Host: [{0}].", this.hostName), LogLevel.Verbose);

                    // Download each file
                    foreach (SFTPFileInfo filePath in fileList)
                    {
                        if (!sftp.Exists(filePath.RemotePath))
                        {
                            this.Log(String.Format("Remote Path Does Not Exist: [{0}].", this.hostName), LogLevel.Verbose);
                            if (this.stopOnFailure)
                                throw new Exception(String.Format("Remote Path Does Not Exist: [{0}]", filePath.RemotePath));
                            else
                                continue;
                        }

                        if (Directory.Exists(filePath.LocalPath))
                            filePath.LocalPath = Path.Combine(filePath.LocalPath, filePath.RemotePath.Substring(filePath.RemotePath.LastIndexOf("/") + 1));

                        // Can we overwrite the local file
                        if (!filePath.OverwriteDestination && File.Exists(filePath.LocalPath))
                            throw new Exception("Local File Already Exists.");

                        this.Log(String.Format("Downloading File: [{0}] -> [{1}].", filePath.RemotePath, filePath.LocalPath), LogLevel.Minimal);
                        using (FileStream fileStream = File.OpenWrite(filePath.LocalPath))
                        {
                            sftp.DownloadFile(filePath.RemotePath, fileStream);
                        }
                        this.Log(String.Format("File Downloaded: [{0}]", filePath.LocalPath), LogLevel.Verbose);
                    }
                }
                this.Log(String.Format("Disconnected from Host: [{0}].", this.hostName), LogLevel.Minimal);
            }
            catch (Exception ex)
            {
                this.Log(String.Format("Disconnected from Host: [{0}].", this.hostName), LogLevel.Minimal);
                this.ThrowException("Unable to Download: ", ex);
            }
        }

        #endregion

        #region Get File Listing

        /// <summary>
        /// Lists the files.
        /// </summary>
        /// <param name="remotePath">The remote path.</param>
        /// <param name="failRemoteNotExists">if set to <c>true</c> [fail remote not exists].</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public List<IRemoteFileInfo> ListFiles(string remotePath)
        {
            List<IRemoteFileInfo> fileList = new List<IRemoteFileInfo>();
            try
            {
                this.Log(String.Format("Connecting to Host: [{0}].", this.hostName), LogLevel.Minimal);
                using (SftpClient sftp = new SftpClient(this.hostName, this.portNumber, this.userName, this.passWord))
                {
                    sftp.Connect();
                    this.Log(String.Format("Connected to Host: [{0}].", this.hostName), LogLevel.Verbose);

                    if (!sftp.Exists(remotePath))
                    {
                        this.Log(String.Format("Remote Path Does Not Exist: [{0}].", this.hostName), LogLevel.Verbose);
                        if (this.stopOnFailure)
                            throw new Exception(String.Format("Invalid Path: [{0}]", remotePath));
                    }
                    else
                    {
                        this.Log(String.Format("Listing Files: [{0}].", remotePath), LogLevel.Minimal);
                        this.Log(String.Format("Getting Attributes: [{0}].", remotePath), LogLevel.Verbose);

                        SftpFile sftpFileInfo = sftp.Get(remotePath);
                        if (sftpFileInfo.IsDirectory)
                        {
                            this.Log(String.Format("Path is a Directory: [{0}].", remotePath), LogLevel.Verbose);
                            IEnumerable<SftpFile> dirList = sftp.ListDirectory(remotePath);
                            foreach (SftpFile sftpFile in dirList)
                                fileList.Add(this.CreateFileInfo(sftpFile));
                        }
                        else
                        {
                            this.Log(String.Format("Path is a File: [{0}].", remotePath), LogLevel.Verbose);
                            fileList.Add(this.CreateFileInfo(sftpFileInfo));
                        }
                    }
                }
                this.Log(String.Format("Disconnected from Host: [{0}].", this.hostName), LogLevel.Minimal);
            }
            catch (Exception ex)
            {
                this.Log(String.Format("Disconnected from Host: [{0}].", this.hostName), LogLevel.Minimal);
                this.ThrowException("Unable to List: ", ex);
            }
            return fileList;
        }

        private RemoteFileInfo CreateFileInfo(SftpFile sftpFile)
        {
            RemoteFileInfo fileInfo = new RemoteFileInfo();
            fileInfo.Name = sftpFile.Name;
            fileInfo.FullName = sftpFile.FullName;
            fileInfo.Extension = Path.GetExtension(sftpFile.FullName);
            fileInfo.IsDirectory = sftpFile.IsDirectory;
            fileInfo.Size = sftpFile.Length;
            fileInfo.ModifiedTime = sftpFile.LastWriteTime;
            return fileInfo;
        }

        #endregion
    }
}
  