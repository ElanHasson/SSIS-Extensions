using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace SSIS.Extensions.UI
{
    public class EnumTypeConverter : EnumConverter
    {
        private Type enumType;

        public EnumTypeConverter(Type type)
            : base(type)
        {
            this.enumType = type;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
        {
            return destType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
        {
            FieldInfo field = enumType.GetField(Enum.GetName(enumType, value));
            DescriptionAttribute attr = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            if (attr != null)
                return attr.Description;
            else
                return value.ToString();
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
        {
            return srcType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            foreach (FieldInfo field in enumType.GetFields())
            {
                DescriptionAttribute attr = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
                if ((attr != null) && ((string)value == attr.Description))
                    return Enum.Parse(enumType, field.Name);
            }
            return Enum.Parse(enumType, (string)value);
        }
    }
}


