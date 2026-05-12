import { Field, Float, ID, InputType, Int, ObjectType } from "type-graphql";

@ObjectType()
export class ProductDto {
    @Field(() => ID)
    id!: string;

    @Field(() => String)
    name!: string;

    @Field(() => Float)
    price!: number;

    @Field(() => String)
    productType!: string;

    @Field(() => Int)
    stockQuantity!: number;

    @Field(() => String)
    imageUri!: string;
}

@InputType()
export class ProductInput {
    @Field(() => String)
    name!: string;

    @Field(() => Float)
    price!: number;

    @Field(() => String)
    productType!: string;

    @Field(() => Int, { defaultValue: 0 })
    stockQuantity!: number;

    @Field(() => String)
    imageUri!: string;
}
