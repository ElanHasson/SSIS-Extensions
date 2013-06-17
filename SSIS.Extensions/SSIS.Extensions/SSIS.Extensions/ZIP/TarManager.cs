using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.SqlServer.Dts.Runtime;
using System;
using System.Collections.Generic;
using System.IO;

namespace SSIS.Extensions.ZipTask
{
    public class TarManager
    { 
        #region Properties

        public string source { get; set; }
        public string target { get; set; }
        public string password { get; set; }
        public TarCompressionLevel tarLevel { get; set; }
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
        public TarManager(string source, string target, TarCompressionLevel tarLevel, string password, bool recursive, bool removeSouce, bool overwriteTarget, LogLevel loglevel, IDTSComponentEvents componentEvents)
        {
            this.source = source;
            this.target = target;
            this.tarLevel = tarLevel;
            this.password = password;
            this.recursive = recursive;
            this.removeSource = removeSource;
            this.fileFilter = String.IsNullOrEmpty(fileFilter) ? "true" : fileFilter;
            this.overwriteTarget = overwriteTarget;
            this.logLevel = logLevel;
            this.componentEvents = componentEvents;
        }

        #endregion

        #region Compress

        /// <summary>
        /// Compress a given file into Tar archive.
        /// </summary>
        public void Compress()
        {
            Common.ValidateOverwrite(this.overwriteTarget, this.target);
            DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(this.source));
            this.Log(String.Format("Archiving: [{0}] -> [{1}].", this.source, this.target), LogLevel.Minimal);

            if (tarLevel == TarCompressionLevel.None)
            {
                using (Stream outStream = File.Create(this.target))
                {
                    ArchiveFile(outStream, dir);
                }
            }
            else if (tarLevel == TarCompressionLevel.BZip2)
            {
                using (BZip2OutputStream bz2Stream = new BZip2OutputStream(File.Create(this.target), 9))
                {
                    ArchiveFile(bz2Stream, dir);
                }
            }
            else if (tarLevel == TarCompressionLevel.GZip)
            {
                using (GZipOutputStream gzoStream = new GZipOutputStream(File.Create(this.target)))
                {
                    gzoStream.SetLevel(9);
                    ArchiveFile(gzoStream, dir);
                }
            }

            this.Log(String.Format("Successfully Archived: [{0}] -> [{1}].", this.source, this.target), LogLevel.Minimal);
            Common.RemoveFile(this.removeSource, this.source);
        }

        /// <summary>
        /// Recurse through directory and place matching files into archive.
        /// </summary>
        /// <param name="outStream">The out stream.</param>
        /// <param name="dir">The dir.</param>
        private void ArchiveFile(Stream outStream, DirectoryInfo dir)
        {
            using (TarArchive tar = TarArchive.CreateOutputTarArchive(outStream))
            {
                tar.RootPath = dir.FullName.Replace("\\", "/");
                List<string> fileList = Common.GetFileList(this.source, this.fileFilter, this.recursive);
                if (fileList.Count == 0)
                    this.Log(String.Format("No files found matching the filter criteria."), LogLevel.Verbose);

                foreach (string filePath in fileList)
                {
                    this.Log(String.Format("Archiving: [{0}].", filePath), LogLevel.Verbose);
                    TarEntry entry = TarEntry.CreateEntryFromFile(filePath);
                    tar.WriteEntry(entry, true);
                }
            }
        }
        #endregion

        #region Decompress

        /// <summary>
        /// Decompress a Tar Archive.
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        public void Decompress()
        {
            Common.ValidateFile(this.source);

            if (Directory.Exists(this.target) && !this.overwriteTarget)
                throw new Exception(String.Format("Directory already exists: [{0}].", this.target));

            if (!Directory.Exists(this.target))
                Directory.CreateDirectory(this.target);

            this.Log(String.Format("Extracting Archive: [{0}] -> [{1}].", this.source, this.target), LogLevel.Minimal);

            using (Stream inStream = File.OpenRead(source))
            {
                if (tarLevel == TarCompressionLevel.None)
                    ExtractArchive(inStream);
                else if (tarLevel == TarCompressionLevel.BZip2)
                {
                    using (Stream bzipStream = new BZip2InputStream(inStream))
                    {
                        ExtractArchive(bzipStream);
                    }
                }
                else if (tarLevel == TarCompressionLevel.GZip)
                {
                    using (Stream gzipStream = new GZipInputStream(inStream))
                    {
                        ExtractArchive(gzipStream);
                    }
                }
            }

            this.Log(String.Format("Successfully Extracted: [{0}] -> [{1}].", this.source, this.target), LogLevel.Minimal);
            Common.RemoveFile(this.removeSource, this.source);
        }

        /// <summary>
        /// Extracts the archive.
        /// </summary>
        /// <param name="inStream">The in stream.</param>
        private void ExtractArchive(Stream inStream)
        {
            using (TarArchive tarArchive = TarArchive.CreateInputTarArchive(inStream))
            {
                tarArchive.ExtractContents(this.target);
            }
        }

        #endregion
    }
}
