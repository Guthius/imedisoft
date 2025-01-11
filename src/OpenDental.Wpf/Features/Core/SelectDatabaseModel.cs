using System;
using System.IO;
using Newtonsoft.Json;

namespace Imedisoft.Features.Core;

public sealed record SelectDatabaseModel
{
    public string Server { get; set; } = "localhost";
    public int Port { get; set; } = 3306;
    public string UserId { get; set; } = "root";
    public string Password { get; set; } = string.Empty;
    public string Database { get; set; } = "opendental";
    public bool HideOnStartup { get; set; }

    public static SelectDatabaseModel Load()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenDental");

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        path = Path.Combine(path, "Database.json");
        if (!File.Exists(path))
        {
            return new SelectDatabaseModel();
        }

        try
        {
            var json = File.ReadAllText(path);

            return JsonConvert.DeserializeObject<SelectDatabaseModel>(json);
        }
        catch
        {
            return new SelectDatabaseModel();
        }
    }

    public void Save()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenDental");

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        path = Path.Combine(path, "Database.json");

        var json = JsonConvert.SerializeObject(this);

        File.WriteAllText(path, json);
    }
}