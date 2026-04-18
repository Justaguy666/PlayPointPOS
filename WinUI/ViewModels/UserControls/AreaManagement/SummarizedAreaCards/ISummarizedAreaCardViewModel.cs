using Domain.Enums;
using WinUI.UIModels.Management;

namespace WinUI.ViewModels.AreaManagement.SummarizedAreaCards;

public interface ISummarizedAreaCardViewModel
{
    public AreaModel Model { get; }
    public string AreaName { get; }
    public PlayAreaType PlayAreaType { get; }
    public PlayAreaStatus Status { get; }
}
