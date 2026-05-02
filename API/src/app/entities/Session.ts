import { Field, ID, ObjectType } from "type-graphql";
import { BaseEntity, Column, Entity, PrimaryGeneratedColumn } from "typeorm";

@ObjectType()
@Entity("Session")
export class Session extends BaseEntity {
    @Field(() => ID)
    @PrimaryGeneratedColumn()
    ID!: number;

    @Field(() => ID)
    @Column({ type: "int" })
    ShopID!: number;

    @Field(() => String)
    @Column({ type: "varchar", length: 255 })
    Token!: string;

    @Field(() => Date)
    @Column({ type: "timestamp" })
    ExpiresAt!: Date;
}
