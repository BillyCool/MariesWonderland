using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace MariesWonderland.Web.Components.Shared;

/// <summary>Base for admin pages: who is signed in, and running an action with its outcome in a snackbar.</summary>
public abstract class AdminPage : ComponentBase
{
    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationState { get; set; }

    [Inject]
    protected ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    protected IDialogService Dialogs { get; set; } = default!;

    protected string AdminName { get; private set; } = "admin";

    protected override async Task OnParametersSetAsync()
    {
        if (AuthenticationState != null)
        {
            AdminName = (await AuthenticationState).User.Identity?.Name ?? "admin";
        }
        await base.OnParametersSetAsync();
    }

    /// <summary>Runs <paramref name="action"/>; shows <paramref name="success"/> or the error.</summary>
    protected bool Run(Action action, string? success = null)
    {
        try
        {
            action();
            if (success != null)
            {
                Snackbar.Add(success, Severity.Success);
            }
            return true;
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
            return false;
        }
    }

    protected async Task<bool> Confirm(string title, string message, string yes = "Yes")
        => await Dialogs.ShowMessageBoxAsync(title, message, yesText: yes, cancelText: "Cancel") == true;

    protected static string When(DateTime? utc) => utc == null ? "—" : utc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

    protected static string WhenMs(long ms) => ms <= 0 ? "—" : DateTimeOffset.FromUnixTimeMilliseconds(ms).LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss");
}
