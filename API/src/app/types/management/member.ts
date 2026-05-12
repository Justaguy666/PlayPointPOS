import { Field, Float, ID, InputType, Int, ObjectType } from "type-graphql";

@ObjectType()
export class MembershipDto {
    @Field(() => ID)
    id!: string;

    @Field(() => String)
    name!: string;

    @Field(() => String)
    color!: string;

    @Field(() => Float)
    minSpentAmount!: number;

    @Field(() => Float)
    discountRate!: number;

    @Field(() => Int)
    priority!: number;

    @Field(() => Boolean)
    isDefault!: boolean;
}

@InputType()
export class MembershipInput {
    @Field(() => String)
    name!: string;

    @Field(() => String)
    color!: string;

    @Field(() => Float)
    minSpentAmount!: number;

    @Field(() => Float)
    discountRate!: number;
}

@ObjectType()
export class MemberDto {
    @Field(() => ID)
    id!: string;

    @Field(() => String)
    code!: string;

    @Field(() => String)
    fullName!: string;

    @Field(() => String)
    phoneNumber!: string;

    @Field(() => Float)
    totalSpentAmount!: number;

    @Field(() => String)
    membershipId!: string;

    @Field(() => String)
    membershipName!: string;
}

@InputType()
export class MemberInput {
    @Field(() => String)
    fullName!: string;

    @Field(() => String)
    phoneNumber!: string;

    @Field(() => Float)
    totalSpentAmount!: number;
}
