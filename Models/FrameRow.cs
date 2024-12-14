using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameSurgeon.Models
{
    public class FrameRow
    {
        public string Name { get; set; }
        public string AbsolutePath { get; set; }


        public FrameRow()
        {
        }

        public FrameRow(string name, string path)
        {
            this.Name = name;
            this.AbsolutePath = path;
        }

    }
}
