using System.Windows;
using System.Windows.Controls;

namespace SAA.DesktopApp.Views;

/// <summary>
/// Drives the loading / content / empty / error visual states shared by
/// every data-backed view, so each view handles all three async outcomes.
/// </summary>
internal static class AsyncState
{
    public static async Task RunAsync(
        FrameworkElement loading,
        FrameworkElement content,
        FrameworkElement empty,
        FrameworkElement error,
        TextBlock errorText,
        Func<Task<bool>> work)
    {
        loading.Visibility = Visibility.Visible;
        content.Visibility = Visibility.Collapsed;
        empty.Visibility = Visibility.Collapsed;
        error.Visibility = Visibility.Collapsed;

        try
        {
            var hasData = await work();
            loading.Visibility = Visibility.Collapsed;
            content.Visibility = hasData ? Visibility.Visible : Visibility.Collapsed;
            empty.Visibility = hasData ? Visibility.Collapsed : Visibility.Visible;
        }
        catch (Exception ex)
        {
            loading.Visibility = Visibility.Collapsed;
            errorText.Text = ex.Message;
            error.Visibility = Visibility.Visible;
        }
    }
}
