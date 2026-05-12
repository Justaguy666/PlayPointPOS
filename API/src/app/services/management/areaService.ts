import { AppDataSource } from "../../../config/database.js";
import { AreaDto, AreaInput } from "../../types/management/area.js";
import { requireRow, toDateValue, toNumber, toOptionalString, toStringValue, type SqlRow } from "./shared.js";

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
            m."ID" AS "MemberID",
            m."PhoneNumber" AS "MemberPhoneNumber",
            m."Name" AS "MemberName",
            s."ID" AS "SessionID",
            s."GuestCount" AS "SessionGuestCount",
            s."StartTime" AS "SessionStartTime",
            s."SessionAmount"
        FROM "Area" a
        LEFT JOIN "Reservation" r ON r."AreaID" = a."ID" AND r."ShopID" = a."ShopID"
        LEFT JOIN "Member" m ON m."ID" = r."MemberID"
        LEFT JOIN "Session" s ON s."AreaID" = a."ID" AND s."ShopID" = a."ShopID" AND s."EndTime" IS NULL
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

function mapAreaRow(row: SqlRow): AreaDto {
    const memberId = toOptionalString(row.MemberID);
    const checkInDateTime = toDateValue(row.ReservationTime);
    const startTime = toDateValue(row.SessionStartTime);

    return {
        id: toStringValue(row.ID),
        areaName: toStringValue(row.Name),
        playAreaType: toStringValue(row.Type),
        status: row.SessionID ? "Rented" : row.ReservationID ? "Reserved" : "Available",
        maxCapacity: toNumber(row.MaxCapacity),
        hourlyPrice: toNumber(row.HourlyPrice),
        customerName: toStringValue(row.MemberName),
        phoneNumber: toStringValue(row.MemberPhoneNumber),
        capacity: toNumber(row.SessionGuestCount ?? row.GuestCount),
        isSessionPaused: false,
        sessionPausedDurationSeconds: 0,
        totalAmount: toNumber(row.SessionAmount),
        ...(memberId ? { memberId } : {}),
        ...(checkInDateTime ? { checkInDateTime } : {}),
        ...(startTime ? { startTime } : {}),
    };
}
