import "reflect-metadata";
import { ApolloServer } from "@apollo/server";
import { startStandaloneServer } from "@apollo/server/standalone";
import { buildSchema } from "type-graphql";
import { AppDataSource } from "./config/database.js";
import { ShopResolver } from "./app/resolvers/shop.resolver.js";
import { env } from "./config/env.js";

await AppDataSource.initialize();

const schema = await buildSchema({
    resolvers: [ShopResolver],
});

const server = new ApolloServer({ schema });

const { url } = await startStandaloneServer(server, {
    listen: { port: Number(env.port) },
});

console.log(`Server running at ${url}`);
