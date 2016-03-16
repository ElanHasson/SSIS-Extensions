using System;
using Microsoft.SqlServer.Dts.Runtime;
using System.Xml;
using System.IO;

namespace SSIS.Extensions.ZipTask
{
    [DtsTask(DisplayName = "Zip Task",
             Description = "A custom task to Zip and UnZip files.",
             TaskType = "Zip Task",
             RequiredProductLevel = DTSProductLevel.None,
             UITypeName = "SSIS.Extensions.UI.ZipTask.ZipUI,SSIS.Extensions.UI.2012,Version=2.0.1.2,Culture=neutral,PublicKeyToken=be2dd18b41995f85",
             IconResource = "SSIS.Extensions.Assets.Zip.ico"
             )]
    public class ZipTask : Microsoft.SqlServer.Dts.Runtime.Task, IDTSComponentPersist
    {
        #region Properties
        
        public ZipFileAction fileAction { get; set; }
        public CompressionType compressionType { get; set; }
        public ZipCompressionLevel zipCompressionLevel { get; set; }
        public TarCompressionLevel tarCompressionLevel { get; set; }
        public string zipPassword { get; set; }
        public string sourceFile { get; set; }
        public bool removeSource { get; set; }
        public bool recursive { get; set; }
        public string targetFile { get; set; }
        public bool overwriteTarget { get; set; }
        public string msg { get; set; }
        public string fileFilter { get; set; }
        private LogLevel _logLevel = LogLevel.None;
        public LogLevel logLevel
        {
            get { return _logLevel; }
            set { _logLevel = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipTask"/> class.
        /// </summary>
        public ZipTask() { }

        #endregion

        #region Validate

        /// <summary>
        /// Validates the specified connections.
        /// </summary>
        /// <param name="connections">The connections.</param>
        /// <param name="variableDispenser">The variable dispenser.</param>
        /// <param name="componentEvents">The component events.</param>
        /// <param name="log">The log.</param>
        /// <returns></returns>
        public override DTSExecResult Validate(Connections connections, VariableDispenser variableDispenser, IDTSComponentEvents componentEvents, IDTSLogging log)
        {
            try
            {
                string _sourceFile = Common.GetVariableValue<string>(this.sourceFile, variableDispenser);
                string _targetFile = Common.GetVariableValue<string>(this.targetFile, variableDispenser);

                string errorMsg = String.Empty;
                if (String.IsNullOrEmpty(_sourceFile))
                    errorMsg += " Source,";

                if (String.IsNullOrEmpty(_targetFile))
                    errorMsg += " Target,";

                if (errorMsg.Trim().Length > 0)
                {
                    componentEvents.FireError(0, "", "Missing:" + errorMsg.Remove(errorMsg.Length - 1) + ".", "", 0);
                    return DTSExecResult.Failure;
                }
                
                return base.Validate(connections, variableDispenser, componentEvents, log);
            }
            catch (Exception ex)
            {
                componentEvents.FireError(0, "Validate: ", ex.Message + Environment.NewLine + ex.StackTrace, "", 0);
                return DTSExecResult.Failure;
            }
        }

        #endregion

        #region Execute

        /// <summary>
        /// Executes the specified action based on settings.
        /// </summary>
        /// <param name="connections">The connections.</param>
        /// <param name="variableDispenser">The variable dispenser.</param>
        /// <param name="componentEvents">The component events.</param>
        /// <param name="log">The log.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        public override DTSExecResult Execute(Connections connections, VariableDispenser variableDispenser, IDTSComponentEvents componentEvents, IDTSLogging log, object transaction)
        {
            try
            {
                string _sourceFile = Common.GetVariableValue<string>(this.sourceFile, variableDispenser);
                string _targetFile = Common.GetVariableValue<string>(this.targetFile, variableDispenser);
                string _password = Common.GetVariableValue<string>(this.zipPassword, variableDispenser);
                string _fileFilter = Common.GetVariableValue<string>(this.fileFilter, variableDispenser);
        
                if (this.compressionType == CompressionType.Zip)
                {
                    ZipManager zipManager = new ZipManager(_sourceFile, _targetFile,this.zipCompressionLevel, _password, this.recursive, _fileFilter, this.removeSource, this.overwriteTarget, this.logLevel, componentEvents);
                    if (this.fileAction == ZipFileAction.Compress)
                        zipManager.Zip();
                    else if (this.fileAction == ZipFileAction.Decompress)
                        zipManager.UnZip();
                }
                else if (this.compressionType == CompressionType.Tar)
                {
                    TarManager tarManager = new TarManager(_sourceFile, _targetFile, this.tarCompressionLevel, _password, this.recursive, this.removeSource, this.overwriteTarget, this.logLevel, componentEvents);
                    if (this.fileAction == ZipFileAction.Compress)
                        tarManager.Compress();
                    else if (this.fileAction == ZipFileAction.Decompress)
                        tarManager.Decompress();
                }
            }
            catch (Exception ex)
            {
                componentEvents.FireError(0, "Execute: ", ex.Message + Environment.NewLine + ex.StackTrace, "", 0);
                return DTSExecResult.Failure;
            }
            return DTSExecResult.Success;
        }
        #endregion
        
        #region IDTS Members

        /// <summary>
        /// Loads settings from XML.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="infoEvents">The info events.</param>
        /// <exception cref="System.Exception"></exception>
        void IDTSComponentPersist.LoadFromXML(System.Xml.XmlElement node, IDTSInfoEvents infoEvents)
        {
            try
            {
                //    This might occur if the task's XML has been modified outside of the Business Intelligence
                //    Or SQL Server Workbenches.
                if (node.Name != "ZipTask")
                {
                    throw new Exception(string.Format("Unexpected task element when loading task - {0}.", "ZipTask"));
                }
                else
                {
                    // let error bubble up
                    // populate the private property variables with values from the DTS node.
                    this.fileAction = (ZipFileAction)Enum.Parse(typeof(ZipFileAction), node.Attributes.GetNamedItem(CONSTANTS.ZIPFILEACTION).Value);
                    this.compressionType = (CompressionType)Enum.Parse(typeof(CompressionType), node.Attributes.GetNamedItem(CONSTANTS.ZIPCOMPRESSIONTYPE).Value);
                    this.zipCompressionLevel = (ZipCompressionLevel)Enum.Parse(typeof(ZipCompressionLevel), node.Attributes.GetNamedItem(CONSTANTS.ZIPCOMPRESSIONLEVEL).Value);
                    this.tarCompressionLevel = (TarCompressionLevel)Enum.Parse(typeof(TarCompressionLevel), node.Attributes.GetNamedItem(CONSTANTS.TARCOMPRESSIONLEVEL).Value);
                    this.sourceFile = node.Attributes.GetNamedItem(CONSTANTS.ZIPSOURCE).Value;
                    this.zipPassword = node.Attributes.GetNamedItem(CONSTANTS.ZIPPASSWORD).Value;
                    this.targetFile = node.Attributes.GetNamedItem(CONSTANTS.ZIPTARGET).Value;
                    this.removeSource = bool.Parse(node.Attributes.GetNamedItem(CONSTANTS.ZIPREMOVESOURCE).Value);
                    this.recursive = bool.Parse(node.Attributes.GetNamedItem(CONSTANTS.ZIPRECURSIVE).Value);
                    this.overwriteTarget = bool.Parse(node.Attributes.GetNamedItem(CONSTANTS.ZIPOVERWRITE).Value);
                    this.fileFilter = node.Attributes.GetNamedItem(CONSTANTS.ZIPFILEFILTER).Value;
                    this.logLevel = (LogLevel)Enum.Parse(typeof(LogLevel), node.GetAttribute(CONSTANTS.SFTPLOGLEVEL, String.Empty));
                }
            }
            catch (Exception ex)
            {                
                infoEvents.FireError(0, "Load From XML: ", ex.Message, "", 0);
            }
        }

        /// <summary>
        /// Saves settings to XML.
        /// </summary>
        /// <param name="doc">The doc.</param>
        /// <param name="infoEvents">The info events.</param>
        void IDTSComponentPersist.SaveToXML(System.Xml.XmlDocument doc, IDTSInfoEvents infoEvents)
        {
            try
            {
                XmlElement taskElement = doc.CreateElement(string.Empty, "ZipTask", string.Empty);
                doc.AppendChild(taskElement);

                taskElement.SetAttribute(CONSTANTS.ZIPFILEACTION, null, this.fileAction.ToString());
                taskElement.SetAttribute(CONSTANTS.ZIPCOMPRESSIONTYPE, null, this.compressionType.ToString());
                taskElement.SetAttribute(CONSTANTS.ZIPCOMPRESSIONLEVEL, null, this.zipCompressionLevel.ToString());
                taskElement.SetAttribute(CONSTANTS.TARCOMPRESSIONLEVEL, null, this.tarCompressionLevel.ToString());
                taskElement.SetAttribute(CONSTANTS.ZIPPASSWORD, null, this.zipPassword);
                taskElement.SetAttribute(CONSTANTS.ZIPSOURCE, null, this.sourceFile);
                taskElement.SetAttribute(CONSTANTS.ZIPTARGET, null, this.targetFile);
                taskElement.SetAttribute(CONSTANTS.ZIPREMOVESOURCE, null, this.removeSource.ToString());
                taskElement.SetAttribute(CONSTANTS.ZIPRECURSIVE, null, this.recursive.ToString());
                taskElement.SetAttribute(CONSTANTS.ZIPOVERWRITE, null, this.overwriteTarget.ToString());
                taskElement.SetAttribute(CONSTANTS.ZIPFILEFILTER, null, this.fileFilter);
                taskElement.SetAttribute(CONSTANTS.ZIPLOGLEVEL, null, this.logLevel.ToString());
            }
            catch (Exception ex)
            {
                infoEvents.FireError(0, "Save To XML: ", ex.Message + ex.StackTrace, "", 0);
            }
        }

        #endregion
    }
}
