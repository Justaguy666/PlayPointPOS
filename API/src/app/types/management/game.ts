import { Field, Float, ID, InputType, Int, ObjectType } from "type-graphql";

@ObjectType()
export class GameTypeDto {
    @Field(() => ID)
    id!: string;

    @Field(() => String)
    name!: string;
}

@InputType()
export class GameTypeInput {
    @Field(() => String)
    name!: string;
}

@ObjectType()
export class GameDto {
    @Field(() => ID)
    id!: string;

    @Field(() => String)
    name!: string;

    @Field(() => Float)
    hourlyPrice!: number;

    @Field(() => Int)
    minPlayers!: number;

    @Field(() => Int)
    maxPlayers!: number;

    @Field(() => String)
    gameTypeId!: string;

    @Field(() => String)
    gameTypeName!: string;

    @Field(() => String)
    difficulty!: string;

    @Field(() => Int)
    stockQuantity!: number;

    @Field(() => String)
    imageUri!: string;
}

@InputType()
export class GameInput {
    @Field(() => String)
    name!: string;

    @Field(() => Float)
    hourlyPrice!: number;

    @Field(() => Int)
    minPlayers!: number;

    @Field(() => Int)
    maxPlayers!: number;

    @Field(() => String)
    gameTypeId!: string;

    @Field(() => String)
    difficulty!: string;

    @Field(() => Int)
    stockQuantity!: number;

    @Field(() => String)
    imageUri!: string;
}
