import { Arg, Mutation, Query, Resolver } from "type-graphql";
import argon2 from "argon2";
import jwt from "jsonwebtoken";
import { Shop } from "../entities/Shop.js";
import { OTP } from "../entities/OTP.js";
import { RegisterShopInput } from "../types/RegisterShopInput.js";
import { SendOtpInput } from "../types/SendOtpInput.js";
import { MutationResponse } from "../types/MutationResponse.js";
import { ShopMutationResponse } from "../types/ShopMutationResponse.js";
import { checkOtp } from "../utils/checkOtp.js";
import { sendOtpEmail } from "../services/mailer.js";
import { env } from "../../config/env.js";

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
        @Arg("input", () => RegisterShopInput) input: RegisterShopInput
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

            return { code: 201, success: true, message: "Shop registered successfully", shop: newShop };
        } catch {
            return { code: 500, success: false, message: "Internal server error" };
        }
    }
}
