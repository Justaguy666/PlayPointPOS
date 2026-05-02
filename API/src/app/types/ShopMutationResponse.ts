import { Field, ID, ObjectType } from "type-graphql";
import { MutationResponse } from "./MutationResponse.js";

@ObjectType()
export class ShopMutationResponse extends MutationResponse {
    @Field(() => ID, { nullable: true })
    shopId?: number;

    @Field(() => String, { nullable: true })
    accessToken?: string;

    @Field(() => String, { nullable: true })
    refreshToken?: string;
}
