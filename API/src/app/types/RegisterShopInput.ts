import { Field, InputType } from "type-graphql";

@InputType()
export class RegisterShopInput {
    @Field(() => String)
    email!: string;

    @Field(() => String)
    password!: string;

    @Field(() => String)
    otp!: string;
}
