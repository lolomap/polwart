import type { SymbolType } from "./Legend/symbol-type";
import type { Layer } from "./layer";

export type Map = {
    layers: Layer[];
    legend: SymbolType[];
};