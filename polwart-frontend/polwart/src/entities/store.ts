import { ref, reactive, watch } from 'vue';
import { defineStore } from 'pinia';
import { type User } from '@/entities/user';
import { type Map } from '@/entities/Map/map';
import { type Symbol } from './Map/Legend/symbol';
import * as jsonpatch from 'jsonpatch';
import type { ClientState, MapState } from './client-state';

export const useSessionStore = defineStore('session', {
    state: () => {
        return {
            mapData: null as Map | null
        }
    },

    actions: {
        patch(patchData: string) {
            this.mapData = jsonpatch.apply_patch(this.mapData, patchData);
        }
    }
});

// Use this store for persistent data like User credentials
export const usePersistentStore = defineStore('persistent', {
    state: () => {
        const __user__: User = {
            id: -1,
            login: ''
        };

        const __client_state__: ClientState = {};

        const jwt = ref('');
        const user = reactive(__user__);
        const lastMap = ref(-1);
        const clientState = reactive(__client_state__);

        return { user, jwt, lastMap, clientState };
    },
    persist: true
});