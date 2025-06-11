<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import RelationGraph, { type JsonLine, type JsonNode, type RGEventHandler, type RGNode, type RGUserEvent } from 'relation-graph-vue3';
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
import type { RelationGraphFinal } from 'relation-graph-vue3/types/types/relation-graph-models/models/RelationGraphFinal';
import FileUpload, { type FileUploadSelectEvent } from 'primevue/fileupload';
import { Timeline } from 'primevue';
import { DateTimePicker } from '@/shared/datetimepicker';
import type { Layer } from '@/entities/Map/layer';


const isOpenStypeEditor = ref(false);
const currentSType = ref<SymbolType>({
    id: 0,
    name: '',
    properties: [],
    iconFormat: 'png',
    isArea: false,
    areaColor: '#ff0000'
});
const isOpenSymbolEditor = ref(false);
const isReadSymbolEditor = ref(true);
const isDrawArea = ref(false);
const currentSymbol = ref<Symbol>({
    id: 0,
    type: 0,
    title: '',
    value: {},
    x: 0,
    y: 0
});
const blockModalClose = ref(false);
const doesExists = ref(false);
const isOpenTooltip = ref(false);

const isOpenLayerEditor = ref(false);
const currentLayer = ref(0);
const tempTimestampISO = ref('');
const timelineZoom = ref(0);

let bgUrl = ref('');
const bgWidth = ref(0);
const bgHeight = ref(0);
const iconsRefresherKey = ref(0);
const forceRerenderIcons = () => {
    iconsRefresherKey.value += 1;
}

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
let graphInstance: RelationGraphFinal | undefined;

const route = useRoute();
let mapId: number = -1;
if (Array.isArray(route.params.mapId))
    mapId = parseInt(route.params.mapId[0]);
else mapId = parseInt(route.params.mapId);

//*
if (mapId > -1)
{
    api.Connect(mapId)
    .then((map: any) => {
        bgUrl.value = api.GetMapImageAddress() + '.' + map.backgroundFormat;

        currentLayer.value = (session.mapData?.layers.length ?? 1) - 1;
        // Load every symbol on layer
        OnLoadMapAllSymbols();
        watch(currentLayer, async (newLayer, oldLayer) => {
            graphInstance?.clearGraph();
            OnLoadMapAllSymbols();
        });

        // Subscribe GUI on layer changes
        api.Events.addEventListener('symbolUpdated', (e) => {OnLoadMapSymbol((e as CustomEvent).detail)});
    });
}
//*/
const session = useSessionStore();

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

    graphInstance = graphRef.value?.getInstance();
    if (graphInstance) {
        await graphInstance.setJsonData(__graph_json_data);
        await graphInstance.moveToCenter();
        await graphInstance.zoomToFit();
    }
}

function OnLoadMapAllSymbols() {
    for (const [index, symbol] of session.mapData?.layers[currentLayer.value].content.entries() ?? []) {
        OnLoadMapSymbol({
            layer: currentLayer.value,
            index: index,
            value: symbol,
            type: 'add'
        });
    };
}

function OnLoadMapSymbol(event: api.UpdateEvent) {
    if (currentLayer.value != event.layer) return;
    switch (event.type) {
        case 'x':
        case 'y':
        {
            let symbol: Symbol | undefined = session.mapData?.layers[event.layer].content[event.index];
            if (!symbol) return;

            let node: RGNode | undefined = graphInstance?.getNodeById(symbol.id.toString());
            if (!node) return;

            if (event.type == "x")
                node.x = event.value;
            else node.y = event.value;

            break;
        }
        case 'connection':
        {
            console.log('connection');
            const symbol: Symbol | undefined = session.mapData?.layers[event.layer].content[event.index];
            if (!symbol) return;
            const symbolType = map.GetSymbolType(session.mapData!, symbol.type);
            if (!symbolType) return;
            const node: RGNode | undefined = graphInstance?.getNodeById(symbol.id.toString());
            if (!node) return;

            const line: JsonLine[] = [
                {
                    from: symbol.value['#connection'].toString(),
                    to: symbol.id.toString(),
                    color: symbolType.areaColor,
                    isHideArrow: true
                }
            ];

            // Remove old connection before draw new one
            graphInstance?.removeLinkByTwoNode(symbol.value['#connection'].toString(), symbol.id.toString());
            
            graphInstance?.addLines(line);

            break;
        }
        case 'add':
        {    
            const symbol: Symbol | undefined = session.mapData?.layers[event.layer].content[event.index];
            if (!symbol) return;

            GraphAddNode(symbol);
            break;
        }
        case 'remove':
        {
            console.log(event);
            GraphRemoveNode(event.value);
            break;
        }
    }
}

