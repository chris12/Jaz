using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JazInterpreter
{
    public class Node
    {
        public string command { get; set; }
        public string value { get; set; }

        public Node(string command, string value)
        {
            this.command = command;
            this.value = value;
        }

        
    }
}
