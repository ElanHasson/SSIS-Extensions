using System;

namespace SSIS.Extensions.SFTP
{
    #region Interface
    /// <summary>
    /// Remote File Info Interface
    /// </summary>
    public interface IRemoteFileInfo
    {
        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { set; get; }

        /// <summary>
        /// Gets or sets the full file name.
        /// </summary>
        /// <value>
        /// The full name.
        /// </value>
        string FullName { set; get; }

        /// <summary>
        /// Gets or sets the file extension.
        /// </summary>
        /// <value>
        /// The extension.
        /// </value>
        string Extension { get; set; }

        /// <summary>
        /// Gets or sets the file size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        long Size { set; get; }

        /// <summary>
        /// Gets or sets the modified time.
        /// </summary>
        /// <value>
        /// The modified time.
        /// </value>
        DateTime ModifiedTime { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is directory.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is directory; otherwise, <c>false</c>.
        /// </value>
        bool IsDirectory { set; get; }
    }

    #endregion

    #region Class
    /// <summary>
    /// Stores the Remote file information
    /// </summary>
    internal class RemoteFileInfo : IRemoteFileInfo
    {
        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { set; get; }

        /// <summary>
        /// Gets or sets the full file name.
        /// </summary>
        /// <value>
        /// The full name.
        /// </value>
        public string FullName { set; get; }

        /// <summary>
        /// Gets or sets the file extension.
        /// </summary>
        /// <value>
        /// The extension.
        /// </value>
        public string Extension { get; set; }

        /// <summary>
        /// Gets or sets the file size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public long Size { set; get; }

        /// <summary>
        /// Gets or sets the modified time.
        /// </summary>
        /// <value>
        /// The modified time.
        /// </value>
        public DateTime ModifiedTime { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is directory.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is directory; otherwise, <c>false</c>.
        /// </value>
        public bool IsDirectory { set; get; }
    }

    #endregion
}
