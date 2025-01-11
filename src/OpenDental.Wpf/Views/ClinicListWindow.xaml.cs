using System.Windows;
using Imedisoft.ViewModels;

namespace Imedisoft.Views;

public partial class ClinicListWindow : Window
{
    public ClinicListWindow()
    {
        InitializeComponent();

        DataContext = new ClinicListViewModel();
    }
}