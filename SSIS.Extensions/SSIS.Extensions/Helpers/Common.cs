using Ciloci.Flee;
using Microsoft.SqlServer.Dts.Runtime;
using SSIS.Extensions.SFTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SSIS.Extensions
{
    /// <summary>
    /// Contains some Commonly used methods accross different classes.
    /// </summary>
    public class Common
    {
        /// <summary>
        /// Logs information to BIDS.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <param name="componentEvents">The component events.</param>
        public static void FireInfo(string msg, IDTSComponentEvents componentEvents)
        {
            bool fireAgain = true;
            componentEvents.FireInformation(0, "", msg, "", 0, ref fireAgain);
        }

        /// <summary>
        /// Gets the variable value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="varName">Name of the var.</param>
        /// <param name="variableDispenser">The variable dispenser.</param>
        /// <returns></returns>
        public static T GetVariableValue<T>(string varName, VariableDispenser variableDispenser)
        {
            if (String.IsNullOrEmpty(varName))
                return default(T);

            T value = default(T);

            if (variableDispenser.Contains(varName))
            {
                Variables vars = null;
                variableDispenser.LockForRead(varName);
                variableDispenser.GetVariables(ref vars);
                string obj = vars.OfType<Variable>().Where(x => x.QualifiedName == varName).FirstOrDefault().Value.ToString();
                value = (T)System.ComponentModel.TypeDescriptor.GetConverter(typeof(T)).ConvertFrom(obj);
            }

            return value;
        }
                
        /// <summary>
        /// Gets the type of the variable.
        /// </summary>
        /// <param name="varName">Name of the var.</param>
        /// <param name="variableDispenser">The variable dispenser.</param>
        /// <returns></returns>
        public static TypeCode GetVariableType(string varName, VariableDispenser variableDispenser)
        {
            if (String.IsNullOrEmpty(varName))
                return TypeCode.Empty;

            TypeCode typeCode = TypeCode.Empty; 

            if (variableDispenser.Contains(varName))
            {
                Variables vars = null;
                variableDispenser.LockForRead(varName);
                variableDispenser.GetVariables(ref vars);
                typeCode = vars.OfType<Variable>().Where(x => x.QualifiedName == varName).FirstOrDefault().DataType;
            }

            return typeCode;
        }

        /// <summary>
        /// Sets the variable value.
        /// </summary>
        /// <param name="varName">Name of the var.</param>
        /// <param name="value">The value.</param>
        /// <param name="variableDispenser">The variable dispenser.</param>
        /// <returns></returns>
        public static void SetVariableValue(string varName, object value, VariableDispenser variableDispenser)
        {
            if (String.IsNullOrEmpty(varName))
                return;
            
            if (variableDispenser.Contains(varName))
            {
                Variables vars = null;
                variableDispenser.LockForRead(varName);
                variableDispenser.GetVariables(ref vars);
                Variable var = vars.OfType<Variable>().Where(x => x.QualifiedName == varName).FirstOrDefault();
                var.Value = value;
            }
        }

        /// <summary>
        /// Validates the file for existance.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        public static bool ValidateFile(string fileName)
        {
            if (File.Exists(fileName) == false)
            {
                throw new FileNotFoundException(String.Format("File does not exist: [{0}].", fileName));
            }
            return true;
        }

        /// <summary>
        /// Validate that a file can be overwritten.
        /// </summary>
        /// <param name="overwrite">if set to <c>true</c> [overwrite].</param>
        /// <param name="file">The file.</param>
        /// <exception cref="System.Exception"></exception>
        public static void ValidateOverwrite(bool overwrite, string file)
        {
            if (overwrite == false && File.Exists(file))
                throw new Exception(String.Format("File already exists: [{0}]", file));
        }

        /// <summary>
        /// Validate and remove a file/directory.
        /// </summary>
        /// <param name="remove">if set to <c>true</c> [remove].</param>
        /// <param name="filePath">The file path.</param>
        public static void RemoveFile(bool remove, string filePath)
        {
            if (remove == false)
                return;

            DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(filePath));
            string filePattern = Path.GetFileName(filePath);
            if (String.IsNullOrEmpty(filePattern))
                filePattern = "*";

            foreach (FileInfo file in dir.GetFiles(filePattern))
                file.Delete();
        }
        
        /// <summary>
        /// Return directory path for the given file path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public static string GetFilePath(string filePath)
        {
            return Path.GetFullPath(filePath.Substring(0, 1 + filePath.LastIndexOfAny(@"\/".ToCharArray()))); 
        }

        /// <summary>
        /// Gets the FLEE context.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        private static IGenericExpression<bool> GetFLEEContext(object owner, string filter)
        {
            ExpressionContext context = new ExpressionContext(owner);
            context.Variables.Add("Now", DateTime.Now);
            return context.CompileGeneric<bool>(filter);
        }

        /// <summary>
        /// Get list of all files.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="isRecursive">if set to <c>true</c> [is recursive].</param>
        /// <returns></returns>
        private static string[] GetFileList(string filePath, bool isRecursive)
        {
            string fullPath = GetFilePath(filePath);
            string fileName = Path.GetFileName(filePath);
            SearchOption searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            string[] pathArray = null;
            string path = Path.Combine(fullPath, fileName);
            if (File.Exists(path))
            {
                pathArray = new string[] { path };
            }
            else if (Directory.Exists(path))
            {
                pathArray = Directory.GetFiles(path, "*", searchOption);
            }
            else
            {
                pathArray = Directory.GetFiles(fullPath, fileName, searchOption);
            }

            return pathArray;
        }

        /// <summary>
        /// Gets the file list.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="isRecursive">if set to <c>true</c> [is recursive].</param>
        /// <returns></returns>
        public static List<string> GetFileList(string filePath, string filter, bool isRecursive)
        {
            filter = String.IsNullOrEmpty(filter) ? "true" : filter.Trim();
            IGenericExpression<bool> expression = GetFLEEContext(new FileInfo("."), filter);
            List<string> fileList = new List<string>();
                        
            foreach (string path in GetFileList(filePath,isRecursive))
            {
                if (expression != null)
                {
                    FileInfo fileInfo = new FileInfo(path);
                    expression.Owner = fileInfo;
                    if (expression.Evaluate())
                    {
                        fileList.Add(path);
                    }
                }
            }

            return fileList;
        }

        /// <summary>
        /// Gets the remote file list after filtering.
        /// </summary>
        /// <param name="remoteFileList">The remote file list.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static List<IRemoteFileInfo> GetRemoteFileList(List<IRemoteFileInfo> remoteFileList, string filter)
        {
            filter = String.IsNullOrEmpty(filter) ? "true" : filter.Trim();
            IGenericExpression<bool> expression = GetFLEEContext(new RemoteFileInfo(), filter);
            List<IRemoteFileInfo> fileList = new List<IRemoteFileInfo>();

            foreach (IRemoteFileInfo file in remoteFileList)
            {
                if (expression != null)
                {
                    expression.Owner = file;
                    if (expression.Evaluate())
                    {
                        fileList.Add(file);
                    }
                }
            }

            return fileList;
        }
    }
}
