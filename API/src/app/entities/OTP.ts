import { Field, ID, ObjectType } from "type-graphql";
import { BaseEntity, Column, Entity, PrimaryGeneratedColumn } from "typeorm";

@ObjectType()
@Entity("OTP")
export class OTP extends BaseEntity {
    @Field(() => ID)
    @PrimaryGeneratedColumn()
    ID!: number;

    @Field(() => String)
    @Column({ type: "varchar", length: 255 })
    Email!: string;

    @Field(() => String)
    @Column({ type: "varchar", length: 6 })
    Otp!: string;

    @Field(() => Date)
    @Column({ type: "timestamp" })
    ExpiresAt!: Date;
}