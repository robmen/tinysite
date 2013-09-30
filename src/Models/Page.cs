using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinySite.Models
{
    public class Page
    {
        public bool Active { get; set; }

        public int Number { get; set; }

        public string Url { get; set; }
    }
}
