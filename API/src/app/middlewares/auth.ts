import type { Context } from "../types/Context.js";
import type { MiddlewareFn } from "type-graphql";

export const checkAuth: MiddlewareFn<Context> = async ({ context }, next) => {
    try {
        const authHeader = context.req.headers['authorization'];
        const accessToken = authHeader?.replace('Bearer ', '');

        if (!accessToken) {
            throw new Error('No token provided');
        }

        return next();
    } catch (error) {
        throw new Error('Invalid token');
    }
};