import "reflect-metadata";
import { ApolloServer } from "@apollo/server";
import { startStandaloneServer } from "@apollo/server/standalone";
import { buildSchema } from "type-graphql";
import { AppDataSource } from "./config/database.js";
import { ShopResolver } from "./app/resolvers/shop.js";
import { env } from "./config/env.js";
import type { Context } from "./app/types/Context.js";

await AppDataSource.initialize();

const schema = await buildSchema({
    resolvers: [ShopResolver],
});

const server = new ApolloServer<Context>({ schema });

const { url } = await startStandaloneServer(server, {
    listen: { port: Number(env.port) },
    context: async ({ req }): Promise<Context> => {
        return { req };
    },
});

console.log(`Server running at ${url}`);
