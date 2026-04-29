import { Field, Int, ObjectType } from "type-graphql";

@ObjectType()
export class MutationResponse {
    @Field(() => Int)
    code!: number;

    @Field(() => Boolean)
    success!: boolean;

    @Field(() => String, { nullable: true })
    message?: string;
}
