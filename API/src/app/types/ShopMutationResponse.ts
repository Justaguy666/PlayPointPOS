import { Field, ObjectType } from "type-graphql";
import { Shop } from "../entities/Shop.js";
import { MutationResponse } from "./MutationResponse.js";

@ObjectType()
export class ShopMutationResponse extends MutationResponse {
    @Field(() => Shop, { nullable: true })
    shop?: Shop;
}
