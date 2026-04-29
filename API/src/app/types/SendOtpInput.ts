import { Field, InputType } from "type-graphql";

@InputType()
export class SendOtpInput {
    @Field(() => String)
    email!: string;
}
