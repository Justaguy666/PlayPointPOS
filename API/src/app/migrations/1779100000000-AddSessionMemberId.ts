import type { MigrationInterface, QueryRunner } from "typeorm";

export class AddSessionMemberId1779100000000 implements MigrationInterface {
    name = "AddSessionMemberId1779100000000";

    public async up(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(`
            ALTER TABLE "Session"
            ADD COLUMN IF NOT EXISTS "MemberID" integer NULL
        `);
        await queryRunner.query(`
            DO $$ BEGIN
                ALTER TABLE "Session"
                ADD CONSTRAINT "FK_Session_MemberID" FOREIGN KEY ("MemberID") REFERENCES "Member"("ID") ON DELETE SET NULL;
            EXCEPTION
                WHEN duplicate_object THEN NULL;
            END $$;
        `);
    }

    public async down(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(`ALTER TABLE "Session" DROP CONSTRAINT IF EXISTS "FK_Session_MemberID"`);
        await queryRunner.query(`ALTER TABLE "Session" DROP COLUMN IF EXISTS "MemberID"`);
    }
}
