import { Field, Float, ID, InputType, Int, ObjectType } from "type-graphql";

@ObjectType()
export class TransactionLineDto {
    @Field(() => ID)
    id!: string;

    @Field(() => String)
    itemType!: string;

    @Field(() => String)
    itemName!: string;

    @Field(() => Float)
    unitPrice!: number;

    @Field(() => Float)
    quantity!: number;

    @Field(() => Float)
    lineTotal!: number;
}

@ObjectType()
export class TransactionDto {
    @Field(() => ID)
    id!: string;

    @Field(() => String)
    code!: string;

    @Field(() => String, { nullable: true })
    memberId?: string;

    @Field(() => String)
    customerName!: string;

    @Field(() => String)
    paymentMethod!: string;

    @Field(() => Float)
    subtotalAmount!: number;

    @Field(() => Float)
    depositRefund!: number;

    @Field(() => Float)
    discountAmount!: number;

    @Field(() => Float)
    totalAmount!: number;

    @Field(() => Date)
    createdAt!: Date;

    @Field(() => [TransactionLineDto])
    lines!: TransactionLineDto[];
}

@InputType()
export class CheckoutExtraLineInput {
    @Field(() => String)
    kind!: string;

    @Field(() => Int)
    catalogId!: number;

    @Field(() => Float)
    quantity!: number;

    @Field(() => Float)
    unitPrice!: number;
}

@InputType()
export class AreaSessionCheckoutInput {
    @Field(() => Int)
    areaId!: number;

    @Field(() => Int)
    sessionId!: number;

    @Field(() => String)
    paymentMethod!: string;

    @Field(() => Float)
    areaServiceCharge!: number;

    @Field(() => [CheckoutExtraLineInput])
    extras!: CheckoutExtraLineInput[];
}
