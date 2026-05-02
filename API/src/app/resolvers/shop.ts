import { Arg, Mutation, Query, Resolver } from "type-graphql";
import argon2 from "argon2";
import jwt from "jsonwebtoken";
import { Shop } from "../entities/Shop.js";
import { OTP } from "../entities/OTP.js";
import { RegisterShopInput } from "../types/RegisterShopInput.js";
import { SendOtpInput } from "../types/SendOtpInput.js";
import { MutationResponse } from "../types/MutationResponse.js";
import { ShopMutationResponse } from "../types/ShopMutationResponse.js";
import { LoginShopInput } from "../types/LoginShopInput.js";
import { checkOtp } from "../utils/checkOtp.js";
import { sendOtpEmail } from "../services/mailer.js";
import { env } from "../../config/env.js";
import { Session } from "../entities/Session.js";
import { generateToken } from "../utils/auth.js";

@Resolver()
export class ShopResolver {
    @Query(() => [Shop])
    async shops() {
        return Shop.find();
    }

    @Mutation(() => MutationResponse)
    async sendOtp(
        @Arg("input", () => SendOtpInput) input: SendOtpInput
    ): Promise<MutationResponse> {
        try {
            const { email } = input;

            const existingShop = await Shop.findOne({ where: { Email: email } });
            if (existingShop) {
                return { code: 400, success: false, message: "Email already registered" };
            }

            const existingOtp = await OTP.findOne({ where: { Email: email } });
            if (existingOtp) await existingOtp.remove();

            const otp = Math.floor(100000 + Math.random() * 900000).toString();
            const expiresAt = new Date(Date.now() + 5 * 60 * 1000);

            await OTP.create({ Email: email, Otp: otp, ExpiresAt: expiresAt }).save();
            await sendOtpEmail(email, otp);

            return { code: 200, success: true, message: "OTP sent to your email" };
        } catch (err) {
            console.error("sendOtp error:", err);
            return { code: 500, success: false, message: "Internal server error" };
        }
    }

    @Mutation(() => ShopMutationResponse)
    async register(
        @Arg("input") input: RegisterShopInput
    ): Promise<ShopMutationResponse> {
        try {
            const { email, password, otp } = input;

            const { valid, error, record } = await checkOtp(email, otp);
            if (!valid) {
                return { code: 400, success: false, message: error ?? "Invalid OTP" };
            }

            const hashedPassword = await argon2.hash(password);
            const newShop = Shop.create({ Email: email, HashedPassword: hashedPassword });
            await newShop.save();

            await record!.remove();

            return { code: 201, success: true, message: "Shop registered successfully" };
        } catch {
            return { code: 500, success: false, message: "Internal server error" };
        }
    }

    @Mutation(() => ShopMutationResponse)
    async login(
        @Arg("input") input: LoginShopInput
    ): Promise<ShopMutationResponse> {
        try {
            const { email, password } = input;

            const isExistingShop = await Shop.findOne({ where: { Email: email } });
            if (!isExistingShop) {
                return { code: 400, success: false, message: "Invalid email or password" };
            }

            const isPasswordValid = await argon2.verify(isExistingShop.HashedPassword, password);
            if (!isPasswordValid) {
                return { code: 400, success: false, message: "Invalid email or password" };
            }

            const isExistingSession = await Session.findOne({ where: { ShopID: isExistingShop.ID } });
            if (isExistingSession) await isExistingSession.remove();

            const accessToken = generateToken(isExistingShop.ID, "access", "15m");
            const refreshToken = generateToken(isExistingShop.ID, "refresh", "30d");
            const expiresAt = new Date(Date.now() + 30 * 24 * 60 * 60 * 1000);

            await Session.create({ ShopID: isExistingShop.ID, Token: refreshToken, ExpiresAt: expiresAt }).save();

            return { 
                code: 200, 
                success: true, 
                message: "Login successful", 
                shopId: isExistingShop.ID, 
                accessToken: accessToken, 
                refreshToken: refreshToken
            };
        } catch (err) {
            console.error("login error:", err);
            return { code: 500, success: false, message: "Internal server error" };
        }
    }
}