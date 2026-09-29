using System.Windows;
using System.Windows.Controls;

namespace SAA.DesktopApp.Views;

public partial class SelectionView : UserControl
{
    private readonly Action _onCustomer;
    private readonly Action _onAdmin;

    public SelectionView(Action onCustomer, Action onAdmin)
    {
        InitializeComponent();
        _onCustomer = onCustomer;
        _onAdmin = onAdmin;
    }

    private void Customer_Click(object sender, RoutedEventArgs e) => _onCustomer();
    private void Admin_Click(object sender, RoutedEventArgs e) => _onAdmin();
}
