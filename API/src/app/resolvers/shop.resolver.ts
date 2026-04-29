import { Resolver, Query } from "type-graphql";
import { Shop } from "../entities/shop.js";

@Resolver()
export class ShopResolver {
    @Query(() => [Shop])
    async shops() {
        return Shop.find();
    }
}
