using System;
using Application.Areas;
using WinUI.UIModels.Management;

namespace WinUI.Services.Factories;

public sealed class AreaModelFactory
{
    public AreaModel Create(AreaRecord source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new AreaModel
        {
            Id = source.Id,
            AreaName = source.AreaName,
            PlayAreaType = source.PlayAreaType,
            Status = source.Status,
            MaxCapacity = source.MaxCapacity,
            HourlyPrice = source.HourlyPrice,
            CustomerName = source.CustomerName,
            PhoneNumber = source.PhoneNumber,
            MemberId = source.MemberId,
            CheckInDateTime = source.CheckInDateTime,
            Capacity = source.Capacity,
            StartTime = source.StartTime,
            IsSessionPaused = source.IsSessionPaused,
            SessionPausedAt = source.SessionPausedAt,
            SessionPausedDuration = source.SessionPausedDuration,
            TotalAmount = source.TotalAmount,
            ActiveSessionId = source.ActiveSessionId,
        };
    }

    public AreaModel Clone(AreaModel source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var clone = new AreaModel
        {
            Id = source.Id,
            AreaName = source.AreaName,
            PlayAreaType = source.PlayAreaType,
            Status = source.Status,
            MaxCapacity = source.MaxCapacity,
            HourlyPrice = source.HourlyPrice,
            CustomerName = source.CustomerName,
            PhoneNumber = source.PhoneNumber,
            MemberId = source.MemberId,
            CheckInDateTime = source.CheckInDateTime,
            Capacity = source.Capacity,
            StartTime = source.StartTime,
            IsSessionPaused = source.IsSessionPaused,
            SessionPausedAt = source.SessionPausedAt,
            SessionPausedDuration = source.SessionPausedDuration,
            TotalAmount = source.TotalAmount,
            ActiveSessionId = source.ActiveSessionId,
        };

        foreach (PendingSessionSaleLine line in source.PendingSessionLines)
        {
            clone.PendingSessionLines.Add(new PendingSessionSaleLine
            {
                IsGame = line.IsGame,
                CatalogId = line.CatalogId,
                DisplayName = line.DisplayName,
                UnitPrice = line.UnitPrice,
                GameRentalStartUtc = line.GameRentalStartUtc,
                ProductQuantity = line.ProductQuantity,
            });
        }

        return clone;
    }
}
