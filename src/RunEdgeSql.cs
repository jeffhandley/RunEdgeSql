using System.Diagnostics;
using System.IO;

if (args.Length < 3 || string.IsNullOrEmpty(args[0]) || string.IsNullOrEmpty(args[1]) || string.IsNullOrEmpty(args[2])) {
    Console.WriteLine("Arguments Expected: 3");
    Console.WriteLine("  1: Edge profile email address");
    Console.WriteLine("  2: Path to sqlite3.exe");
    Console.WriteLine("  3: Path to SQL file to execute");
    return;
}

var emailAddress = args[0];
var sqlitePath = args[1];
var sqlFile = args[2];

var edgeUserData = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "Microsoft",
    "Edge",
    "User Data"
);

// Kill all Edge processes
foreach (var edge in Process.GetProcessesByName("msedge")) edge.Kill();

// Find the matching profile
var profiles = Directory.GetFiles(edgeUserData, "Preferences", new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = 1 });

foreach (var profile in profiles) {
    var profileData = File.ReadAllText(profile);

    if (profileData.Contains(emailAddress)) {
        var database = Path.Combine(Path.GetDirectoryName(profile)!, "Web Data");
        var sqlite = Process.Start(sqlitePath, new string[] { database, $@".read '{sqlFile}'" });
        sqlite.WaitForExit();
    }
}
