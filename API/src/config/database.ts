import { DataSource } from "typeorm";
import { env } from "./env.js";

export const AppDataSource = new DataSource({
    type: "postgres",
    url: env.databaseUrl,
    entities: ["src/app/entities/*.ts"],
    migrations: ["src/app/migrations/*.ts"],
    synchronize: false,
});