function GetSymbolImageAdress(id: number): string {
    const symbol = map.GetSymbol(session.mapData!, currentLayer.value, Number(id));
    if (!symbol) return '#';
    const symbolType = map.GetSymbolType(session.mapData!, symbol.type);
    if (!symbolType) return '#';
    return api.GetSTypeImageAddress(symbolType);
}

function GraphGetPolygon(root: RGNode): string {
    const points = [];
    
    points.push([root.x + bgWidth.value / 2 + 8, root.y + bgHeight.value / 2 + 8]);
    let node = root.targetTo[0];

    while (node && node.id != root.id) {
        points.push([node.x + bgWidth.value / 2 + 8, node.y + bgHeight.value / 2 + 8]);

        node = node.targetTo[0];
    }

    let res = '';
    points.forEach(x => {
        res += `${x[0]}px ${x[1]}px, `
    });

    res = res.substring(0, res.length - 2);
    return `polygon(${res})`;
}

function GraphClickCanvas($event: RGUserEvent) {
    if (!isDrawArea.value) return;
    
    let x = ($event as MouseEvent).offsetX - bgWidth.value / 2;
    let y = ($event as MouseEvent).offsetY - bgHeight.value / 2;

    CreateSymbol(currentSType.value, x, y, currentSymbol?.value?.id ?? -1);
}

function GraphClickNode(node: RGNode, event: RGUserEvent) {
    const symbol = map.GetSymbol(session.mapData!, currentLayer.value, Number(node.id));
    if (!symbol) return;
    const symbolType = map.GetSymbolType(session.mapData!, symbol.type);
    if (!symbolType) return;

    console.log(currentSymbol.value);
    console.log(symbol);

    if (isDrawArea.value)
    {
        symbol.value['#connection'] = currentSymbol?.value?.id ?? -1;
        isDrawArea.value = false;
        RequestUpdateSymbolConnection(currentLayer.value, symbol);

        return;
    }

    currentSymbol.value = symbol;
    currentSType.value = symbolType;
    isOpenSymbolEditor.value = true;
}

function GraphNodeOver(node: RGNode, $event: any) {
    if (isDrawArea) return;

    const symbol = map.GetSymbol(session.mapData!, currentLayer.value, Number(node.id));
    if (!symbol) return;
    const symbolType = map.GetSymbolType(session.mapData!, symbol.type);
    if (!symbolType) return;

    currentSymbol.value = symbol;
    currentSType.value = symbolType;
    isOpenTooltip.value = true;
}

function GraphNodeOut(node: RGNode, $event: any) {
    isOpenTooltip.value = false;
}

async function GraphAddNode(symbol: Symbol) {
    const symbolType = map.GetSymbolType(session.mapData!, symbol.type);
    if (!symbolType) return;

    graphInstance = graphRef.value?.getInstance();
    if (graphInstance) {
        const node: JsonNode[] = [
            {
                id: symbol.id.toString(),
                //text: symbol.title,
                width: 1,//symbolType.isArea ? 15 : 50,
                height: 1,//symbolType.isArea ? 15 : 50,
                color: 'transparent',
                borderColor: 'transparent',
                fixed: true,
                x: symbol.x,
                y: symbol.y,
                data: {
                    isArea: symbolType.isArea,
                    color: symbolType.areaColor,
                    isRoot: false
                }
            }
        ];

        await graphInstance.addNodes(node);

        if (symbol.value['#connection'] && symbol.value['#connection'] != -1)
        {
            const line: JsonLine[] = [
                {
                    from: symbol.value['#connection'].toString(),
                    to: symbol.id.toString(),
                    color: symbolType.areaColor,
                    isHideArrow: true
                }
            ];
            graphInstance.addLines(line);
            
            // Mark area's root symbol
            const prevNode = graphInstance.getNodeById(symbol.value['#connection'].toString());
            if (prevNode.targetFrom.length == 0 && prevNode.data)
                prevNode.data.isRoot = true
        }
    }
}

async function GraphRemoveNode(id: number) {
    graphInstance = graphRef.value?.getInstance();
    if (graphInstance) {
        await graphInstance.removeNodeById(id.toString());
    }
}

