import jwt from "jsonwebtoken";
import type { StringValue } from "ms";
import { env } from "../../config/env.js";

export const generateToken = (shopId: number, type: "access" | "refresh", expiresIn: StringValue = "15m"): string => jwt.sign(
    { shopId },
    type === "access" ? env.jwtAccessSecret : env.jwtRefreshSecret,
    { expiresIn }
)
