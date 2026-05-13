import { AppDataSource } from "../../../config/database.js";
import type { AreaSessionCheckoutInput } from "../../types/management/transaction.js";
import { TransactionDto } from "../../types/management/transaction.js";
import { requireRow, toDateCell, toNumberCell, toStringCell, cell, type SqlRow } from "./shared.js";

export async function getTransactions(shopId: number): Promise<TransactionDto[]> {
    const transactionRows = await AppDataSource.query(`
        SELECT t."ID", t."MemberID", t."SubtotalAmount", t."DepositAppliedAmount", t."DiscountAmount", t."TotalAmount", t."PaymentMethod", t."CreatedAt",
               m."Name" AS "MemberName"
        FROM "Transaction" t
        LEFT JOIN "Member" m ON m."ID" = t."MemberID"
        WHERE t."ShopID" = $1
        ORDER BY t."CreatedAt" DESC, t."ID" DESC
    `, [shopId]) as SqlRow[];

    const lineRows = await AppDataSource.query(`
        SELECT
            tl."ID",
            tl."TransactionID",
            tl."ItemType",
            COALESCE(p."Name", bg."Name", a."Name", '') AS "ItemName",
            tl."UnitPrice",
            tl."Quantity",
            tl."LineTotal"
        FROM "TransactionLine" tl
        LEFT JOIN "Product" p ON p."ID" = tl."ProductID"
        LEFT JOIN "GameRental" gr ON gr."ID" = tl."GameRentalID"
        LEFT JOIN "BoardGame" bg ON bg."ID" = gr."BoardGameID"
        LEFT JOIN "Session" s ON s."ID" = tl."SessionID"
        LEFT JOIN "Area" a ON a."ID" = s."AreaID"
        WHERE tl."TransactionID" IN (
            SELECT "ID" FROM "Transaction" WHERE "ShopID" = $1
        )
        ORDER BY tl."ID" ASC
    `, [shopId]) as SqlRow[];

    return transactionRows.map((row: any) => mapTransactionRow(row, lineRows));
}

export async function updateTransactionPaymentMethod(id: number, paymentMethod: string): Promise<TransactionDto> {
    const rows = await AppDataSource.query(`
        UPDATE "Transaction"
        SET "PaymentMethod" = $2::"Transaction_PaymentMethod_enum"
        WHERE "ID" = $1
        RETURNING "ID", "MemberID", "SubtotalAmount", "DepositAppliedAmount", "DiscountAmount", "TotalAmount", "PaymentMethod", "CreatedAt"
    `, [id, paymentMethod]) as SqlRow[];

    const row = requireRow(rows, "Transaction", id);
    const memberRows = cell(row, "MemberID")
        ? await AppDataSource.query(`SELECT "Name" FROM "Member" WHERE "ID" = $1`, [toNumberCell(row, "MemberID")]) as SqlRow[]
        : [];
    const lineRows = await AppDataSource.query(`
        SELECT
            tl."ID",
            tl."TransactionID",
            tl."ItemType",
            COALESCE(p."Name", bg."Name", a."Name", '') AS "ItemName",
            tl."UnitPrice",
            tl."Quantity",
            tl."LineTotal"
        FROM "TransactionLine" tl
        LEFT JOIN "Product" p ON p."ID" = tl."ProductID"
        LEFT JOIN "GameRental" gr ON gr."ID" = tl."GameRentalID"
        LEFT JOIN "BoardGame" bg ON bg."ID" = gr."BoardGameID"
        LEFT JOIN "Session" s ON s."ID" = tl."SessionID"
        LEFT JOIN "Area" a ON a."ID" = s."AreaID"
        WHERE tl."TransactionID" = $1
        ORDER BY tl."ID" ASC
    `, [id]) as SqlRow[];

    return mapTransactionRow({ ...row, MemberName: memberRows.length > 0 ? toStringCell(memberRows[0] as SqlRow, "Name") : undefined }, lineRows);
}

