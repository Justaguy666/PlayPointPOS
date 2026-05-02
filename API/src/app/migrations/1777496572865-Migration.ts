import type { MigrationInterface, QueryRunner } from "typeorm";

export class Migration1777496572865 implements MigrationInterface {
    name = 'Migration1777496572865'

    public async up(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(`CREATE TABLE "Session" ("ID" SERIAL NOT NULL, "ShopID" integer NOT NULL, "Token" character varying(255) NOT NULL, "ExpiresAt" TIMESTAMP NOT NULL, "shopID" integer, CONSTRAINT "PK_042b591044f9118d5721c3e963f" PRIMARY KEY ("ID"))`);
        await queryRunner.query(`ALTER TABLE "Session" ADD CONSTRAINT "FK_8d3108a8ba774a07177873136af" FOREIGN KEY ("shopID") REFERENCES "Shop"("ID") ON DELETE CASCADE ON UPDATE NO ACTION`);
    }

    public async down(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(`ALTER TABLE "Session" DROP CONSTRAINT "FK_8d3108a8ba774a07177873136af"`);
        await queryRunner.query(`DROP TABLE "Session"`);
    }

}
