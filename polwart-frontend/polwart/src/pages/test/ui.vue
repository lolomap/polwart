<script setup lang="ts">
import type { SymbolType } from '@/entities/Map/Legend/symbol-type';
import { useSessionStore } from '@/entities/store';
import * as map from '@/entities/Map/map';
import { type Map } from '@/entities/Map/map';

const session = useSessionStore();

const connect = () => {
    fetch('https://localhost:7238/map/connect', {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify(
            {
                'mapId': 123
            }
        )
    });
};

let patch = () => {
    fetch('https://localhost:7238/map/patch', {
        method: 'PATCH',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify(
            {
                'mapId': 123,
                'timestamp': (new Date()).getTime(),
                'patch': JSON.stringify(
                    [
                        {
                            "op": "add",
                            "path": "/legend/0",
                            "value": {
                                "id": 0,
                                "type": "float"
                            }
                        },
                        {
                            "op": "add",
                            "path": "/layers/0/content/0",
                            "value": {
                                "id": 0,
                                "type": 0,
                                "title": "test",
                                "value": 123.4
                            }
                        }
                    ]
                )
            }
        )
    })
    .then(response => response.json())
    .then(data => console.log(data));
};

</script>

<template>
    <div>
        <button :onclick="connect">CONNECT MAP</button>
        <button :onclick="patch">PATCH MAP</button>
    </div>
</template>

<style scoped>

</style>