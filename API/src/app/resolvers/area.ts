import { Arg, Int, Mutation, Query, Resolver } from "type-graphql";
import { MutationResponse } from "../types/MutationResponse.js";
import { AreaDto, AreaInput, AreaSessionStartPayload } from "../types/management/area.js";
import { createArea, deleteArea, getAreas, startAreaSession as insertAreaSession, updateArea } from "../services/management/areaService.js";

@Resolver()
export class AreaResolver {
    @Query(() => [AreaDto])
    async areas(@Arg("shopId", () => Int) shopId: number): Promise<AreaDto[]> {
        return getAreas(shopId);
    }

    @Mutation(() => AreaDto)
    async createArea(
        @Arg("shopId", () => Int) shopId: number,
        @Arg("input", () => AreaInput) input: AreaInput,
    ): Promise<AreaDto> {
        return createArea(shopId, input);
    }

    @Mutation(() => AreaDto)
    async updateArea(
        @Arg("id", () => Int) id: number,
        @Arg("input", () => AreaInput) input: AreaInput,
    ): Promise<AreaDto> {
        return updateArea(id, input);
    }

    @Mutation(() => MutationResponse)
    async deleteArea(@Arg("id", () => Int) id: number): Promise<MutationResponse> {
        await deleteArea(id);
        return { code: 200, success: true, message: "Area deleted successfully" };
    }

    @Mutation(() => AreaSessionStartPayload)
    async startAreaSession(
        @Arg("shopId", () => Int) shopId: number,
        @Arg("areaId", () => Int) areaId: number,
        @Arg("guestCount", () => Int) guestCount: number,
        @Arg("memberId", () => Int, { nullable: true }) memberId?: number | null,
    ): Promise<AreaSessionStartPayload> {
        return insertAreaSession(shopId, areaId, guestCount, memberId ?? undefined);
    }
}
