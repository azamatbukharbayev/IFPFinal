namespace TaskFlow.Domain.ValueObjects;
public sealed record Label(string Name, string Color)
{
    public static Label Create(string name, string color)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Label name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(color)) throw new ArgumentException("Label color cannot be empty.", nameof(color));
        return new Label(name.Trim(), color.Trim());
    }
}
