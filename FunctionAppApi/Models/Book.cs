using System.Collections.Generic;

namespace FunctionApp.Models
{
    class Book
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
