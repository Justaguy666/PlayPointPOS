using Application.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace WinUI.UIModels;

public partial class ServiceInSessionModel : ObservableObject
{
    public enum ServiceType
    {
        Game,
        Product,
    }

    private readonly ILocalizationService _localizationService;
    private readonly Action _onTotalChanged;
    private readonly Action<ServiceInSessionModel> _onDelete;
    private readonly Func<ServiceInSessionModel, Task> _onStopGameRequested;
    
    public string Service { get; }
    public ServiceType Type { get; }
    
    public decimal UnitPrice { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ServiceInfo))]
    [NotifyPropertyChangedFor(nameof(TotalPrice))]
    [NotifyPropertyChangedFor(nameof(TotalPriceText))]
    [NotifyPropertyChangedFor(nameof(IsGameStopped))]
    [NotifyPropertyChangedFor(nameof(IsGameRunning))]
    [NotifyPropertyChangedFor(nameof(IsGameNotStarted))]
    private DateTime? _startTime;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ServiceInfo))]
    [NotifyPropertyChangedFor(nameof(TotalPrice))]
    [NotifyPropertyChangedFor(nameof(TotalPriceText))]
    [NotifyPropertyChangedFor(nameof(IsGameStopped))]
    [NotifyPropertyChangedFor(nameof(IsGameRunning))]
    [NotifyPropertyChangedFor(nameof(IsGameNotStarted))]
    private DateTime? _endTime;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ServiceInfo))]
    [NotifyPropertyChangedFor(nameof(TotalPrice))]
    [NotifyPropertyChangedFor(nameof(TotalPriceText))]
    private int _quantity;

    public bool IsGame => Type == ServiceType.Game;
    public bool IsProduct => Type == ServiceType.Product;
    
    public bool IsGameRunning => StartTime.HasValue && !EndTime.HasValue;
    public bool IsGameStopped => StartTime.HasValue && EndTime.HasValue;
    public bool IsGameNotStarted => !StartTime.HasValue;

    public decimal TotalPrice => CalculateTotalPrice();
    public string TotalPriceText => _localizationService.FormatCurrency(TotalPrice);

    public string ServiceInfo => GenerateServiceInfo();

    public string ActionPlaceholderText { get; }
    public bool HasReturnAction { get; }
    public string ReturnActionText { get; }
    public string EditActionText { get; }
    public string RemoveActionText { get; }
    public string StartGameButtonText { get; }
    public string StopGameButtonText { get; }

    public ServiceInSessionModel(
        ILocalizationService localizationService,
        string service,
        ServiceType type,
        DateTime? startTime,
        int quantity,
        decimal unitPrice,
        Action onTotalChanged = null,
        Action<ServiceInSessionModel> onDelete = null,
        Func<ServiceInSessionModel, Task> onStopGameRequested = null)
    {
        _localizationService = localizationService;
        Service = service ?? throw new ArgumentNullException(nameof(service));
        Type = type;
        StartTime = startTime;
        Quantity = quantity;
        UnitPrice = unitPrice;
        _onTotalChanged = onTotalChanged;
        _onDelete = onDelete;
        _onStopGameRequested = onStopGameRequested;

        ActionPlaceholderText = _localizationService.GetString("ServiceActionPlaceholderText");
        HasReturnAction = false;
        ReturnActionText = _localizationService.GetString("ReturnButtonText");
        EditActionText = _localizationService.GetString("EditButtonText");
        RemoveActionText = _localizationService.GetString("RemoveButtonText");
        StartGameButtonText = _localizationService.GetString("StartGameButtonText");
        StopGameButtonText = _localizationService.GetString("StopGameButtonText");
    }

    [RelayCommand]
    private void StartGame()
    {
        if (!IsGame) return;
        StartTime = DateTime.Now;
        EndTime = null;
        Refresh();
    }

    [RelayCommand]
    private async Task StopGame()
    {
        if (!IsGame) return;
        if (_onStopGameRequested != null)
        {
            await _onStopGameRequested(this);
        }
        else
        {
            EndTime = DateTime.Now;
            Refresh();
        }
    }

    [RelayCommand]
    private void IncrementQuantity()
    {
        if (!IsProduct) return;
        Quantity++;
        _onTotalChanged?.Invoke();
    }

    [RelayCommand]
    private void DecrementQuantity()
    {
        if (!IsProduct) return;
        if (Quantity > 1)
        {
            Quantity--;
            _onTotalChanged?.Invoke();
        }
    }

    [RelayCommand]
    private void Delete()
    {
        _onDelete?.Invoke(this);
    }

    public void RefreshTimer()
    {
        if (IsGame && IsGameRunning)
        {
            Refresh();
        }
    }

    public void Refresh()
    {
        OnPropertyChanged(nameof(TotalPrice));
        OnPropertyChanged(nameof(TotalPriceText));
        OnPropertyChanged(nameof(ServiceInfo));
        OnPropertyChanged(nameof(IsGameRunning));
        OnPropertyChanged(nameof(IsGameStopped));
        OnPropertyChanged(nameof(IsGameNotStarted));
        _onTotalChanged?.Invoke();
    }

    private decimal CalculateTotalPrice()
    {
        if (IsGame)
        {
            if (!StartTime.HasValue) return 0m;
            var end = EndTime ?? DateTime.Now;
            var hours = (decimal)(end - StartTime.Value).TotalHours;
            if (hours < 0) hours = 0;
            return UnitPrice * hours;
        }
        else if (IsProduct)
        {
            return UnitPrice * Quantity;
        }
        return 0m;
    }

    private string GenerateServiceInfo()
    {
        if (IsGame)
        {
            string startTimeText = StartTime?.ToString("HH:mm", _localizationService.Culture) ?? "--:--";
            string endTimeText = EndTime?.ToString("HH:mm", _localizationService.Culture) ?? "";
            
            if (IsGameRunning)
            {
                var duration = DateTime.Now - (StartTime ?? DateTime.Now);
                return $"Started: {startTimeText} ({(int)duration.TotalHours}h {duration.Minutes}m)";
            }
            else if (IsGameStopped)
            {
                return $"Played: {startTimeText} - {endTimeText}";
            }
            return "Not started";
        }
        else if (IsProduct)
        {
            var formattedUnitPrice = _localizationService.FormatCurrency(UnitPrice);
            return $"{formattedUnitPrice} x {Quantity}";
        }
        return string.Empty;
    }
}
