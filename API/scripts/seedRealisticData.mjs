import argon2 from "argon2";
import { AppDataSource } from "../src/config/database.js";
const SEED_SHOP_EMAIL = "seed.playpoint@demo.local";
const SEED_SHOP_PASSWORD = "Seed@123456";
const UTC7_OFFSET_HOURS = 7;
const TRANSACTION_COUNT = 180;
const MEMBERSHIPS = [
    { name: "Bronze", color: "#B45309", minSpent: 0, discountPercent: 0 },
    { name: "Silver", color: "#6B7280", minSpent: 3_000_000, discountPercent: 3 },
    { name: "Gold", color: "#D97706", minSpent: 8_000_000, discountPercent: 5 },
    { name: "Diamond", color: "#7C3AED", minSpent: 15_000_000, discountPercent: 8 },
];
const MEMBERS = [
    { name: "Nguyễn Minh Khang", phone: "0903000001" },
    { name: "Trần Bảo Châu", phone: "0903000002" },
    { name: "Lê Quỳnh Như", phone: "0903000003" },
    { name: "Phạm Hoàng Nam", phone: "0903000004" },
    { name: "Võ Gia Huy", phone: "0903000005" },
    { name: "Đỗ Khánh Linh", phone: "0903000006" },
    { name: "Bùi Thành Đạt", phone: "0903000007" },
    { name: "Hoàng Thu Trang", phone: "0903000008" },
    { name: "Đặng Đức Anh", phone: "0903000009" },
    { name: "Trương Mỹ Tiên", phone: "0903000010" },
    { name: "Ngô Tuấn Kiệt", phone: "0903000011" },
    { name: "Phan Quốc Bảo", phone: "0903000012" },
    { name: "Mai Hồng Hạnh", phone: "0903000013" },
    { name: "Tạ Bảo Ngọc", phone: "0903000014" },
    { name: "Dương Ngọc Hân", phone: "0903000015" },
    { name: "Lý Hoàng Long", phone: "0903000016" },
    { name: "Trần Gia Hân", phone: "0903000017" },
    { name: "Nguyễn Thu Phương", phone: "0903000018" },
    { name: "Phùng Văn Phúc", phone: "0903000019" },
    { name: "Lê Ngọc Trâm", phone: "0903000020" },
];
const AREA_BLUEPRINTS = [
    { name: "Bàn A1", hourlyPrice: 60_000, maxCapacity: 4 },
    { name: "Bàn A2", hourlyPrice: 60_000, maxCapacity: 4 },
    { name: "Bàn B1", hourlyPrice: 70_000, maxCapacity: 6 },
    { name: "Bàn B2", hourlyPrice: 70_000, maxCapacity: 6 },
    { name: "Phòng VIP 1", hourlyPrice: 130_000, maxCapacity: 10 },
    { name: "Phòng VIP 2", hourlyPrice: 150_000, maxCapacity: 12 },
];
const PRODUCTS = [
    { name: "Cà phê sữa đá", price: 32_000, stock: 500 },
    { name: "Bạc xỉu", price: 35_000, stock: 320 },
    { name: "Trà đào cam sả", price: 40_000, stock: 420 },
    { name: "Trà vải", price: 38_000, stock: 280 },
    { name: "Soda chanh dây", price: 42_000, stock: 260 },
    { name: "Nước suối", price: 15_000, stock: 700 },
    { name: "Khoai tây chiên", price: 45_000, stock: 260 },
    { name: "Xúc xích nướng", price: 39_000, stock: 240 },
    { name: "Cơm cuộn rong biển", price: 52_000, stock: 190 },
    { name: "Mì xào hải sản", price: 69_000, stock: 150 },
    { name: "Gà rán sốt cay", price: 79_000, stock: 130 },
    { name: "Bánh waffle kem", price: 55_000, stock: 120 },
];
const GAMES = [
    { categoryName: "Party", name: "Ma sói", difficulty: "Easy", hourlyPrice: 25_000, minPlayer: 5, maxPlayer: 16, stock: 8 },
    { categoryName: "Party", name: "Exploding Kittens", difficulty: "Easy", hourlyPrice: 20_000, minPlayer: 2, maxPlayer: 5, stock: 6 },
    { categoryName: "Family", name: "Ticket to Ride", difficulty: "Medium", hourlyPrice: 28_000, minPlayer: 2, maxPlayer: 5, stock: 4 },
    { categoryName: "Family", name: "Catan", difficulty: "Medium", hourlyPrice: 30_000, minPlayer: 3, maxPlayer: 4, stock: 5 },
    { categoryName: "Strategy", name: "Terraforming Mars", difficulty: "Hard", hourlyPrice: 35_000, minPlayer: 1, maxPlayer: 5, stock: 3 },
    { categoryName: "Strategy", name: "Ark Nova", difficulty: "Hard", hourlyPrice: 38_000, minPlayer: 1, maxPlayer: 4, stock: 2 },
];
function mulberry32(seed) {
    return function random() {
        let t = seed += 0x6d2b79f5;
        t = Math.imul(t ^ (t >>> 15), t | 1);
        t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
        return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
    };
}
function randomInt(random, minInclusive, maxInclusive) {
    return Math.floor(random() * (maxInclusive - minInclusive + 1)) + minInclusive;
}
function pickOne(random, items) {
    return items[randomInt(random, 0, items.length - 1)];
}
function pickSome(random, items, count) {
    const clone = [...items];
    const selected = [];
    for (let i = 0; i < count && clone.length > 0; i += 1) {
        const idx = randomInt(random, 0, clone.length - 1);
        selected.push(clone[idx]);
        clone.splice(idx, 1);
    }
    return selected;
}
function toUtc7Timestamp(value) {
    const shifted = new Date(value.getTime() + UTC7_OFFSET_HOURS * 60 * 60 * 1000);
    const year = shifted.getUTCFullYear();
    const month = String(shifted.getUTCMonth() + 1).padStart(2, "0");
    const day = String(shifted.getUTCDate()).padStart(2, "0");
    const hours = String(shifted.getUTCHours()).padStart(2, "0");
    const minutes = String(shifted.getUTCMinutes()).padStart(2, "0");
    const seconds = String(shifted.getUTCSeconds()).padStart(2, "0");
    return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
}
async function getEnumValue(enumTypeName, preferredLabels) {
    const rows = await AppDataSource.query(`
        SELECT e.enumlabel AS "Label"
        FROM pg_type t
        JOIN pg_enum e ON e.enumtypid = t.oid
        WHERE t.typname = $1
        ORDER BY e.enumsortorder ASC
        `, [enumTypeName]);
    if (rows.length === 0) {
        throw new Error(`Enum type ${enumTypeName} was not found.`);
    }
    for (const preferred of preferredLabels) {
        const match = rows.find((row) => row.Label.toLowerCase() === preferred.toLowerCase());
        if (match) {
            return match.Label;
        }
    }
    return rows[0].Label;
}
function resolveMembership(totalSpent, memberships) {
    let selected = memberships[0];
    for (const tier of memberships) {
        if (totalSpent >= tier.minSpent) {
            selected = tier;
        }
    }
    return selected;
}
async function ensureSeedShopId() {
    const existing = await AppDataSource.query(`SELECT "ID" FROM "Shop" WHERE "Email" = $1 LIMIT 1`, [SEED_SHOP_EMAIL]);
    if (existing.length > 0) {
        return Number(existing[0].ID);
    }
    const hashedPassword = await argon2.hash(SEED_SHOP_PASSWORD);
    const created = await AppDataSource.query(`
        INSERT INTO "Shop" ("Email", "HashedPassword", "Name", "Address", "PhoneNumber")
        VALUES ($1, $2, $3, $4, $5)
        RETURNING "ID"
        `, [
        SEED_SHOP_EMAIL,
        hashedPassword,
        "PlayPoint Seed Demo",
        "123 Nguyễn Huệ, Quận 1, TP.HCM",
        "0903999888",
    ]);
    return Number(created[0].ID);
}
async function cleanShopData(shopId) {
    await AppDataSource.query(`
        DELETE FROM "TransactionLine"
        WHERE "TransactionID" IN (
            SELECT "ID" FROM "Transaction" WHERE "ShopID" = $1
        )
        `, [shopId]);
    await AppDataSource.query(`
        DELETE FROM "GameRental"
        WHERE "SessionID" IN (
            SELECT "ID" FROM "Session" WHERE "ShopID" = $1
        )
        `, [shopId]);
    await AppDataSource.query(`DELETE FROM "Transaction" WHERE "ShopID" = $1`, [shopId]);
    await AppDataSource.query(`DELETE FROM "Reservation" WHERE "ShopID" = $1`, [shopId]);
    await AppDataSource.query(`DELETE FROM "Session" WHERE "ShopID" = $1`, [shopId]);
    await AppDataSource.query(`DELETE FROM "Member" WHERE "ShopID" = $1`, [shopId]);
    await AppDataSource.query(`DELETE FROM "Membership" WHERE "ShopID" = $1`, [shopId]);
    await AppDataSource.query(`DELETE FROM "Product" WHERE "ShopID" = $1`, [shopId]);
    await AppDataSource.query(`DELETE FROM "BoardGame" WHERE "ShopID" = $1`, [shopId]);
    await AppDataSource.query(`DELETE FROM "GameCategory" WHERE "ShopID" = $1`, [shopId]);
    await AppDataSource.query(`DELETE FROM "Area" WHERE "ShopID" = $1`, [shopId]);
}
async function seedRealisticData() {
    await AppDataSource.initialize();
    try {
        const shopId = await ensureSeedShopId();
        await cleanShopData(shopId);
        const areaTypeTable = await getEnumValue("Area_Type_enum", ["Table"]);
        const areaTypeRoom = await getEnumValue("Area_Type_enum", ["Room"]);
        const productTypeFood = await getEnumValue("Product_Type_enum", ["Food"]);
        const productTypeDrink = await getEnumValue("Product_Type_enum", ["Drink"]);
        const paymentCash = await getEnumValue("Transaction_PaymentMethod_enum", ["Cash"]);
        const paymentBanking = await getEnumValue("Transaction_PaymentMethod_enum", ["Banking"]);
        const reservationStatus = await getEnumValue("Reservation_Status_enum", ["Reserved", "Active", "Pending"]);
        const areasToCreate = AREA_BLUEPRINTS.map((area, index) => ({
            ...area,
            type: index <= 3 ? areaTypeTable : areaTypeRoom,
        }));
        const productsToCreate = PRODUCTS.map((product, index) => ({
            ...product,
            type: index <= 5 ? productTypeDrink : productTypeFood,
        }));
        for (const tier of MEMBERSHIPS) {
            await AppDataSource.query(`
                INSERT INTO "Membership" ("ShopID", "Name", "Color", "MinSpent", "DiscountPercent")
                VALUES ($1, $2, $3, $4, $5)
                `, [shopId, tier.name, tier.color, tier.minSpent, tier.discountPercent]);
        }
        const membershipRows = await AppDataSource.query(`
            SELECT "ID", "Name", "MinSpent", "DiscountPercent"
            FROM "Membership"
            WHERE "ShopID" = $1
            ORDER BY "MinSpent" ASC, "ID" ASC
            `, [shopId]);
        const memberships = membershipRows.map((row) => ({
            id: Number(row.ID),
            name: String(row.Name),
            minSpent: Number(row.MinSpent),
            discountPercent: Number(row.DiscountPercent),
        }));
        for (const member of MEMBERS) {
            const bronze = memberships[0];
            await AppDataSource.query(`
                INSERT INTO "Member" ("ShopID", "MembershipID", "Name", "PhoneNumber", "TotalSpent")
                VALUES ($1, $2, $3, $4, 0)
                `, [shopId, bronze.id, member.name, member.phone]);
        }
        const memberRows = await AppDataSource.query(`
            SELECT "ID", "Name"
            FROM "Member"
            WHERE "ShopID" = $1
            ORDER BY "ID" ASC
            `, [shopId]);
        const members = memberRows.map((row) => ({ id: Number(row.ID), name: row.Name }));
        for (const area of areasToCreate) {
            await AppDataSource.query(`
                INSERT INTO "Area" ("ShopID", "Name", "Type", "HourlyPrice", "MaxCapacity")
                VALUES ($1, $2, $3::"Area_Type_enum", $4, $5)
                `, [shopId, area.name, area.type, area.hourlyPrice, area.maxCapacity]);
        }
        const areaRows = await AppDataSource.query(`
            SELECT "ID", "Name", "HourlyPrice", "MaxCapacity"
            FROM "Area"
            WHERE "ShopID" = $1
            ORDER BY "ID" ASC
            `, [shopId]);
        const areas = areaRows.map((row) => ({
            id: Number(row.ID),
            name: row.Name,
            hourlyPrice: Number(row.HourlyPrice),
            maxCapacity: Number(row.MaxCapacity),
        }));
        const categoryNames = [...new Set(GAMES.map((game) => game.categoryName))];
        for (const categoryName of categoryNames) {
            await AppDataSource.query(`INSERT INTO "GameCategory" ("ShopID", "Name") VALUES ($1, $2)`, [shopId, categoryName]);
        }
        const categoryRows = await AppDataSource.query(`
            SELECT "ID", "Name"
            FROM "GameCategory"
            WHERE "ShopID" = $1
            `, [shopId]);
        const categoryIdByName = new Map(categoryRows.map((row) => [row.Name, Number(row.ID)]));
        for (const game of GAMES) {
            const categoryId = categoryIdByName.get(game.categoryName);
            if (!categoryId) {
                continue;
            }
            await AppDataSource.query(`
                INSERT INTO "BoardGame" (
                    "ShopID", "CategoryID", "Name", "Difficulty", "ImageUrl",
                    "HourlyPrice", "MinPlayer", "MaxPlayer", "StockQuantity"
                )
                VALUES ($1, $2, $3, $4::"BoardGame_Difficulty_enum", $5, $6, $7, $8, $9)
                `, [
                shopId,
                categoryId,
                game.name,
                game.difficulty,
                "https://placehold.co/400x300/png",
                game.hourlyPrice,
                game.minPlayer,
                game.maxPlayer,
                game.stock,
            ]);
        }
        for (const product of productsToCreate) {
            await AppDataSource.query(`
                INSERT INTO "Product" ("ShopID", "Name", "Type", "ImageUrl", "Price", "StockQuantity")
                VALUES ($1, $2, $3::"Product_Type_enum", $4, $5, $6)
                `, [
                shopId,
                product.name,
                product.type,
                "https://placehold.co/300x200/png",
                product.price,
                product.stock,
            ]);
        }
        const productRows = await AppDataSource.query(`
            SELECT "ID", "Name", "Price"
            FROM "Product"
            WHERE "ShopID" = $1
            ORDER BY "ID" ASC
            `, [shopId]);
        const products = productRows.map((row) => ({
            id: Number(row.ID),
            name: row.Name,
            price: Number(row.Price),
        }));
        const random = mulberry32(20260513);
        const spentByMemberId = new Map();
        for (const member of members) {
            spentByMemberId.set(member.id, 0);
        }
        const now = new Date();
        const minStart = new Date(now.getTime() - 55 * 24 * 60 * 60 * 1000);
        const maxStart = new Date(now.getTime() - 2 * 60 * 60 * 1000);
        const spreadMs = maxStart.getTime() - minStart.getTime();
        for (let i = 0; i < TRANSACTION_COUNT; i += 1) {
            const area = pickOne(random, areas);
            const member = random() < 0.72 ? pickOne(random, members) : null;
            const guestCount = randomInt(random, 1, Math.max(1, area.maxCapacity));
            const startAt = new Date(minStart.getTime() + Math.floor(random() * spreadMs));
            const durationMinutes = randomInt(random, 45, 220);
            const endAt = new Date(startAt.getTime() + durationMinutes * 60 * 1000);
            const sessionHours = durationMinutes / 60;
            const areaFee = Math.round(sessionHours * area.hourlyPrice);
            const productCount = randomInt(random, 1, 3);
            const selectedProducts = pickSome(random, products, productCount);
            const productLines = selectedProducts.map((product) => {
                const quantity = randomInt(random, 1, 3);
                const lineTotal = quantity * product.price;
                return {
                    productId: product.id,
                    quantity,
                    unitPrice: product.price,
                    lineTotal,
                };
            });
            const productsTotal = productLines.reduce((acc, line) => acc + line.lineTotal, 0);
            const subtotal = areaFee + productsTotal;
            const discountRate = member
                ? resolveMembership(spentByMemberId.get(member.id) ?? 0, memberships).discountPercent
                : 0;
            const discountAmount = Math.round(subtotal * (discountRate / 100));
            const totalAmount = subtotal - discountAmount;
            const paymentMethod = random() < 0.62 ? paymentCash : paymentBanking;
            const sessionInsert = await AppDataSource.query(`
                INSERT INTO "Session" (
                    "ShopID", "AreaID", "MemberID", "ReservationID", "GuestCount",
                    "StartTime", "EndTime", "Duration", "SessionAmount"
                )
                VALUES (
                    $1, $2, $3, NULL, $4,
                    $5::timestamp, $6::timestamp, $7, $8
                )
                RETURNING "ID"
                `, [
                shopId,
                area.id,
                member?.id ?? null,
                guestCount,
                toUtc7Timestamp(startAt),
                toUtc7Timestamp(endAt),
                durationMinutes,
                totalAmount,
            ]);
            const sessionId = Number(sessionInsert[0].ID);
            const transactionInsert = await AppDataSource.query(`
                INSERT INTO "Transaction" (
                    "ShopID", "MemberID", "SubtotalAmount", "DepositAppliedAmount",
                    "DiscountAmount", "TotalAmount", "PaymentMethod", "CreatedAt"
                )
                VALUES ($1, $2, $3, 0, $4, $5, $6::"Transaction_PaymentMethod_enum", $7::timestamp)
                RETURNING "ID"
                `, [
                shopId,
                member?.id ?? null,
                subtotal,
                discountAmount,
                totalAmount,
                paymentMethod,
                toUtc7Timestamp(endAt),
            ]);
            const transactionId = Number(transactionInsert[0].ID);
            await AppDataSource.query(`
                INSERT INTO "TransactionLine" (
                    "TransactionID", "ItemType", "SessionID", "GameRentalID", "ProductID",
                    "UnitPrice", "Quantity", "LineTotal"
                )
                VALUES (
                    $1, 'Session'::"TransactionLine_ItemType_enum", $2, NULL, NULL,
                    $3, 1, $4
                )
                `, [transactionId, sessionId, areaFee, areaFee]);
            for (const line of productLines) {
                await AppDataSource.query(`
                    INSERT INTO "TransactionLine" (
                        "TransactionID", "ItemType", "SessionID", "GameRentalID", "ProductID",
                        "UnitPrice", "Quantity", "LineTotal"
                    )
                    VALUES (
                        $1, 'Product'::"TransactionLine_ItemType_enum", $2, NULL, $3,
                        $4, $5, $6
                    )
                    `, [
                    transactionId,
                    sessionId,
                    line.productId,
                    line.unitPrice,
                    line.quantity,
                    line.lineTotal,
                ]);
                await AppDataSource.query(`
                    UPDATE "Product"
                    SET "StockQuantity" = GREATEST(0, "StockQuantity" - $1)
                    WHERE "ID" = $2 AND "ShopID" = $3
                    `, [line.quantity, line.productId, shopId]);
            }
            if (member) {
                const running = (spentByMemberId.get(member.id) ?? 0) + totalAmount;
                spentByMemberId.set(member.id, running);
            }
        }
        const openSessionAreas = pickSome(random, areas, Math.min(2, areas.length));
        for (const area of openSessionAreas) {
            const startAt = new Date(now.getTime() - randomInt(random, 15, 120) * 60 * 1000);
            await AppDataSource.query(`
                INSERT INTO "Session" (
                    "ShopID", "AreaID", "MemberID", "ReservationID", "GuestCount",
                    "StartTime", "EndTime", "Duration", "SessionAmount"
                )
                VALUES ($1, $2, NULL, NULL, $3, $4::timestamp, NULL, 0, 0)
                `, [shopId, area.id, randomInt(random, 2, Math.max(2, area.maxCapacity)), toUtc7Timestamp(startAt)]);
        }
        const reservedCandidates = areas.filter((area) => !openSessionAreas.some((s) => s.id === area.id));
        if (reservedCandidates.length > 0 && members.length > 0) {
            const reservedArea = pickOne(random, reservedCandidates);
            const reservedMember = pickOne(random, members);
            const reservationAt = new Date(now.getTime() + randomInt(random, 1, 8) * 60 * 60 * 1000);
            await AppDataSource.query(`
                INSERT INTO "Reservation" (
                    "ShopID", "MemberID", "AreaID", "ReservationTime", "GuestCount", "DepositAmount", "Status"
                )
                VALUES ($1, $2, $3, $4::timestamp, $5, $6, $7::"Reservation_Status_enum")
                `, [
                shopId,
                reservedMember.id,
                reservedArea.id,
                toUtc7Timestamp(reservationAt),
                randomInt(random, 2, Math.max(2, reservedArea.maxCapacity)),
                randomInt(random, 100_000, 300_000),
                reservationStatus,
            ]);
        }
        for (const member of members) {
            const totalSpent = Math.round(spentByMemberId.get(member.id) ?? 0);
            const membership = resolveMembership(totalSpent, memberships);
            await AppDataSource.query(`
                UPDATE "Member"
                SET "MembershipID" = $2,
                    "TotalSpent" = $3
                WHERE "ID" = $1 AND "ShopID" = $4
                `, [member.id, membership.id, totalSpent, shopId]);
        }
        const summaryRows = await AppDataSource.query(`
            SELECT
                (SELECT COUNT(*) FROM "Area" WHERE "ShopID" = $1) AS "AreaCount",
                (SELECT COUNT(*) FROM "Member" WHERE "ShopID" = $1) AS "MemberCount",
                (SELECT COUNT(*) FROM "BoardGame" WHERE "ShopID" = $1) AS "GameCount",
                (SELECT COUNT(*) FROM "Product" WHERE "ShopID" = $1) AS "ProductCount",
                (SELECT COUNT(*) FROM "Session" WHERE "ShopID" = $1) AS "SessionCount",
                (SELECT COUNT(*) FROM "Transaction" WHERE "ShopID" = $1) AS "TransactionCount"
            `, [shopId]);
        const summary = summaryRows[0];
        console.log("Seed completed successfully.");
        console.log(`Shop email: ${SEED_SHOP_EMAIL}`);
        console.log(`Shop password: ${SEED_SHOP_PASSWORD}`);
        console.log(`Areas: ${summary.AreaCount}`);
        console.log(`Members: ${summary.MemberCount}`);
        console.log(`Games: ${summary.GameCount}`);
        console.log(`Products: ${summary.ProductCount}`);
        console.log(`Sessions: ${summary.SessionCount}`);
        console.log(`Transactions: ${summary.TransactionCount}`);
    }
    finally {
        if (AppDataSource.isInitialized) {
            await AppDataSource.destroy();
        }
    }
}
seedRealisticData().catch((error) => {
    console.error("Seed failed:", error);
    process.exit(1);
});
//# sourceMappingURL=seedRealisticData.mjs.map