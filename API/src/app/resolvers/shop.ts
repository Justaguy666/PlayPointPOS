import { Arg, Int, Mutation, Query, Resolver } from "type-graphql";
import argon2 from "argon2";
import { Shop } from "../entities/shop.js";
import { OTP } from "../entities/OTP.js";
import { RegisterShopInput } from "../types/RegisterShopInput.js";
import { ResetPasswordInput } from "../types/ResetPasswordInput.js";
import { SendOtpInput } from "../types/SendOtpInput.js";
import { MutationResponse } from "../types/MutationResponse.js";
import { ShopMutationResponse } from "../types/ShopMutationResponse.js";
import { LoginShopInput } from "../types/LoginShopInput.js";
import { checkOtp } from "../utils/checkOtp.js";
import { sendOtpEmail } from "../services/mailer.js";
import { AuthSession } from "../entities/AuthSession.js";
import { generateToken } from "../utils/auth.js";

@Resolver()
export class ShopResolver {
    @Query(() => [Shop])
    async shops() {
        return Shop.find();
    }

    @Query(() => Shop, { nullable: true })
    async shopProfile(
        @Arg("shopId", () => Int) shopId: number
    ): Promise<Shop | null> {
        return await Shop.findOne({ where: { ID: shopId } });
    }

    @Mutation(() => Shop)
    async updateShopProfile(
        @Arg("shopId", () => Int) shopId: number,
        @Arg("name", () => String, { nullable: true }) name?: string,
        @Arg("address", () => String, { nullable: true }) address?: string,
        @Arg("email", () => String, { nullable: true }) email?: string,
        @Arg("phoneNumber", () => String, { nullable: true }) phoneNumber?: string,
    ): Promise<Shop> {
        const shop = await Shop.findOne({ where: { ID: shopId } });
        if (!shop) {
            throw new Error("Shop not found.");
        }

        const normalizedName = name?.trim() ?? "";
        const normalizedAddress = address?.trim() ?? "";
        const normalizedEmail = email?.trim() ?? "";
        const normalizedPhone = phoneNumber?.trim() ?? "";

        if (normalizedEmail && normalizedEmail.toLowerCase() !== shop.Email.toLowerCase()) {
            const existingByEmail = await Shop.findOne({ where: { Email: normalizedEmail } });
            if (existingByEmail && existingByEmail.ID !== shopId) {
                throw new Error("Email is already in use by another shop.");
            }
        }

        shop.Name = normalizedName || shop.Name || "";
        shop.Address = normalizedAddress || shop.Address || "";
        shop.Email = normalizedEmail || shop.Email;
        shop.PhoneNumber = normalizedPhone || shop.PhoneNumber || "";
        await shop.save();

        return shop;
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

    @Mutation(() => MutationResponse)
    async sendPasswordResetOtp(
        @Arg("input", () => SendOtpInput) input: SendOtpInput
    ): Promise<MutationResponse> {
        try {
            const email = input.email?.trim() ?? "";
            if (!email) {
                return { code: 400, success: false, message: "Email is required." };
            }

            const existingShop = await Shop.findOne({ where: { Email: email } });
            if (!existingShop) {
                return { code: 404, success: false, message: "Shop account not found." };
            }

            const existingOtp = await OTP.findOne({ where: { Email: email } });
            if (existingOtp) await existingOtp.remove();

            const otp = Math.floor(100000 + Math.random() * 900000).toString();
            const expiresAt = new Date(Date.now() + 5 * 60 * 1000);

            await OTP.create({ Email: email, Otp: otp, ExpiresAt: expiresAt }).save();
            await sendOtpEmail(email, otp);

            return { code: 200, success: true, message: "OTP sent to your email" };
        } catch (err) {
            console.error("sendPasswordResetOtp error:", err);
            return { code: 500, success: false, message: "Internal server error" };
        }
    }

    @Mutation(() => ShopMutationResponse)
    async register(
        @Arg("input", () => RegisterShopInput) input: RegisterShopInput
    ): Promise<ShopMutationResponse> {
        try {
            const { email, password, otp } = input;

            const existingShop = await Shop.findOne({ where: { Email: email } });
            if (existingShop) {
                return { code: 400, success: false, message: "Email already registered" };
            }

            const { valid, error, record } = await checkOtp(email, otp);
            if (!valid) {
                return { code: 400, success: false, message: error ?? "Invalid OTP" };
            }

            const hashedPassword = await argon2.hash(password);
            const newShop = Shop.create({ Email: email, HashedPassword: hashedPassword });
            await newShop.save();

            await record!.remove();

            const accessToken = generateToken(newShop.ID, "access", "15m");
            const refreshToken = generateToken(newShop.ID, "refresh", "30d");
            const expiresAt = new Date(Date.now() + 30 * 24 * 60 * 60 * 1000);

            await AuthSession.create({ ShopID: newShop.ID, Token: refreshToken, ExpiresAt: expiresAt }).save();

            return {
                code: 201,
                success: true,
                message: "Shop registered successfully",
                shopId: newShop.ID,
                accessToken,
                refreshToken,
            };
        } catch (err) {
            console.error("register error:", err);
            return { code: 500, success: false, message: "Internal server error" };
        }
    }

    @Mutation(() => MutationResponse)
    async resetPassword(
        @Arg("input", () => ResetPasswordInput) input: ResetPasswordInput
    ): Promise<MutationResponse> {
        try {
            const email = input.email?.trim() ?? "";
            const otp = input.otp?.trim() ?? "";
            const newPassword = input.newPassword?.trim() ?? "";

            if (!email || !otp || !newPassword) {
                return { code: 400, success: false, message: "Email, OTP, and new password are required." };
            }

            const shop = await Shop.findOne({ where: { Email: email } });
            if (!shop) {
                return { code: 404, success: false, message: "Shop account not found." };
            }

            const { valid, error, record } = await checkOtp(email, otp);
            if (!valid) {
                return { code: 400, success: false, message: error ?? "Invalid OTP" };
            }

            shop.HashedPassword = await argon2.hash(newPassword);
            await shop.save();

            await record!.remove();
            await AuthSession.delete({ ShopID: shop.ID });

            return { code: 200, success: true, message: "Password reset successfully." };
        } catch (err) {
            console.error("resetPassword error:", err);
            return { code: 500, success: false, message: "Internal server error" };
        }
    }

    @Mutation(() => ShopMutationResponse)
    async login(
        @Arg("input", () => LoginShopInput) input: LoginShopInput
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

            const isExistingSession = await AuthSession.findOne({ where: { ShopID: isExistingShop.ID } });
            if (isExistingSession) await isExistingSession.remove();

            const accessToken = generateToken(isExistingShop.ID, "access", "15m");
            const refreshToken = generateToken(isExistingShop.ID, "refresh", "30d");
            const expiresAt = new Date(Date.now() + 30 * 24 * 60 * 60 * 1000);

            await AuthSession.create({ ShopID: isExistingShop.ID, Token: refreshToken, ExpiresAt: expiresAt }).save();

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
