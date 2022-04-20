using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogReader
{
    internal class BaseErrorModel
    {
        public int Id { get; set; }
        public string? LogType { get; set; }
        public string? Middelware { get; set; }
        public string? Message { get; set; }
    }
}
