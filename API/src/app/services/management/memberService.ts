import { AppDataSource } from "../../../config/database.js";
import { MemberDto, MemberInput, MembershipDto, MembershipInput } from "../../types/management/member.js";
import { requireRow, toStringValue, toNumberCell, toStringCell, cell, type SqlRow } from "./shared.js";

export async function getMemberships(shopId: number): Promise<MembershipDto[]> {
    const rows = await AppDataSource.query(`
        SELECT "ID", "Name", "Color", "MinSpent", "DiscountPercent"
        FROM "Membership"
        WHERE "ShopID" = $1
        ORDER BY "MinSpent" ASC, "ID" ASC
    `, [shopId]) as SqlRow[];

    return rows.map((row, index) => mapMembershipRow(row, index + 1, index === 0));
}

export async function createMembership(shopId: number, input: MembershipInput): Promise<MembershipDto> {
    const rows = await AppDataSource.query(`
        INSERT INTO "Membership" ("ShopID", "Name", "Color", "MinSpent", "DiscountPercent")
        VALUES ($1, $2, $3, $4, $5)
        RETURNING "ID", "Name", "Color", "MinSpent", "DiscountPercent"
    `, [shopId, input.name, input.color, input.minSpentAmount, input.discountRate]) as SqlRow[];

    const row = requireRow(rows, "Membership");
    const priority = await getMembershipPriority(shopId, toNumberCell(row, "ID"));
    return mapMembershipRow(row, priority, priority === 1);
}

export async function updateMembership(id: number, input: MembershipInput): Promise<MembershipDto> {
    const rows = await AppDataSource.query(`
        UPDATE "Membership"
        SET "Name" = $2,
            "Color" = $3,
            "MinSpent" = $4,
            "DiscountPercent" = $5
        WHERE "ID" = $1
        RETURNING "ID", "ShopID", "Name", "Color", "MinSpent", "DiscountPercent"
    `, [id, input.name, input.color, input.minSpentAmount, input.discountRate]) as SqlRow[];

    const row = requireRow(rows, "Membership", id);
    const priority = await getMembershipPriority(toNumberCell(row, "ShopID"), toNumberCell(row, "ID"));
    return mapMembershipRow(row, priority, priority === 1);
}

export async function deleteMembership(id: number): Promise<void> {
    const rows = await AppDataSource.query(`
        DELETE FROM "Membership"
        WHERE "ID" = $1
        RETURNING "ID"
    `, [id]) as SqlRow[];

    requireRow(rows, "Membership", id);
}

export async function getMembers(shopId: number): Promise<MemberDto[]> {
    const rows = await AppDataSource.query(`
        SELECT m."ID", m."Name", m."PhoneNumber", m."TotalSpent", ms."ID" AS "MembershipID", ms."Name" AS "MembershipName"
        FROM "Member" m
        LEFT JOIN "Membership" ms ON ms."ID" = m."MembershipID"
        WHERE m."ShopID" = $1
        ORDER BY m."ID" ASC
    `, [shopId]) as SqlRow[];

    return rows.map(mapMemberRow);
}

export async function createMember(shopId: number, input: MemberInput): Promise<MemberDto> {
    const membership = await resolveMembershipForSpentAsync(shopId, input.totalSpentAmount);
    const rows = await AppDataSource.query(`
        INSERT INTO "Member" ("ShopID", "MembershipID", "Name", "PhoneNumber", "TotalSpent")
        VALUES ($1, $2, $3, $4, $5)
        RETURNING "ID", "Name", "PhoneNumber", "TotalSpent"
    `, [shopId, membership.id, input.fullName, input.phoneNumber, input.totalSpentAmount]) as SqlRow[];

    return mapMemberRow({ ...requireRow(rows, "Member"), MembershipID: membership.id, MembershipName: membership.name });
}

