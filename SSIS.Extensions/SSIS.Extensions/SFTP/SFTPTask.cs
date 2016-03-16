using Microsoft.SqlServer.Dts.Runtime;
using SSIS.Extensions.SFTP;
using System;
using System.Collections.Generic;
using System.Xml;

namespace SSIS.Extensions.SFTPTask
{
    [DtsTask(DisplayName = "SFTP Task",
             Description = "A custom task to Securely FTP files.",
             TaskType = "SFTP Task",
             RequiredProductLevel = DTSProductLevel.None,
             UITypeName = "SSIS.Extensions.UI.SFTPTask.SFTPUI,SSIS.Extensions.UI.2012,Version=2.0.1.2,Culture=neutral,PublicKeyToken=be2dd18b41995f85",
             IconResource = "SSIS.Extensions.Assets.SFTPTask.ico"
             )]
    public class SFTPTask : Microsoft.SqlServer.Dts.Runtime.Task, IDTSComponentPersist
    {
        #region Properties
        
        public string localFile { get; set; }
        public string remoteFile { get; set; }
        public SFTPFileAction fileAction { get; set; }
        public string fileInfo { get; set; }
        public bool overwriteDest { get; set; }
        public bool removeSource { get; set; }
        public bool isRecursive { get; set; }
        public bool stopOnFailure { get; set; }
        public string fileFilter { get; set; }
        public string hostName { get; set; }
        public string portNumber { get; set; }
        public string userName { get; set; }
        public string passWord { get; set; }
        public string remoteFileListVariable { get; set; }

        //private int _reTries = 1;
        //public int reTries {
        //    get { return _reTries; }
        //    set { _reTries = value; }
        //}

        private LogLevel _logLevel = LogLevel.None;
        public LogLevel logLevel
        {
            get { return _logLevel; }
            set { _logLevel = value; }
        }

        #endregion

        #region Constructor

        public SFTPTask() { }

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
                string _localFile = Common.GetVariableValue<string>(this.localFile, variableDispenser);
                string _remoteFile = Common.GetVariableValue<string>(this.remoteFile, variableDispenser);
                string _hostName = Common.GetVariableValue<string>(this.hostName, variableDispenser);
                string _portNumber = Common.GetVariableValue<string>(this.portNumber, variableDispenser);
                string _userName = Common.GetVariableValue<string>(this.userName, variableDispenser);
                string _passWord = Common.GetVariableValue<string>(this.passWord, variableDispenser);
                string _fileListVar = Common.GetVariableValue<string>(this.remoteFileListVariable, variableDispenser);
                TypeCode _fileListVarType = Common.GetVariableType(this.remoteFileListVariable, variableDispenser);
                string _fileInfoVar = Common.GetVariableValue<string>(this.fileInfo, variableDispenser);
                TypeCode _fileInfoType = Common.GetVariableType(this.fileInfo, variableDispenser);

                string errorMsg = String.Empty;

                if (String.IsNullOrEmpty(_hostName))
                    errorMsg += " Host Name,";

                if (String.IsNullOrEmpty(_portNumber))
                    errorMsg += " Port Number,";
                else
                {
                    int p;
                    if (Int32.TryParse(_portNumber, out p) == false)
                        errorMsg += " Port Number(must be a valid integer),";
                }

                if (String.IsNullOrEmpty(_userName))
                    errorMsg += " User Name,";

                if (String.IsNullOrEmpty(_passWord))
                    errorMsg += " Password,";

                if (String.IsNullOrEmpty(_localFile) && (this.fileAction == SFTPFileAction.Send || this.fileAction == SFTPFileAction.Receive))
                    errorMsg += " Local File,";

                if (String.IsNullOrEmpty(_remoteFile) && (this.fileAction == SFTPFileAction.Send || this.fileAction == SFTPFileAction.Receive || this.fileAction == SFTPFileAction.List))
                    errorMsg += " Remote File,";
                
                if (this.fileAction == SFTPFileAction.List && String.IsNullOrEmpty(_fileListVar))
                    errorMsg += " Result Variable,";
                else if (this.fileAction == SFTPFileAction.List && _fileListVarType != TypeCode.Object)
                    errorMsg += " Result Variable(must be of type Object),";

