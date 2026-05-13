import { Field, InputType } from "type-graphql";

@InputType()
export class ResetPasswordInput {
    @Field(() => String)
    email!: string;

    @Field(() => String)
    otp!: string;

    @Field(() => String)
    newPassword!: string;
}