export async function updateMember(id: number, input: MemberInput): Promise<MemberDto> {
    const shopRows = await AppDataSource.query(`SELECT "ShopID" FROM "Member" WHERE "ID" = $1`, [id]) as SqlRow[];
    const shopRow = requireRow(shopRows, "Member", id);
    const shopId = toNumberCell(shopRow, "ShopID");
    const membership = await resolveMembershipForSpentAsync(shopId, input.totalSpentAmount);
    const rows = await AppDataSource.query(`
        UPDATE "Member"
        SET "MembershipID" = $2,
            "Name" = $3,
            "PhoneNumber" = $4,
            "TotalSpent" = $5
        WHERE "ID" = $1
        RETURNING "ID", "Name", "PhoneNumber", "TotalSpent"
    `, [id, membership.id, input.fullName, input.phoneNumber, input.totalSpentAmount]) as SqlRow[];

    return mapMemberRow({ ...requireRow(rows, "Member", id), MembershipID: membership.id, MembershipName: membership.name });
}

export async function deleteMember(id: number): Promise<void> {
    const rows = await AppDataSource.query(`
        DELETE FROM "Member"
        WHERE "ID" = $1
        RETURNING "ID"
    `, [id]) as SqlRow[];

    requireRow(rows, "Member", id);
}

async function getMembershipPriority(shopId: number, membershipId: number): Promise<number> {
    const rows = await AppDataSource.query(`
        SELECT "ID"
        FROM "Membership"
        WHERE "ShopID" = $1
        ORDER BY "MinSpent" ASC, "ID" ASC
    `, [shopId]) as SqlRow[];

    return rows.findIndex((item: SqlRow) => toNumberCell(item, "ID") === membershipId) + 1;
}

async function resolveMembershipForSpentAsync(shopId: number, totalSpentAmount: number): Promise<{ id: number; name: string }> {
    let rows = await AppDataSource.query(`
        SELECT "ID", "Name", "MinSpent"
        FROM "Membership"
        WHERE "ShopID" = $1
        ORDER BY "MinSpent" ASC, "ID" ASC
    `, [shopId]) as SqlRow[];

    if (rows.length === 0) {
        await AppDataSource.query(`
            INSERT INTO "Membership" ("ShopID", "Name", "Color", "MinSpent", "DiscountPercent")
            VALUES ($1, 'Bronze', '#A16207', 0, 0)
        `, [shopId]);

        rows = await AppDataSource.query(`
            SELECT "ID", "Name", "MinSpent"
            FROM "Membership"
            WHERE "ShopID" = $1
            ORDER BY "MinSpent" ASC, "ID" ASC
        `, [shopId]) as SqlRow[];
    }

    let selected = requireRow(rows, "Membership");
    for (const row of rows) {
        if (toNumberCell(row, "MinSpent") <= totalSpentAmount) {
            selected = row;
        }
    }

    return { id: toNumberCell(selected, "ID"), name: toStringCell(selected, "Name") };
}

function mapMembershipRow(row: SqlRow, priority: number, isDefault: boolean): MembershipDto {
    return {
        id: toStringCell(row, "ID"),
        name: toStringCell(row, "Name"),
        color: toStringCell(row, "Color"),
        minSpentAmount: toNumberCell(row, "MinSpent"),
        discountRate: toNumberCell(row, "DiscountPercent"),
        priority,
        isDefault,
    };
}

function mapMemberRow(row: SqlRow): MemberDto {
    const memberId = toStringCell(row, "ID");
    const membershipIdRaw = cell(row, "MembershipID");
    return {
        id: memberId,
        code: `#${memberId.padStart(4, "0")}`,
        fullName: toStringCell(row, "Name"),
        phoneNumber: toStringCell(row, "PhoneNumber"),
        totalSpentAmount: toNumberCell(row, "TotalSpent"),
        membershipId: membershipIdRaw != null && String(membershipIdRaw) !== "" ? toStringValue(membershipIdRaw) : "",
        membershipName: toStringCell(row, "MembershipName"),
    };
}
