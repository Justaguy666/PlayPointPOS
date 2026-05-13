import "dotenv/config";

function required(name: string): string {
    const value = process.env[name];
    if (!value) throw new Error(`Missing environment variable: ${name}`);
    return value;
}

function optional(name: string, fallback: string): string {
    return process.env[name] ?? fallback;
}

export const env = {
    port: optional("PORT", "4000"),
    databaseUrl: required("DATABASE_URL"),
    jwtAccessSecret: required("JWT_ACCESS_SECRET"),
    jwtRefreshSecret: required("JWT_REFRESH_SECRET"),
    smtpHost: required("SMTP_HOST"),
    smtpPort: Number(optional("SMTP_PORT", "587")),
    smtpUser: required("SMTP_USER"),
    smtpPass: required("SMTP_PASS"),
    smtpFrom: optional("SMTP_FROM", "noreply@playpointpos.com"),
    cloudinaryCloudName: optional("CLOUDINARY_CLOUD_NAME", ""),
    cloudinaryApiKey: optional("CLOUDINARY_API_KEY", ""),
    cloudinaryApiSecret: optional("CLOUDINARY_API_SECRET", ""),
    cloudinaryUploadFolder: optional("CLOUDINARY_UPLOAD_FOLDER", "playpointpos"),
};

export function isCloudinaryConfigured(): boolean {
    return Boolean(
        env.cloudinaryCloudName.trim() &&
            env.cloudinaryApiKey.trim() &&
            env.cloudinaryApiSecret.trim(),
    );
}
