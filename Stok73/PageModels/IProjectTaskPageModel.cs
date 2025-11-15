using CommunityToolkit.Mvvm.Input;
using Stok73.Models;

namespace Stok73.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}