using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Runtime;
using SSIS.Extensions;

namespace SSIS.Extensions.UI.PGPTask
{
    public partial class PGPUIForm : Form
    {
        private TaskHost _taskHost;

        #region Constructor
        public PGPUIForm(TaskHost taskHost)
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
        private void PGPUIForm_Load(object sender, EventArgs e)
        {
            try
            {
                PropertiyBag prop = new PropertiyBag();

                if (_taskHost != null)
                {
                    /* Add User Variables to the Dropdown. */
                    List<string> vars = _taskHost.Variables.OfType<Variable>().Where(x => x.SystemVariable == false).Select(x => x.QualifiedName).ToList<string>();
                    UserVariables.ListOfUserVariables = new string[vars.Count];
                    UserVariables.ListOfUserVariables = vars.ToArray();

                    prop.name = _taskHost.Name;
                    prop.description = _taskHost.Description;
                    prop.sourceFile = _taskHost.GetValue<string>(CONSTANTS.PGPSOURCEFILE);
                    prop.targetFile = _taskHost.GetValue<string>(CONSTANTS.PGPTARGETFILE);
                    prop.publicKey = _taskHost.GetValue<string>(CONSTANTS.PGPPUBLICKEY);
                    prop.privateKey = _taskHost.GetValue<string>(CONSTANTS.PGPPRIVATEKEY);
                    prop.passPhrase = _taskHost.GetValue<string>(CONSTANTS.PGPPASSPHRASE);
                    prop.overwriteTarget = _taskHost.GetValue<bool>(CONSTANTS.PGPOVERWRITETARGET);
                    prop.removeSource = _taskHost.GetValue<bool>(CONSTANTS.PGPREMOVESOURCE);
                    prop.fileAction = (PGPFileAction)Enum.Parse(typeof(PGPFileAction), _taskHost.GetValue<string>(CONSTANTS.PGPFILEACTION));
                    prop.isArmored = _taskHost.GetValue<bool>(CONSTANTS.PGPARMORED);
                    //prop.logLevel = (LogLevel)Enum.Parse(typeof(LogLevel), _taskHost.GetValue<string>(CONSTANTS.PGPLOGLEVEL));
                }

                this.propertyGrid.SelectedObject = prop;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
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

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                PropertiyBag prop = (PropertiyBag)this.propertyGrid.SelectedObject;
                _taskHost.SetValue(CONSTANTS.PGPSOURCEFILE, prop.sourceFile);
                _taskHost.SetValue(CONSTANTS.PGPTARGETFILE, prop.targetFile);
                _taskHost.SetValue(CONSTANTS.PGPPUBLICKEY, prop.publicKey);
                _taskHost.SetValue(CONSTANTS.PGPPRIVATEKEY, prop.privateKey);
                _taskHost.SetValue(CONSTANTS.PGPPASSPHRASE, prop.passPhrase);
                _taskHost.SetValue(CONSTANTS.PGPOVERWRITETARGET, (bool)prop.overwriteTarget);
                _taskHost.SetValue(CONSTANTS.PGPREMOVESOURCE, (bool)prop.removeSource);
                _taskHost.SetValue(CONSTANTS.PGPARMORED, (bool)prop.isArmored);
                _taskHost.SetValue(CONSTANTS.PGPFILEACTION, prop.fileAction);
                //_taskHost.Properties[CONSTANTS.PGPLOGLEVEL, prop.logLevel);
                _taskHost.Name = prop.name;
                _taskHost.Description = prop.description;

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        /// <summary>
        /// Handles the PropertyValueChanged event of the propertyGrid control.
        /// </summary>
        /// <param name="s">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyValueChangedEventArgs"/> instance containing the event data.</param>
        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            this.propertyGrid.Refresh();
        }

        #endregion
    }
}
