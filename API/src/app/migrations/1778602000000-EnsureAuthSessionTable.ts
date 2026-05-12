import type { MigrationInterface, QueryRunner } from "typeorm";

export class EnsureAuthSessionTable1778602000000 implements MigrationInterface {
    name = "EnsureAuthSessionTable1778602000000";

    public async up(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(`
            CREATE TABLE IF NOT EXISTS "AuthSession" (
                "ID" SERIAL NOT NULL,
                "ShopID" integer NOT NULL,
                "Token" character varying(255) NOT NULL,
                "ExpiresAt" TIMESTAMP NOT NULL,
                CONSTRAINT "PK_AuthSession_ID" PRIMARY KEY ("ID")
            )
        `);
    }

    public async down(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(`DROP TABLE IF EXISTS "AuthSession"`);
    }
}
