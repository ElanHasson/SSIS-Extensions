using System;
using System.Collections;
using System.ComponentModel;
using SSIS.Extensions;

namespace SSIS.Extensions.UI.PGPTask
{
    public class PropertiyBag : ICustomTypeDescriptor
    {
        #region Private Properties

        private string[] invalidEncrypt = { "privateKey", "passPhrase"};
        private string[] invalidDecrypt = { "publicKey", "isArmored"};

        #endregion

        #region Public Properties

        [Category("General"), DisplayName("Name"), Description("Specifies the name of the task.")]
        public string name { get; set; }

        [Category("General"), DisplayName("Description"), Description("Specifies the description of the task.")]
        public string description { get; set; }

        [Category("Settings"), DisplayName("File Action"), Description("Specifies the action for the files.")]
        public PGPFileAction fileAction { get; set; }

        [CategoryAttribute("Settings"), DisplayName("Public Key"), Description("Specifies the variable that contains the public key. Required for Encryption.")]
        [TypeConverter(typeof(RuleConverter))]
        public string publicKey { get; set; }

        [Category("Settings"), DisplayName("Private Key"), Description("Specifies the variable that contains the private key. Required for signed Encryption and Decryption.")]
        [TypeConverter(typeof(RuleConverter))]
        public string privateKey { get; set; }

        [Category("Settings"), DisplayName("Pass Phrase"), Description("Specifies the variable that contains the passphrase. Required for Decryption and signed Encryption.")]
        [TypeConverter(typeof(RuleConverter))]
        public string passPhrase { get; set; }

        [Category("Settings"), DisplayName("Armored"), Description("Specifies if output is Armored.")]
        public bool isArmored { get; set; }

        //[Category("\tSettings"), DisplayName("Log Level"), Description("Specifies the logging option.")]
        //public LogLevel logLevel { get; set; }
        
        [Category("Source"), DisplayName("Source File"), Description("Specifies the variable that contains the source file name.")]
        [TypeConverter(typeof(RuleConverter))]
        public string sourceFile { get; set; }

        [Category("Source"), DisplayName("Remove Source"), Description("Specifies if the source file should be removed.")]
        public bool removeSource { get; set; }

        [Category("Target"), DisplayName("Target File"), Description("Specifies the variable that contains the target file name.")]
        [TypeConverter(typeof(RuleConverter))]
        public string targetFile { get; set; }

        [Category("Target"), DisplayName("Overwrite Existing"), Description("Spcifies if the target file should be overwrite.")]
        public bool overwriteTarget { get; set; }

        #endregion

        #region Private Methods
        /*
         * Does the property filtering...
         */
        private PropertyDescriptorCollection FilterProperties(PropertyDescriptorCollection pdc)
        {
            ArrayList toRemove = new ArrayList();

            PropertyDescriptor pd = pdc.Find("fileAction", true);
            if (pd != null)
            {
                object val = pd.GetValue(this);
                if (val != null)
                {
                    if ((PGPFileAction)val == PGPFileAction.Decrypt)
                        foreach (string s in invalidDecrypt)
                            toRemove.Add(s);
                    else if ((PGPFileAction)val == PGPFileAction.Encrypt)
                        foreach (string s in invalidEncrypt)
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
