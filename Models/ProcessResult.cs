using FrameSurgeon.Enums;

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


