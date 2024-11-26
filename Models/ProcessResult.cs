using FrameSurgeon.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameSurgeon.Models
{
    public class ProcessResult
    {
        public Result Result { get; set; }
        public string? Message { get; set; }


        public ProcessResult(Result result, string? message = null)
        {
            Result = result;
            Message = message;
        }
    }
}


