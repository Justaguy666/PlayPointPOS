import type { MigrationInterface, QueryRunner } from "typeorm";

export class Migration1777496653703 implements MigrationInterface {
    name = 'Migration1777496653703'

    public async up(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(`CREATE TABLE "Session" ("ID" SERIAL NOT NULL, "ShopID" integer NOT NULL, "Token" character varying(255) NOT NULL, "ExpiresAt" TIMESTAMP NOT NULL, CONSTRAINT "PK_042b591044f9118d5721c3e963f" PRIMARY KEY ("ID"))`);
    }

    public async down(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(`DROP TABLE "Session"`);
    }

}
