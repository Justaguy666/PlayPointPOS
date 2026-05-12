import { AppDataSource } from "../../../config/database.js";
import { TransactionDto } from "../../types/management/transaction.js";
import { requireRow, toDateValue, toNumber, toStringValue, type SqlRow } from "./shared.js";

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
    const memberRows = row.MemberID
        ? await AppDataSource.query(`SELECT "Name" FROM "Member" WHERE "ID" = $1`, [toNumber(row.MemberID)]) as SqlRow[]
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

    return mapTransactionRow({ ...row, MemberName: memberRows[0]?.Name }, lineRows);
}

function mapTransactionRow(row: SqlRow, lineRows: SqlRow[]): TransactionDto {
    const transactionId = toNumber(row.ID);
    const memberId = row.MemberID ? `#${String(toNumber(row.MemberID)).padStart(4, "0")}` : undefined;
    return {
        id: toStringValue(row.ID),
        code: `TXN${String(transactionId).padStart(4, "0")}`,
        customerName: toStringValue(row.MemberName, "Khach vang lai"),
        paymentMethod: toStringValue(row.PaymentMethod),
        subtotalAmount: toNumber(row.SubtotalAmount),
        depositRefund: toNumber(row.DepositAppliedAmount),
        discountAmount: toNumber(row.DiscountAmount),
        totalAmount: toNumber(row.TotalAmount),
        createdAt: toDateValue(row.CreatedAt) ?? new Date(0),
        lines: lineRows
            .filter((line) => toNumber(line.TransactionID) === transactionId)
            .map((line) => ({
                id: toStringValue(line.ID),
                itemType: toStringValue(line.ItemType),
                itemName: toStringValue(line.ItemName),
                unitPrice: toNumber(line.UnitPrice),
                quantity: toNumber(line.Quantity),
                lineTotal: toNumber(line.LineTotal),
            })),
        ...(memberId ? { memberId } : {}),
    };
}