function GraphDragEndNode(node: RGNode, event: RGUserEvent) {
    const symbol = map.GetSymbol(session.mapData!, currentLayer.value, Number(node.id));
    if (!symbol) return;

    symbol.x = node.x;
    symbol.y = node.y;
    RequestUpdateSymbolPos(currentLayer.value, symbol);
}

function RequestCreateLayer() {
    const patch = map.AddLayer(session.mapData!, tempTimestampISO.value, session.mapData?.layers[currentLayer.value].content ?? []);
    api.Patch(patch);
}

function RequestCreateSType(stype: SymbolType) {
    if (!session.mapData) {
        console.error('Map is not instantiated');
        return;
    }

    const patch = map.AddSymbolType(session.mapData, stype);
    api.Patch(patch);
}
function RequestRemoveSType(stype: SymbolType) {
    if (!session.mapData) {
        console.error('Map is not instantiated');
        return;
    }

    const patch = map.RemoveSymbolType(session.mapData, stype);
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
function RequestRemoveSymbol(layer: number, s: Symbol) {
    if (!session.mapData) {
        console.error('Map is not instantiated');
        return;
    }

    const patch = map.RemoveSymbol(session.mapData, layer, s);
    api.Patch(patch);
}
function RequestUpdateSymbol(layer: number, s: Symbol) {
    if (!session.mapData) {
        console.error('Map is not instantiated');
        return;
    }

    const patch = map.UpdateSymbol(session.mapData, layer, s);
    api.Patch(patch);
}
function RequestUpdateSymbolPos(layer: number, s: Symbol) {
    if (!session.mapData) {
        console.error('Map is not instantiated');
        return;
    }

    const patch = map.UpdateSymbolPos(session.mapData, layer, s);
    api.Patch(patch);
}
function RequestUpdateSymbolConnection(layer: number, s: Symbol) {
    if (!session.mapData) {
        console.error('Map is not instantiated');
        return;
    }

    const patch = map.UpdateSymbolConnection(session.mapData, layer, s);
    api.Patch(patch);
}

function CreateSymbol(stype: SymbolType, x: number = 0, y: number = 0, connection: number = -1) {
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

    initValue.set('#connection', connection);

    const symbol: Symbol = {id: Date.now(), type: stype.id, title: stype.name, value: Object.fromEntries(initValue), x: x, y: y};
    const patch = map.AddSymbol(session.mapData, currentLayer.value, symbol);
    //GraphAddNode(symbol);
    api.Patch(patch);
    currentSymbol.value = symbol;
}

</script>

<template>
    <Modal :is-open="isOpenStypeEditor">
        <div class="modal-stype-editor">
            <div class="stype-editor-header">
                <input
                    v-if="currentSType.isArea"
                    type="color"
                    :value="currentSType?.areaColor"
                    @input="(input) => {
                        if(input.target)
                            currentSType.areaColor = (input.target as HTMLInputElement).value;
                    }"
                />
                <FileUpload
                    v-else
                    accept="image/*"
                    :multiple="false"
                    :maxFileSize="15 * 1000000"
                    @select="async (e: FileUploadSelectEvent) => {
                        let icon: File;
                        if (Array.isArray(e.files))
                            icon = e.files[e.files.length - 1];
                        else icon = e.files;

                        let extension: string = (/(?:\.([^.]+))?$/.exec(icon.name) ?? [])[1];
                            currentSType.iconFormat = extension;
                        blockModalClose = true;
                        await api.MediaUpload(`SType_${mapId}_${currentSType.id}.${extension}`, icon);
                        blockModalClose = false;
                    }"
                    :auto="false"
                >
                    <template #header="{ chooseCallback, uploadCallback, clearCallback, files }">
                        <div class="icon-uploader-header">
                            <img v-if="files.length < 1" class="stype-icon"
                                src="/add_photo.svg"
                                @click="chooseCallback()" />
                            <img v-if="files.length > 0" class="stype-icon"
                                :src="files[files.length - 1].objectURL"
                                @click="chooseCallback()" 
                            />
                        </div>
                    </template>
                    <template #content><div></div></template>
                    <template #empty>
                        <div>
                            
                        </div>
                    </template>
                </FileUpload>
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
                :disabled="blockModalClose"
                @click="() => {
                    if (!doesExists)
                        RequestCreateSType(currentSType);
                    else RequestUpdateSType(currentSType);
                    doesExists = false;
                    isOpenStypeEditor = false;
                    forceRerenderIcons();
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
                    <Button
                        color="alert"
                        @click="() => {
                            RequestRemoveSymbol(currentLayer, currentSymbol);
                            isOpenSymbolEditor = false;
                        }"
                    >X</Button>
                </div>
            </div>

            <div class="symbol-editor-properties-list">
                <div class="symbol-editor-property-row" v-for="property in currentSType.properties">
                    <Typography tag="p">{{ property.name }}:</Typography>

                    <Typography tag="p"
                        v-if="(isReadSymbolEditor || !property.isEditable) && property?.type != 'boolean'">
                        {{ currentSymbol.value[property.name] }}
                    </Typography>
                    <Typography tag="p"
                        v-if="(isReadSymbolEditor || !property.isEditable) && property?.type == 'boolean'">
                        {{ currentSymbol.value[property.name] ? '✅' : '❌' }}
                    </Typography>
                    <!-- Different inputs depending on property.type. They should be disabled in non-edit mode -->
                    <Field v-if="(!isReadSymbolEditor && property.isEditable) && property?.type == 'string'"
                        :onChange="(text: string) => {currentSymbol.value[property.name] = text;}"
                        :value="currentSymbol.value[property.name]"
                    />
                    <Field v-else-if="(!isReadSymbolEditor && property.isEditable) && property?.type == 'integer'"
                        :isNumber="true"
                        :onChange="(text: string) => {currentSymbol.value[property.name] = Number.parseInt(text);}"
                        :value="currentSymbol.value[property.name]"
                    />
                    <Field v-else-if="(!isReadSymbolEditor && property.isEditable) && property?.type == 'float'"
                        :isNumber="true"
                        :onChange="(text: string) => {currentSymbol.value[property.name] = Number.parseFloat(text);}"
                        :value="currentSymbol.value[property.name]"
                    />
                    <Checkbox v-else-if="(!isReadSymbolEditor && property.isEditable) && property?.type == 'boolean'"
                        :onChange="(checked: boolean) => {currentSymbol.value[property.name] = checked;}"
                        :value="currentSymbol.value[property.name]"
                    />
                </div>
            </div>

            <Button
                @click="() => {
                    RequestUpdateSymbol(currentLayer, currentSymbol);
                    isOpenSymbolEditor = false;
                }"
                color="good"
            >
                ОК
            </Button>
        </div>
    </Modal>

    <Modal :is-open="isOpenLayerEditor">
        <Typography tag="h4">Позиция фиксации на таймлайне:</Typography>
        <DateTimePicker :onChange="(value: string) => {tempTimestampISO = value;}" />
        <Button
            @click="() => {
                RequestCreateLayer();
                isOpenLayerEditor = false;
            }"
            color="good"
        >
            ОК
        </Button>
    </Modal>

    <UseMouse v-slot="{ x, y }">
        <div v-if="isOpenTooltip" class="node-tooltip"
            :style="{top: (y + 10) + 'px', left: (x + 10) + 'px'}"
        >
            <div class="symbol-tooltip-header">
                <Typography tag="h4">{{ currentSymbol.title }}</Typography>
            </div>

            <div class="symbol-tooltip-properties-list">
                <div class="symbol-tooltip-property-row" v-for="property in currentSType.properties">
                    <Typography tag="p">{{ property.name }}:</Typography>
                    <Typography tag="p"
                        v-if="property?.type != 'boolean'"
                    >
                        {{ currentSymbol.value[property.name] }}
                    </Typography>
                    <Typography tag="p"
                        v-if="property?.type == 'boolean'"
                    >
                        {{ currentSymbol.value[property.name] ? '✅' : '❌' }}
                    </Typography>
                </div>
            </div>
        </div>
    </UseMouse>

    <div class="legend-block">
        <Typography tag="h4"><u>Легенда</u></Typography>
        <div class="legend-stype-list">
            <div class="legend-stype-row" v-for="stype in session.mapData?.legend" :key="stype.id + iconsRefresherKey">
                <div class="legend-stype-name">
                    <img class="stype-icon"
                        v-if="!stype.isArea"
                        :src="api.GetSTypeImageAddress(stype)"
                        @error="($event.target! as HTMLImageElement).src='/add_photo.svg'"
                        @click="() => {
                            currentSType = stype;
                            doesExists = true;
                            isOpenStypeEditor = true;
                        }"
                    />
                    <div
                        v-else
                        class="stype-color"
                        :style="{backgroundColor: stype.areaColor}"
                    >
                    </div>

                    <Typography tag="p">{{ stype.name }}</Typography>
                </div>
                <div class="legend-stype-tools">
                    <Button
                        v-if="!stype.isArea"
                        @click="() => {
                            CreateSymbol(stype);
                        }"
                        color="good"
                    >+</Button>
                    <Button
                        v-else
                        @click="() => {
                            isDrawArea = true;
                            currentSymbol = {...currentSymbol, id: -1};
                            currentSType = stype;
                        }"
                        color="good"
                    >D</Button>

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
                        @click="() => {
                            RequestRemoveSType(stype);
                        }"
                    >X</Button>
                </div>
            </div>
        </div>
        <Button
            @click="() => {
                doesExists = false;
                currentSType = {id: Date.now(), name: '', properties: [], iconFormat: 'png', isArea: false, areaColor: '#ff0000'};
                isOpenStypeEditor = true;
            }"
        >
            Добавить обозначение (объект)
        </Button>
        <Button
            @click="() => {
                doesExists = false;
                currentSType = {id: Date.now(), name: '', properties: [], iconFormat: 'png', isArea: true, areaColor: '#ff0000'};
                isOpenStypeEditor = true;
            }"
        >
            Добавить обозначение (область)
        </Button>
    </div>

    <div class="timeline-block">
        <div class="timeline-header">
            <div class="timeline-options">
                <Button
                    :color="timelineZoom != 0 ? 'normal' : 'default'"
                    @click="() => {
                        timelineZoom = 0;
                    }"
                >
                    Год
                </Button>
                <Button
                    :color="timelineZoom != 1 ? 'normal' : 'default'"
                    @click="() => {
                        timelineZoom = 1;
                    }"
                >
                    Месяц
                </Button>
                <Button
                    :color="timelineZoom != 2 ? 'normal' : 'default'"
                    @click="() => {
                        timelineZoom = 2;
                    }"
                >
                    День
                </Button>
                <Button
                    :color="timelineZoom != 3 ? 'normal' : 'default'"
                    @click="() => {
                        timelineZoom = 3;
                    }"
                >
                    Час
                </Button>
                <Button
                    :color="timelineZoom != 4 ? 'normal' : 'default'"
                    @click="() => {
                        timelineZoom = 4;
                    }"
                >
                    Минута
                </Button>

                <Button
                    class="timeline-add-button"
                    color="good"
                    @click="() => {
                        isOpenLayerEditor = true;
                    }"
                >
                    +
                </Button>
            </div>
            <Typography class="timeline-current" tag="p">
                {{ new Date(session.mapData?.layers[currentLayer].timestampISO ?? '').toString() }}
            </Typography>
        </div>
        <div class="timeline-content">
            <Timeline
                :value="session.mapData?.layers.sort((a, b) => {
                    const dA = new Date(a.timestampISO);
                    const dB = new Date(b.timestampISO);

                    if (dA < dB)
                        return -1;
                    if (dA > dB)
                        return 1;

                    return 0;
                })"
                layout="horizontal"
                align="bottom"
            >
                <template #content="slotProps">
                    <Typography tag="p" v-if="timelineZoom == 0">
                        {{ new Date((slotProps.item as Layer).timestampISO).getFullYear() }}
                    </Typography>
                    <Typography tag="p" v-else-if="timelineZoom == 1">
                        {{ new Date((slotProps.item as Layer).timestampISO).getMonth() }}
                    </Typography>
                    <Typography tag="p" v-else-if="timelineZoom == 2">
                        {{ new Date((slotProps.item as Layer).timestampISO).getDate() }}
                    </Typography>
                    <Typography tag="p" v-else-if="timelineZoom == 3">
                        {{ new Date((slotProps.item as Layer).timestampISO).getHours() }}
                    </Typography>
                    <Typography tag="p" v-else-if="timelineZoom == 4">
                        {{ new Date((slotProps.item as Layer).timestampISO).getMinutes() }}
                    </Typography>
                </template>
                <template #marker="slotMarker">
                    <div>
                        <Button
                            class="timeline-marker"
                            :color="(slotMarker.item as Layer).timestampISO == session.mapData?.layers[currentLayer].timestampISO
                                ? 'great' : 'default'"
                            @click="() => {
                                currentLayer = session.mapData?.layers.findIndex(
                                    x => x.timestampISO == (slotMarker.item as Layer).timestampISO
                                ) ?? 0;
                            }"
                        >
                        </Button>
                    </div>
                </template>
            </Timeline>
        </div>
    </div>


    <div class="root-graph">
        <RelationGraph ref="graphRef"
            :options="graphOptions" 
            @node-click="GraphClickNode"
            @node-drag-end="GraphDragEndNode"
            @canvas-click="GraphClickCanvas"
        >
            <template #node="{node}">
                <div
                    class="symbol-node"
                    @mouseover="GraphNodeOver(node, $event)"
                    @mouseout="GraphNodeOut(node, $event)"
                >
                    <img
                        v-if="!(node as RGNode).data?.isArea"
                        :src="`${GetSymbolImageAdress(Number((node as RGNode).id))}`"
                        class="symbol-icon"
                        :key="(node as RGNode).id + iconsRefresherKey"
                        :width="`${(node as RGNode).width}px`"
                        :height="`${(node as RGNode).height}px`"
                    />
                    <div
                        v-else
                        :style="{backgroundColor: (node as RGNode).data?.color}"
                        class="symbol-color"
                    >
                    </div>
                </div>
            </template>
            <template #canvas-plug>
                <div class="canvas">
                    <img class="map-image" :src=bgUrl
                        @load="($event) => {
                            bgWidth = ($event.target! as HTMLImageElement).width;
                            bgHeight = ($event.target! as HTMLImageElement).height;
                            console.log(bgWidth);
                        }"
                    />
                    <div v-if="isDrawArea" class="map-areas-canvas"
                        :style="{width: bgWidth + 'px', height: bgHeight + 'px'}"
                    >

                    </div>

                    <div v-for="node in graphInstance?.getNodes().filter(x => x.data?.isRoot)"
                        class="symbol-area"
                        :style="{
                            backgroundColor: (node as RGNode).data?.color,
                            width: bgWidth + 'px',
                            height: bgHeight + 'px',
                            clipPath: GraphGetPolygon(node)
                        }"    
                    >
                        
                    </div>
                </div>
            </template>
        </RelationGraph>
    </div>
