namespace WinUI.UIModels;

public sealed class LabelValueRowModel
{
    public LabelValueRowModel(string label, string value, bool showDivider = true, bool isHighlighted = false)
    {
        Label = label;
        Value = value;
        ShowDivider = showDivider;
        IsHighlighted = isHighlighted;
    }

    public string Label { get; }

    public string Value { get; }

    public bool ShowDivider { get; }

    public bool IsHighlighted { get; }
}
