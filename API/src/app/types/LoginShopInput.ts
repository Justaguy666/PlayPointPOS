import { Field, InputType } from "type-graphql";

@InputType()
export class LoginShopInput {
    @Field(() => String)
    email!: string;

    @Field(() => String)
    password!: string;
}
