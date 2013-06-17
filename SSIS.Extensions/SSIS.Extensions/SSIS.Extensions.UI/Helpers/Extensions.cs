using Microsoft.SqlServer.Dts.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace SSIS.Extensions.UI
{
    public static class Extensions
    {
        /// <summary>
        /// Gets the config value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="taskHost">The task host.</param>
        /// <param name="configName">Name of the config.</param>
        /// <returns></returns>
        public static T GetValue<T>(this TaskHost taskHost, string configName)
        {
            T configValue = default(T);

            if (taskHost.Properties[configName] != null && taskHost.Properties[configName].GetValue(taskHost) != null)
            {
                string value = taskHost.Properties[configName].GetValue(taskHost).ToString().Trim();
                configValue = (T)System.ComponentModel.TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(value);
            }

            return configValue;
        }

        /// <summary>
        /// Sets the value in Task host.
        /// </summary>
        /// <param name="taskHost">The task host.</param>
        /// <param name="configName">Name of the config.</param>
        /// <param name="value">The value.</param>
        public static void SetValue(this TaskHost taskHost, string configName, object value)
        {
            if (!String.IsNullOrEmpty(configName))
                taskHost.Properties[configName].SetValue(taskHost, value);
        }

        public static List<string> EnumMemberList<T>()
        {
            List<T> fieldList = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            List<string> descriptionList = new List<string>();

            Type type = typeof(T);

            foreach (T item in fieldList)
            {
                string name = Enum.GetName(type, item);
                if (name != null)
                {
                    FieldInfo field = type.GetField(name);
                    if (field != null)
                    {
                        DescriptionAttribute attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                        if (attr != null)
                        {
                            descriptionList.Add(attr.Description);
                        }
                        else
                            descriptionList.Add(field.Name);
                    }
                }
            }

            return descriptionList;
        }

        public static object ToEnumField<T>(string fieldDescription)
        {
            Type type = typeof(T);
            foreach (FieldInfo field in type.GetFields())
            {
                DescriptionAttribute attr = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));

                if ((attr != null) && (fieldDescription == attr.Description))
                    return Enum.Parse(type, field.Name);
            }
            return Enum.Parse(type, fieldDescription);
        }

        public static object ToEnumDescription<T>(string fieldDescription)
        {
            Type type = typeof(T);
            FieldInfo fi = type.GetField(Enum.GetName(type, fieldDescription));
            DescriptionAttribute attr = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));

            if (attr != null)
                return attr.Description;
            else
                return fieldDescription.ToString();
        }
    }
}
