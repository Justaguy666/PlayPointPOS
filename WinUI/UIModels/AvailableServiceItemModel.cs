using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace WinUI.UIModels;

public partial class AvailableServiceItemModel : ObservableObject
{
    public object Record { get; }

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _priceText;

    public AvailableServiceItemModel(object record, string name, string priceText)
    {
        Record = record ?? throw new ArgumentNullException(nameof(record));
        Name = name;
        PriceText = priceText;
    }
}
