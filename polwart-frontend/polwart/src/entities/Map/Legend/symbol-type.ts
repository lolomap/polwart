export type SymbolProperty = {
    name: string;
    type: 'integer' | 'float' | 'string' | 'boolean';

    isEditable: boolean;

    isCombo: boolean;
    comboValues: object[];
};

export type SymbolType = {
    id: number;
    name: string;

    iconFormat: string;
    
    properties: SymbolProperty[];
};