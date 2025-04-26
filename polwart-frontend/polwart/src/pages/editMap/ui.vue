<script setup lang="ts">
import { ref, onMounted } from 'vue';
import RelationGraph, { type JsonNode, type RGNode, type RGUserEvent } from 'relation-graph-vue3';
import { RelationGraphComponent, type RGOptions } from 'relation-graph-vue3';
import { Typography } from '@/shared/typography';
import { useSessionStore } from '@/entities/store';
import type { Symbol } from '@/entities/Map/Legend/symbol';
import * as api from '@/features/api';
import * as map from '@/features/map';
import type { SymbolType } from '@/entities/Map/Legend/symbol-type';
import { Button } from '@/shared/button';
import { Modal } from '@/widgets/modal';
import { Field } from '@/shared/field';
import { Checkbox } from '@/shared/checkbox';
import { UseMouse } from '@vueuse/components';
import { useRoute } from 'vue-router';

const isOpenStypeEditor = ref(false);
const currentSType = ref<SymbolType>({
    id: 0,
    name: '',
    properties: []
});
const isOpenSymbolEditor = ref(false);
const isReadSymbolEditor = ref(true);
const currentSymbol = ref<Symbol>({
    id: 0,
    type: 0,
    title: '',
    value: undefined
});
const doesExists = ref(false);
const isOpenTooltip = ref(false);

let bgUrl = '';

const graphRef = ref<RelationGraphComponent | null>(null);

const graphOptions: RGOptions = {
    layout: {layoutName: 'fixed'},
    allowSwitchLineShape: true,
    allowSwitchJunctionPoint: true,
    defaultJunctionPoint: 'border',
    defaultNodeShape: 0,    //circle
    defaultLineShape: 1,
    defaultNodeColor: 'rgba(0, 206, 209, 1)',
    defaultLineColor: 'rgba(15, 71, 255)'
}

const route = useRoute();
let mapId: number = -1;
if (Array.isArray(route.params.mapId))
    mapId = parseInt(route.params.mapId[0]);
else mapId = parseInt(route.params.mapId);

if (mapId > -1)
{
    api.Connect(mapId)
    .then(() => {bgUrl = api.GetMapImageAddress();});
}
const session = useSessionStore();

// For testing without backend
/*
session.mapData = {
    layers: [
        {content: [], timestampISO: ''}
    ],
    legend: [
        {
            id: 0,
            name: 'Test Type',
            properties: [
                {
                    name: 'Name',
                    type: 'string',
                    isEditable: true,
                    isCombo: false,
                    comboValues: []
                },
                {
                    name: 'Value',
                    type: 'integer',
                    isEditable: true,
                    isCombo: false,
                    comboValues: []
                }
            ]
        }
    ]
};
//*/

onMounted(() => {
    ShowGraph();
});

async function ShowGraph() {
    const __graph_json_data = {
        nodes: [
            // { id: 'a', text: 'Node A' },
            // { id: 'b', text: 'Node B' },
            // { id: 'c', text: 'Node C' },
        ],
        lines: [
            // { from: 'a', to: 'b', text: 'Connection A-B' },
            // { from: 'b', to: 'c', text: 'Connection B-C' },
        ]
    };

    const graphInstance = graphRef.value?.getInstance();
    if (graphInstance) {
        await graphInstance.setJsonData(__graph_json_data);
        await graphInstance.moveToCenter();
        await graphInstance.zoomToFit();
    }
}

function GraphClickNode(node: RGNode, event: RGUserEvent) {
    const symbol = map.GetSymbol(session.mapData!, 0, Number(node.id));
    if (!symbol) return;
    const symbolType = map.GetSymbolType(session.mapData!, symbol.type);
    if (!symbolType) return;

    currentSymbol.value = symbol;
    currentSType.value = symbolType;
    isOpenSymbolEditor.value = true;
}

async function GraphAddNode(symbol: Symbol) {
    const graphInstance = graphRef.value?.getInstance();
    if (graphInstance) {
        const node: JsonNode[] = [
            {
                id: symbol.id.toString(),
                //text: symbol.title,
                width: 20,
                height: 20
            }
        ];
        await graphInstance.addNodes(node);

        // Bind hower tooltip to node
        const htmlElement = document.querySelector(`[data-id="${symbol.id}"]`);
        htmlElement?.addEventListener('mouseover', (event) => {
            currentSymbol.value = symbol;
            isOpenTooltip.value = true;
        });
        htmlElement?.addEventListener('mouseout', (event) => {
            isOpenTooltip.value = false;
        });
    }
}

