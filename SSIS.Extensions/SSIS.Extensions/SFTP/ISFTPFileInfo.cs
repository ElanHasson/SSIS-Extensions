using System;

namespace SSIS.Extensions.SFTP
{
    #region Interface
    /// <summary>
    /// SFTP File information interface
    /// </summary>
    public interface ISFTPFileInfo
    {
        /// <summary>
        /// Local file path.
        /// </summary>
        /// <value>
        /// The local path.
        /// </value>
        string LocalPath { set; get; }

        /// <summary>
        /// Remote file path.
        /// </summary>
        /// <value>
        /// The remote path.
        /// </value>
        string RemotePath { set; get; }

        /// <summary>
        /// Overwrite the destination file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [overwrite destination]; otherwise, <c>false</c>.
        /// </value>
        bool OverwriteDestination { set; get; }

        /// <summary>
        /// Remove the source file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [remove source]; otherwise, <c>false</c>.
        /// </value>
        bool RemoveSource { set; get; }
    }

    #endregion

    #region Class

    /// <summary>
    /// Stores the file information for sending and receiving
    /// </summary>
    public class SFTPFileInfo : ISFTPFileInfo
    {
        /// <summary>
        /// Local file path.
        /// </summary>
        /// <value>
        /// The local path.
        /// </value>
        public string LocalPath { set; get; }

        /// <summary>
        /// Remote file path.
        /// </summary>
        /// <value>
        /// The remote path.
        /// </value>
        public string RemotePath { set; get; }

        /// <summary>
        /// Overwrite the destination file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [overwrite destination]; otherwise, <c>false</c>.
        /// </value>
        public bool OverwriteDestination { set; get; }

        /// <summary>
        /// Remove the source file.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [remove source]; otherwise, <c>false</c>.
        /// </value>
        public bool RemoveSource { set; get; }

        public SFTPFileInfo() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SFTPFileInfo"/> class.
        /// </summary>
        /// <param name="localPath">The local path.</param>
        /// <param name="remotePath">The remote path.</param>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <param name="removeSource">if set to <c>true</c> [remove source].</param>
        public SFTPFileInfo(string localPath, string remotePath, bool overwrite, bool removeSource)
        {
            this.LocalPath = localPath;
            this.RemotePath = remotePath;
            this.OverwriteDestination = overwrite;
            this.RemoveSource = removeSource;
        }
    }

    #endregion
}
