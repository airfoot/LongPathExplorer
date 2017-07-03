using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongPathExplorer
{
    public class Result
    {
        public string SourceFileName { get; set; }
        public string DestinationFileName { get; set; }
        public string State { get; set; }

        public Result(string source, string destination, string state)
        {
            SourceFileName = source;
            DestinationFileName = destination;
            State = state;
        }
    }
}
