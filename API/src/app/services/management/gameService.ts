import { AppDataSource } from "../../../config/database.js";
import { GameDto, GameInput, GameTypeDto, GameTypeInput } from "../../types/management/game.js";
import { requireRow, toNumber, toStringValue, type SqlRow } from "./shared.js";

export async function getGameTypes(shopId: number): Promise<GameTypeDto[]> {
    const rows = await AppDataSource.query(`
        SELECT "ID", "Name"
        FROM "GameCategory"
        WHERE "ShopID" = $1
        ORDER BY "Name" ASC
    `, [shopId]) as SqlRow[];

    return rows.map((row) => ({ id: toStringValue(row.ID), name: toStringValue(row.Name) }));
}

export async function createGameType(shopId: number, input: GameTypeInput): Promise<GameTypeDto> {
    const rows = await AppDataSource.query(`
        INSERT INTO "GameCategory" ("ShopID", "Name")
        VALUES ($1, $2)
        RETURNING "ID", "Name"
    `, [shopId, input.name]) as SqlRow[];

    const row = requireRow(rows, "Game type");
    return { id: toStringValue(row.ID), name: toStringValue(row.Name) };
}

export async function updateGameType(id: number, input: GameTypeInput): Promise<GameTypeDto> {
    const rows = await AppDataSource.query(`
        UPDATE "GameCategory"
        SET "Name" = $2
        WHERE "ID" = $1
        RETURNING "ID", "Name"
    `, [id, input.name]) as SqlRow[];

    const row = requireRow(rows, "Game type", id);
    return { id: toStringValue(row.ID), name: toStringValue(row.Name) };
}

export async function deleteGameType(id: number): Promise<void> {
    const rows = await AppDataSource.query(`
        DELETE FROM "GameCategory"
        WHERE "ID" = $1
        RETURNING "ID"
    `, [id]) as SqlRow[];

    requireRow(rows, "Game type", id);
}

export async function getGames(shopId: number): Promise<GameDto[]> {
    const rows = await AppDataSource.query(`
        SELECT
            g."ID",
            g."Name",
            g."HourlyPrice",
            g."MinPlayer",
            g."MaxPlayer",
            g."Difficulty",
            g."StockQuantity",
            g."ImageUrl",
            c."ID" AS "CategoryID",
            c."Name" AS "CategoryName"
        FROM "BoardGame" g
        JOIN "GameCategory" c ON c."ID" = g."CategoryID"
        WHERE g."ShopID" = $1
        ORDER BY g."ID" ASC
    `, [shopId]) as SqlRow[];

    return rows.map(mapGameRow);
}

export async function createGame(shopId: number, input: GameInput): Promise<GameDto> {
    const rows = await AppDataSource.query(`
        INSERT INTO "BoardGame" ("ShopID", "CategoryID", "Name", "Difficulty", "ImageUrl", "HourlyPrice", "MinPlayer", "MaxPlayer", "StockQuantity")
        VALUES ($1, $2, $3, $4::"BoardGame_Difficulty_enum", $5, $6, $7, $8, $9)
        RETURNING "ID", "CategoryID", "Name", "Difficulty", "ImageUrl", "HourlyPrice", "MinPlayer", "MaxPlayer", "StockQuantity"
    `, [shopId, Number(input.gameTypeId), input.name, input.difficulty, input.imageUri, input.hourlyPrice, input.minPlayers, input.maxPlayers, input.stockQuantity]) as SqlRow[];

    const gameRow = requireRow(rows, "Game");
    const categoryRows = await AppDataSource.query(`SELECT "Name" FROM "GameCategory" WHERE "ID" = $1`, [Number(input.gameTypeId)]) as SqlRow[];
    return mapGameRow({ ...gameRow, CategoryName: requireRow(categoryRows, "Game type", Number(input.gameTypeId)).Name });
}

export async function updateGame(id: number, input: GameInput): Promise<GameDto> {
    const rows = await AppDataSource.query(`
        UPDATE "BoardGame"
        SET "CategoryID" = $2,
            "Name" = $3,
            "Difficulty" = $4::"BoardGame_Difficulty_enum",
            "ImageUrl" = $5,
            "HourlyPrice" = $6,
            "MinPlayer" = $7,
            "MaxPlayer" = $8,
            "StockQuantity" = $9
        WHERE "ID" = $1
        RETURNING "ID", "CategoryID", "Name", "Difficulty", "ImageUrl", "HourlyPrice", "MinPlayer", "MaxPlayer", "StockQuantity"
    `, [id, Number(input.gameTypeId), input.name, input.difficulty, input.imageUri, input.hourlyPrice, input.minPlayers, input.maxPlayers, input.stockQuantity]) as SqlRow[];

    const gameRow = requireRow(rows, "Game", id);
    const categoryRows = await AppDataSource.query(`SELECT "Name" FROM "GameCategory" WHERE "ID" = $1`, [Number(input.gameTypeId)]) as SqlRow[];
    return mapGameRow({ ...gameRow, CategoryName: requireRow(categoryRows, "Game type", Number(input.gameTypeId)).Name });
}

export async function deleteGame(id: number): Promise<void> {
    const rows = await AppDataSource.query(`
        DELETE FROM "BoardGame"
        WHERE "ID" = $1
        RETURNING "ID"
    `, [id]) as SqlRow[];

    requireRow(rows, "Game", id);
}

function mapGameRow(row: SqlRow): GameDto {
    return {
        id: toStringValue(row.ID),
        name: toStringValue(row.Name),
        hourlyPrice: toNumber(row.HourlyPrice),
        minPlayers: toNumber(row.MinPlayer),
        maxPlayers: toNumber(row.MaxPlayer),
        gameTypeId: toStringValue(row.CategoryID),
        gameTypeName: toStringValue(row.CategoryName),
        difficulty: toStringValue(row.Difficulty),
        stockQuantity: toNumber(row.StockQuantity),
        imageUri: toStringValue(row.ImageUrl),
    };
}
