import type { Symbol } from "./Legend/symbol";
import type { SymbolType } from "./Legend/symbol-type";
import type { Layer } from "./layer";

export type Map = {
    layers: Layer[];
    legend: SymbolType[];
};

//TODO: move it to features
// All functions should return jsonPatch

export function AddSymbolType(map: Map, stype: SymbolType): void {
    map.legend.push(stype);
}
export function UpdateSymbolType(map: Map, stype: SymbolType): void {
    const index = map.legend.findIndex(el => el.id == stype.id);
    if (index < 0 || map.legend.length <= index) {
        console.error('Cannot update SymbolType that not presented in Legend');
        return;
    }

    map.legend[index] = stype;
}

export function AddSymbol(map: Map, layer: number, symbol: Symbol): void {
    map.layers[layer].content.push(symbol);
}

export function GetSymbol(map: Map, layer: number, id: number): Symbol | undefined {
    if (layer < 0 || map.layers.length <= layer) return undefined;
    return map.layers[layer].content.find(x => x.id == id);
}

export function GetSymbolType(map: Map, id: number): SymbolType | undefined {
    return map.legend.find(x => x.id == id);
}