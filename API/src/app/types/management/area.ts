import { Field, Float, ID, InputType, Int, ObjectType } from "type-graphql";

@ObjectType()
export class AreaDto {
    @Field(() => ID)
    id!: string;

    @Field(() => String)
    areaName!: string;

    @Field(() => String)
    playAreaType!: string;

    @Field(() => String)
    status!: string;

    @Field(() => Int)
    maxCapacity!: number;

    @Field(() => Float)
    hourlyPrice!: number;

    @Field(() => String)
    customerName!: string;

    @Field(() => String)
    phoneNumber!: string;

    @Field(() => String, { nullable: true })
    memberId?: string;

    @Field(() => Date, { nullable: true })
    checkInDateTime?: Date;

    @Field(() => Int)
    capacity!: number;

    @Field(() => Date, { nullable: true })
    startTime?: Date;

    @Field(() => Boolean)
    isSessionPaused!: boolean;

    @Field(() => Date, { nullable: true })
    sessionPausedAt?: Date;

    @Field(() => Int)
    sessionPausedDurationSeconds!: number;

    @Field(() => Float)
    totalAmount!: number;

    @Field(() => Int, { nullable: true })
    activeSessionId?: number;
}

@ObjectType()
export class AreaSessionStartPayload {
    @Field(() => Int)
    sessionId!: number;

    @Field(() => Date)
    startTime!: Date;
}

@InputType()
export class AreaInput {
    @Field(() => String)
    areaName!: string;

    @Field(() => String)
    playAreaType!: string;

    @Field(() => Int)
    maxCapacity!: number;

    @Field(() => Float)
    hourlyPrice!: number;
}