                if (this.fileAction == SFTPFileAction.SendMultiple || this.fileAction == SFTPFileAction.ReceiveMultiple)
                {
                    if (String.IsNullOrEmpty(_fileInfoVar))
                        errorMsg += " Data Set Variable,";
                    else if (_fileInfoType != TypeCode.Object)
                        errorMsg += " Data Set Variable(must be of type Object),";
                }

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
        /// Executes the action based on seleted options.
        /// </summary>
        /// <param name="connections">The connections.</param>
        /// <param name="variableDispenser">The variable dispenser.</param>
        /// <param name="componentEvents">The component events.</param>
        /// <param name="log">The log.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Data Set Variable Must Contain a Valid List<ISFTPFileInfo> Object.</exception>
        public override DTSExecResult Execute(Connections connections, VariableDispenser variableDispenser, IDTSComponentEvents componentEvents, IDTSLogging log, object transaction)
        {
            try
            {
                bool fireAgain = true;
                string _localFile = Common.GetVariableValue<string>(this.localFile, variableDispenser);
                string _remoteFile = Common.GetVariableValue<string>(this.remoteFile, variableDispenser);
                string _hostName = Common.GetVariableValue<string>(this.hostName, variableDispenser);
                int _portNumber = Common.GetVariableValue<int>(this.portNumber, variableDispenser);
                string _userName = Common.GetVariableValue<string>(this.userName, variableDispenser);
                string _passWord = Common.GetVariableValue<string>(this.passWord, variableDispenser);
                string _fileFilter = Common.GetVariableValue<string>(this.fileFilter, variableDispenser);

                List<ISFTPFileInfo> sftpFileInfo = new List<ISFTPFileInfo>();
                SFTPConnection sftp = new SFTPConnection(_hostName, _userName, _passWord, _portNumber, this.stopOnFailure, componentEvents, this.logLevel);

                if (this.fileAction == SFTPFileAction.ReceiveMultiple || this.fileAction == SFTPFileAction.SendMultiple)
                {
                    if (Common.GetVariableValue<object>(fileInfo, variableDispenser).GetType() != typeof(List<ISFTPFileInfo>))
                        throw new Exception("Data Set Variable Must Contain a Valid List<ISFTPFileInfo> Object.");
                }

                if (this.fileAction == SFTPFileAction.Send)
                {
                    List<string> fileList = Common.GetFileList(_localFile, _fileFilter, this.isRecursive);
                    foreach (string fileName in fileList)
                        sftpFileInfo.Add(new SFTPFileInfo(fileName, _remoteFile, this.overwriteDest, this.removeSource));

                    if (sftpFileInfo.Count > 0)
                        sftp.UploadFiles(sftpFileInfo);
                    else
                        componentEvents.FireInformation(1, "", "No files selected for Upload.", "", 1, ref fireAgain);
                }
                else if (this.fileAction == SFTPFileAction.SendMultiple)
                {
                    sftpFileInfo = (List<ISFTPFileInfo>)Common.GetVariableValue<object>(fileInfo, variableDispenser);
                    if (sftpFileInfo.Count > 0)
                        sftp.UploadFiles(sftpFileInfo);
                    else
                        componentEvents.FireInformation(1, "", "No files selected for Upload.", "", 1, ref fireAgain);
                }
                else if (this.fileAction == SFTPFileAction.Receive)
                {
                    List<IRemoteFileInfo> remoteFileList = sftp.ListFiles(_remoteFile);
                    remoteFileList = Common.GetRemoteFileList(remoteFileList, _fileFilter);
                    foreach (IRemoteFileInfo remoteFile in remoteFileList)
                        sftpFileInfo.Add(new SFTPFileInfo(_localFile, remoteFile.FullName, this.overwriteDest, this.removeSource));

                    if (sftpFileInfo.Count > 0)
                        sftp.DownloadFiles(sftpFileInfo);
                    else
                        componentEvents.FireInformation(1, "", "No files selected for Download.", "", 1, ref fireAgain);
                }
                else if (this.fileAction == SFTPFileAction.ReceiveMultiple)
                {
                    sftpFileInfo = (List<ISFTPFileInfo>)Common.GetVariableValue<object>(fileInfo, variableDispenser);
                    if (sftpFileInfo.Count > 0)
                        sftp.DownloadFiles(sftpFileInfo);
                    else
                        componentEvents.FireInformation(1, "", "No files selected for Download.", "", 1, ref fireAgain);
                }
                else if (this.fileAction == SFTPFileAction.List)
                {
                    List<IRemoteFileInfo> remoteFileList = sftp.ListFiles(_remoteFile);
                    Common.SetVariableValue(this.remoteFileListVariable, remoteFileList, variableDispenser);
                }
            }
            catch (Exception ex)
            {
                componentEvents.FireError(0, "Execute: ", ex.Message +Environment.NewLine + ex.StackTrace, "", 0);
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
            //TaskProperties prop = this.Properties;

            try
            {
                //    This might occur if the task's XML has been modified outside of the Business Intelligence
                //    Or SQL Server Workbenches.
                if (node.Name != "SFTPTask")
                {
                    throw new Exception(string.Format("Unexpected task element when loading task - {0}.", "SFTPTask"));
                }
                else
                {
                    // let error bubble up
                    // populate the private property variables with values from the DTS node.
                    this.localFile = node.GetAttribute(CONSTANTS.SFTPLOCALFILE, String.Empty);
                    this.remoteFile = node.GetAttribute(CONSTANTS.SFTPREMOTEFILE, String.Empty);
                    this.fileAction = (SFTPFileAction)Enum.Parse(typeof(SFTPFileAction), node.GetAttribute(CONSTANTS.SFTPFILEACTION, String.Empty));
                    this.fileInfo = node.GetAttribute(CONSTANTS.SFTPFILEINFO, String.Empty);
                    this.overwriteDest = Convert.ToBoolean(node.GetAttribute(CONSTANTS.SFTPOVERWRITEDEST, String.Empty));
                    this.removeSource = Convert.ToBoolean(node.GetAttribute(CONSTANTS.SFTPREMOVESOURCE, String.Empty));
                    this.isRecursive = Convert.ToBoolean(node.GetAttribute(CONSTANTS.SFTPISRECURSIVE, String.Empty));
                    this.fileFilter = node.GetAttribute(CONSTANTS.SFTPFILEFILTER, String.Empty);
                    //this.reTries = Convert.ToInt32(node.GetAttribute(CONSTANTS.SFTPRETRIES, String.Empty));
                    this.hostName = node.GetAttribute(CONSTANTS.SFTPHOST, String.Empty);
                    this.portNumber = node.GetAttribute(CONSTANTS.SFTPPORT, String.Empty);
                    this.userName = node.GetAttribute(CONSTANTS.SFTPUSER, String.Empty);
                    this.passWord = node.GetAttribute(CONSTANTS.SFTPPASSWORD, String.Empty);
                    this.stopOnFailure = Convert.ToBoolean(node.GetAttribute(CONSTANTS.SFTPSTOPONFAILURE, String.Empty));
                    this.remoteFileListVariable = node.GetAttribute(CONSTANTS.SFTPREMOTEFILELISTVAR, String.Empty);
                    this.logLevel = (LogLevel)Enum.Parse(typeof(LogLevel), node.GetAttribute(CONSTANTS.SFTPLOGLEVEL, String.Empty));
                }
            }
            catch (Exception ex)
            {
                infoEvents.FireError(0, "Load From XML: ", ex.Message + Environment.NewLine + ex.StackTrace, "", 0);
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
                //create node in the package xml document
                XmlElement taskElement = doc.CreateElement(string.Empty, "SFTPTask", string.Empty);
                doc.AppendChild(taskElement);
                taskElement.SetAttribute(CONSTANTS.SFTPLOCALFILE, null, this.localFile);
                taskElement.SetAttribute(CONSTANTS.SFTPREMOTEFILE, null, this.remoteFile);
                taskElement.SetAttribute(CONSTANTS.SFTPFILEACTION, null, this.fileAction.ToString());
                taskElement.SetAttribute(CONSTANTS.SFTPFILEINFO, null, this.fileInfo);
                taskElement.SetAttribute(CONSTANTS.SFTPOVERWRITEDEST, null, this.overwriteDest.ToString());
                taskElement.SetAttribute(CONSTANTS.SFTPREMOVESOURCE, null, this.removeSource.ToString());
                taskElement.SetAttribute(CONSTANTS.SFTPISRECURSIVE, null, this.isRecursive.ToString());
                taskElement.SetAttribute(CONSTANTS.SFTPFILEFILTER, null, this.fileFilter);
                //taskElement.SetAttribute(CONSTANTS.SFTPRETRIES, null, this.reTries.ToString());
                taskElement.SetAttribute(CONSTANTS.SFTPHOST, null, this.hostName);
                taskElement.SetAttribute(CONSTANTS.SFTPPORT, null, this.portNumber);
                taskElement.SetAttribute(CONSTANTS.SFTPUSER, null, this.userName);
                taskElement.SetAttribute(CONSTANTS.SFTPPASSWORD, null, this.passWord);
                taskElement.SetAttribute(CONSTANTS.SFTPSTOPONFAILURE, null, this.stopOnFailure.ToString());
                taskElement.SetAttribute(CONSTANTS.SFTPREMOTEFILELISTVAR, null, this.remoteFileListVariable);
                taskElement.SetAttribute(CONSTANTS.SFTPLOGLEVEL, null, this.logLevel.ToString());
            }
            catch (Exception ex)
            {
                infoEvents.FireError(0, "Save To XML: ", ex.Message + Environment.NewLine + ex.StackTrace, "", 0);
            }
        }

        #endregion
    }
}

#region x
//object obj = null;
//obj = Common.getVariableValueobj(this.dataSetName, variableDispenser); //var[this.dataSetName].Value;
//DataSet ds = obj as DataSet;
//DataTable dt = obj as DataTable;
//Recordset rs = obj as Recordset;

//if (ds != null)
//{
//    if (ds.Tables.Count > 0)
//        dt = ds.Tables[0];
//}
//else if (rs != null)
//{
//    OleDbDataAdapter oldb = new OleDbDataAdapter();
//    dt = new DataTable();
//    oldb.Fill(dt, rs);
//}

//bool fireAgain = true;
//foreach (DataRow row in dt.Rows)
//    componentEvents.FireInformation(0, "Item: ", row[0].ToString(), "", 0, ref fireAgain);


//return DTSExecResult.Success;
#endregion