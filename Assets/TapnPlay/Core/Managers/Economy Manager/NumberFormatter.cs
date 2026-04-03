
public static class NumberFormatter
{
    public static string FormatNumber(int value)
    {
        if (value >= 1_000_000)
            return (value / 1_000_000f).ToString("0.#") + "M";
        else if (value >= 1_000)
            return (value / 1_000f).ToString("0.#") + "k";
        else
            return value.ToString();
    }
}
