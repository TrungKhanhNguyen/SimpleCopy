using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCopy
{
    public class CPObj
    {
        public string Channel { get; set; }
        public string SourceChannel {  get; set; }
        public string DestinationChannel { get; set; }  
        public bool Running { get; set; }
    }
}
