import { AppDataSource } from "../../../config/database.js";
import { ProductDto, ProductInput } from "../../types/management/product.js";
import { requireRow, toNumberCell, toStringCell, type SqlRow } from "./shared.js";

export async function getProducts(shopId: number): Promise<ProductDto[]> {
    await ensureProductStockQuantityColumnAsync();

    const rows = await AppDataSource.query(`
        SELECT "ID", "Name", "Type", "ImageUrl", "Price", "StockQuantity"
        FROM "Product"
        WHERE "ShopID" = $1
        ORDER BY "ID" ASC
    `, [shopId]) as SqlRow[];

    return rows.map(mapProductRow);
}

export async function createProduct(shopId: number, input: ProductInput): Promise<ProductDto> {
    await ensureProductStockQuantityColumnAsync();

    const rows = await AppDataSource.query(`
        INSERT INTO "Product" ("ShopID", "Name", "Type", "ImageUrl", "Price", "StockQuantity")
        VALUES ($1, $2, $3::"Product_Type_enum", $4, $5, $6)
        RETURNING "ID", "Name", "Type", "ImageUrl", "Price", "StockQuantity"
    `, [shopId, input.name, input.productType, input.imageUri, input.price, input.stockQuantity]) as SqlRow[];

    return mapProductRow(requireRow(rows, "Product"));
}

export async function updateProduct(shopId: number, id: number, input: ProductInput): Promise<ProductDto> {
    await ensureProductStockQuantityColumnAsync();

    const rows = await AppDataSource.query(`
        UPDATE "Product"
        SET "Name" = $3,
            "Type" = $4::"Product_Type_enum",
            "ImageUrl" = $5,
            "Price" = $6,
            "StockQuantity" = $7
        WHERE "ID" = $2 AND "ShopID" = $1
        RETURNING "ID", "Name", "Type", "ImageUrl", "Price", "StockQuantity"
    `, [shopId, id, input.name, input.productType, input.imageUri, input.price, input.stockQuantity]) as SqlRow[];

    return mapProductRow(requireRow(rows, "Product", id));
}

async function ensureProductStockQuantityColumnAsync(): Promise<void> {
    await AppDataSource.query(`
        ALTER TABLE "Product"
        ADD COLUMN IF NOT EXISTS "StockQuantity" integer NOT NULL DEFAULT 0
    `);
}

export async function deleteProduct(shopId: number, id: number): Promise<void> {
    const rows = await AppDataSource.query(`
        DELETE FROM "Product"
        WHERE "ID" = $2 AND "ShopID" = $1
        RETURNING "ID"
    `, [shopId, id]) as SqlRow[];

    requireRow(rows, "Product", id);
}

function mapProductRow(row: SqlRow): ProductDto {
    return {
        id: toStringCell(row, "ID"),
        name: toStringCell(row, "Name"),
        price: toNumberCell(row, "Price"),
        productType: toStringCell(row, "Type"),
        stockQuantity: toNumberCell(row, "StockQuantity"),
        imageUri: toStringCell(row, "ImageUrl"),
    };
}
