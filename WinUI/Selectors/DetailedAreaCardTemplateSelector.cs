using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.ViewModels.AreaManagement.DetailedAreaCards;

namespace WinUI.Selectors;

public sealed class DetailedAreaCardTemplateSelector : DataTemplateSelector
{
    public DataTemplate? AvailableTemplate { get; set; }

    public DataTemplate? ReservedTemplate { get; set; }

    public DataTemplate? RentedTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        return ResolveTemplate(item);
    }

    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
    {
        return ResolveTemplate(item);
    }

    private DataTemplate? ResolveTemplate(object item)
    {
        return item switch
        {
            DetailedAvailableCardViewModel => AvailableTemplate,
            DetailedReservedCardViewModel => ReservedTemplate,
            DetailedRentedCardViewModel => RentedTemplate,
            _ => base.SelectTemplateCore(item),
        };
    }
}
