using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI.ViewModels.AreaManagement.SummarizedAreaCards;

namespace WinUI.Selectors;

public sealed class SummarizedAreaCardTemplateSelector : DataTemplateSelector
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
            SummarizedAvailableCardViewModel => AvailableTemplate,
            SummarizedReservedCardViewModel => ReservedTemplate,
            SummarizedRentedCardViewModel => RentedTemplate,
            _ => base.SelectTemplateCore(item),
        };
    }
}
