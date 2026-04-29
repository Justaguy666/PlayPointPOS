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
    jwtSecret: required("JWT_SECRET"),
};
