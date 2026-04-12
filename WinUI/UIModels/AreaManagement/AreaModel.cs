using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Domain.Enums;

namespace WinUI.UIModels.AreaManagement;

public sealed class AreaModel : ObservableObject
{
    private string _areaName = string.Empty;
    private PlayAreaType _playAreaType = PlayAreaType.Table;
    private PlayAreaStatus _status = PlayAreaStatus.Available;
    private int _maxCapacity;
    private decimal _hourlyPrice = 30000m;
    private string _customerName = string.Empty;
    private string _phoneNumber = string.Empty;
    private string? _memberId;
    private DateTime? _checkInDateTime;
    private int _capacity;
    private DateTime? _startTime;
    private bool _isSessionPaused;
    private DateTime? _sessionPausedAt;
    private TimeSpan _sessionPausedDuration;
    private decimal _totalAmount;

    public string AreaName
    {
        get => _areaName;
        set => SetProperty(ref _areaName, value);
    }

    public PlayAreaType PlayAreaType
    {
        get => _playAreaType;
        set => SetProperty(ref _playAreaType, value);
    }

    public PlayAreaStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public int MaxCapacity
    {
        get => _maxCapacity;
        set => SetProperty(ref _maxCapacity, value);
    }

    public decimal HourlyPrice
    {
        get => _hourlyPrice;
        set
        {
            if (SetProperty(ref _hourlyPrice, value))
            {
                OnPropertyChanged(nameof(ReservationPrice));
            }
        }
    }

    public decimal ReservationPrice => HourlyPrice * 2m;

    public string CustomerName
    {
        get => _customerName;
        set => SetProperty(ref _customerName, value);
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set => SetProperty(ref _phoneNumber, value);
    }

    public string? MemberId
    {
        get => _memberId;
        set => SetProperty(ref _memberId, value);
    }

    public DateTime? CheckInDateTime
    {
        get => _checkInDateTime;
        set => SetProperty(ref _checkInDateTime, value);
    }

    public int Capacity
    {
        get => _capacity;
        set => SetProperty(ref _capacity, value);
    }

    public DateTime? StartTime
    {
        get => _startTime;
        set => SetProperty(ref _startTime, value);
    }

    public bool IsSessionPaused
    {
        get => _isSessionPaused;
        set => SetProperty(ref _isSessionPaused, value);
    }

    public DateTime? SessionPausedAt
    {
        get => _sessionPausedAt;
        set => SetProperty(ref _sessionPausedAt, value);
    }

    public TimeSpan SessionPausedDuration
    {
        get => _sessionPausedDuration;
        set => SetProperty(ref _sessionPausedDuration, value);
    }

    public decimal TotalAmount
    {
        get => _totalAmount;
        set => SetProperty(ref _totalAmount, value);
    }

    public TimeSpan GetSessionElapsedTime(DateTime utcNow)
    {
        if (StartTime is not DateTime startTime)
        {
            return TimeSpan.Zero;
        }

        DateTime effectiveNowUtc = IsSessionPaused && SessionPausedAt is DateTime pausedAt
            ? NormalizeToUtc(pausedAt)
            : NormalizeToUtc(utcNow);

        TimeSpan elapsedTime = effectiveNowUtc - NormalizeToUtc(startTime) - SessionPausedDuration;
        return elapsedTime < TimeSpan.Zero ? TimeSpan.Zero : elapsedTime;
    }

    public void PauseSession(DateTime utcNow)
    {
        if (IsSessionPaused || StartTime is null)
        {
            return;
        }

        SessionPausedAt = NormalizeToUtc(utcNow);
        IsSessionPaused = true;
    }

    public void ResumeSession(DateTime utcNow)
    {
        if (!IsSessionPaused)
        {
            return;
        }

        DateTime resumedAtUtc = NormalizeToUtc(utcNow);
        if (SessionPausedAt is DateTime pausedAt)
        {
            TimeSpan pausedDuration = resumedAtUtc - NormalizeToUtc(pausedAt);
            if (pausedDuration > TimeSpan.Zero)
            {
                SessionPausedDuration += pausedDuration;
            }
        }

        SessionPausedAt = null;
        IsSessionPaused = false;
    }

    public AreaModel Clone()
    {
        return new AreaModel
        {
            AreaName = AreaName,
            PlayAreaType = PlayAreaType,
            Status = Status,
            MaxCapacity = MaxCapacity,
            HourlyPrice = HourlyPrice,
            CustomerName = CustomerName,
            PhoneNumber = PhoneNumber,
            MemberId = MemberId,
            CheckInDateTime = CheckInDateTime,
            Capacity = Capacity,
            StartTime = StartTime,
            IsSessionPaused = IsSessionPaused,
            SessionPausedAt = SessionPausedAt,
            SessionPausedDuration = SessionPausedDuration,
            TotalAmount = TotalAmount,
        };
    }

    private static DateTime NormalizeToUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : value.ToUniversalTime();
    }
}
