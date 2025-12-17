export type ClientState = Record<number, MapState>;

export type MapState = {
    pos: {x: number; y: number;};
    zoom: number;
    layer: number;
};