function RequestCreateSType(stype: SymbolType) {
    if (!session.mapData) {
        console.error('Map is not instantiated');
        return;
    }

    const patch = map.AddSymbolType(session.mapData, stype);
    api.Patch(patch);
}
function RequestUpdateSType(stype: SymbolType) {
    if (!session.mapData) {
        console.error('Map is not instantiated');
        return;
    }

    const patch = map.UpdateSymbolType(session.mapData, stype);
    api.Patch(patch);
}

function CreateSymbol(stype: SymbolType) {
    if (!session.mapData) {
        console.error('Map is not instantiated');
        return;
    }

    const initValue: Map<string, any> = new Map();
    stype.properties.forEach(property => {
        let v: any;
        switch (property.type) {
            case 'string':
                v = '';
                break;
            case 'boolean':
                v = false;
                break;
            case 'integer':
            case 'float':
                v = 0;
                break;
        }

        initValue.set(property.name, v);
    });

    const symbol: Symbol = {id: Date.now(), type: stype.id, title: `${Date.now()}`, value: initValue};
    const patch = map.AddSymbol(session.mapData, 0, symbol);
    GraphAddNode(symbol);
    console.log(patch);
    api.Patch(patch);
}

</script>

<template>
    <Modal :is-open="isOpenStypeEditor">
        <div class="modal-stype-editor">
            <div class="stype-editor-header">
                <Field
                    placeholder="Название обозначения"
                    :value="currentSType?.name"
                    :onChange ="(text: string) => {currentSType.name = text;}"
                />
            </div>

            <div class="stype-editor-properties-list">
                <div class="stype-editor-property-row" v-for="property in currentSType?.properties">
                    <Field
                        placeholder="Название свойства"
                        :onChange="(text: string) => {property.name = text;}"
                        :value="property.name"
                    />
                    <select
                        :value="property.type"
                        :onchange="(e: Event) => {
                            property.type = (e.target as HTMLSelectElement).selectedOptions[0].value as any;
                        }"
                    >
                        <option value="string">Текст</option>
                        <option value="integer">Целое число</option>
                        <option value="float">Вещественное число</option>
                        <option value="boolean">Флаг</option>
                    </select>
                    <Checkbox 
                        :value="property.isEditable"
                        title="Изменяемый"
                        :onChange="(flag: boolean) => {property.isEditable = flag;}"
                    />
                    <div class="editor-property-right">
                        <Button
                            @click="() => {
                                if (!doesExists) {
                                    RequestCreateSType(currentSType);
                                    doesExists = true;
                                }
                                else RequestUpdateSType(currentSType);
                                const index = currentSType.properties.indexOf(property);
                                if (index > -1)
                                    currentSType.properties.splice(index, 1);
                            }"
                        >X</Button>
                    </div>
                </div>
                <Button
                    @click="() => {
                        if (!doesExists) {
                            RequestCreateSType(currentSType);
                            doesExists = true;
                        }
                        else RequestUpdateSType(currentSType);
                        currentSType.properties.push({
                            name: '',
                            type: 'string',
                            isEditable: true,
                            isCombo: false,
                            comboValues: []
                        });
                    }"
                >
                    Добавить свойство
                </Button>
            </div>

            <Button
                @click="() => {
                    if (!doesExists)
                        RequestCreateSType(currentSType);
                    else RequestUpdateSType(currentSType);
                    doesExists = false;
                    isOpenStypeEditor = false;
                }"
                color="good"
            >
                ОК
            </Button>
        </div>
    </Modal>

    <Modal :is-open="isOpenSymbolEditor">
        <div class="modal-symbol-editor">
            <div class="symbol-editor-header">
                <Field
                    v-if="!isReadSymbolEditor"
                    placeholder="Название элемента"
                    :value="currentSymbol?.title"
                    :onChange ="(text: string) => {currentSymbol.title = text;}"
                />
                <Typography tag="h4" v-if="isReadSymbolEditor">{{ currentSymbol.title }}</Typography>

                <div class="editor-property-right">
                    <Button
                        @click="() => {
                            isReadSymbolEditor = !isReadSymbolEditor;
                        }"
                    >E</Button>
                </div>
            </div>

            <div class="symbol-editor-properties-list">
                <div class="symbol-editor-property-row" v-for="property in currentSymbol?.value?.entries()">
                    <Typography tag="p">{{ property[0] }}:</Typography>

                    <Typography tag="p" v-if="isReadSymbolEditor">{{ property[1] }}</Typography>
                    <!-- Different inputs depending on property.type. They should be disabled in non-edit mode -->
                    <Field v-if="!isReadSymbolEditor" />
                </div>
            </div>

            <Button
                @click="() => {
                    if (!doesExists)
                        {/**/}
                    else {/** */}
                    doesExists = false;
                    isOpenSymbolEditor = false;
                }"
                color="good"
            >
                ОК
            </Button>
        </div>
    </Modal>

    <UseMouse v-slot="{ x, y }">
        <div v-if="isOpenTooltip" class="node-tooltip"
            :style="{top: (y + 10) + 'px', left: (x + 10) + 'px'}"
        >
            <div class="symbol-tooltip-header">
                <Typography tag="h4">{{ currentSymbol.title }}</Typography>
            </div>

            <div class="symbol-tooltip-properties-list">
                <div class="symbol-tooltip-property-row" v-for="property in currentSymbol?.value?.entries()">
                    <Typography tag="p">{{ property[0] }}:</Typography>
                    <Typography tag="p">{{ property[1] }}</Typography>
                </div>
            </div>
        </div>
    </UseMouse>

    <div class="legend-block">
        <Typography tag="h4"><u>Легенда</u></Typography>
        <div class="legend-stype-list">
            <div class="legend-stype-row" v-for="stype in session.mapData?.legend">
                <div class="legend-stype-name">
                    <Typography tag="p">{{ stype.name }}</Typography>
                </div>
                <div class="legend-stype-tools">
                    <Button
                        @click="() => {
                            CreateSymbol(stype);
                        }"
                        color="good"
                    >+</Button>
                    <Button
                        @click="() => {
                            currentSType = stype;
                            doesExists = true;
                            isOpenStypeEditor = true;
                        }"
                        color="normal"
                    >E</Button>
                    <Button
                        color="alert"
                    >X</Button>
                </div>
            </div>
        </div>
        <Button
            @click="() => {
                doesExists = false;
                currentSType = {id: Date.now(), name: '', properties: []};
                isOpenStypeEditor = true;
            }"
        >
            Добавить новое обозначение
        </Button>
    </div>

    <div class="root-graph">
        <RelationGraph ref="graphRef" :options="graphOptions" @node-click="GraphClickNode">
            <template #canvas-plug>
                <div class="canvas">
                    <img class="map-image" :src=bgUrl />
                </div>
            </template>
        </RelationGraph>
    </div>
