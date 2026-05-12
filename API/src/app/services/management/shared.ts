export type SqlRow = Record<string, unknown>;

export function requireRow<T>(rows: T[], entityName: string, id?: number): T {
    const row = rows[0];
    if (!row) {
        throw new Error(id ? `${entityName} ${id} not found` : `${entityName} not found`);
    }

    return row;
}

export function toNumber(value: unknown, fallback = 0): number {
    if (typeof value === "number") {
        return value;
    }

    if (typeof value === "string") {
        const parsed = Number(value);
        return Number.isNaN(parsed) ? fallback : parsed;
    }

    return fallback;
}

export function toStringValue(value: unknown, fallback = ""): string {
    if (typeof value === "string") {
        return value;
    }

    if (value === null || value === undefined) {
        return fallback;
    }

    return String(value);
}

export function toOptionalString(value: unknown): string | undefined {
    if (value === null || value === undefined) {
        return undefined;
    }

    return String(value);
}

export function toDateValue(value: unknown): Date | undefined {
    if (value instanceof Date) {
        return value;
    }

    if (typeof value === "string" || typeof value === "number") {
        const parsed = new Date(value);
        return Number.isNaN(parsed.getTime()) ? undefined : parsed;
    }

    return undefined;
}
