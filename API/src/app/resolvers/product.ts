import { Arg, Int, Mutation, Query, Resolver } from "type-graphql";
import { MutationResponse } from "../types/MutationResponse.js";
import { ProductDto, ProductInput } from "../types/management/product.js";
import { createProduct, deleteProduct, getProducts, updateProduct } from "../services/management/productService.js";

@Resolver()
export class ProductResolver {
    @Query(() => [ProductDto])
    async products(@Arg("shopId", () => Int) shopId: number): Promise<ProductDto[]> {
        return getProducts(shopId);
    }

    @Mutation(() => ProductDto)
    async createProduct(
        @Arg("shopId", () => Int) shopId: number,
        @Arg("input", () => ProductInput) input: ProductInput,
    ): Promise<ProductDto> {
        return createProduct(shopId, input);
    }

    @Mutation(() => ProductDto)
    async updateProduct(
        @Arg("shopId", () => Int) shopId: number,
        @Arg("id", () => Int) id: number,
        @Arg("input", () => ProductInput) input: ProductInput,
    ): Promise<ProductDto> {
        return updateProduct(shopId, id, input);
    }

    @Mutation(() => MutationResponse)
    async deleteProduct(
        @Arg("shopId", () => Int) shopId: number,
        @Arg("id", () => Int) id: number,
    ): Promise<MutationResponse> {
        await deleteProduct(shopId, id);
        return { code: 200, success: true, message: "Product deleted successfully" };
    }
}
