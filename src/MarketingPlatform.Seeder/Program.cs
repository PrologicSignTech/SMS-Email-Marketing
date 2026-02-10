using Microsoft.Data.SqlClient;

Console.WriteLine("=== MarketingPlatform Database Seeder ===");
Console.WriteLine();

var connectionString = "Data Source=208.91.198.196;Initial Catalog=plogi7dd_textingpro;Persist Security Info=True;User ID=textingpro;Password=e~8F#llpEfM5qr0r;Pooling=True;Encrypt=True;TrustServerCertificate=True;";

var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "seed_all.sql");
if (!File.Exists(scriptPath))
{
    // Try relative to current directory
    scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "seed_all.sql");
}
if (!File.Exists(scriptPath))
{
    Console.WriteLine($"ERROR: Cannot find seed_all.sql");
    Console.WriteLine($"Searched: {scriptPath}");
    return;
}

Console.WriteLine($"Loading SQL script from: {scriptPath}");
var sql = File.ReadAllText(scriptPath);
Console.WriteLine($"Script loaded ({sql.Length} characters)");
Console.WriteLine();

try
{
    Console.WriteLine("Connecting to database...");
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();
    Console.WriteLine("Connected successfully!");
    Console.WriteLine();

    // Split by GO statements for batch execution
    var batches = System.Text.RegularExpressions.Regex.Split(sql, @"^\s*GO\s*$",
        System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    var batchNum = 0;
    foreach (var batch in batches)
    {
        var trimmed = batch.Trim();
        if (string.IsNullOrWhiteSpace(trimmed)) continue;

        batchNum++;
        Console.WriteLine($"Executing batch {batchNum}...");
        using var command = new SqlCommand(trimmed, connection);
        command.CommandTimeout = 120;
        var result = await command.ExecuteNonQueryAsync();
        Console.WriteLine($"  Batch {batchNum} completed. Rows affected: {result}");
    }

    Console.WriteLine();
    Console.WriteLine("=== SEEDING COMPLETED SUCCESSFULLY ===");
}
catch (SqlException ex)
{
    Console.WriteLine($"SQL ERROR: {ex.Message}");
    Console.WriteLine($"Error Number: {ex.Number}");
    Console.WriteLine($"State: {ex.State}");
    Console.WriteLine($"Line: {ex.LineNumber}");
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"INNER: {ex.InnerException.Message}");
}