</template>

<style>
.p-fileupload-header {
    border: none !important;
}

.symbol-node {
    transform: translate(-50%, -50%);
    width: fit-content;
    height: fit-content;
}

.symbol-icon {
    width: 50px;
    height: 50px;
    pointer-events: none;
}

.symbol-color {
    width: 15px;
    height: 15px;
    border-radius: 15px;
    pointer-events: none;
}

:root {
    --p-content-border-color: black;
    --p-timeline-event-connector-color: var(--p-content-border-color);
    --p-timeline-event-connector-size: 2px;
}
</style>

<style scoped>
.timeline-block {
    z-index: 10;
    position: absolute;
    
    border: solid;
    border-width: 2px;
    background-color: white;
    border-radius: 32px;

    display: flex;
    flex-direction: column;
    gap: 8px;

    top: 98%;
    transform: translateY(-100%);
    width: 100%;
    padding: 16px;
}

.timeline-header {
    display: flex;
    flex-direction: row;
    align-items: center;
}

.timeline-current {
    margin-left: auto;
}

.timeline-add-button {
    width: fit-content;
}

.timeline-marker {
    border-radius: 35px;
    height: 32px;
    width: 32px;
}

.timeline-options {
    display: flex;
    flex-direction: row;
    gap: 8px;
    width: fit-content;
}

.timeline-content {
    display: flex;
    flex-direction: row;
    gap: 8px;
    width: 100%;
}

.stype-icon {
    width: 42px;
    height: 42px;
    cursor: pointer;
}

.stype-color {
    width: 42px;
    height: 42px;
    border-radius: 42px;
}

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
    gap: 8px;
}

.root-graph {
    height: calc(100vh);
}

.canvas {
    position: absolute;
}

.map-image {
    position: absolute;
    transform: translateY(-50%) translateX(-50%);
    pointer-events: none;
}

.map-areas-canvas {
    position: absolute;
    background-color: rgb(104, 104, 104);
    opacity: 0.5;
    transform: translateY(-50%) translateX(-50%);
}

.symbol-area {
    position: absolute;
    transform: translateY(-50%) translateX(-50%);
    opacity: 0.3;
    pointer-events: none;
}

</style>