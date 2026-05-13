import { AppDataSource } from "../../../config/database.js";
import { AreaDto, AreaInput } from "../../types/management/area.js";
import { requireRow, toDateCell, toNumberCell, toOptionalString, toStringCell, cell, type SqlRow } from "./shared.js";

export async function getAreas(shopId: number): Promise<AreaDto[]> {
    const rows = await AppDataSource.query(`
        SELECT
            a."ID",
            a."Name",
            a."Type",
            a."HourlyPrice",
            a."MaxCapacity",
            r."ID" AS "ReservationID",
            r."ReservationTime",
            r."GuestCount",
            rm."ID" AS "ReservationMemberID",
            rm."PhoneNumber" AS "ReservationMemberPhoneNumber",
            rm."Name" AS "ReservationMemberName",
            s."ID" AS "SessionID",
            s."GuestCount" AS "SessionGuestCount",
            s."StartTime" AS "SessionStartTime",
            s."SessionAmount",
            sm."ID" AS "SessionMemberID",
            sm."PhoneNumber" AS "SessionMemberPhoneNumber",
            sm."Name" AS "SessionMemberName"
        FROM "Area" a
        LEFT JOIN "Reservation" r ON r."AreaID" = a."ID" AND r."ShopID" = a."ShopID"
        LEFT JOIN "Member" rm ON rm."ID" = r."MemberID"
        LEFT JOIN "Session" s ON s."AreaID" = a."ID" AND s."ShopID" = a."ShopID" AND s."EndTime" IS NULL
        LEFT JOIN "Member" sm ON sm."ID" = s."MemberID"
        WHERE a."ShopID" = $1
        ORDER BY a."ID" ASC
    `, [shopId]) as SqlRow[];

    return rows.map(mapAreaRow);
}

export async function createArea(shopId: number, input: AreaInput): Promise<AreaDto> {
    const rows = await AppDataSource.query(`
        INSERT INTO "Area" ("ShopID", "Name", "Type", "HourlyPrice", "MaxCapacity")
        VALUES ($1, $2, $3::"Area_Type_enum", $4, $5)
        RETURNING "ID", "Name", "Type", "HourlyPrice", "MaxCapacity"
    `, [shopId, input.areaName, input.playAreaType, input.hourlyPrice, input.maxCapacity]) as SqlRow[];

    return mapAreaRow(requireRow(rows, "Area"));
}

export async function updateArea(id: number, input: AreaInput): Promise<AreaDto> {
    const rows = await AppDataSource.query(`
        UPDATE "Area"
        SET "Name" = $2,
            "Type" = $3::"Area_Type_enum",
            "HourlyPrice" = $4,
            "MaxCapacity" = $5
        WHERE "ID" = $1
        RETURNING "ID", "Name", "Type", "HourlyPrice", "MaxCapacity"
    `, [id, input.areaName, input.playAreaType, input.hourlyPrice, input.maxCapacity]) as SqlRow[];

    return mapAreaRow(requireRow(rows, "Area", id));
}

export async function deleteArea(id: number): Promise<void> {
    const rows = await AppDataSource.query(`
        DELETE FROM "Area"
        WHERE "ID" = $1
        RETURNING "ID"
    `, [id]) as SqlRow[];

    requireRow(rows, "Area", id);
}

export async function startAreaSession(
    shopId: number,
    areaId: number,
    guestCount: number,
    memberId?: number | null,
): Promise<{ sessionId: number; startTime: Date }> {
    const queryRunner = AppDataSource.createQueryRunner();
    await queryRunner.connect();
    await queryRunner.startTransaction();
    try {
        const areaRows = (await queryRunner.query(
            `SELECT "ID" FROM "Area" WHERE "ID" = $1 AND "ShopID" = $2`,
            [areaId, shopId],
        )) as SqlRow[];
        if (areaRows.length === 0) {
            throw new Error("Area not found.");
        }

        const openRows = (await queryRunner.query(
            `SELECT "ID" FROM "Session" WHERE "AreaID" = $1 AND "ShopID" = $2 AND "EndTime" IS NULL`,
            [areaId, shopId],
        )) as SqlRow[];
        if (openRows.length > 0) {
            throw new Error("Area already has an active session.");
        }

        let sessionMemberId: number | null = null;
        if (memberId != null && Number.isFinite(memberId) && memberId > 0) {
            const memberRows = (await queryRunner.query(
                `SELECT "ID" FROM "Member" WHERE "ID" = $1 AND "ShopID" = $2`,
                [memberId, shopId],
            )) as SqlRow[];
            if (memberRows.length === 0) {
                throw new Error("Member not found for this shop.");
            }

            sessionMemberId = Math.trunc(memberId);
        }

        const insertRows = (await queryRunner.query(
            `
            INSERT INTO "Session" ("ShopID", "AreaID", "GuestCount", "StartTime", "EndTime", "SessionAmount", "MemberID")
            VALUES ($1, $2, $3, timezone('Asia/Ho_Chi_Minh', NOW()), NULL, 0, $4)
            RETURNING "ID", "StartTime"
            `,
            [shopId, areaId, guestCount, sessionMemberId],
        )) as SqlRow[];

        const row = requireRow(insertRows, "Session");
        await queryRunner.commitTransaction();

        const startTime = toDateCell(row, "StartTime");
        if (!startTime) {
            throw new Error("Session start time was not returned.");
        }

        return {
            sessionId: toNumberCell(row, "ID"),
            startTime,
        };
    } catch (err) {
        await queryRunner.rollbackTransaction();
        throw err;
    } finally {
        await queryRunner.release();
    }
}

function mapAreaRow(row: SqlRow): AreaDto {
    const isRented = !!cell(row, "SessionID");
    const sessionMemberId = toOptionalString(cell(row, "SessionMemberID"));
    const reservationMemberId = toOptionalString(cell(row, "ReservationMemberID"));
    const memberId = isRented
        ? (sessionMemberId ?? reservationMemberId)
        : reservationMemberId;

    const customerName = isRented
        ? (toStringCell(row, "SessionMemberName").trim() || toStringCell(row, "ReservationMemberName"))
        : toStringCell(row, "ReservationMemberName");

    const phoneNumber = isRented
        ? (toStringCell(row, "SessionMemberPhoneNumber").trim() || toStringCell(row, "ReservationMemberPhoneNumber"))
        : toStringCell(row, "ReservationMemberPhoneNumber");

    const checkInDateTime = toDateCell(row, "ReservationTime");
    const startTime = toDateCell(row, "SessionStartTime");

    return {
        id: toStringCell(row, "ID"),
        areaName: toStringCell(row, "Name"),
        playAreaType: toStringCell(row, "Type"),
        status: isRented ? "Rented" : cell(row, "ReservationID") ? "Reserved" : "Available",
        maxCapacity: toNumberCell(row, "MaxCapacity"),
        hourlyPrice: toNumberCell(row, "HourlyPrice"),
        customerName,
        phoneNumber,
        capacity: toNumberCell(row, "SessionGuestCount") || toNumberCell(row, "GuestCount"),
        isSessionPaused: false,
        sessionPausedDurationSeconds: 0,
        totalAmount: toNumberCell(row, "SessionAmount"),
        ...(memberId ? { memberId } : {}),
        ...(checkInDateTime ? { checkInDateTime } : {}),
        ...(startTime ? { startTime } : {}),
        ...(cell(row, "SessionID") ? { activeSessionId: toNumberCell(row, "SessionID") } : {}),
    };
}