export async function persistAreaSessionCheckout(shopId: number, input: AreaSessionCheckoutInput): Promise<TransactionDto> {
    const areaId = input.areaId;
    const sessionId = input.sessionId;
    const paymentMethod = input.paymentMethod?.trim() || "Cash";

    let extrasTotal = 0;
    for (const line of input.extras ?? []) {
        const lineTotal = Number(line.quantity) * Number(line.unitPrice);
        if (lineTotal < 0 || !Number.isFinite(lineTotal)) {
            throw new Error("Invalid extra line amount.");
        }
        extrasTotal += lineTotal;
    }

    const areaCharge = Number(input.areaServiceCharge);
    if (!Number.isFinite(areaCharge) || areaCharge < 0) {
        throw new Error("Invalid area service charge.");
    }

    const grandTotal = areaCharge + extrasTotal;

    const queryRunner = AppDataSource.createQueryRunner();
    await queryRunner.connect();
    await queryRunner.startTransaction();
    try {
        const sessionRows = (await queryRunner.query(
            `
            SELECT s."ID", s."AreaID", s."ShopID", s."MemberID"
            FROM "Session" s
            WHERE s."ID" = $1 AND s."ShopID" = $2 AND s."AreaID" = $3 AND s."EndTime" IS NULL
            `,
            [sessionId, shopId, areaId],
        )) as SqlRow[];

        if (sessionRows.length === 0) {
            throw new Error("Active session not found for this area.");
        }

        const sessionRow = requireRow(sessionRows, "Session");
        const sessionMemberId = toNumberCell(sessionRow, "MemberID");
        const transactionMemberId = sessionMemberId > 0 ? sessionMemberId : null;

        const txnRows = (await queryRunner.query(
            `
            INSERT INTO "Transaction" ("ShopID", "MemberID", "SubtotalAmount", "DepositAppliedAmount", "DiscountAmount", "TotalAmount", "PaymentMethod", "CreatedAt")
            VALUES ($1, $2, $3, 0, 0, $3, $4::"Transaction_PaymentMethod_enum", timezone('Asia/Ho_Chi_Minh', NOW()))
            RETURNING "ID", "MemberID", "SubtotalAmount", "DepositAppliedAmount", "DiscountAmount", "TotalAmount", "PaymentMethod", "CreatedAt"
            `,
            [shopId, transactionMemberId, grandTotal, paymentMethod],
        )) as SqlRow[];

        let txnRow = requireRow(txnRows, "Transaction");
        const linkedMemberId = toNumberCell(txnRow, "MemberID");
        if (linkedMemberId > 0) {
            const nameRows = (await queryRunner.query(
                `SELECT "Name" AS "MemberName" FROM "Member" WHERE "ID" = $1 AND "ShopID" = $2`,
                [linkedMemberId, shopId],
            )) as SqlRow[];
            if (nameRows.length > 0) {
                txnRow = { ...txnRow, MemberName: toStringCell(nameRows[0] as SqlRow, "MemberName") };
            }
        }
        const transactionId = toNumberCell(txnRow, "ID");

        await queryRunner.query(
            `
            INSERT INTO "TransactionLine" ("TransactionID", "ItemType", "ProductID", "GameRentalID", "SessionID", "Quantity", "UnitPrice", "LineTotal")
            VALUES ($1, 'Session'::"TransactionLine_ItemType_enum", NULL, NULL, $2, 1, $3, $4)
            `,
            [transactionId, sessionId, areaCharge, areaCharge],
        );

        for (const line of input.extras ?? []) {
            const qty = Number(line.quantity);
            const unit = Number(line.unitPrice);
            const lineTotal = qty * unit;
            const kind = (line.kind || "").toLowerCase();

            if (kind === "product") {
                await queryRunner.query(
                    `
                    INSERT INTO "TransactionLine" ("TransactionID", "ItemType", "ProductID", "GameRentalID", "SessionID", "Quantity", "UnitPrice", "LineTotal")
                    VALUES ($1, 'Product'::"TransactionLine_ItemType_enum", $2, NULL, $3, $4, $5, $6)
                    `,
                    [transactionId, line.catalogId, sessionId, qty, unit, lineTotal],
                );

                await queryRunner.query(
                    `UPDATE "Product" SET "StockQuantity" = GREATEST(0, "StockQuantity" - $1) WHERE "ID" = $2 AND "ShopID" = $3`,
                    [Math.floor(qty), line.catalogId, shopId],
                );
            } else if (kind === "game") {
                const grRows = (await queryRunner.query(
                    `INSERT INTO "GameRental" ("ShopID", "BoardGameID", "SessionID") VALUES ($1, $2, $3) RETURNING "ID"`,
                    [shopId, line.catalogId, sessionId],
                )) as SqlRow[];

                const gr = requireRow(grRows, "GameRental");
                const gameRentalId = toNumberCell(gr, "ID");

                await queryRunner.query(
                    `
                    INSERT INTO "TransactionLine" ("TransactionID", "ItemType", "ProductID", "GameRentalID", "SessionID", "Quantity", "UnitPrice", "LineTotal")
                    VALUES ($1, 'GameRental'::"TransactionLine_ItemType_enum", NULL, $2, $3, $4, $5, $6)
                    `,
                    [transactionId, gameRentalId, sessionId, qty, unit, lineTotal],
                );
            } else {
                throw new Error(`Unknown checkout line kind: ${line.kind}`);
            }
        }

        await queryRunner.query(
            `UPDATE "Session" SET "EndTime" = timezone('Asia/Ho_Chi_Minh', NOW()), "SessionAmount" = $1 WHERE "ID" = $2 AND "ShopID" = $3 AND "EndTime" IS NULL`,
            [grandTotal, sessionId, shopId],
        );

        await queryRunner.commitTransaction();

        const lineRows = (await AppDataSource.query(
            `
            SELECT
                tl."ID",
                tl."TransactionID",
                tl."ItemType",
                COALESCE(p."Name", bg."Name", a."Name", '') AS "ItemName",
                tl."UnitPrice",
                tl."Quantity",
                tl."LineTotal"
            FROM "TransactionLine" tl
            LEFT JOIN "Product" p ON p."ID" = tl."ProductID"
            LEFT JOIN "GameRental" gr ON gr."ID" = tl."GameRentalID"
            LEFT JOIN "BoardGame" bg ON bg."ID" = gr."BoardGameID"
            LEFT JOIN "Session" s ON s."ID" = tl."SessionID"
            LEFT JOIN "Area" a ON a."ID" = s."AreaID"
            WHERE tl."TransactionID" = $1
            ORDER BY tl."ID" ASC
            `,
            [transactionId],
        )) as SqlRow[];

        return mapTransactionRow(txnRow, lineRows);
    } catch (err) {
        await queryRunner.rollbackTransaction();
        throw err;
    } finally {
        await queryRunner.release();
    }
}

