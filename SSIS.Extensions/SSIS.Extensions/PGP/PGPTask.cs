using System;
using System.Xml;
using Microsoft.SqlServer.Dts.Runtime;

namespace SSIS.Extensions.PGPTask
{
    [DtsTask(DisplayName = "PGP Task",
             Description = "A custom task to Encrypt and Decrypt Files using PGP.",
             TaskType = "PGP Task",
             RequiredProductLevel = DTSProductLevel.None,
             UITypeName = "SSIS.Extensions.UI.PGPTask.PGPUI,SSIS.Extensions.UI.2012,Version=2.0.1.2,Culture=neutral,PublicKeyToken=be2dd18b41995f85",
             IconResource = "SSIS.Extensions.Assets.PGP.ico"
             )]
    public class PGPTask : Microsoft.SqlServer.Dts.Runtime.Task, IDTSComponentPersist
    {
        #region Properties
        
        public string sourceFile { get; set; }
        public string targetFile { get; set; }
        public string publicKey { get; set; }
        public string privateKey { get; set; }
        public string passPhrase { get; set; }
        public PGPFileAction fileAction { get; set; }
        public bool overwriteTarget { get; set; }
        public bool removeSource { get; set; }
        public bool isArmored { get; set; }
        //private LogLevel _logLevel = LogLevel.None;
        //public LogLevel logLevel
        //{
        //    get { return _logLevel; }
        //    set { _logLevel = value; }
        //}   

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PGPTask"/> class.
        /// </summary>
        public PGPTask() { }

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
                string sourceFile = Common.GetVariableValue<string>(this.sourceFile, variableDispenser);
                string targetFile = Common.GetVariableValue<string>(this.targetFile, variableDispenser);
                string publicKey = Common.GetVariableValue<string>(this.publicKey, variableDispenser);
                string privateKey = Common.GetVariableValue<string>(this.privateKey, variableDispenser);
                string passPhrase = Common.GetVariableValue<string>(this.passPhrase, variableDispenser);

                string errorMsg = String.Empty;
                if (String.IsNullOrEmpty(sourceFile))
                    errorMsg += " Source File,";

                if (String.IsNullOrEmpty(targetFile) || targetFile.Trim().Length == 0)
                    errorMsg += " Target File,";

                if (String.IsNullOrEmpty(publicKey) && this.fileAction != PGPFileAction.Decrypt)
                    errorMsg += " Public Key,";

                if (String.IsNullOrEmpty(privateKey) && this.fileAction != PGPFileAction.Encrypt)
                    errorMsg += " Private Key for Decryption and Encryption with signature,";

