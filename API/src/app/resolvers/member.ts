import { Arg, Int, Mutation, Query, Resolver } from "type-graphql";
import { MutationResponse } from "../types/MutationResponse.js";
import { MemberDto, MemberInput, MembershipDto, MembershipInput } from "../types/management/member.js";
import {
    createMember,
    createMembership,
    deleteMember,
    deleteMembership,
    getMembers,
    getMemberships,
    updateMember,
    updateMembership,
} from "../services/management/memberService.js";

@Resolver()
export class MemberResolver {
    @Query(() => [MembershipDto])
    async memberships(@Arg("shopId", () => Int) shopId: number): Promise<MembershipDto[]> {
        return getMemberships(shopId);
    }

    @Mutation(() => MembershipDto)
    async createMembership(
        @Arg("shopId", () => Int) shopId: number,
        @Arg("input", () => MembershipInput) input: MembershipInput,
    ): Promise<MembershipDto> {
        return createMembership(shopId, input);
    }

    @Mutation(() => MembershipDto)
    async updateMembership(
        @Arg("id", () => Int) id: number,
        @Arg("input", () => MembershipInput) input: MembershipInput,
    ): Promise<MembershipDto> {
        return updateMembership(id, input);
    }

    @Mutation(() => MutationResponse)
    async deleteMembership(@Arg("id", () => Int) id: number): Promise<MutationResponse> {
        await deleteMembership(id);
        return { code: 200, success: true, message: "Membership deleted successfully" };
    }

    @Query(() => [MemberDto])
    async members(@Arg("shopId", () => Int) shopId: number): Promise<MemberDto[]> {
        return getMembers(shopId);
    }

    @Mutation(() => MemberDto)
    async createMember(
        @Arg("shopId", () => Int) shopId: number,
        @Arg("input", () => MemberInput) input: MemberInput,
    ): Promise<MemberDto> {
        return createMember(shopId, input);
    }

    @Mutation(() => MemberDto)
    async updateMember(
        @Arg("id", () => Int) id: number,
        @Arg("input", () => MemberInput) input: MemberInput,
    ): Promise<MemberDto> {
        return updateMember(id, input);
    }

    @Mutation(() => MutationResponse)
    async deleteMember(@Arg("id", () => Int) id: number): Promise<MutationResponse> {
        await deleteMember(id);
        return { code: 200, success: true, message: "Member deleted successfully" };
    }
}
