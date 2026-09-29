namespace Personalhandboksassistent.Configuration;

// Läser in lokal konfiguration från .env om variabeln inte redan är satt.
public static class DotEnvLoader
{
    public static void LoadFromCurrentDirectory()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        // Sök uppåt så kommandot fungerar från både projektroten och projektmappen.
        while (directory is not null)
        {
            var envPath = Path.Combine(directory.FullName, ".env");
            if (File.Exists(envPath))
            {
                LoadFile(envPath);
                return;
            }

            directory = directory.Parent;
        }
    }

    private static void LoadFile(string envPath)
    {
        foreach (var line in File.ReadLines(envPath))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#') || !trimmed.Contains('='))
            {
                continue;
            }

            var separator = trimmed.IndexOf('=');
            var key = trimmed[..separator].Trim();
            var value = trimmed[(separator + 1)..].Trim().Trim('"', '\'');

            // Miljövariabler från datorn eller driftsmiljön ska ha företräde.
            if (key.Length > 0 && Environment.GetEnvironmentVariable(key) is null)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}