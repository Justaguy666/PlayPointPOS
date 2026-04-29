import { OTP } from "../entities/OTP.js";

interface CheckOtpResult {
    valid: boolean;
    error?: string;
    record?: OTP;
}

export async function checkOtp(email: string, otp: string): Promise<CheckOtpResult> {
    const record = await OTP.findOne({ where: { Email: email } });

    if (!record) return { valid: false, error: "OTP not found or already used" };
    if (record.ExpiresAt < new Date()) return { valid: false, error: "OTP has expired" };
    if (record.Otp !== otp) return { valid: false, error: "Invalid OTP" };

    return { valid: true, record };
}
