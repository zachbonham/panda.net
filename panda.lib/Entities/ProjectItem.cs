using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace panda.lib.Entities
{
    public class ProjectItem
    {
        public string Name { get; set; }
        public Guid ProjectId { get; set; }
        public string RelativePath { get; set; }
        public string FullPath { get; set; }
    }
}
