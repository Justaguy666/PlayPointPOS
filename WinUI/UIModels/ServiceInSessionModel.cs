using Application.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace WinUI.UIModels;

public sealed class ServiceInSessionModel
{
    public enum ServiceType
    {
        Game,
        Product,
    }

    private readonly ILocalizationService _localizationService;
    public string Service { get; }
    public ServiceType Type { get; }
    public string ServiceInfo { get; }
    public DateTime? StartTime { get; }
    public int? Quantity { get; }
    public decimal? UnitPrice { get; }
    public decimal TotalPrice { get; }
    public string TotalPriceText { get; }
    public string ActionPlaceholderText { get; }
    public bool HasReturnAction { get; }
    public string ReturnActionText { get; }
    public string EditActionText { get; }
    public string RemoveActionText { get; }

    public ServiceInSessionModel(
        ILocalizationService localizationService,
        string service,
        ServiceType type,
        DateTime? startTime,
        int? quantity,
        decimal? unitPrice,
        decimal? totalPriceOverride = null)
    {
        _localizationService = localizationService;
        Service = service ?? throw new ArgumentNullException(nameof(service));
        Type = type;
        StartTime = startTime;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = totalPriceOverride ?? CalculateTotalPrice();
        TotalPriceText = _localizationService.FormatCurrency(TotalPrice);
        ServiceInfo = GenerateServiceInfo();
        ActionPlaceholderText = _localizationService.GetString("ServiceActionPlaceholderText");
        HasReturnAction = Type == ServiceType.Game && startTime is not null;
        ReturnActionText = _localizationService.GetString("ReturnButtonText");
        EditActionText = _localizationService.GetString("EditButtonText");
        RemoveActionText = _localizationService.GetString("RemoveButtonText");
    }

    private decimal CalculateTotalPrice()
    {
        if (Type == ServiceType.Game)
        {
            if (StartTime is null)
            {
                // Catalog rows (not yet rented) should display price info only, not accrue running cost.
                return 0m;
            }

            double elapsedHours = (DateTime.Now - StartTime.Value).TotalHours;
            return (UnitPrice ?? 0m) * (decimal)Math.Max(0d, elapsedHours);
        }
        else if (Type == ServiceType.Product)
        {
            return (UnitPrice ?? 0m) * (Quantity ?? 0);
        }
        return 0m;
    }

    private string GenerateServiceInfo()
    {
        if (Type == ServiceType.Game)
        {
            if (StartTime is null)
            {
                return _localizationService.FormatCurrency(UnitPrice ?? 0m);
            }

            string startTimeText = StartTime.Value.ToString("HH:mm", _localizationService.Culture);

            return string.Format(
                _localizationService.Culture,
                _localizationService.GetString("GameServiceInfoFormat"),
                startTimeText);
        }
        else if (Type == ServiceType.Product)
        {
            var formattedUnitPrice = _localizationService.FormatCurrency(UnitPrice ?? 0m);
            if (Quantity is null)
            {
                return formattedUnitPrice;
            }

            return $"{formattedUnitPrice} x {Quantity}";
        }
        return string.Empty;
    }

}
