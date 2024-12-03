using FrameSurgeon.Enums;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FrameSurgeon.ViewModels
{
    public class DialogViewModel : ViewModelBase
    {
        public string? Title { get; set; }
        public string? Message { get; set; }
        public string? Color { get; set; }

        public Interaction<Unit, Unit> CloseDialog { get; } = new Interaction<Unit, Unit>();

        public DialogViewModel(DialogType dialogType, string? message)
        {
            Message = message;

            Title = dialogType switch
            {
                DialogType.Warning => "WARNING",
                DialogType.Success => "SUCCESS",
                DialogType.Info => "INFO",
                DialogType.Error => "ERROR",
                _ => "UNKNOWN",
            };


            Color = dialogType switch
            {
                DialogType.Warning => "Orange",
                DialogType.Success => "GreenYellow",
                DialogType.Info => "LightBlue",
                DialogType.Error => "Red",
                _ => "White",
            };

        }

    }
}
