import { Arg, Int, Mutation, Query, Resolver } from "type-graphql";
import { TransactionDto, AreaSessionCheckoutInput } from "../types/management/transaction.js";
import { getTransactions, updateTransactionPaymentMethod, persistAreaSessionCheckout } from "../services/management/transactionService.js";

@Resolver()
export class TransactionResolver {
    @Query(() => [TransactionDto])
    async transactions(@Arg("shopId", () => Int) shopId: number): Promise<TransactionDto[]> {
        return getTransactions(shopId);
    }

    @Mutation(() => TransactionDto)
    async createAreaSessionCheckout(
        @Arg("shopId", () => Int) shopId: number,
        @Arg("input", () => AreaSessionCheckoutInput) input: AreaSessionCheckoutInput,
    ): Promise<TransactionDto> {
        return persistAreaSessionCheckout(shopId, input);
    }
}
