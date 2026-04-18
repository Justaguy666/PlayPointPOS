using Application.Areas;
using Application.Services.Areas;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Services.Areas;

public sealed class MockAreaCatalogService : IAreaCatalogService
{
    public IReadOnlyList<AreaRecord> GetAreas()
    {
        PlayAreaUnit tableA01 = CreateAvailableUnit("Ban A01", PlayAreaType.Table, 4, 30000m);
        PlayAreaUnit tableA02 = CreateAvailableUnit("Ban A02", PlayAreaType.Table, 4, 30000m);
        PlayAreaUnit tableB01 = CreateAvailableUnit("Ban B01", PlayAreaType.Table, 6, 45000m);
        PlayAreaUnit roomVip01 = CreateAvailableUnit("Phong VIP 01", PlayAreaType.Room, 8, 80000m);
        PlayAreaUnit roomVip02 = CreateAvailableUnit("Phong VIP 02", PlayAreaType.Room, 10, 100000m);
        PlayAreaUnit tableS05 = CreateAvailableUnit("Ban S05", PlayAreaType.Table, 4, 35000m);

        PlayAreaUnit roomR01 = CreateReservedUnit("Phong R01", PlayAreaType.Room, 8, 80000m);
        PlayAreaReservation reservationRoomR01 = CreateReservation(
            roomR01,
            "Tran Minh Anh",
            "0789 608 537",
            DateTime.Today.AddHours(13).AddMinutes(30),
            6);

        PlayAreaUnit tableC02 = CreateReservedUnit("Ban C02", PlayAreaType.Table, 4, 40000m);
        PlayAreaReservation reservationTableC02 = CreateReservation(
            tableC02,
            "Nguyen Hoang Long",
            "0789 608 537",
            DateTime.Today.AddHours(15),
            4);

        PlayAreaUnit roomVip03 = CreateReservedUnit("Phong VIP 03", PlayAreaType.Room, 10, 100000m);
        PlayAreaReservation reservationRoomVip03 = CreateReservation(
            roomVip03,
            "Le Thu Ha",
            "0789 608 537",
            DateTime.Today.AddHours(18).AddMinutes(15),
            8);

        PlayAreaUnit tableK04 = CreateReservedUnit("Ban K04 (Future)", PlayAreaType.Table, 6, 50000m);
        PlayAreaReservation reservationTableK04 = CreateReservation(
            tableK04,
            "Ngo Bao Toan",
            "0123 456 789",
            DateTime.Now.AddHours(2),
            5);

        PlayAreaUnit room01 = CreateRentedUnit("Phong 01", PlayAreaType.Room, 8, 10000m, 6);
        PlayAreaSession sessionRoom01 = CreateSession(room01, TimeSpan.FromHours(1) + TimeSpan.FromMinutes(31) + TimeSpan.FromSeconds(34), 110000m);

        PlayAreaUnit tableD02 = CreateRentedUnit("Ban D02", PlayAreaType.Table, 4, 30000m, 3);
        PlayAreaSession sessionTableD02 = CreateSession(tableD02, TimeSpan.FromMinutes(52), 85000m);

        PlayAreaUnit roomVip04 = CreateRentedUnit("Phong VIP 04", PlayAreaType.Room, 10, 40000m, 8);
        PlayAreaSession sessionRoomVip04 = CreateSession(roomVip04, TimeSpan.FromHours(2), 205000m);

        return
        [
            CreateAreaRecord(tableA01),
            CreateAreaRecord(tableA02),
            CreateAreaRecord(tableB01),
            CreateAreaRecord(roomVip01),
            CreateAreaRecord(roomVip02),
            CreateAreaRecord(tableS05),
            CreateAreaRecord(roomR01, reservation: reservationRoomR01),
            CreateAreaRecord(tableC02, reservation: reservationTableC02),
            CreateAreaRecord(roomVip03, reservation: reservationRoomVip03),
            CreateAreaRecord(tableK04, reservation: reservationTableK04),
            CreateAreaRecord(room01, session: sessionRoom01),
            CreateAreaRecord(tableD02, session: sessionTableD02),
            CreateAreaRecord(roomVip04, session: sessionRoomVip04),
        ];
    }

    private static AreaRecord CreateAreaRecord(
        PlayAreaUnit unit,
        PlayAreaReservation? reservation = null,
        PlayAreaSession? session = null)
    {
        return new AreaRecord
        {
            AreaName = unit.Name,
            PlayAreaType = unit.Type,
            Status = unit.Status,
            MaxCapacity = unit.MaxOccupancy,
            HourlyPrice = unit.HourlyPrice,
            CustomerName = reservation?.CustomerName ?? string.Empty,
            PhoneNumber = reservation?.CustomerPhone ?? string.Empty,
            MemberId = reservation?.MemberId ?? session?.MemberId,
            CheckInDateTime = reservation?.ReservedFrom,
            Capacity = reservation?.PartySize ?? session?.CurrentOccupancy ?? 0,
            StartTime = session?.StartedAt,
            TotalAmount = session?.AreaCharge ?? 0m,
        };
    }

    private static PlayAreaUnit CreateAvailableUnit(string name, PlayAreaType type, int maxOccupancy, decimal hourlyPrice)
    {
        return new PlayAreaUnit
        {
            Name = name,
            Type = type,
            Status = PlayAreaStatus.Available,
            MaxOccupancy = maxOccupancy,
            HourlyPrice = hourlyPrice,
        };
    }

    private static PlayAreaUnit CreateReservedUnit(string name, PlayAreaType type, int maxOccupancy, decimal hourlyPrice)
    {
        return new PlayAreaUnit
        {
            Name = name,
            Type = type,
            Status = PlayAreaStatus.Reserved,
            MaxOccupancy = maxOccupancy,
            HourlyPrice = hourlyPrice,
        };
    }

    private static PlayAreaUnit CreateRentedUnit(string name, PlayAreaType type, int maxOccupancy, decimal hourlyPrice, int currentOccupancy)
    {
        return new PlayAreaUnit
        {
            Name = name,
            Type = type,
            Status = PlayAreaStatus.Rented,
            MaxOccupancy = maxOccupancy,
            CurrentOccupancy = currentOccupancy,
            HourlyPrice = hourlyPrice,
        };
    }

    private static PlayAreaReservation CreateReservation(
        PlayAreaUnit unit,
        string customerName,
        string customerPhone,
        DateTime reservedFrom,
        int partySize)
    {
        return new PlayAreaReservation
        {
            PlayAreaUnitId = unit.Id,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            PartySize = partySize,
            ReservedFrom = reservedFrom,
            ReservedUntil = reservedFrom.AddHours(2),
        };
    }

    private static PlayAreaSession CreateSession(PlayAreaUnit unit, TimeSpan elapsedTime, decimal areaCharge)
    {
        return new PlayAreaSession
        {
            PlayAreaUnitId = unit.Id,
            CurrentOccupancy = unit.CurrentOccupancy,
            HourlyPriceSnapshot = unit.HourlyPrice,
            StartedAt = DateTime.UtcNow - elapsedTime,
            AreaCharge = areaCharge,
        };
    }
}
