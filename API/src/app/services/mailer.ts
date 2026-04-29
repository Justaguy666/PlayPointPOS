import nodemailer from "nodemailer";
import { env } from "../../config/env.js";

const transporter = nodemailer.createTransport({
    host: env.smtpHost,
    port: env.smtpPort,
    secure: env.smtpPort === 465,
    auth: {
        user: env.smtpUser,
        pass: env.smtpPass,
    },
});

export async function sendOtpEmail(to: string, otp: string): Promise<void> {
    await transporter.sendMail({
        from: env.smtpFrom,
        to,
        subject: "PlayPoint - Verification Code",
        text: `Your OTP code is: ${otp}. It expires in 5 minutes.`,
        html: `
<!DOCTYPE html>
<html>
<body style="margin:0;padding:0;background-color:#f4f4f4;font-family:Arial,sans-serif;">
  <table width="100%" cellpadding="0" cellspacing="0">
    <tr>
      <td align="center" style="padding:40px 0;">
        <table width="480" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.08);">

          <tr>
            <td style="background:linear-gradient(135deg,#F97316,#FBBF24);padding:32px;text-align:center;">
              <h1 style="margin:0;color:#ffffff;font-size:24px;letter-spacing:1px;">PlayPoint POS</h1>
            </td>
          </tr>

          <tr>
            <td style="padding:40px 32px 24px;text-align:center;">
              <p style="margin:0 0 8px;color:#374151;font-size:16px;">Your verification code is</p>
              <div style="display:inline-block;background:#FFF7ED;border:2px dashed #F97316;border-radius:12px;padding:16px 40px;margin:16px 0;">
                <span style="font-size:36px;font-weight:bold;letter-spacing:10px;color:#F97316;">${otp}</span>
              </div>
              <p style="margin:16px 0 0;color:#6B7280;font-size:14px;">This code expires in <strong>5 minutes</strong>.</p>
            </td>
          </tr>

          <tr>
            <td style="padding:0 32px 32px;text-align:center;">
              <p style="margin:0;color:#9CA3AF;font-size:12px;">If you did not request this, please ignore this email.</p>
            </td>
          </tr>

          <tr>
            <td style="background:#f9fafb;padding:16px;text-align:center;border-top:1px solid #E5E7EB;">
              <p style="margin:0;color:#9CA3AF;font-size:12px;">© 2025 PlayPoint POS. All rights reserved.</p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>`,
    });
}
