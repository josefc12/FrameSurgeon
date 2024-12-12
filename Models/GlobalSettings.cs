using FrameSurgeon.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameSurgeon.Models
{
    public class GlobalSettings
    {
        public required ExportMode ExportMode { get; set; }
        public required List<string> LoadedFiles { get; set; }
        public required string OutputPath { get; set; }
        public required string SelectedExtension { get; set; }
        public required bool TransparencyEnabled { get; set; }
        public required bool SkipFramesEnabled { get; set; }
        public required int FrameSizeWidth { get; set; }
        public required int FrameSizeHeight { get; set; }
    }
}
