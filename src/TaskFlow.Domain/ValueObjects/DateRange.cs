namespace TaskFlow.Domain.ValueObjects;
public sealed record DateRange(DateTime Start, DateTime End)
{
    public bool Contains(DateTime date) => date >= Start && date <= End;
    public int TotalDays => (int)(End - Start).TotalDays;
    public static DateRange Create(DateTime start, DateTime end) => 
        end < start ? throw new ArgumentException("End must be after start.") : new DateRange(start, end);
}