                if (String.IsNullOrEmpty(passPhrase) && this.fileAction != PGPFileAction.Encrypt)
                    errorMsg += " Pass Phrase for Decryption and Encryption with signature,\n";

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
        /// Executes the specified actions based on settings.
        /// </summary>
        /// <param name="connections">The connections.</param>
        /// <param name="variableDispenser">The variable dispenser.</param>
        /// <param name="componentEvents">The component events.</param>
        /// <param name="log">The log.</param>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        public override DTSExecResult Execute(Connections connections, VariableDispenser variableDispenser, IDTSComponentEvents componentEvents, IDTSLogging log, object transaction)
        {
            bool fireAgain = true;
            try
            {
                string sourceFile = Common.GetVariableValue<string>(this.sourceFile, variableDispenser);
                string targetFile = Common.GetVariableValue<string>(this.targetFile, variableDispenser);
                string publicKey = Common.GetVariableValue<string>(this.publicKey, variableDispenser);
                string privateKey = Common.GetVariableValue<string>(this.privateKey, variableDispenser);
                string passPhrase = Common.GetVariableValue<string>(this.passPhrase, variableDispenser);

                if (this.fileAction == PGPFileAction.Decrypt)
                {
                    componentEvents.FireInformation(1, "", String.Format("Decrypting file [{0}] -> [{1}]", sourceFile, targetFile), "", 0, ref fireAgain);
                    PGPManager.Decrypt(sourceFile, privateKey, passPhrase, targetFile, this.overwriteTarget,this.removeSource);
                    componentEvents.FireInformation(1, "", String.Format("Successfully Decrypted file [{0}] -> [{1}]. ", sourceFile, targetFile), "", 0, ref fireAgain);
                }
                else if (this.fileAction == PGPFileAction.Encrypt)
                {
                    componentEvents.FireInformation(1, "", String.Format("Encrypting file [{0}] -> [{1}]", sourceFile, targetFile), "", 0, ref fireAgain);
                    PGPManager.Encrypt(sourceFile, publicKey, targetFile, this.overwriteTarget, this.removeSource, this.isArmored);
                    componentEvents.FireInformation(1, "", String.Format("Sucessfully Encrypted file [{0}] -> [{1}]", sourceFile, targetFile), "", 0, ref fireAgain);
                }
                else if (this.fileAction == PGPFileAction.EncryptAndSign)
                {
                    componentEvents.FireInformation(1, "", String.Format("Encrypting and Signing file [{0}] -> [{1}]", sourceFile, targetFile), "", 0, ref fireAgain);
                    PGPManager.EncryptAndSign(sourceFile, publicKey, privateKey, passPhrase, targetFile, this.overwriteTarget, this.removeSource, this.isArmored);
                    componentEvents.FireInformation(1, "", String.Format("Sucessfully Encrypted and Signed file [{0}] -> [{1}]", sourceFile, targetFile), "", 0, ref fireAgain);
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
                if (node.Name != "PGPTask")
                {
                    throw new Exception(string.Format("Unexpected task element when loading task - {0}.", "PGPTask"));
                }
                else
                {
                    // let error bubble up
                    // populate the private property variables with values from the DTS node.
                    this.sourceFile = node.Attributes.GetNamedItem(CONSTANTS.PGPSOURCEFILE).Value;
                    this.targetFile = node.Attributes.GetNamedItem(CONSTANTS.PGPTARGETFILE).Value;
                    this.publicKey = node.Attributes.GetNamedItem(CONSTANTS.PGPPUBLICKEY).Value;
                    this.privateKey = node.Attributes.GetNamedItem(CONSTANTS.PGPPRIVATEKEY).Value;
                    this.passPhrase = node.Attributes.GetNamedItem(CONSTANTS.PGPPASSPHRASE).Value;
                    this.fileAction = (PGPFileAction)Enum.Parse(typeof(PGPFileAction), node.Attributes.GetNamedItem(CONSTANTS.PGPFILEACTION).Value);
                    this.overwriteTarget = bool.Parse(node.Attributes.GetNamedItem(CONSTANTS.PGPOVERWRITETARGET).Value);
                    this.removeSource = bool.Parse(node.Attributes.GetNamedItem(CONSTANTS.PGPREMOVESOURCE).Value);
                    this.isArmored = bool.Parse(node.Attributes.GetNamedItem(CONSTANTS.PGPARMORED).Value);
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
                //create node in the package xml document
                XmlElement taskElement = doc.CreateElement(string.Empty, "PGPTask", string.Empty);

                XmlAttribute sourceAttr = doc.CreateAttribute(string.Empty, CONSTANTS.PGPSOURCEFILE, string.Empty);
                sourceAttr.Value = this.sourceFile;
                taskElement.Attributes.Append(sourceAttr);

                XmlAttribute targetAttr = doc.CreateAttribute(string.Empty, CONSTANTS.PGPTARGETFILE, string.Empty);
                targetAttr.Value = this.targetFile;
                taskElement.Attributes.Append(targetAttr);

                XmlAttribute publicAttr = doc.CreateAttribute(string.Empty, CONSTANTS.PGPPUBLICKEY, string.Empty);
                publicAttr.Value = this.publicKey;
                taskElement.Attributes.Append(publicAttr);

                XmlAttribute privateAttr = doc.CreateAttribute(string.Empty, CONSTANTS.PGPPRIVATEKEY, string.Empty);
                privateAttr.Value = this.privateKey;
                taskElement.Attributes.Append(privateAttr);

                XmlAttribute passAttr = doc.CreateAttribute(string.Empty, CONSTANTS.PGPPASSPHRASE, string.Empty);
                passAttr.Value = this.passPhrase;
                taskElement.Attributes.Append(passAttr);

                XmlAttribute fileActionAttr = doc.CreateAttribute(string.Empty, CONSTANTS.PGPFILEACTION, string.Empty);
                fileActionAttr.Value = this.fileAction.ToString();
                taskElement.Attributes.Append(fileActionAttr);

                XmlAttribute overwriteRemoteAttr = doc.CreateAttribute(string.Empty, CONSTANTS.PGPOVERWRITETARGET, string.Empty);
                overwriteRemoteAttr.Value = this.overwriteTarget.ToString();
                taskElement.Attributes.Append(overwriteRemoteAttr);

                XmlAttribute removeAttr = doc.CreateAttribute(string.Empty, CONSTANTS.PGPREMOVESOURCE, string.Empty);
                removeAttr.Value = this.removeSource.ToString();
                taskElement.Attributes.Append(removeAttr);

                XmlAttribute armoredAttr = doc.CreateAttribute(string.Empty, CONSTANTS.PGPARMORED, string.Empty);
                armoredAttr.Value = this.isArmored.ToString();
                taskElement.Attributes.Append(armoredAttr);

                //add the new element to the package document
                doc.AppendChild(taskElement);
            }
            catch (Exception ex)
            {
                infoEvents.FireError(0, "Save To XML: ", ex.Message + ex.StackTrace, "", 0);
            }
        }

        #endregion
    }
}
