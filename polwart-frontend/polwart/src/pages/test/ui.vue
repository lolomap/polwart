<script setup lang="ts">
import * as api from '@/features/api';

let connect = () => {api.Connect(123);}
let patch = () => {
    api.Patch(
        JSON.stringify(
            [
                {
                    op: "add",
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
    );
}

</script>

<template>
    <div>
        <button :onclick="connect">CONNECT MAP</button>
        <button :onclick="patch">PATCH MAP</button>
    </div>
</template>

<style scoped>

</style>