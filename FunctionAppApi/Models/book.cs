using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApp.Models
{
    class book
    {
        public string id { get; set; }
        public string isbn { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string author { get; set; }
        public string description { get; set; }
        public List<string> categories { get; set; }
    }
}
