using Microsoft.SqlServer.Dts.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace SSIS.Extensions.UI.SFTPTask
{
    public partial class SFTPUIForm : Form
    {
        private TaskHost _taskHost;

        #region Constructor
        public SFTPUIForm(TaskHost taskHost)
        {
            InitializeComponent();
            this._taskHost = taskHost;
        }
        #endregion

        #region Form Load
     
        /// <summary>
        /// Set default property values.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void SFTPUIForm_Load(object sender, EventArgs e)
        {
            try
            {
                PropertyBag prop = new PropertyBag();

                if (_taskHost != null)
                {
                    /* Add User Variables to the Dropdown. */
                    List<string> vars = _taskHost.Variables.OfType<Variable>().Where(x => x.SystemVariable == false).Select(x => x.QualifiedName).ToList<string>();
                    UserVariables.ListOfUserVariables = new string[vars.Count];
                    UserVariables.ListOfUserVariables = vars.ToArray();

                    prop.name = _taskHost.Name;
                    prop.description = _taskHost.Description;
                    prop.localFile = _taskHost.GetValue<string>(CONSTANTS.SFTPLOCALFILE);
                    prop.remoteFile = _taskHost.GetValue<string>(CONSTANTS.SFTPREMOTEFILE);
                    prop.remoteFileListVariable = _taskHost.GetValue<string>(CONSTANTS.SFTPREMOTEFILELISTVAR);
                    prop.hostName = _taskHost.GetValue<string>(CONSTANTS.SFTPHOST);
                    prop.portNumber = _taskHost.GetValue<string>(CONSTANTS.SFTPPORT);
                    prop.userName = _taskHost.GetValue<string>(CONSTANTS.SFTPUSER);
                    prop.passWord = _taskHost.GetValue<string>(CONSTANTS.SFTPPASSWORD);
                    prop.stopOnFailure = _taskHost.GetValue<bool>(CONSTANTS.SFTPSTOPONFAILURE);
                    prop.localOverwrite = _taskHost.GetValue<bool>(CONSTANTS.SFTPOVERWRITEDEST);
                    prop.remoteOverwrite = _taskHost.GetValue<bool>(CONSTANTS.SFTPOVERWRITEDEST);
                    prop.localRemove = _taskHost.GetValue<bool>(CONSTANTS.SFTPREMOVESOURCE);
                    prop.remoteRemove = _taskHost.GetValue<bool>(CONSTANTS.SFTPREMOVESOURCE);
                    //prop.reTries = Convert.ToInt32(GetValue(CONSTANTS.SFTPRETRIES));
                    prop.fileAction = (SFTPFileAction)Enum.Parse(typeof(SFTPFileAction), _taskHost.GetValue<string>(CONSTANTS.SFTPFILEACTION));
                    prop.sftpFileInfo = _taskHost.GetValue<string>(CONSTANTS.SFTPFILEINFO);
                    prop.localIncludeSubFolders = _taskHost.GetValue<bool>(CONSTANTS.SFTPISRECURSIVE);
                    prop.localFilter = _taskHost.GetValue<string>(CONSTANTS.SFTPFILEFILTER);
                    prop.remoteFilter = _taskHost.GetValue<string>(CONSTANTS.SFTPFILEFILTER);
                    prop.logLevel = (LogLevel)Enum.Parse(typeof(LogLevel), _taskHost.GetValue<string>(CONSTANTS.SFTPLOGLEVEL));
                }

                this.propertyGrid.SelectedObject = prop;
            }
            catch (Exception ex)
            {
                //IDTSComponentEvents ce = (IDTSComponentEvents)_taskHost.Properties[CONSTANTS.COMPONENTEVENTS].GetValue(_taskHost);
                //ce.FireError(1, "PGP Task UI Form", ex.Message, "", 0);
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region Control Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Handles the Click event of the btnOK control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                PropertyBag prop = (PropertyBag)this.propertyGrid.SelectedObject;
                _taskHost.Name = prop.name;
                _taskHost.Description = prop.description;
                _taskHost.SetValue(CONSTANTS.SFTPLOCALFILE, prop.localFile);
                _taskHost.SetValue(CONSTANTS.SFTPREMOTEFILE, prop.remoteFile);
                _taskHost.SetValue(CONSTANTS.SFTPREMOTEFILELISTVAR, prop.remoteFileListVariable);
                _taskHost.SetValue(CONSTANTS.SFTPHOST, prop.hostName);
                _taskHost.SetValue(CONSTANTS.SFTPPORT, prop.portNumber);
                _taskHost.SetValue(CONSTANTS.SFTPUSER, prop.userName);
                _taskHost.SetValue(CONSTANTS.SFTPPASSWORD, prop.passWord);
                _taskHost.SetValue(CONSTANTS.SFTPSTOPONFAILURE, prop.stopOnFailure);
                _taskHost.SetValue(CONSTANTS.SFTPFILEACTION, prop.fileAction);
                //_taskHost.SetValue(CONSTANTS.SFTPRETRIES, prop.reTries);
                _taskHost.SetValue(CONSTANTS.SFTPFILEINFO, prop.sftpFileInfo);
                _taskHost.SetValue(CONSTANTS.SFTPLOGLEVEL, prop.logLevel);

                if (prop.fileAction == SFTPFileAction.Send || prop.fileAction == SFTPFileAction.SendMultiple)
                {
                    _taskHost.SetValue(CONSTANTS.SFTPFILEFILTER, prop.localFilter);
                    _taskHost.SetValue(CONSTANTS.SFTPOVERWRITEDEST, prop.remoteOverwrite);
                    _taskHost.SetValue(CONSTANTS.SFTPREMOVESOURCE, prop.localRemove);
                    _taskHost.SetValue(CONSTANTS.SFTPISRECURSIVE, prop.localIncludeSubFolders);
                }
                else
                {
                    _taskHost.SetValue(CONSTANTS.SFTPFILEFILTER, prop.remoteFilter);
                    _taskHost.SetValue(CONSTANTS.SFTPOVERWRITEDEST, prop.localOverwrite);
                    _taskHost.SetValue(CONSTANTS.SFTPREMOVESOURCE, prop.remoteRemove);
                }

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            this.propertyGrid.Refresh();
        }

        #endregion       
    }
}
