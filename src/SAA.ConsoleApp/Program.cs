using SAA.Data;

// SAA Flight Booking - Console Application (Task 5, PA0104)
// Connects to the SQL Server database, confirms the connection and
// retrieves all passenger records from the Passengers table.

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("South African Airways - Flight Booking System");
Console.WriteLine("Database connectivity check (ADO.NET / Microsoft.Data.SqlClient)");
Console.WriteLine(new string('=', 62));

var repository = new FlightBookingRepository(DbConfig.Resolve());

try
{
    var serverVersion = await repository.TestConnectionAsync();
    Console.WriteLine("Connection established successfully.");
    Console.WriteLine($"SQL Server version: {serverVersion.Split('\n')[0].Trim()}");
    Console.WriteLine();

    var passengers = await repository.GetPassengersAsync();
    Console.WriteLine($"Passengers retrieved: {passengers.Count}");
    Console.WriteLine();
    Console.WriteLine($"{"ID",-4}{"Name",-22}{"Email",-34}{"Phone",-18}");
    Console.WriteLine(new string('-', 78));
    foreach (var p in passengers)
        Console.WriteLine($"{p.PassengerId,-4}{p.FullName,-22}{p.Email,-34}{p.Phone,-18}");

    Console.WriteLine();
    Console.WriteLine("Data retrieval completed successfully.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine("Could not connect to or read from the database.");
    Console.Error.WriteLine($"Reason: {ex.Message}");
    Console.Error.WriteLine(@"Check that SQL Server is running and that the SAA_FlightBooking");
    Console.Error.WriteLine(@"database exists (run the scripts in the sql folder), or set the");
    Console.Error.WriteLine(@"SAA_CONNECTION environment variable to a valid connection string.");
    return 1;
}
