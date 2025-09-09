using System.ComponentModel;

namespace StoreRealtime.ContextManagers;

public class BookingContext(Func<string, Task> addMessage)
{
    [Description("Determines whether a table is available on a given date for a given number of people")]
    public bool CheckTableAvailability(DateOnly date, int numPeople)
        => Random.Shared.NextDouble() < (1.0 / numPeople);

    public async void BookTableAsync(DateOnly date, int numPeople, string customerName)
    {
        await addMessage($"***** Booked table on {date} for {numPeople} people (name: {customerName}).");
    }
}