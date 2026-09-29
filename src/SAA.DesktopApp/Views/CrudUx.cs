using System.Windows;
using SAA.Data;

namespace SAA.DesktopApp.Views;

/// <summary>Shared confirm/save helpers so constraint errors reach the user cleanly.</summary>
internal static class CrudUx
{
    public static async Task<bool> TrySaveAsync(Func<Task> action)
    {
        try
        {
            await action();
            return true;
        }
        catch (DataException ex)
        {
            MessageBox.Show(ex.Message, "Cannot complete the action", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Unexpected error", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    public static bool Confirm(string message) =>
        MessageBox.Show(message, "Please confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
}
