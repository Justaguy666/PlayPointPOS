import { Arg, Int, Mutation, Query, Resolver } from "type-graphql";
import { TransactionDto } from "../types/management/transaction.js";
import { getTransactions, updateTransactionPaymentMethod } from "../services/management/transactionService.js";

@Resolver()
export class TransactionResolver {
    @Query(() => [TransactionDto])
    async transactions(@Arg("shopId", () => Int) shopId: number): Promise<TransactionDto[]> {
        return getTransactions(shopId);
    }

    @Mutation(() => TransactionDto)
    async updateTransactionPaymentMethod(
        @Arg("id", () => Int) id: number,
        @Arg("paymentMethod", () => String) paymentMethod: string,
    ): Promise<TransactionDto> {
        return updateTransactionPaymentMethod(id, paymentMethod);
    }
}
