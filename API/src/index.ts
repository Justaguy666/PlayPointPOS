import "reflect-metadata";
import http from "http";
import express, { type Request, type Response } from "express";
import cors from "cors";
import multer from "multer";
import { v2 as cloudinary } from "cloudinary";
import { ApolloServer } from "@apollo/server";
import { expressMiddleware } from "@as-integrations/express5";
import { ApolloServerPluginDrainHttpServer } from "@apollo/server/plugin/drainHttpServer";
import { buildSchema } from "type-graphql";
import { AppDataSource } from "./config/database.js";
import { AreaResolver } from "./app/resolvers/area.js";
import { GameResolver } from "./app/resolvers/game.js";
import { MemberResolver } from "./app/resolvers/member.js";
import { ProductResolver } from "./app/resolvers/product.js";
import { ShopResolver } from "./app/resolvers/shop.js";
import { TransactionResolver } from "./app/resolvers/transaction.js";
import { env, isCloudinaryConfigured } from "./config/env.js";
import type { Context } from "./app/types/Context.js";

await AppDataSource.initialize();

const schema = await buildSchema({
    resolvers: [
        ShopResolver,
        AreaResolver,
        GameResolver,
        ProductResolver,
        MemberResolver,
        TransactionResolver,
    ],
});

const app = express();
const httpServer = http.createServer(app);

const server = new ApolloServer<Context>({
    schema,
    plugins: [ApolloServerPluginDrainHttpServer({ httpServer })],
});

await server.start();

if (isCloudinaryConfigured()) {
    cloudinary.config({
        cloud_name: env.cloudinaryCloudName,
        api_key: env.cloudinaryApiKey,
        api_secret: env.cloudinaryApiSecret,
    });
}

const upload = multer({
    storage: multer.memoryStorage(),
    limits: { fileSize: 8 * 1024 * 1024 },
});

app.use(cors({ origin: true, credentials: true }));

app.post("/upload/image", upload.single("file"), async (req: Request, res: Response) => {
    try {
        if (!isCloudinaryConfigured()) {
            res.status(503).json({ error: "Cloudinary is not configured on the server." });
            return;
        }

        const file = req.file;
        if (!file?.buffer?.length) {
            res.status(400).json({ error: "Missing image file (multipart field: file)." });
            return;
        }

        const url = await new Promise<string>((resolve, reject) => {
            const stream = cloudinary.uploader.upload_stream(
                { folder: env.cloudinaryUploadFolder },
                (error, result) => {
                    if (error) {
                        reject(error);
                        return;
                    }
                    const secure = result?.secure_url ?? result?.url;
                    if (!secure) {
                        reject(new Error("Cloudinary returned no URL"));
                        return;
                    }
                    resolve(secure);
                },
            );
            stream.end(file.buffer);
        });

        res.json({ url });
    } catch (err) {
        console.error("upload/image error:", err);
        res.status(500).json({ error: "Image upload failed" });
    }
});

app.use(
    "/",
    express.json({ limit: "4mb" }),
    expressMiddleware(server, {
        context: async ({ req }): Promise<Context> => ({ req }),
    }),
);

const port = Number(env.port);
await new Promise<void>((resolve) => {
    httpServer.listen(port, () => resolve());
});

console.log(`HTTP + GraphQL ready at http://localhost:${port}/`);
console.log(`Image upload POST http://localhost:${port}/upload/image`);
