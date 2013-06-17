using System;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Design;

namespace SSIS.Extensions.UI.SFTPTask
{
    [DtsPipelineComponent(DisplayName = "SFTP", ComponentType = ComponentType.Transform)]
    public class SFTPUI : IDtsTaskUI 
    {
        private TaskHost _taskHost = null;        

        public void Delete(IWin32Window parentWindow)
        {
            
        }

        public ContainerControl GetView()
        {
            return new SFTPUIForm(_taskHost);
        }

        public void Initialize(TaskHost taskHost, IServiceProvider serviceProvider)
        {
            _taskHost = taskHost;           
        }

        public void New(IWin32Window parentWindow)
        {
            
        }
    }
}
