using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.SqlServer.Dts.Runtime;
using System;
using System.Collections.Generic;
using System.IO;

namespace SSIS.Extensions.ZipTask
{
    public class ZipManager
    {

        #region Properties

        public string source { get; set; }
        public string target { get; set; }
        public string password { get; set; }
        public ZipCompressionLevel zipLevel { get; set; }
        public bool overwriteTarget { get; set; }
        public bool recursive { get; set; }
        public string fileFilter { get; set; }
        public bool removeSource { get; set; }
        public LogLevel logLevel { get; set; }
        public IDTSComponentEvents componentEvents { get; set; }

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

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipManager"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="zipLevel">The zip level.</param>
        /// <param name="zipPassword">The zip password.</param>
        /// <param name="recursive">if set to <c>true</c> [recursive].</param>
        /// <param name="removeSouce">if set to <c>true</c> [remove souce].</param>
        /// <param name="overwriteTarget">if set to <c>true</c> [overwrite target].</param>
        /// <param name="loglevel">The loglevel.</param>
        /// <param name="componentEvents">The component events.</param>
        public ZipManager(string source, string target, ZipCompressionLevel zipLevel, string password, bool recursive, string fileFilter, bool removeSouce, bool overwriteTarget, LogLevel logLevel, IDTSComponentEvents componentEvents)
        {
            this.source = source;
            this.target = target;
            this.zipLevel = zipLevel;
            this.password = password;
            this.recursive = recursive;
            this.fileFilter = String.IsNullOrEmpty(fileFilter) ? "true" : fileFilter;
            this.removeSource = removeSource;
            this.overwriteTarget = overwriteTarget;
            this.logLevel = logLevel;
            this.componentEvents = componentEvents;
        }

        #endregion

        #region Compress

        /// <summary>
        /// Zips the specified source (File or Directory).
        /// </summary>
        public void Zip()
        {
            Common.ValidateOverwrite(this.overwriteTarget, this.target);

            this.Log(String.Format("Zipping: [{0}] -> [{1}].", this.source, this.target), LogLevel.Minimal);
            using (ZipOutputStream zipStream = new ZipOutputStream(File.Create(this.target)))
            {
                zipStream.SetLevel((int)zipLevel);
                if (!String.IsNullOrEmpty(this.password))
                    zipStream.Password = this.password;

                DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(this.source));
                List<string> fileList = Common.GetFileList(this.source, this.fileFilter, this.recursive);
                if (fileList.Count == 0)
                    this.Log(String.Format("No files found matching the filter criteria."), LogLevel.Verbose);

                foreach (string filePath in fileList)
                {
                    this.zipFile(zipStream, dir, new FileInfo(filePath));
                }

                zipStream.IsStreamOwner = true;
                zipStream.Close();
            }

            this.Log(String.Format("Successfully Zipped: [{0}] -> [{1}].", this.source, this.target), LogLevel.Minimal);
            Common.RemoveFile(this.removeSource, this.source);
        }

        /// <summary>
        /// Zips the file.
        /// </summary>
        /// <param name="zipStream">The zip stream.</param>
        /// <param name="baseDir">The base dir.</param>
        /// <param name="inputFile">The input file.</param>
        private void zipFile(ZipOutputStream zipStream, DirectoryInfo baseDir, FileInfo inputFile)
        {
            string filePath = inputFile.FullName.Replace(baseDir.FullName + @"\", "");
            this.Log(String.Format("Zipping: [{0}].", filePath), LogLevel.Verbose);

            ZipEntry entry = new ZipEntry(filePath);
            entry.DateTime = inputFile.LastWriteTime;
            entry.Size = inputFile.Length;
            zipStream.PutNextEntry(entry);

            byte[] buffer = new byte[4096];
            using (FileStream streamReader = File.OpenRead(inputFile.FullName))
            {
                StreamUtils.Copy(streamReader, zipStream, buffer);
            }
            this.Log(String.Format("Zipped: [{0}].", filePath), LogLevel.Verbose);
        }

        #endregion

        #region Decompress

        /// <summary>
        /// Unzips the specified source.
        /// </summary>
        public void UnZip()
        {
            string targetPath = Path.Combine(this.target, Path.GetFileName(this.source));
            Common.ValidateFile(this.source);
            this.Log(String.Format("Unzipping: [{0}] -> [{1}].", this.source, this.target), LogLevel.Minimal); 

            using (FileStream fileStreamIn = File.OpenRead(this.source))
            using (ZipFile zipFile = new ZipFile(fileStreamIn))
            {
                if(zipFile.Count == 0)
                    this.Log(String.Format("No files found in the archive."), LogLevel.Verbose);

                foreach (ZipEntry entry in zipFile)
                {
                    if (entry.IsDirectory)
                    {
                        continue;
                    }
                    else
                    {
                        String entryFileName = entry.Name;
                        this.Log(String.Format("UnZipping: [{0}].", entryFileName), LogLevel.Verbose);

                        byte[] buffer = new byte[4096];
                        Stream zipStream = zipFile.GetInputStream(entry);

                        string fullZipToPath = Path.Combine(this.target, entryFileName);
                        string directoryName = Path.GetDirectoryName(fullZipToPath);
                        Common.ValidateOverwrite(this.overwriteTarget, fullZipToPath); 

                        if (directoryName.Length > 0)
                            Directory.CreateDirectory(directoryName);

                        using (FileStream streamWriter = File.Create(fullZipToPath))
                        {
                            StreamUtils.Copy(zipStream, streamWriter, buffer);
                        }
                    }
                }
            }
            this.Log(String.Format("Successfully UnZipped: [{0}] -> [{1}].", this.source, this.target), LogLevel.Minimal); 
            Common.RemoveFile(this.removeSource, this.source);
        }

        #endregion
    }
}

