using Application.Areas;
using Application.Games;
using Application.Members;
using Application.Products;
using Application.Transactions;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public interface IManagementApiService
{
    Task<IReadOnlyList<AreaRecord>> GetAreasAsync();
    Task<AreaRecord> CreateAreaAsync(AreaRecord area);
    Task<AreaRecord> UpdateAreaAsync(AreaRecord area);
    Task DeleteAreaAsync(string id);

    Task<AreaSessionStartResult> StartAreaSessionAsync(int areaId, int guestCount, int? memberId = null);

    Task<IReadOnlyList<GameType>> GetGameTypesAsync();
    Task<GameType> CreateGameTypeAsync(GameType gameType);
    Task<GameType> UpdateGameTypeAsync(GameType gameType);
    Task DeleteGameTypeAsync(string id);

    Task<IReadOnlyList<GameRecord>> GetGamesAsync();
    Task<GameRecord> CreateGameAsync(GameRecord game);
    Task<GameRecord> UpdateGameAsync(GameRecord game);
    Task DeleteGameAsync(string id);

    Task<IReadOnlyList<ProductRecord>> GetProductsAsync();
    Task<ProductRecord> CreateProductAsync(ProductRecord product);
    Task<ProductRecord> UpdateProductAsync(ProductRecord product);
    Task DeleteProductAsync(string id);

    Task<IReadOnlyList<MembershipRank>> GetMembershipRanksAsync();
    Task<MembershipRank> CreateMembershipRankAsync(MembershipRank rank);
    Task<MembershipRank> UpdateMembershipRankAsync(MembershipRank rank);
    Task DeleteMembershipRankAsync(string id);

    Task<IReadOnlyList<MemberRecord>> GetMembersAsync();
    Task<MemberRecord> CreateMemberAsync(MemberRecord member);
    Task<MemberRecord> UpdateMemberAsync(MemberRecord member);
    Task DeleteMemberAsync(string id);

    Task<IReadOnlyList<Transaction>> GetTransactionsAsync();
    Task<Transaction> UpdateTransactionPaymentMethodAsync(string id, PaymentMethod paymentMethod);

    Task<Transaction> CompleteAreaSessionCheckoutAsync(AreaSessionCheckoutArgs args);

    Task<ShopProfile> GetShopProfileAsync();
    Task<ShopProfile> UpdateShopProfileAsync(ShopProfile shopProfile);
}
