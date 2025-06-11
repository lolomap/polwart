import type { SymbolType } from "@/entities/Map/Legend/symbol-type";
import type { Symbol } from '@/entities/Map/Legend/symbol';
import type { Map } from '@/entities/Map/map';
import type { Layer } from "@/entities/Map/layer";

// --------- Patches ---------

export function AddLayer(map: Map, timestampISO: string, content: Symbol[]): string {
    const layer: Layer = {
        timestampISO: timestampISO,
        content: content
    };
    
    const timestamp = new Date(timestampISO);
    const index: number = map.layers.findIndex(x => (new Date(x.timestampISO)) <= timestamp);

    return `
    [
        {
            "op": "add",
            "path": "/layers/-",
            "value": ${JSON.stringify(layer)}
        }
    ]
    `;
}

export function AddSymbolType(map: Map, stype: SymbolType): string {
    //map.legend.push(stype);

    return `
    [
        {
            "op": "add",
            "path": "/legend/-",
            "value": ${JSON.stringify(stype)}
        }
    ]
    `;
}

export function RemoveSymbolType(map: Map, stype: SymbolType): string {
    const index = map.legend.findIndex(el => el.id == stype.id);

    return `
    [
        {
            "op": "remove",
            "path": "/legend/${index}"
        }
    ]
    `;
}

export function UpdateSymbolType(map: Map, stype: SymbolType): string {
    const index = map.legend.findIndex(el => el.id == stype.id);
    if (index < 0 || map.legend.length <= index) {
        console.error('Cannot update SymbolType that not presented in Legend');
        return '';
    }

    //map.legend[index] = stype;
    return `
    [
        {
            "op": "replace",
            "path": "/legend/${index}",
            "value": ${JSON.stringify(stype)}
        }
    ]
    `;
}

export function AddSymbol(map: Map, layer: number, symbol: Symbol): string {
    //map.layers[layer].content.push(symbol);

    return `
    [
        {
            "op": "add",
            "path": "/layers/${layer}/content/-",
            "value": ${JSON.stringify(symbol)}
        }
    ]
    `;
}

export function RemoveSymbol(map: Map, layer: number, symbol: Symbol) {
    if (map.layers.length < layer || layer < 0) {
        console.error('Cannot remove Symbol that not presented in Content');
        return '';
    }
    const index = map.layers[layer].content.findIndex(el => el.id == symbol.id);
    if (index < 0 || map.layers[layer].content.length <= index) {
        console.error('Cannot remove Symbol that not presented in Content');
        return '';
    }

    return `
    [
        {
            "op": "remove",
            "path": "/layers/${layer}/content/${index}"
        }
    ]
    `;
}

export function UpdateSymbol(map: Map, layer: number, symbol: Symbol) {
    if (map.layers.length < layer || layer < 0) {
        console.error('Cannot update Symbol that not presented in Content');
        return '';
    }
    const index = map.layers[layer].content.findIndex(el => el.id == symbol.id);
    if (index < 0 || map.layers[layer].content.length <= index) {
        console.error('Cannot update Symbol that not presented in Content');
        return '';
    }

    return `
    [
        {
            "op": "replace",
            "path": "/layers/${layer}/content/${index}",
            "value": ${JSON.stringify(symbol)}
        }
    ]
    `;
}

export function UpdateSymbolPos(map: Map, layer: number, symbol: Symbol) {
    if (map.layers.length < layer || layer < 0) {
        console.error('Cannot update Symbol that not presented in Content');
        return '';
    }
    const index = map.layers[layer].content.findIndex(el => el.id == symbol.id);
    if (index < 0 || map.layers[layer].content.length <= index) {
        console.error('Cannot update Symbol that not presented in Content');
        return '';
    }

    return `
    [
        {
            "op": "replace",
            "path": "/layers/${layer}/content/${index}/x",
            "value": ${symbol.x}
        },
        {
            "op": "replace",
            "path": "/layers/${layer}/content/${index}/y",
            "value": ${symbol.y}
        }
    ]
    `;
}

export function UpdateSymbolConnection(map: Map, layer: number, symbol: Symbol) {
    if (map.layers.length < layer || layer < 0) {
        console.error('Cannot update Symbol that not presented in Content');
        return '';
    }
    const index = map.layers[layer].content.findIndex(el => el.id == symbol.id);
    if (index < 0 || map.layers[layer].content.length <= index) {
        console.error('Cannot update Symbol that not presented in Content');
        return '';
    }

    return `
    [
        {
            "op": "replace",
            "path": "/layers/${layer}/content/${index}/value/#connection",
            "value": ${symbol.value['#connection']}
        }
    ]
    `;
}


// ------------- Getters ---------

export function GetSymbol(map: Map, layer: number, id: number): Symbol | undefined {
    if (layer < 0 || map.layers.length <= layer) return undefined;
    return map.layers[layer].content.find(x => x.id == id);
}

export function GetSymbolType(map: Map, id: number): SymbolType | undefined {
    return map.legend.find(x => x.id == id);
}