</template>

<style scoped>
.modal-stype-editor, .modal-symbol-editor, .node-tooltip {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8px;
}

.node-tooltip {
    position: absolute;
    z-index: 10;
    border: solid black 2px;
    background-color: white;
}

.stype-editor-properties-list, .symbol-editor-properties-list {
    width: 600px;
    height: 500px;

    display: flex;
    flex-direction: column;
    
    gap: 8px;

    overflow-y: auto;
}

.symbol-tooltip-properties-list {
    display: flex;
    flex-direction: column;
    
    gap: 8px;

    overflow-y: auto;
}

.legend-block {
    z-index: 10;
    position: absolute;
    
    border: solid;
    border-width: 2px;
    background-color: white;

    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 8px;

    top: 50%;
    transform: translateY(-50%);
    left: 16px;
    width: 300px;
    padding: 8px;
}

.legend-stype-list {
    display: flex;
    flex-direction: column;
    width: 100%;
    height: 350px;
    gap: 8px;
    padding: 8px;

    border: solid;
    border-width: 1px;

    overflow-y: auto;
}

.stype-editor-header, .symbol-editor-header, .legend-stype-row, .stype-editor-property-row,
.symbol-editor-property-row, .symbol-tooltip-property-row, .symbol-tooltip-header {
    display: flex;
    flex-direction: row;

    gap: 8px;
}

.legend-stype-tools, .editor-property-right {
    display: flex;
    flex-direction: row;
    gap: 8px;
    margin-left: auto;
}

.legend-stype-name {
    display: flex;
    align-items: center;
}

.root-graph {
    height: calc(100vh);
}

.canvas {
    position: absolute;
}

.map-image {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translateY(-50%) translateX(-50%);
    pointer-events: none;
}
</style>