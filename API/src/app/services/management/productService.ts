import { AppDataSource } from "../../../config/database.js";
import { ProductDto, ProductInput } from "../../types/management/product.js";
import { requireRow, toNumber, toStringValue, type SqlRow } from "./shared.js";

export async function getProducts(shopId: number): Promise<ProductDto[]> {
    const rows = await AppDataSource.query(`
        SELECT "ID", "Name", "Type", "ImageUrl", "Price"
        FROM "Product"
        WHERE "ShopID" = $1
        ORDER BY "ID" ASC
    `, [shopId]) as SqlRow[];

    return rows.map(mapProductRow);
}

export async function createProduct(shopId: number, input: ProductInput): Promise<ProductDto> {
    const rows = await AppDataSource.query(`
        INSERT INTO "Product" ("ShopID", "Name", "Type", "ImageUrl", "Price")
        VALUES ($1, $2, $3::"Product_Type_enum", $4, $5)
        RETURNING "ID", "Name", "Type", "ImageUrl", "Price"
    `, [shopId, input.name, input.productType, input.imageUri, input.price]) as SqlRow[];

    return mapProductRow(requireRow(rows, "Product"), input.stockQuantity);
}

export async function updateProduct(id: number, input: ProductInput): Promise<ProductDto> {
    const rows = await AppDataSource.query(`
        UPDATE "Product"
        SET "Name" = $2,
            "Type" = $3::"Product_Type_enum",
            "ImageUrl" = $4,
            "Price" = $5
        WHERE "ID" = $1
        RETURNING "ID", "Name", "Type", "ImageUrl", "Price"
    `, [id, input.name, input.productType, input.imageUri, input.price]) as SqlRow[];

    return mapProductRow(requireRow(rows, "Product", id), input.stockQuantity);
}

export async function deleteProduct(id: number): Promise<void> {
    const rows = await AppDataSource.query(`
        DELETE FROM "Product"
        WHERE "ID" = $1
        RETURNING "ID"
    `, [id]) as SqlRow[];

    requireRow(rows, "Product", id);
}

function mapProductRow(row: SqlRow, stockQuantity = 0): ProductDto {
    return {
        id: toStringValue(row.ID),
        name: toStringValue(row.Name),
        price: toNumber(row.Price),
        productType: toStringValue(row.Type),
        stockQuantity,
        imageUri: toStringValue(row.ImageUrl),
    };
}
