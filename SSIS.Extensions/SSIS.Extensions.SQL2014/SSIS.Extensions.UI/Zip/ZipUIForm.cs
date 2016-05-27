using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Runtime;
using SSIS.Extensions.UI;

namespace SSIS.Extensions.UI.ZipTask
{
    public partial class ZipUIForm : Form
    {
        private TaskHost _taskHost;

        #region Constructor
        public ZipUIForm(TaskHost taskHost)
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
        private void ZipUIForm_Load(object sender, EventArgs e)
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
                    prop.fileAction = (ZipFileAction)Enum.Parse(typeof(ZipFileAction), _taskHost.GetValue<string>(CONSTANTS.ZIPFILEACTION));
                    prop.compressionType = (CompressionType)Enum.Parse(typeof(CompressionType), _taskHost.GetValue<string>(CONSTANTS.ZIPCOMPRESSIONTYPE));
                    prop.zipCompressionLevel = (ZipCompressionLevel)Enum.Parse(typeof(ZipCompressionLevel), _taskHost.GetValue<string>(CONSTANTS.ZIPCOMPRESSIONLEVEL));
                    prop.tarCompressionLevel = (TarCompressionLevel)Enum.Parse(typeof(TarCompressionLevel), _taskHost.GetValue<string>(CONSTANTS.TARCOMPRESSIONLEVEL));
                    prop.zipPassword = _taskHost.GetValue<string>(CONSTANTS.ZIPPASSWORD);
                    prop.sourceFile = _taskHost.GetValue<string>(CONSTANTS.ZIPSOURCE);
                    prop.removeSource = _taskHost.GetValue<bool>(CONSTANTS.ZIPREMOVESOURCE);
                    prop.recursive = _taskHost.GetValue<bool>(CONSTANTS.ZIPRECURSIVE);
                    prop.targetFile = _taskHost.GetValue<string>(CONSTANTS.ZIPTARGET);
                    prop.overwriteTarget = _taskHost.GetValue<bool>(CONSTANTS.ZIPOVERWRITE);
                    prop.fileFilter = _taskHost.GetValue<string>(CONSTANTS.ZIPFILEFILTER);
                    prop.logLevel = (LogLevel)Enum.Parse(typeof(LogLevel), _taskHost.GetValue<string>(CONSTANTS.ZIPLOGLEVEL));
                }

                this.propertyGrid.SelectedObject = prop;
            }
            catch (Exception ex)
            {
                //IDTSComponentEvents ce = (IDTSComponentEvents)_taskHost.Properties[CONSTANTS.COMPONENTEVENTS].GetValue(_taskHost);
                //ce.FireError(1, "PGP Task UI Form", ex.Message, "", 0);
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
                _taskHost.SetValue(CONSTANTS.ZIPFILEACTION, prop.fileAction);
                _taskHost.SetValue(CONSTANTS.ZIPCOMPRESSIONTYPE, prop.compressionType);
                _taskHost.SetValue(CONSTANTS.ZIPCOMPRESSIONLEVEL, prop.zipCompressionLevel);
                _taskHost.SetValue(CONSTANTS.TARCOMPRESSIONLEVEL, prop.tarCompressionLevel);
                _taskHost.SetValue(CONSTANTS.ZIPPASSWORD, prop.zipPassword);
                _taskHost.SetValue(CONSTANTS.ZIPSOURCE, prop.sourceFile);
                _taskHost.SetValue(CONSTANTS.ZIPREMOVESOURCE, prop.removeSource);
                _taskHost.SetValue(CONSTANTS.ZIPRECURSIVE, prop.recursive);
                _taskHost.SetValue(CONSTANTS.ZIPTARGET, prop.targetFile);
                _taskHost.SetValue(CONSTANTS.ZIPOVERWRITE, prop.overwriteTarget);
                _taskHost.SetValue(CONSTANTS.ZIPFILEFILTER, prop.fileFilter);
                _taskHost.SetValue(CONSTANTS.ZIPLOGLEVEL, prop.logLevel);
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

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            this.propertyGrid.Refresh();
        }

        #endregion  
    }
}
