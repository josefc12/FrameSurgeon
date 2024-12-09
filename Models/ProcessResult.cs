using FrameSurgeon.Enums;
using ImageMagick;

namespace FrameSurgeon.Models
{
    public class ProcessResult
    {
        public Result Result { get; set; }
        public string? Message { get; set; }
        public MagickImage? Image { get; set; }
        public MagickImageCollection? Collection { get; set; }


        public ProcessResult(Result result, string? message = null, MagickImage? image = null, MagickImageCollection? collection = null)
        {
            Result = result;
            Message = message;
            Image = image;
            Collection = collection;
        }
    }
}


