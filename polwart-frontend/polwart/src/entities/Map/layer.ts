import type { Symbol } from "./Legend/symbol";

export type Layer = {
    timestampISO: string;
    content: Symbol[];
};