export type SqlRow = Record<string, unknown>;

/**
 * Reads a column from a raw SQL row. node-pg (and some TypeORM paths) may normalize
 * quoted identifiers to lowercase keys, so "ID" in SQL may appear as row.id.
 */
export function cell(row: SqlRow, key: string): unknown {
    if (row == null || typeof row !== "object") {
        return undefined;
    }

    const candidates = [...new Set([key, key.toLowerCase(), key.toUpperCase()])];
    for (const k of candidates) {
        if (Object.prototype.hasOwnProperty.call(row, k) && row[k] !== undefined) {
            return row[k];
        }
    }

    return undefined;
}

export function toStringCell(row: SqlRow, key: string, fallback = ""): string {
    return toStringValue(cell(row, key), fallback);
}

export function toNumberCell(row: SqlRow, key: string, fallback = 0): number {
    return toNumber(cell(row, key), fallback);
}

export function toDateCell(row: SqlRow, key: string): Date | undefined {
    return toDateValue(cell(row, key));
}

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
