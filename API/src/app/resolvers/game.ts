import { Arg, Int, Mutation, Query, Resolver } from "type-graphql";
import { MutationResponse } from "../types/MutationResponse.js";
import { GameDto, GameInput, GameTypeDto, GameTypeInput } from "../types/management/game.js";
import {
    createGame,
    createGameType,
    deleteGame,
    deleteGameType,
    getGames,
    getGameTypes,
    updateGame,
    updateGameType,
} from "../services/management/gameService.js";

@Resolver()
export class GameResolver {
    @Query(() => [GameTypeDto])
    async gameTypes(@Arg("shopId", () => Int) shopId: number): Promise<GameTypeDto[]> {
        return getGameTypes(shopId);
    }

    @Mutation(() => GameTypeDto)
    async createGameType(
        @Arg("shopId", () => Int) shopId: number,
        @Arg("input", () => GameTypeInput) input: GameTypeInput,
    ): Promise<GameTypeDto> {
        return createGameType(shopId, input);
    }

    @Mutation(() => GameTypeDto)
    async updateGameType(
        @Arg("id", () => Int) id: number,
        @Arg("input", () => GameTypeInput) input: GameTypeInput,
    ): Promise<GameTypeDto> {
        return updateGameType(id, input);
    }

    @Mutation(() => MutationResponse)
    async deleteGameType(@Arg("id", () => Int) id: number): Promise<MutationResponse> {
        await deleteGameType(id);
        return { code: 200, success: true, message: "Game type deleted successfully" };
    }

    @Query(() => [GameDto])
    async games(@Arg("shopId", () => Int) shopId: number): Promise<GameDto[]> {
        return getGames(shopId);
    }

    @Mutation(() => GameDto)
    async createGame(
        @Arg("shopId", () => Int) shopId: number,
        @Arg("input", () => GameInput) input: GameInput,
    ): Promise<GameDto> {
        return createGame(shopId, input);
    }

    @Mutation(() => GameDto)
    async updateGame(
        @Arg("id", () => Int) id: number,
        @Arg("input", () => GameInput) input: GameInput,
    ): Promise<GameDto> {
        return updateGame(id, input);
    }

    @Mutation(() => MutationResponse)
    async deleteGame(@Arg("id", () => Int) id: number): Promise<MutationResponse> {
        await deleteGame(id);
        return { code: 200, success: true, message: "Game deleted successfully" };
    }
}
