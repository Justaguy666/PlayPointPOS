using System;

namespace WinUI.UIModels.Management
{
    public sealed class PendingSessionSaleLine
    {
        public bool IsGame { get; init; }

        public int CatalogId { get; init; }

        public string DisplayName { get; init; } = string.Empty;

        public decimal UnitPrice { get; init; }

        /// <summary>UTC moment the board-game rental clock starts (games only).</summary>
        public DateTime? GameRentalStartUtc { get; init; }

        public int ProductQuantity { get; init; }
    }
}
