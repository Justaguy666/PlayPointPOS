import { DataSource } from "typeorm";
import { env } from "./env.js";
import { Shop } from "../app/entities/Shop.js";
import { OTP } from "../app/entities/OTP.js";
import { Session } from "../app/entities/Session.js";

export const AppDataSource = new DataSource({
    type: "postgres",
    url: env.databaseUrl,
    entities: [Shop, OTP, Session],
    migrations: ["src/app/migrations/*.ts"],
    synchronize: false,
});
