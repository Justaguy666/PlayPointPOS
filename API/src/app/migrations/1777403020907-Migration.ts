import type { MigrationInterface, QueryRunner } from "typeorm";

export class Migration1777403020907 implements MigrationInterface {
    name = 'Migration1777403020907'

    public async up(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(`CREATE TABLE "Shop" ("ID" SERIAL NOT NULL, "Email" character varying(255) NOT NULL, "HashedPassword" character varying(255) NOT NULL, "Name" character varying(255) NOT NULL, "Address" text, "PhoneNumber" character varying(20), CONSTRAINT "UQ_af19c7203491eff8fd7ab5a4c51" UNIQUE ("Email"), CONSTRAINT "PK_3b08993e4e217517cb8f6e028f4" PRIMARY KEY ("ID"))`);
    }

    public async down(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(`DROP TABLE "Shop"`);
    }

}
