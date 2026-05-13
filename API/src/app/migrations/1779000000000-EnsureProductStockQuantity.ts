import type { MigrationInterface, QueryRunner } from "typeorm";

export class EnsureProductStockQuantity1779000000000 implements MigrationInterface {
    name = "EnsureProductStockQuantity1779000000000";

    public async up(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(`
            ALTER TABLE "Product"
            ADD COLUMN IF NOT EXISTS "StockQuantity" integer NOT NULL DEFAULT 0
        `);
    }

    public async down(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(`
            ALTER TABLE "Product"
            DROP COLUMN IF EXISTS "StockQuantity"
        `);
    }
}
