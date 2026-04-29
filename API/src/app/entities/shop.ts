import { Field, ID, ObjectType } from "type-graphql";
import { BaseEntity, Column, Entity, PrimaryGeneratedColumn } from "typeorm";

@ObjectType()
@Entity("Shop")
export class Shop extends BaseEntity {
    @Field(() => ID)
    @PrimaryGeneratedColumn()
    ID!: number;

    @Field(() => String)
    @Column({ type: "varchar", length: 255, unique: true })
    Email!: string;

    @Column({ type: "varchar", length: 255 })
    HashedPassword!: string;

    @Field(() => String, { nullable: true })
    @Column({ type: "varchar", length: 255, nullable: true })
    Name?: string;

    @Field(() => String, { nullable: true })
    @Column({ type: "text", nullable: true })
    Address?: string;

    @Field(() => String, { nullable: true })
    @Column({ type: "varchar", length: 20, nullable: true })
    PhoneNumber?: string;
}
