using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogReader
{
    internal class LogsModel : BaseErrorModel
    {
        public string? Class { get; set; }
        public string? StackTrace { get; set; }
    }
}
