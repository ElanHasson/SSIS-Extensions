using System;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Pipeline;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Design;

namespace SSIS.Extensions.UI.PGPTask
{
    [DtsPipelineComponent(DisplayName = "PGP", ComponentType = ComponentType.Transform)]
    public class PGPUI : IDtsTaskUI 
    {
        private TaskHost _taskHost = null;        

        public void Delete(IWin32Window parentWindow)
        {
            
        }

        public ContainerControl GetView()
        {
            return new PGPUIForm(_taskHost);
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
