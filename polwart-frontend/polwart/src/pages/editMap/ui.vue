<script setup lang="ts">
import { ref, onMounted } from 'vue';
import RelationGraph, { type JsonNode } from 'relation-graph-vue3';
import { RelationGraphComponent, type RGOptions } from 'relation-graph-vue3';
import { Typography } from '@/shared/typography';
import { useSessionStore } from '@/entities/store';
import type { Symbol } from '@/entities/Map/Legend/symbol';

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

const session = useSessionStore();

onMounted(() => {
    showGraph();
});

const showGraph = async() => {
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

const addNode = async(symbol: Symbol) => {
    const graphInstance = graphRef.value?.getInstance();
    if (graphInstance) {
        const node: JsonNode[] = [
            {id: symbol.id.toString(), text: symbol.title}
        ]
        await graphInstance.addNodes(node);
    }
}

</script>

<template>
    <div class="legend-block">
        <Typography tag="h4">Легенда</Typography>
        <button :onclick="() => {addNode({id: Date.now(), type: 0, title: '', value: 0});}">ДОБАВИТЬ</button>
    </div>
    <div class="root-graph">
        <RelationGraph ref="graphRef" :options="graphOptions">
            <template #canvas-plug>
                <div class="canvas">
                    <div class="map-image"></div>
                    
                </div>
            </template>
        </RelationGraph>
    </div>
</template>

<style scoped>
.legend-block {
    z-index: 10;
    position: absolute;
    
    border: solid;
    border-width: 2px;
    background-color: white;

    display: flex;
    flex-direction: column;
    align-items: center;

    top: 50%;
    transform: translateY(-50%);
    width: 300px;
}

.root-graph {
    height: calc(100vh);
}

.canvas {
    position: absolute;
}

.map-image {
    position: absolute;
    background-repeat: no-repeat;
    height: 800px;
    width: 800px;
    background-image: url('/favicon.ico');
}
</style>