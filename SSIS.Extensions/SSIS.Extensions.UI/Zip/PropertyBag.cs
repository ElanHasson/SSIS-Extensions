using System;
using System.Collections;
using System.ComponentModel;

namespace SSIS.Extensions.UI.ZipTask
{
    public class PropertyBag : ICustomTypeDescriptor
    {
        #region Private Properties

        private string[] invalidDeCompress = { "includeSubfolders", "zipCompressionLevel", "fileFilter" };
        private string[] invalidZip = { "tarCompressionLevel"};
        private string[] invalidTar = { "zipCompressionLevel", "zipPassword" };

        #endregion

        #region Public Properties
                
        [Category("\t\tGeneral"), DisplayName("Name"), Description("Specifies the name of the task.")]
        public string name { get; set; }

        [Category("\t\tGeneral"), DisplayName("Description"), Description("Specifies the description of the task.")]
        public string description { get; set; }

        [Category("\tSettings"), DisplayName("Action"), Description("Specifies task action.")]
        public ZipFileAction fileAction { get; set; }

        [Category("\tSettings"), DisplayName("Compression Type"), Description("Specifies type of compression or decompression.")]
        public CompressionType compressionType { get; set; }

        [Category("\tSettings"), DisplayName("Compression Level"), Description("Specifies level of compression.")]
        public ZipCompressionLevel zipCompressionLevel { get; set; }

        [Category("\tSettings"), DisplayName("Compression Level"), Description("Specifies level of compression.")]
        public TarCompressionLevel tarCompressionLevel { get; set; }

        [Category("\tSettings"), DisplayName("Password"), Description("Specifies encryption/decryption password.")]
        [TypeConverter(typeof(RuleConverter))]
        public string zipPassword { get; set; }
        
        [Category("\tSettings"), DisplayName("Log Level"), Description("Specifies the logging option.")]
        public LogLevel logLevel { get; set; }
        
        [Category("Source"), DisplayName("Source"), Description("Specifies variable that contains source file name.")]
        [TypeConverter(typeof(RuleConverter))]
        public string sourceFile { get; set; }

        [Category("Source"), DisplayName("Remove Source"), Description("Specifies if source should be removed after compression/decompression.")]
        public bool removeSource { get; set; }

        [Category("Source"), DisplayName("Include Subfolders"), Description("Specifies if sub-folder files should be included.")]
        public bool recursive { get; set; }

        [Category("Source"), DisplayName("Filter"), Description("Spcifies filter expression. The criteria is specified using FLEE expression. Available elements: CreationTime, Directory, DirectoryName, Extension, FullName, IsReadOnly, LastAccessTime, LastWriteTime, Length, Name.")]
        [TypeConverter(typeof(RuleConverter))]
        public string fileFilter { get; set; }

        [Category("Target"), DisplayName("Target"), Description("Specifies variable that contains the target for compression/decompression.")]
        [TypeConverter(typeof(RuleConverter))]
        public string targetFile { get; set; }

        [Category("Target"), DisplayName("Overwrite"), Description("Specifies target should be overwritten.")]
        public bool overwriteTarget { get; set; }
        
        #endregion

        #region Private Methods

        /*
         * Does the property filtering...
         */
        private PropertyDescriptorCollection FilterProperties(PropertyDescriptorCollection pdc)
        {
            ArrayList toRemove = new ArrayList();

            /* Hide invalid File Actions */
            PropertyDescriptor pd = pdc.Find("fileAction", true);
            if (pd != null)
            {
                object val = pd.GetValue(this);
                if (val != null)
                {
                    if((ZipFileAction)val == ZipFileAction.Decompress)
                        foreach (string s in invalidDeCompress)
                            toRemove.Add(s);
                }
            }

            /* Hide Compression types */
            PropertyDescriptor cTypePD = pdc.Find("compressionType", true);
            if (cTypePD != null)
            {
                object val = cTypePD.GetValue(this);
                if (val != null)
                {
                    if ((CompressionType)val == CompressionType.Zip)
                        foreach (string s in invalidZip)
                            toRemove.Add(s);
                    else if ((CompressionType)val == CompressionType.Tar)
                        foreach (string s in invalidTar)
                            toRemove.Add(s);
                }
            }

            PropertyDescriptorCollection adjustedProps = new PropertyDescriptorCollection(new PropertyDescriptor[] { });
            foreach (PropertyDescriptor p in pdc)
                if (!toRemove.Contains(p.Name))
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
