using System;
using System.Collections;
using System.ComponentModel;

namespace SSIS.Extensions.UI.SFTPTask
{
    public class PropertyBag : ICustomTypeDescriptor
    {
        #region Private Properties

        private string[] validSend = { "hostName", "portNumber", "userName", "passWord", "stopOnFailure", "logLevel", "fileAction", "reTries", "localFile", "localRemove", "localIncludeSubFolders", "localFilter", "remoteFile", "remoteOverwrite" };
        private string[] validSendMultiple = { "hostName", "portNumber", "userName", "passWord", "stopOnFailure", "logLevel", "fileAction", "reTries", "sftpFileInfo" };
        private string[] validReceive = { "hostName", "portNumber", "userName", "passWord", "stopOnFailure", "logLevel", "fileAction", "reTries", "localFile", "localOverwrite", "remoteFile", "remoteRemove", "remoteFilter" };
        private string[] validReceiveMultiple = { "hostName", "portNumber", "userName", "passWord", "stopOnFailure", "logLevel", "fileAction", "reTries", "sftpFileInfo" };
        private string[] validList = { "hostName", "portNumber", "userName", "passWord", "stopOnFailure", "logLevel", "fileAction", "reTries", "remoteFileListVariable", "remoteFile", "remoteFilter" };

        #endregion

        #region Public Properties
                
        [Category("\t\tGeneral"), DisplayName("Name"), Description("Specifies the name of the task.")]
        public string name { get; set; }

        [Category("\t\tGeneral"), DisplayName("Description"), Description("Specifies the description of the task.")]
        public string description { get; set; }

        [Category("\tSettings"), DisplayName("Host"), Description("Specifies the variable that contains the Host name.")]
        [TypeConverter(typeof(RuleConverter))]
        public string hostName { get; set; }

        [Category("\tSettings"), DisplayName("Port"), Description("Specifies the variable that contains the Port number.")]
        [TypeConverter(typeof(RuleConverter))]
        public string portNumber { get; set; }

        [Category("\tSettings"), DisplayName("User Name"), Description("Specifies the variable that contains the User name.")]
        [TypeConverter(typeof(RuleConverter))]
        public string userName { get; set; }
        
        [Category("\tSettings"), DisplayName("Password"), Description("Specifies the variable that contains the Password.")]
        [TypeConverter(typeof(RuleConverter))]
        public string passWord { get; set; }
        
        [Category("\tSettings"), DisplayName("File Action"), Description("Specifies the action for the files.")]
        [TypeConverter(typeof(EnumTypeConverter))]
        public SFTPFileAction fileAction { get; set; }

        //[Category("\tSettings"), DisplayName("Re-Tries"), Description("Specifies the number of attempts before failing.")]
        //public int reTries { get; set; }

        [Category("\tSettings"), DisplayName("Stop On Failure"), Description("Spcifies if task should fail if remote path does not exist.")]
        public bool stopOnFailure { get; set; }

        [Category("\tSettings"), DisplayName("Data Set"), Description("Specifies the variable name that contains data about the local and remote file. Must be of type ISFTPFileInfo.")]
        [TypeConverter(typeof(RuleConverter))]
        public string sftpFileInfo { get; set; }

        [Category("\tSettings"), DisplayName("Log Level"), Description("Specifies the logging option.")]
        public LogLevel logLevel { get; set; }
        
        [CategoryAttribute("Local Parameters"), DisplayName("Local File"), Description("Specifies the variable that contains the local file information.")]
        [TypeConverter(typeof(RuleConverter))]
        public string localFile { get; set; }
        
        [Category("Local Parameters"), DisplayName("Overwrite Local"), Description("Spcifies if the local file should be overwritten.")]
        public bool localOverwrite { get; set; }

        [Category("Local Parameters"), DisplayName("Remove Local"), Description("Specifies if the source file should be removed.")]
        public bool localRemove { get; set; }

        [Category("Local Parameters"), DisplayName("Include Sub Folders"), Description("Spcifies to recursibely include all sub-folder files.")]
        public bool localIncludeSubFolders { get; set; }

        [Category("Local Parameters"), DisplayName("Local Filter"), Description("Spcifies filter expression. The criteria is specified using FLEE expression. Available elements: CreationTime, Directory, DirectoryName, Extension, FullName, IsReadOnly, LastAccessTime, LastWriteTime, Length, Name.")]
        [TypeConverter(typeof(RuleConverter))]
        public string localFilter { get; set; }
        
        [Category("Remote Parameters"), DisplayName("Remote File"), Description("Specifies the variable that contains the remote file.")]
        [TypeConverter(typeof(RuleConverter))]
        public string remoteFile { get; set; }

        [Category("Remote Parameters"), DisplayName("Overwrite Remote"), Description("Spcifies if the remote file should be overwritten.")]
        public bool remoteOverwrite { get; set; }

        [Category("Remote Parameters"), DisplayName("Remove Remote"), Description("Spcifies if the remote file should be overwritten.")]
        public bool remoteRemove { get; set; }
        
        [Category("Remote Parameters"), DisplayName("Result Variable"), Description("Specifies the variable that contains the results.")]
        [TypeConverter(typeof(RuleConverter))]
        public string remoteFileListVariable { get; set; }

        [Category("Remote Parameters"), DisplayName("Remote Filter"), Description("Specify remote files filter criteria. The criteria is specified using FLEE expression. Available elements: Name, Size, ModifiedTime, IsDirectory.")]
        [TypeConverter(typeof(RuleConverter))]
        public string remoteFilter { get; set; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Does the property filtering...
        /// </summary>
        /// <param name="pdc">The PDC.</param>
        /// <returns></returns>
        private PropertyDescriptorCollection FilterProperties(PropertyDescriptorCollection pdc)
        {
            ArrayList validOptions = new ArrayList();

            PropertyDescriptor pd = pdc.Find("fileAction", true);
            if (pd != null)
            {
                object val = pd.GetValue(this);
                if (val != null)
                {
                    if ((SFTPFileAction)val == SFTPFileAction.Send)
                        foreach (string s in validSend)
                            validOptions.Add(s);
                    else if ((SFTPFileAction)val == SFTPFileAction.Receive)
                        foreach (string s in validReceive)
                            validOptions.Add(s);
                    else if ((SFTPFileAction)val == SFTPFileAction.List)
                        foreach (string s in validList)
                            validOptions.Add(s);
                    else if ((SFTPFileAction)val == SFTPFileAction.SendMultiple)
                        foreach (string s in validSendMultiple)
                            validOptions.Add(s);
                    else if ((SFTPFileAction)val == SFTPFileAction.ReceiveMultiple)
                        foreach (string s in validReceiveMultiple)
                            validOptions.Add(s);
                }
            }

            PropertyDescriptorCollection adjustedProps = new PropertyDescriptorCollection(new PropertyDescriptor[] { });
            foreach (PropertyDescriptor p in pdc)
                if (validOptions.Contains(p.Name))
                    adjustedProps.Add(p);

            return adjustedProps;
        }

        #endregion

        #region ICustomTypeDescriptor Members

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        EventDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }


        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(this, attributes, true);//this, attributes, true);
            return FilterProperties(pdc);
        }

        PropertyDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetProperties()
        {
            PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(this, true);
            return FilterProperties(pdc);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        #endregion
    }
}
