import type { IncomingMessage } from "http";

export interface Context {
    req: IncomingMessage;
    shopId?: number;
}
