import type { MigrationInterface, QueryRunner } from "typeorm";

export class Migration1777494897427 implements MigrationInterface {
    name = 'Migration1777494897427'

    public async up(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(`ALTER TABLE "Shop" DROP COLUMN "IsActive"`);
    }

    public async down(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(`ALTER TABLE "Shop" ADD "IsActive" boolean NOT NULL DEFAULT false`);
    }

}
