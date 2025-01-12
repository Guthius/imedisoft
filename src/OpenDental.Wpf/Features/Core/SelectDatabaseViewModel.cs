using System;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataConnectionBase;

namespace Imedisoft.Features.Core;

internal sealed partial class SelectDatabaseViewModel : WindowViewModel
{
    private readonly SelectDatabaseModel _model;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private string _server = "localhost";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private int _port = 3306;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private string _userId = "root";

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private string _database = "opendental";

    [ObservableProperty]
    private bool _hideOnStartup;

    public SelectDatabaseViewModel(SelectDatabaseModel model)
    {
        _model = model;

        Server = model.Server;
        Port = model.Port;
        UserId = model.UserId;
        Password = model.Password;
        Database = model.Database;
        HideOnStartup = model.HideOnStartup;
    }

    private bool CanConnect()
    {
        if (Port is < 1 or > 65535)
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(Server) &&
               !string.IsNullOrWhiteSpace(UserId) &&
               !string.IsNullOrWhiteSpace(Database);
    }

    [RelayCommand(CanExecute = nameof(CanConnect))]
    private void Connect()
    {
        if (!IsValidConnection())
        {
            return;
        }

        SaveSettings();

        Close?.Invoke(true);
    }

    private bool IsValidConnection()
    {
        try
        {
            DataConnection.SetDb(
                Server,
                Database,
                UserId,
                Password,
                false,
                string.Empty);

            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Unable to connect to database.\n\n" + ex.Message,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            return false;
        }
    }

    private void SaveSettings()
    {
        _model.Server = Server;
        _model.Port = Port;
        _model.UserId = UserId;
        _model.Password = Password;
        _model.Database = Database;
        _model.HideOnStartup = HideOnStartup;
        _model.Save();
    }
}