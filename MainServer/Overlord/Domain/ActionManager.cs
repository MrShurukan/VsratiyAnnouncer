using Blazored.Toast.Services;

namespace Overlord.Domain;

public class ActionManager
{
    private readonly IToastService _toastService;
    public ActionManager(IToastService toastService)
    {
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
    }
    public async Task Toastify(Func<Task> action, string successMessage)
    {
        _toastService.ShowInfo("Работаю...", x => x.DisableTimeout = true);
        try
        {
            await action();
            _toastService.ShowSuccess(successMessage);
        }
        catch (Exception e)
        {
            _toastService.ShowError($"[{e.GetType().Name}]: {e.Message}");
        }
        finally
        {
            _toastService.ClearToasts(ToastLevel.Info);
        }
    }
}