using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daemonizer
{
    interface IServiceProcess
    {
        void StartService(string[] args);
        void StopService();
    }
}