function mapTransactionRow(row: SqlRow, lineRows: SqlRow[]): TransactionDto {
    const transactionId = toNumberCell(row, "ID");
    const memberNumericId = toNumberCell(row, "MemberID");
    const memberId = memberNumericId > 0 ? `#${String(memberNumericId).padStart(4, "0")}` : undefined;
    return {
        id: toStringCell(row, "ID"),
        code: `TXN${String(transactionId).padStart(4, "0")}`,
        customerName: toStringCell(row, "MemberName", "Khach vang lai"),
        paymentMethod: toStringCell(row, "PaymentMethod"),
        subtotalAmount: toNumberCell(row, "SubtotalAmount"),
        depositRefund: toNumberCell(row, "DepositAppliedAmount"),
        discountAmount: toNumberCell(row, "DiscountAmount"),
        totalAmount: toNumberCell(row, "TotalAmount"),
        createdAt: toDateCell(row, "CreatedAt") ?? new Date(0),
        lines: lineRows
            .filter((line) => toNumberCell(line, "TransactionID") === transactionId)
            .map((line) => ({
                id: toStringCell(line, "ID"),
                itemType: toStringCell(line, "ItemType"),
                itemName: toStringCell(line, "ItemName"),
                unitPrice: toNumberCell(line, "UnitPrice"),
                quantity: toNumberCell(line, "Quantity"),
                lineTotal: toNumberCell(line, "LineTotal"),
            })),
        ...(memberId ? { memberId } : {}),
    };
}
