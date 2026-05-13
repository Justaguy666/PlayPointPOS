using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Application.Areas;
using Application.Games;
using Application.Members;
using Application.Products;
using Application.Services;
using Application.Transactions;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Services.Management;

public sealed class GraphQLManagementApiService : IManagementApiService, IManagementDataPreloadService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient;
    private readonly IConfigurationService _configurationService;
    private readonly IAuthStateService _authStateService;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private string? _cachedShopId;
    private IReadOnlyList<AreaRecord>? _cachedAreas;
    private IReadOnlyList<GameType>? _cachedGameTypes;
    private IReadOnlyList<GameRecord>? _cachedGames;
    private IReadOnlyList<ProductRecord>? _cachedProducts;
    private IReadOnlyList<MembershipRank>? _cachedMembershipRanks;
    private IReadOnlyList<MemberRecord>? _cachedMembers;
    private IReadOnlyList<Transaction>? _cachedTransactions;

    public GraphQLManagementApiService(
        HttpClient httpClient,
        IConfigurationService configurationService,
        IAuthStateService authStateService)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        _authStateService = authStateService ?? throw new ArgumentNullException(nameof(authStateService));
    }

    public async Task<IReadOnlyList<AreaRecord>> GetAreasAsync()
    {
        int shopId = GetRequiredShopId();
        await EnsureCacheContextAsync(shopId);

        if (_cachedAreas is not null)
        {
            return _cachedAreas.ToList();
        }

        IReadOnlyList<AreaRecord> areas = await FetchAreasAsync(shopId);
        _cachedAreas = areas;
        return areas.ToList();
    }

    public async Task<AreaRecord> CreateAreaAsync(AreaRecord area)
    {
        int shopId = GetRequiredShopId();
        await EnsureCacheContextAsync(shopId);
        var response = await PostAsync<CreateAreaData>(
            """
            mutation CreateArea($shopId: Int!, $input: AreaInput!) {
              createArea(shopId: $shopId, input: $input) {
                id
                areaName
                playAreaType
                status
                maxCapacity
                hourlyPrice
                customerName
                phoneNumber
                memberId
                checkInDateTime
                capacity
                startTime
                isSessionPaused
                sessionPausedAt
                sessionPausedDurationSeconds
                totalAmount
                activeSessionId
              }
            }
            """,
            new
            {
                shopId,
                input = new
                {
                    areaName = area.AreaName,
                    playAreaType = area.PlayAreaType.ToString(),
                    maxCapacity = area.MaxCapacity,
                    hourlyPrice = area.HourlyPrice,
                },
            });

        AreaRecord created = MapArea(RequireData(response.Data?.CreateArea, "createArea"));
        if (_cachedAreas is not null)
        {
            _cachedAreas = [.. _cachedAreas, created];
        }

        return created;
    }

    public async Task<AreaRecord> UpdateAreaAsync(AreaRecord area)
    {
        await EnsureCacheContextAsync(GetRequiredShopId());
        var response = await PostAsync<UpdateAreaData>(
            """
            mutation UpdateArea($id: Int!, $input: AreaInput!) {
              updateArea(id: $id, input: $input) {
                id
                areaName
                playAreaType
                status
                maxCapacity
                hourlyPrice
                customerName
                phoneNumber
                memberId
                checkInDateTime
                capacity
                startTime
                isSessionPaused
                sessionPausedAt
                sessionPausedDurationSeconds
                totalAmount
                activeSessionId
              }
            }
            """,
            new
            {
                id = ParseRequiredIntId(area.Id, nameof(area.Id)),
                input = new
                {
                    areaName = area.AreaName,
                    playAreaType = area.PlayAreaType.ToString(),
                    maxCapacity = area.MaxCapacity,
                    hourlyPrice = area.HourlyPrice,
                },
            });

        AreaRecord updated = MapArea(RequireData(response.Data?.UpdateArea, "updateArea"));
        if (_cachedAreas is not null)
        {
            _cachedAreas = _cachedAreas
                .Select(existing => string.Equals(existing.Id, updated.Id, StringComparison.Ordinal) ? updated : existing)
                .ToList();
        }

        return updated;
    }

    public async Task<AreaSessionStartResult> StartAreaSessionAsync(int areaId, int guestCount, int? memberId = null)
    {
        int shopId = GetRequiredShopId();
        await EnsureCacheContextAsync(shopId);

        var response = await PostAsync<StartAreaSessionData>(
            """
            mutation StartAreaSession($shopId: Int!, $areaId: Int!, $guestCount: Int!, $memberId: Int) {
              startAreaSession(shopId: $shopId, areaId: $areaId, guestCount: $guestCount, memberId: $memberId) {
                sessionId
                startTime
              }
            }
            """,
            new { shopId, areaId, guestCount, memberId });

        StartAreaSessionPayloadDto payload = RequireData(response.Data?.StartAreaSession, "startAreaSession");

        await _cacheLock.WaitAsync().ConfigureAwait(false);
        try
        {
            _cachedAreas = null;
        }
        finally
        {
            _cacheLock.Release();
        }

        DateTime startUtc = payload.StartTime.Kind switch
        {
            DateTimeKind.Utc => payload.StartTime,
            // Backend stores UTC timestamps; sometimes the client payload kind is Local/Unspecified.
            // Treat them as UTC instants to avoid applying UTC+7 offset twice.
            DateTimeKind.Local => DateTime.SpecifyKind(payload.StartTime, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(payload.StartTime, DateTimeKind.Utc),
        };

        return new AreaSessionStartResult(payload.SessionId, startUtc);
    }

    public async Task DeleteAreaAsync(string id)
    {
        await EnsureCacheContextAsync(GetRequiredShopId());
        await PostAsync<DeleteMutationData>(
            """
            mutation DeleteArea($id: Int!) {
              deleteArea(id: $id) {
                success
                message
              }
            }
            """,
            new { id = ParseRequiredIntId(id, nameof(id)) });

        if (_cachedAreas is not null)
        {
            _cachedAreas = _cachedAreas.Where(area => !string.Equals(area.Id, id, StringComparison.Ordinal)).ToList();
        }
    }

    public async Task<IReadOnlyList<GameType>> GetGameTypesAsync()
    {
        int shopId = GetRequiredShopId();
        await EnsureCacheContextAsync(shopId);

        if (_cachedGameTypes is not null)
        {
            return _cachedGameTypes.ToList();
        }

        IReadOnlyList<GameType> gameTypes = await FetchGameTypesAsync(shopId);
        _cachedGameTypes = gameTypes;
        return gameTypes.ToList();
    }

    public async Task<GameType> CreateGameTypeAsync(GameType gameType)
    {
        int shopId = GetRequiredShopId();
        await EnsureCacheContextAsync(shopId);
        var response = await PostAsync<CreateGameTypeData>(
            """
            mutation CreateGameType($shopId: Int!, $input: GameTypeInput!) {
              createGameType(shopId: $shopId, input: $input) {
                id
                name
              }
            }
            """,
            new
            {
                shopId,
                input = new { name = gameType.Name },
            });

        GameType created = MapGameType(RequireData(response.Data?.CreateGameType, "createGameType"));
        if (_cachedGameTypes is not null)
        {
            _cachedGameTypes = [.. _cachedGameTypes, created];
        }

        return created;
    }

    public async Task<GameType> UpdateGameTypeAsync(GameType gameType)
    {
        await EnsureCacheContextAsync(GetRequiredShopId());
        var response = await PostAsync<UpdateGameTypeData>(
            """
            mutation UpdateGameType($id: Int!, $input: GameTypeInput!) {
              updateGameType(id: $id, input: $input) {
                id
                name
              }
            }
            """,
            new
            {
                id = ParseRequiredIntId(gameType.Id, nameof(gameType.Id)),
                input = new { name = gameType.Name },
            });

        GameType updated = MapGameType(RequireData(response.Data?.UpdateGameType, "updateGameType"));
        if (_cachedGameTypes is not null)
        {
            _cachedGameTypes = _cachedGameTypes
                .Select(existing => string.Equals(existing.Id, updated.Id, StringComparison.Ordinal) ? updated : existing)
                .ToList();
        }

        if (_cachedGames is not null)
        {
            _cachedGames = _cachedGames
                .Select(existing =>
                {
                    if (!string.Equals(existing.Type.Id, updated.Id, StringComparison.Ordinal))
                    {
                        return existing;
                    }

                    return existing with
                    {
                        Type = new GameType
                        {
                            Id = updated.Id,
                            Name = updated.Name,
                        }
                    };
                })
                .ToList();
        }

        return updated;
    }

    public async Task DeleteGameTypeAsync(string id)
    {
        await EnsureCacheContextAsync(GetRequiredShopId());
        await PostAsync<DeleteMutationData>(
            """
            mutation DeleteGameType($id: Int!) {
              deleteGameType(id: $id) {
                success
                message
              }
            }
            """,
            new { id = ParseRequiredIntId(id, nameof(id)) });

        if (_cachedGameTypes is not null)
        {
            _cachedGameTypes = _cachedGameTypes.Where(gameType => !string.Equals(gameType.Id, id, StringComparison.Ordinal)).ToList();
        }
    }

    public async Task<IReadOnlyList<GameRecord>> GetGamesAsync()
    {
        int shopId = GetRequiredShopId();
        await EnsureCacheContextAsync(shopId);

        if (_cachedGames is not null)
        {
            return _cachedGames.ToList();
        }

        IReadOnlyList<GameRecord> games = await FetchGamesAsync(shopId);
        _cachedGames = games;
        return games.ToList();
    }

    public async Task<GameRecord> CreateGameAsync(GameRecord game)
    {
        int shopId = GetRequiredShopId();
        await EnsureCacheContextAsync(shopId);
        var response = await PostAsync<CreateGameData>(
            """
            mutation CreateGame($shopId: Int!, $input: GameInput!) {
              createGame(shopId: $shopId, input: $input) {
                id
                name
                hourlyPrice
                minPlayers
                maxPlayers
                gameTypeId
                gameTypeName
                difficulty
                stockQuantity
                imageUri
              }
            }
            """,
            new
            {
                shopId,
                input = new
                {
                    name = game.Name,
                    hourlyPrice = game.HourlyPrice,
                    minPlayers = game.MinPlayers,
                    maxPlayers = game.MaxPlayers,
                    gameTypeId = game.Type.Id,
                    difficulty = game.Difficulty.ToString(),
                    stockQuantity = game.StockQuantity,
                    imageUri = NormalizeImageUri(game.ImageUri),
                },
            });

        GameRecord created = MapGame(RequireData(response.Data?.CreateGame, "createGame"));
        if (_cachedGames is not null)
        {
            _cachedGames = [.. _cachedGames, created];
        }

        return created;
    }

    public async Task<GameRecord> UpdateGameAsync(GameRecord game)
    {
        int shopId = GetRequiredShopId();
        await EnsureCacheContextAsync(shopId);
        var response = await PostAsync<UpdateGameData>(
            """
            mutation UpdateGame($id: Int!, $input: GameInput!) {
              updateGame(id: $id, input: $input) {
                id
                name
                hourlyPrice
                minPlayers
                maxPlayers
                gameTypeId
                gameTypeName
                difficulty
                stockQuantity
                imageUri
              }
            }
            """,
            new
            {
                id = ParseRequiredIntId(game.Id, nameof(game.Id)),
                input = new
                {
                    name = game.Name,
                    hourlyPrice = game.HourlyPrice,
                    minPlayers = game.MinPlayers,
                    maxPlayers = game.MaxPlayers,
                    gameTypeId = game.Type.Id,
                    difficulty = game.Difficulty.ToString(),
                    stockQuantity = game.StockQuantity,
                    imageUri = NormalizeImageUri(game.ImageUri),
                },
            });

        GameRecord updated = MapGame(RequireData(response.Data?.UpdateGame, "updateGame"));
        if (IsIncompleteGameRecord(updated))
        {
            IReadOnlyList<GameRecord> refreshedGames = await FetchGamesAsync(shopId);
            GameRecord? matchedGame = refreshedGames.FirstOrDefault(existing =>
                string.Equals(existing.Id, game.Id, StringComparison.Ordinal));

            if (matchedGame is null)
            {
                throw new InvalidOperationException(
                    $"Game ID '{game.Id}' đã cập nhật nhưng không thể đọc lại dữ liệu từ máy chủ.");
            }

            updated = matchedGame;
            _cachedGames = refreshedGames;
        }

        if (_cachedGames is not null)
        {
            _cachedGames = _cachedGames
                .Select(existing => string.Equals(existing.Id, updated.Id, StringComparison.Ordinal) ? updated : existing)
                .ToList();
        }

        return updated;
    }

    public async Task DeleteGameAsync(string id)
    {
        await EnsureCacheContextAsync(GetRequiredShopId());
        await PostAsync<DeleteMutationData>(
            """
            mutation DeleteGame($id: Int!) {
              deleteGame(id: $id) {
                success
                message
              }
            }
            """,
            new { id = ParseRequiredIntId(id, nameof(id)) });

        if (_cachedGames is not null)
        {
            _cachedGames = _cachedGames.Where(game => !string.Equals(game.Id, id, StringComparison.Ordinal)).ToList();
        }
    }

    public async Task<IReadOnlyList<ProductRecord>> GetProductsAsync()
    {
        int shopId = GetRequiredShopId();
        await EnsureCacheContextAsync(shopId);

        if (_cachedProducts is not null)
        {
            return _cachedProducts.ToList();
        }

        IReadOnlyList<ProductRecord> products = await FetchProductsAsync(shopId);
        _cachedProducts = products;
        return products.ToList();
    }

    public async Task<ProductRecord> CreateProductAsync(ProductRecord product)
    {
        int shopId = GetRequiredShopId();
        await EnsureCacheContextAsync(shopId);
        var response = await PostAsync<CreateProductData>(
            """
            mutation CreateProduct($shopId: Int!, $input: ProductInput!) {
              createProduct(shopId: $shopId, input: $input) {
                id
                name
                price
                productType
                stockQuantity
                imageUri
              }
            }
            """,
            new
            {
                shopId,
                input = new
                {
                    name = product.Name,
                    price = product.Price,
                    productType = product.Type.ToString(),
                    stockQuantity = product.StockQuantity,
                    imageUri = NormalizeImageUri(product.ImageUri),
                },
            });

        ProductRecord created = MapProduct(RequireData(response.Data?.CreateProduct, "createProduct"));
        if (_cachedProducts is not null)
        {
            _cachedProducts = [.. _cachedProducts, created];
        }

        return created;
    }

    public async Task<ProductRecord> UpdateProductAsync(ProductRecord product)
    {
        int shopId = GetRequiredShopId();
        await EnsureCacheContextAsync(shopId);
        var response = await PostAsync<UpdateProductData>(
            """
            mutation UpdateProduct($shopId: Int!, $id: Int!, $input: ProductInput!) {
              updateProduct(shopId: $shopId, id: $id, input: $input) {
                id
                name
                price
                productType
                stockQuantity
                imageUri
              }
            }
            """,
            new
            {
                shopId,
                id = ParseRequiredIntId(product.Id, nameof(product.Id)),
                input = new
                {
                    name = product.Name,
                    price = product.Price,
                    productType = product.Type.ToString(),
                    stockQuantity = product.StockQuantity,
                    imageUri = NormalizeImageUri(product.ImageUri),
                },
            });

        ProductRecord updated = MapProduct(RequireData(response.Data?.UpdateProduct, "updateProduct"));
        if (_cachedProducts is not null)
        {
            _cachedProducts = _cachedProducts
                .Select(existing => string.Equals(existing.Id, updated.Id, StringComparison.Ordinal) ? updated : existing)
                .ToList();
        }

        return updated;
    }

    public async Task DeleteProductAsync(string id)
    {
        int shopId = GetRequiredShopId();
        await EnsureCacheContextAsync(shopId);
        string idNormalized = id?.Trim() ?? string.Empty;
        int numericId = ParseRequiredIntId(idNormalized, nameof(id));
        await PostAsync<DeleteMutationData>(
            """
            mutation DeleteProduct($shopId: Int!, $id: Int!) {
              deleteProduct(shopId: $shopId, id: $id) {
                success
                message
              }
            }
            """,
            new { shopId, id = numericId });

        if (_cachedProducts is not null)
        {
            string idKey = numericId.ToString(CultureInfo.InvariantCulture);
            _cachedProducts = _cachedProducts.Where(product => !string.Equals(product.Id, idKey, StringComparison.Ordinal)).ToList();
        }
    }

    public async Task<IReadOnlyList<MembershipRank>> GetMembershipRanksAsync()
    {
        int shopId = GetRequiredShopId();
        await EnsureCacheContextAsync(shopId);

        if (_cachedMembershipRanks is not null)
        {
            return _cachedMembershipRanks.ToList();
        }

        IReadOnlyList<MembershipRank> membershipRanks = await FetchMembershipRanksAsync(shopId);
        _cachedMembershipRanks = membershipRanks;
        return membershipRanks.ToList();
    }

    public async Task<MembershipRank> CreateMembershipRankAsync(MembershipRank rank)
    {
        int shopId = GetRequiredShopId();
        await EnsureCacheContextAsync(shopId);
        var response = await PostAsync<CreateMembershipData>(
            """
            mutation CreateMembership($shopId: Int!, $input: MembershipInput!) {
              createMembership(shopId: $shopId, input: $input) {
                id
                name
                color
                minSpentAmount
                discountRate
                priority
                isDefault
              }
            }
            """,
            new
            {
                shopId,
                input = new
                {
                    name = rank.Name,
                    color = rank.Color,
                    minSpentAmount = rank.MinSpentAmount,
                    discountRate = rank.DiscountRate,
                },
            });

        MembershipRank created = MapMembership(RequireData(response.Data?.CreateMembership, "createMembership"));
        if (_cachedMembershipRanks is not null)
        {
            _cachedMembershipRanks = [.. _cachedMembershipRanks, created];
        }

        return created;
    }

    public async Task<MembershipRank> UpdateMembershipRankAsync(MembershipRank rank)
    {
        await EnsureCacheContextAsync(GetRequiredShopId());
        var response = await PostAsync<UpdateMembershipData>(
            """
            mutation UpdateMembership($id: Int!, $input: MembershipInput!) {
              updateMembership(id: $id, input: $input) {
                id
                name
                color
                minSpentAmount
                discountRate
                priority
                isDefault
              }
            }
            """,
            new
            {
                id = ParseRequiredIntId(rank.Id, nameof(rank.Id)),
                input = new
                {
                    name = rank.Name,
                    color = rank.Color,
                    minSpentAmount = rank.MinSpentAmount,
                    discountRate = rank.DiscountRate,
                },
            });

        MembershipRank updated = MapMembership(RequireData(response.Data?.UpdateMembership, "updateMembership"));
        if (_cachedMembershipRanks is not null)
        {
            _cachedMembershipRanks = _cachedMembershipRanks
                .Select(existing => string.Equals(existing.Id, updated.Id, StringComparison.Ordinal) ? updated : existing)
                .ToList();
        }

        return updated;
    }

    public async Task DeleteMembershipRankAsync(string id)
    {
        await EnsureCacheContextAsync(GetRequiredShopId());
        await PostAsync<DeleteMutationData>(
            """
            mutation DeleteMembership($id: Int!) {
              deleteMembership(id: $id) {
                success
                message
              }
            }
            """,
            new { id = ParseRequiredIntId(id, nameof(id)) });

        if (_cachedMembershipRanks is not null)
        {
            _cachedMembershipRanks = _cachedMembershipRanks.Where(rank => !string.Equals(rank.Id, id, StringComparison.Ordinal)).ToList();
        }
    }

    public async Task<IReadOnlyList<MemberRecord>> GetMembersAsync()
    {
        int shopId = GetRequiredShopId();
        await EnsureCacheContextAsync(shopId);

        if (_cachedMembers is not null)
        {
            return _cachedMembers.ToList();
        }

        IReadOnlyList<MemberRecord> members = await FetchMembersAsync(shopId);
        _cachedMembers = members;
        return members.ToList();
    }

    public async Task<MemberRecord> CreateMemberAsync(MemberRecord member)
    {
        int shopId = GetRequiredShopId();
        await EnsureCacheContextAsync(shopId);
        var response = await PostAsync<CreateMemberData>(
            """
            mutation CreateMember($shopId: Int!, $input: MemberInput!) {
              createMember(shopId: $shopId, input: $input) {
                id
                code
                fullName
                phoneNumber
                totalSpentAmount
                membershipId
                membershipName
              }
            }
            """,
            new
            {
                shopId,
                input = new
                {
                    fullName = member.FullName,
                    phoneNumber = member.PhoneNumber,
                    totalSpentAmount = member.TotalSpentAmount,
                },
            });

        MemberRecord created = MapMember(RequireData(response.Data?.CreateMember, "createMember"));
        if (_cachedMembers is not null)
        {
            _cachedMembers = [.. _cachedMembers, created];
        }

        return created;
    }

    public async Task<MemberRecord> UpdateMemberAsync(MemberRecord member)
    {
        await EnsureCacheContextAsync(GetRequiredShopId());
        var response = await PostAsync<UpdateMemberData>(
            """
            mutation UpdateMember($id: Int!, $input: MemberInput!) {
              updateMember(id: $id, input: $input) {
                id
                code
                fullName
                phoneNumber
                totalSpentAmount
                membershipId
                membershipName
              }
            }
            """,
            new
            {
                id = ParseRequiredIntId(member.Id, nameof(member.Id)),
                input = new
                {
                    fullName = member.FullName,
                    phoneNumber = member.PhoneNumber,
                    totalSpentAmount = member.TotalSpentAmount,
                },
            });

        MemberRecord updated = MapMember(RequireData(response.Data?.UpdateMember, "updateMember"));
        if (_cachedMembers is not null)
        {
            _cachedMembers = _cachedMembers
                .Select(existing => string.Equals(existing.Id, updated.Id, StringComparison.Ordinal) ? updated : existing)
                .ToList();
        }

        return updated;
    }

    public async Task DeleteMemberAsync(string id)
    {
        await EnsureCacheContextAsync(GetRequiredShopId());
        await PostAsync<DeleteMutationData>(
            """
            mutation DeleteMember($id: Int!) {
              deleteMember(id: $id) {
                success
                message
              }
            }
            """,
            new { id = ParseRequiredIntId(id, nameof(id)) });

        if (_cachedMembers is not null)
        {
            _cachedMembers = _cachedMembers.Where(member => !string.Equals(member.Id, id, StringComparison.Ordinal)).ToList();
        }
    }

    public async Task<IReadOnlyList<Transaction>> GetTransactionsAsync()
    {
        int shopId = GetRequiredShopId();
        await EnsureCacheContextAsync(shopId);

        if (_cachedTransactions is not null)
        {
            return _cachedTransactions.ToList();
        }

        IReadOnlyList<Transaction> transactions = await FetchTransactionsAsync(shopId);
        _cachedTransactions = transactions;
        return transactions.ToList();
    }

    public async Task<Transaction> UpdateTransactionPaymentMethodAsync(string id, PaymentMethod paymentMethod)
    {
        await EnsureCacheContextAsync(GetRequiredShopId());
        var response = await PostAsync<UpdateTransactionData>(
            """
            mutation UpdateTransactionPaymentMethod($id: Int!, $paymentMethod: String!) {
              updateTransactionPaymentMethod(id: $id, paymentMethod: $paymentMethod) {
                id
                code
                memberId
                customerName
                paymentMethod
                subtotalAmount
                depositRefund
                discountAmount
                totalAmount
                createdAt
                lines {
                  id
                  itemType
                  itemName
                  unitPrice
                  quantity
                  lineTotal
                }
              }
            }
            """,
            new
            {
                id = ParseRequiredIntId(id, nameof(id)),
                paymentMethod = paymentMethod.ToString(),
            });

        Transaction updated = MapTransaction(RequireData(response.Data?.UpdateTransactionPaymentMethod, "updateTransactionPaymentMethod"));
        if (_cachedTransactions is not null)
        {
            _cachedTransactions = _cachedTransactions
                .Select(existing => string.Equals(existing.Id, updated.Id, StringComparison.Ordinal) ? updated : existing)
                .ToList();
        }

        return updated;
    }

    public async Task<Transaction> CompleteAreaSessionCheckoutAsync(AreaSessionCheckoutArgs args)
    {
        int shopId = GetRequiredShopId();
        await EnsureCacheContextAsync(shopId);

        var extras = args.Extras.Select(e => new
        {
            kind = e.Kind,
            catalogId = e.CatalogId,
            quantity = e.Quantity,
            unitPrice = e.UnitPrice,
        }).ToList();

        var response = await PostAsync<CreateAreaSessionCheckoutData>(
            """
            mutation CreateAreaSessionCheckout($shopId: Int!, $input: AreaSessionCheckoutInput!) {
              createAreaSessionCheckout(shopId: $shopId, input: $input) {
                id
                code
                memberId
                customerName
                paymentMethod
                subtotalAmount
                depositRefund
                discountAmount
                totalAmount
                createdAt
                lines {
                  id
                  itemType
                  itemName
                  unitPrice
                  quantity
                  lineTotal
                }
              }
            }
            """,
            new
            {
                shopId,
                input = new
                {
                    areaId = args.AreaId,
                    sessionId = args.SessionId,
                    paymentMethod = args.PaymentMethod.ToString(),
                    areaServiceCharge = args.AreaServiceCharge,
                    extras,
                },
            });

        Transaction created = MapTransaction(RequireData(response.Data?.CreateAreaSessionCheckout, "createAreaSessionCheckout"));
        await _cacheLock.WaitAsync().ConfigureAwait(false);
        try
        {
            _cachedTransactions = null;
            _cachedAreas = null;
        }
        finally
        {
            _cacheLock.Release();
        }

        return created;
    }

    public async Task<ShopProfile> GetShopProfileAsync()
    {
        int shopId = GetRequiredShopId();
        await EnsureCacheContextAsync(shopId);

        var response = await PostAsync<ShopProfileData>(
            """
            query ShopProfile($shopId: Int!) {
              shopProfile(shopId: $shopId) {
                name
                address
                email
                phoneNumber
              }
            }
            """,
            new { shopId });

        ShopProfileDto? dto = response.Data?.ShopProfile;
        return MapShopProfile(dto);
    }

    public async Task<ShopProfile> UpdateShopProfileAsync(ShopProfile shopProfile)
    {
        ArgumentNullException.ThrowIfNull(shopProfile);

        int shopId = GetRequiredShopId();
        await EnsureCacheContextAsync(shopId);

        var response = await PostAsync<UpdateShopProfileData>(
            """
            mutation UpdateShopProfile(
              $shopId: Int!,
              $name: String,
              $address: String,
              $email: String,
              $phoneNumber: String
            ) {
              updateShopProfile(
                shopId: $shopId,
                name: $name,
                address: $address,
                email: $email,
                phoneNumber: $phoneNumber
              ) {
                name
                address
                email
                phoneNumber
              }
            }
            """,
            new
            {
                shopId,
                name = shopProfile.ShopName,
                address = shopProfile.Address,
                email = shopProfile.Email,
                phoneNumber = shopProfile.Phone,
            });

        ShopProfileDto dto = RequireData(response.Data?.UpdateShopProfile, "updateShopProfile");
        return MapShopProfile(dto);
    }

    public async Task WarmUpAsync()
    {
        int shopId = GetRequiredShopId();
        await EnsureCacheContextAsync(shopId);

        await Task.WhenAll(
            WarmCachedAreasAsync(shopId),
            WarmCachedGameTypesAsync(shopId),
            WarmCachedGamesAsync(shopId),
            WarmCachedProductsAsync(shopId),
            WarmCachedMembershipRanksAsync(shopId),
            WarmCachedMembersAsync(shopId),
            WarmCachedTransactionsAsync(shopId));
    }

    public void Clear()
    {
        _cacheLock.Wait();
        try
        {
            ClearCacheUnsafe();
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private async Task<GraphQLResponse<TData>> PostAsync<TData>(string query, object variables)
    {
        string endpoint = BuildEndpoint();
        string payload = JsonSerializer.Serialize(new GraphQLRequest
        {
            Query = query,
            Variables = variables,
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json"),
        };

        using HttpResponseMessage response = await _httpClient.SendAsync(request);
        string responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(string.Format(
                CultureInfo.InvariantCulture,
                "Management API request failed with status {0}: {1}",
                (int)response.StatusCode,
                responseContent));
        }

        GraphQLResponse<TData>? graphQlResponse = JsonSerializer.Deserialize<GraphQLResponse<TData>>(responseContent, JsonOptions);
        if (graphQlResponse is null)
        {
            throw new InvalidOperationException("Management API returned an empty response.");
        }

        if (graphQlResponse.Errors is { Length: > 0 })
        {
            throw new InvalidOperationException(graphQlResponse.Errors[0].Message);
        }

        return graphQlResponse;
    }

    private async Task EnsureCacheContextAsync(int shopId)
    {
        await _cacheLock.WaitAsync().ConfigureAwait(false);
        try
        {
            string shopKey = shopId.ToString(CultureInfo.InvariantCulture);
            if (!string.Equals(_cachedShopId, shopKey, StringComparison.Ordinal))
            {
                ClearCacheUnsafe();
                _cachedShopId = shopKey;
            }
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private void ClearCacheUnsafe()
    {
        _cachedShopId = null;
        _cachedAreas = null;
        _cachedGameTypes = null;
        _cachedGames = null;
        _cachedProducts = null;
        _cachedMembershipRanks = null;
        _cachedMembers = null;
        _cachedTransactions = null;
    }

    private async Task<IReadOnlyList<AreaRecord>> FetchAreasAsync(int shopId)
    {
        var response = await PostAsync<AreasData>(
            """
            query Areas($shopId: Int!) {
              areas(shopId: $shopId) {
                id
                areaName
                playAreaType
                status
                maxCapacity
                hourlyPrice
                customerName
                phoneNumber
                memberId
                checkInDateTime
                capacity
                startTime
                isSessionPaused
                sessionPausedAt
                sessionPausedDurationSeconds
                totalAmount
                activeSessionId
              }
            }
            """,
            new { shopId });

        return response.Data?.Areas?.Select(MapArea).ToList() ?? [];
    }

    private async Task<IReadOnlyList<GameType>> FetchGameTypesAsync(int shopId)
    {
        var response = await PostAsync<GameTypesData>(
            """
            query GameTypes($shopId: Int!) {
              gameTypes(shopId: $shopId) {
                id
                name
              }
            }
            """,
            new { shopId });

        return response.Data?.GameTypes?.Select(MapGameType).ToList() ?? [];
    }

    private async Task<IReadOnlyList<GameRecord>> FetchGamesAsync(int shopId)
    {
        var response = await PostAsync<GamesData>(
            """
            query Games($shopId: Int!) {
              games(shopId: $shopId) {
                id
                name
                hourlyPrice
                minPlayers
                maxPlayers
                gameTypeId
                gameTypeName
                difficulty
                stockQuantity
                imageUri
              }
            }
            """,
            new { shopId });

        return response.Data?.Games?.Select(MapGame).ToList() ?? [];
    }

    private async Task<IReadOnlyList<ProductRecord>> FetchProductsAsync(int shopId)
    {
        var response = await PostAsync<ProductsData>(
            """
            query Products($shopId: Int!) {
              products(shopId: $shopId) {
                id
                name
                price
                productType
                stockQuantity
                imageUri
              }
            }
            """,
            new { shopId });

        return response.Data?.Products?.Select(MapProduct).ToList() ?? [];
    }

    private async Task<IReadOnlyList<MembershipRank>> FetchMembershipRanksAsync(int shopId)
    {
        var response = await PostAsync<MembershipsData>(
            """
            query Memberships($shopId: Int!) {
              memberships(shopId: $shopId) {
                id
                name
                color
                minSpentAmount
                discountRate
                priority
                isDefault
              }
            }
            """,
            new { shopId });

        return response.Data?.Memberships?.Select(MapMembership).ToList() ?? [];
    }

    private async Task<IReadOnlyList<MemberRecord>> FetchMembersAsync(int shopId)
    {
        var response = await PostAsync<MembersData>(
            """
            query Members($shopId: Int!) {
              members(shopId: $shopId) {
                id
                code
                fullName
                phoneNumber
                totalSpentAmount
                membershipId
                membershipName
              }
            }
            """,
            new { shopId });

        return response.Data?.Members?.Select(MapMember).ToList() ?? [];
    }

    private async Task<IReadOnlyList<Transaction>> FetchTransactionsAsync(int shopId)
    {
        var response = await PostAsync<TransactionsData>(
            """
            query Transactions($shopId: Int!) {
              transactions(shopId: $shopId) {
                id
                code
                memberId
                customerName
                paymentMethod
                subtotalAmount
                depositRefund
                discountAmount
                totalAmount
                createdAt
                lines {
                  id
                  itemType
                  itemName
                  unitPrice
                  quantity
                  lineTotal
                }
              }
            }
            """,
            new { shopId });

        return response.Data?.Transactions?.Select(MapTransaction).ToList() ?? [];
    }

    private async Task WarmCachedAreasAsync(int shopId)
    {
        if (_cachedAreas is null)
        {
            _cachedAreas = await FetchAreasAsync(shopId);
        }
    }

    private async Task WarmCachedGameTypesAsync(int shopId)
    {
        if (_cachedGameTypes is null)
        {
            _cachedGameTypes = await FetchGameTypesAsync(shopId);
        }
    }

    private async Task WarmCachedGamesAsync(int shopId)
    {
        if (_cachedGames is null)
        {
            _cachedGames = await FetchGamesAsync(shopId);
        }
    }

    private async Task WarmCachedProductsAsync(int shopId)
    {
        if (_cachedProducts is null)
        {
            _cachedProducts = await FetchProductsAsync(shopId);
        }
    }

    private async Task WarmCachedMembershipRanksAsync(int shopId)
    {
        if (_cachedMembershipRanks is null)
        {
            _cachedMembershipRanks = await FetchMembershipRanksAsync(shopId);
        }
    }

    private async Task WarmCachedMembersAsync(int shopId)
    {
        if (_cachedMembers is null)
        {
            _cachedMembers = await FetchMembersAsync(shopId);
        }
    }

    private async Task WarmCachedTransactionsAsync(int shopId)
    {
        if (_cachedTransactions is null)
        {
            _cachedTransactions = await FetchTransactionsAsync(shopId);
        }
    }

    private string BuildEndpoint()
    {
        string serverAddress = _configurationService.ServerAddress?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(serverAddress))
        {
            serverAddress = "http://localhost";
        }

        if (!serverAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            && !serverAddress.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            serverAddress = $"http://{serverAddress}";
        }

        int port = _configurationService.Port is >= 1 and <= 65535
            ? _configurationService.Port
            : 4000;

        return $"{serverAddress.TrimEnd('/')}:{port}/";
    }

    private int GetRequiredShopId()
    {
        if (!int.TryParse(_authStateService.ShopId, NumberStyles.Integer, CultureInfo.InvariantCulture, out int shopId) || shopId <= 0)
        {
            throw new InvalidOperationException("No authenticated shop is available for management requests.");
        }

        return shopId;
    }

    private static T RequireData<T>(T? data, string operationName) where T : class
    {
        return data ?? throw new InvalidOperationException($"GraphQL operation '{operationName}' returned no data.");
    }

    private static int ParseRequiredIntId(string id, string parameterName)
    {
        string trimmed = id?.Trim() ?? string.Empty;
        if (!int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out int numericId) || numericId <= 0)
        {
            throw new InvalidOperationException($"Invalid numeric id supplied for '{parameterName}'.");
        }

        return numericId;
    }

    private static string NormalizeImageUri(string imageUri)
    {
        return string.Equals(imageUri, "ms-appx:///Assets/Mock.png", StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : imageUri;
    }

    private static AreaRecord MapArea(AreaDto dto)
    {
        return new AreaRecord
        {
            Id = dto.Id,
            AreaName = dto.AreaName,
            PlayAreaType = ParseEnumOrDefault(dto.PlayAreaType, PlayAreaType.Table),
            Status = ParseEnumOrDefault(dto.Status, PlayAreaStatus.Available),
            MaxCapacity = dto.MaxCapacity,
            HourlyPrice = dto.HourlyPrice,
            CustomerName = dto.CustomerName,
            PhoneNumber = dto.PhoneNumber,
            MemberId = dto.MemberId,
            CheckInDateTime = dto.CheckInDateTime,
            Capacity = dto.Capacity,
            StartTime = NormalizeInstantToUtc(dto.StartTime),
            IsSessionPaused = dto.IsSessionPaused,
            SessionPausedAt = NormalizeInstantToUtc(dto.SessionPausedAt),
            SessionPausedDuration = TimeSpan.FromSeconds(dto.SessionPausedDurationSeconds),
            TotalAmount = dto.TotalAmount,
            ActiveSessionId = dto.ActiveSessionId?.ToString(CultureInfo.InvariantCulture),
        };
    }

    private static DateTime? NormalizeInstantToUtc(DateTime? value)
    {
        if (value is not DateTime instant)
        {
            return null;
        }

        return instant.Kind switch
        {
            DateTimeKind.Utc => instant,
            DateTimeKind.Local => DateTime.SpecifyKind(instant, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(instant, DateTimeKind.Utc),
        };
    }

    private static GameType MapGameType(GameTypeDto dto)
    {
        return new GameType
        {
            Id = dto.Id,
            Name = dto.Name,
        };
    }

    private static GameRecord MapGame(GameDto dto)
    {
        return new GameRecord
        {
            Id = dto.Id,
            Name = dto.Name,
            HourlyPrice = dto.HourlyPrice,
            MinPlayers = dto.MinPlayers,
            MaxPlayers = dto.MaxPlayers,
            Type = new GameType
            {
                Id = dto.GameTypeId,
                Name = dto.GameTypeName,
            },
            Difficulty = ParseEnumOrDefault(dto.Difficulty, GameDifficulty.Medium),
            StockQuantity = dto.StockQuantity,
            ImageUri = string.IsNullOrWhiteSpace(dto.ImageUri) ? "ms-appx:///Assets/Mock.png" : dto.ImageUri,
        };
    }

    private static bool IsIncompleteGameRecord(GameRecord game)
    {
        return string.IsNullOrWhiteSpace(game.Id)
            || string.IsNullOrWhiteSpace(game.Name)
            || game.Type is null
            || string.IsNullOrWhiteSpace(game.Type.Id)
            || string.IsNullOrWhiteSpace(game.Type.Name);
    }

    private static ProductRecord MapProduct(ProductDto dto)
    {
        return new ProductRecord
        {
            Id = dto.Id,
            Name = dto.Name,
            Price = dto.Price,
            Type = ParseEnumOrDefault(dto.ProductType, ProductType.Food),
            StockQuantity = dto.StockQuantity,
            ImageUri = string.IsNullOrWhiteSpace(dto.ImageUri) ? "ms-appx:///Assets/Mock.png" : dto.ImageUri,
        };
    }

    private static MembershipRank MapMembership(MembershipDto dto)
    {
        return new MembershipRank
        {
            Id = dto.Id,
            Name = dto.Name,
            Color = dto.Color,
            MinSpentAmount = dto.MinSpentAmount,
            DiscountRate = dto.DiscountRate,
            Priority = dto.Priority,
            IsDefault = dto.IsDefault,
        };
    }

    private static MemberRecord MapMember(MemberDto dto)
    {
        return new MemberRecord
        {
            Id = dto.Id ?? string.Empty,
            Code = dto.Code ?? string.Empty,
            FullName = dto.FullName ?? string.Empty,
            PhoneNumber = dto.PhoneNumber ?? string.Empty,
            TotalSpentAmount = dto.TotalSpentAmount,
            CurrentRank = new MembershipRank
            {
                Id = dto.MembershipId ?? string.Empty,
                Name = dto.MembershipName ?? string.Empty,
            },
        };
    }

    private static Transaction MapTransaction(TransactionDto dto)
    {
        return new Transaction
        {
            Id = dto.Id,
            Code = dto.Code,
            MemberId = dto.MemberId,
            CustomerName = dto.CustomerName,
            PaymentMethod = ParseEnumOrDefault(dto.PaymentMethod, PaymentMethod.Cash),
            SubtotalAmount = dto.SubtotalAmount,
            DepositRefund = dto.DepositRefund,
            DiscountAmount = dto.DiscountAmount,
            TotalAmount = dto.TotalAmount,
            CreatedAt = dto.CreatedAt,
            Lines = dto.Lines?.Select(MapTransactionLine).ToList() ?? [],
        };
    }

    private static TransactionLine MapTransactionLine(TransactionLineDto dto)
    {
        return new TransactionLine
        {
            Id = dto.Id,
            Type = dto.ItemType switch
            {
                "Product" => TransactionLineType.ProductSale,
                "GameRental" => TransactionLineType.BoardGameRental,
                "Session" => TransactionLineType.AreaRental,
                _ => TransactionLineType.AreaRental,
            },
            ItemId = dto.Id,
            ItemName = dto.ItemName,
            UnitPrice = dto.UnitPrice,
            Quantity = dto.Quantity,
            TotalAmount = dto.LineTotal,
        };
    }

    private static ShopProfile MapShopProfile(ShopProfileDto? dto)
    {
        if (dto is null)
        {
            return new ShopProfile();
        }

        return new ShopProfile
        {
            ShopName = dto.Name?.Trim() ?? string.Empty,
            Address = dto.Address?.Trim() ?? string.Empty,
            Email = dto.Email?.Trim() ?? string.Empty,
            Phone = dto.PhoneNumber?.Trim() ?? string.Empty,
        };
    }

    private static TEnum ParseEnumOrDefault<TEnum>(string? value, TEnum fallback)
        where TEnum : struct, Enum
    {
        string? trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return fallback;
        }

        return Enum.TryParse<TEnum>(trimmed, ignoreCase: true, out TEnum parsedValue)
            ? parsedValue
            : fallback;
    }

    private sealed class GraphQLRequest
    {
        [JsonPropertyName("query")]
        public string Query { get; init; } = string.Empty;

        [JsonPropertyName("variables")]
        public object Variables { get; init; } = new();
    }

    private sealed class GraphQLResponse<TData>
    {
        [JsonPropertyName("data")]
        public TData? Data { get; init; }

        [JsonPropertyName("errors")]
        public GraphQLError[]? Errors { get; init; }
    }

    private sealed class GraphQLError
    {
        [JsonPropertyName("message")]
        public string Message { get; init; } = string.Empty;
    }

    private sealed class MutationResponseDto
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; }

        [JsonPropertyName("message")]
        public string Message { get; init; } = string.Empty;
    }

    private sealed class AreasData { [JsonPropertyName("areas")] public AreaDto[]? Areas { get; init; } }
    private sealed class CreateAreaData { [JsonPropertyName("createArea")] public AreaDto? CreateArea { get; init; } }
    private sealed class UpdateAreaData { [JsonPropertyName("updateArea")] public AreaDto? UpdateArea { get; init; } }
    private sealed class StartAreaSessionData
    {
        [JsonPropertyName("startAreaSession")]
        public StartAreaSessionPayloadDto? StartAreaSession { get; init; }
    }

    private sealed class StartAreaSessionPayloadDto
    {
        [JsonPropertyName("sessionId")]
        public int SessionId { get; init; }

        [JsonPropertyName("startTime")]
        public DateTime StartTime { get; init; }
    }

    private sealed class GameTypesData { [JsonPropertyName("gameTypes")] public GameTypeDto[]? GameTypes { get; init; } }
    private sealed class CreateGameTypeData { [JsonPropertyName("createGameType")] public GameTypeDto? CreateGameType { get; init; } }
    private sealed class UpdateGameTypeData { [JsonPropertyName("updateGameType")] public GameTypeDto? UpdateGameType { get; init; } }
    private sealed class GamesData { [JsonPropertyName("games")] public GameDto[]? Games { get; init; } }
    private sealed class CreateGameData { [JsonPropertyName("createGame")] public GameDto? CreateGame { get; init; } }
    private sealed class UpdateGameData { [JsonPropertyName("updateGame")] public GameDto? UpdateGame { get; init; } }

    private sealed class ProductsData { [JsonPropertyName("products")] public ProductDto[]? Products { get; init; } }
    private sealed class CreateProductData { [JsonPropertyName("createProduct")] public ProductDto? CreateProduct { get; init; } }
    private sealed class UpdateProductData { [JsonPropertyName("updateProduct")] public ProductDto? UpdateProduct { get; init; } }

    private sealed class MembershipsData { [JsonPropertyName("memberships")] public MembershipDto[]? Memberships { get; init; } }
    private sealed class CreateMembershipData { [JsonPropertyName("createMembership")] public MembershipDto? CreateMembership { get; init; } }
    private sealed class UpdateMembershipData { [JsonPropertyName("updateMembership")] public MembershipDto? UpdateMembership { get; init; } }
    private sealed class MembersData { [JsonPropertyName("members")] public MemberDto[]? Members { get; init; } }
    private sealed class CreateMemberData { [JsonPropertyName("createMember")] public MemberDto? CreateMember { get; init; } }
    private sealed class UpdateMemberData { [JsonPropertyName("updateMember")] public MemberDto? UpdateMember { get; init; } }

    private sealed class TransactionsData { [JsonPropertyName("transactions")] public TransactionDto[]? Transactions { get; init; } }
    private sealed class UpdateTransactionData { [JsonPropertyName("updateTransactionPaymentMethod")] public TransactionDto? UpdateTransactionPaymentMethod { get; init; } }
    private sealed class CreateAreaSessionCheckoutData
    {
        [JsonPropertyName("createAreaSessionCheckout")]
        public TransactionDto? CreateAreaSessionCheckout { get; init; }
    }
    private sealed class ShopProfileData
    {
        [JsonPropertyName("shopProfile")]
        public ShopProfileDto? ShopProfile { get; init; }
    }
    private sealed class UpdateShopProfileData
    {
        [JsonPropertyName("updateShopProfile")]
        public ShopProfileDto? UpdateShopProfile { get; init; }
    }
    private sealed class DeleteMutationData
    {
        [JsonExtensionData]
        public IDictionary<string, JsonElement>? Payload { get; init; }
    }

    private sealed class AreaDto
    {
        public string Id { get; init; } = string.Empty;
        public string AreaName { get; init; } = string.Empty;
        public string PlayAreaType { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public int MaxCapacity { get; init; }
        public decimal HourlyPrice { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string? MemberId { get; init; }
        public DateTime? CheckInDateTime { get; init; }
        public int Capacity { get; init; }
        public DateTime? StartTime { get; init; }
        public bool IsSessionPaused { get; init; }
        public DateTime? SessionPausedAt { get; init; }
        public int SessionPausedDurationSeconds { get; init; }
        public decimal TotalAmount { get; init; }
        public int? ActiveSessionId { get; init; }
    }

    private sealed class GameTypeDto
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
    }

    private sealed class GameDto
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public decimal HourlyPrice { get; init; }
        public int MinPlayers { get; init; }
        public int MaxPlayers { get; init; }
        public string GameTypeId { get; init; } = string.Empty;
        public string GameTypeName { get; init; } = string.Empty;
        public string Difficulty { get; init; } = string.Empty;
        public int StockQuantity { get; init; }
        public string ImageUri { get; init; } = string.Empty;
    }

    private sealed class ProductDto
    {
        [JsonPropertyName("id")]
        public string Id { get; init; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; init; }

        [JsonPropertyName("productType")]
        public string ProductType { get; init; } = string.Empty;

        [JsonPropertyName("stockQuantity")]
        public int StockQuantity { get; init; }

        [JsonPropertyName("imageUri")]
        public string ImageUri { get; init; } = string.Empty;
    }

    private sealed class MembershipDto
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Color { get; init; } = string.Empty;
        public decimal MinSpentAmount { get; init; }
        public decimal DiscountRate { get; init; }
        public int Priority { get; init; }
        public bool IsDefault { get; init; }
    }

    private sealed class MemberDto
    {
        public string Id { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public decimal TotalSpentAmount { get; init; }
        public string MembershipId { get; init; } = string.Empty;
        public string MembershipName { get; init; } = string.Empty;
    }

    private sealed class TransactionDto
    {
        public string Id { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
        public string? MemberId { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public string PaymentMethod { get; init; } = string.Empty;
        public decimal SubtotalAmount { get; init; }
        public decimal DepositRefund { get; init; }
        public decimal DiscountAmount { get; init; }
        public decimal TotalAmount { get; init; }
        public DateTime CreatedAt { get; init; }
        public TransactionLineDto[]? Lines { get; init; }
    }

    private sealed class TransactionLineDto
    {
        public string Id { get; init; } = string.Empty;
        public string ItemType { get; init; } = string.Empty;
        public string ItemName { get; init; } = string.Empty;
        public decimal UnitPrice { get; init; }
        public decimal Quantity { get; init; }
        public decimal LineTotal { get; init; }
    }

    private sealed class ShopProfileDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("address")]
        public string? Address { get; init; }

        [JsonPropertyName("email")]
        public string? Email { get; init; }

        [JsonPropertyName("phoneNumber")]
        public string? PhoneNumber { get; init; }
    }
}
