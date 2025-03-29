import type { SymbolType } from "./Legend/symbol-type";
import type { Layer } from "./layer";

export type Map = {
    layers: Layer[];
    legend: SymbolType[];
};

// It will return jsonPatch
export function AddSymbolType (map: Map, stype: SymbolType): void {
    map.legend.push(stype);
}