namespace Imedisoft.Features.Core;

public partial class SelectDatabaseWindow
{
    public SelectDatabaseWindow(SelectDatabaseModel model)
    {
        InitializeComponent();

        DataContext = new SelectDatabaseViewModel(model)
        {
            Close = result =>
            {
                DialogResult = result;

                Close();
            }
        };
    }
}