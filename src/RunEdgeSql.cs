using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.IO;

if (args.Length < 2 || string.IsNullOrEmpty(args[0]) || string.IsNullOrEmpty(args[1])) {
    Console.WriteLine("Arguments Expected: 2");
    Console.WriteLine("  1: Edge profile email address");
    Console.WriteLine("  2: Path to SQL file to execute");
    return;
}

var emailAddress = args[0];
var sqlFile = args[1];

if (!File.Exists(sqlFile)) {
    Console.WriteLine("SQL file could not be found");
    return;
}

var edgeUserData = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "Microsoft",
    "Edge",
    "User Data"
);

// Kill all Edge processes for the current user
foreach (var edge in Process.GetProcessesByName("msedge").Where(p => p.StartInfo.UserName == Environment.UserName)) edge.Kill();

// Find the matching profile
var profiles = Directory.GetFiles(edgeUserData, "Preferences", new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = 1 });

foreach (var profile in profiles) {
    var profileData = File.ReadAllText(profile);

    if (profileData.Contains(emailAddress)) {
        var database = Path.Combine(Path.GetDirectoryName(profile)!, "Web Data");

        using (var conn = new SqliteConnection($"Data Source={database}")) {
            conn.Open();

            var sql = conn.CreateCommand();
            sql.CommandText = File.ReadAllText(sqlFile);

            sql.ExecuteNonQuery();
        }
    }
}
