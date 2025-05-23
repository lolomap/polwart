import { ref, reactive, watch } from 'vue';
import { defineStore } from 'pinia';
import { type User } from '@/entities/user';
import { type Map } from '@/entities/Map/map';
import { type Symbol } from './Map/Legend/symbol';
import * as jsonpatch from 'jsonpatch';

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
            login: '',
            password: ''
        };
        const jwt = ref('');
        const user = reactive(__user__);

        return { user, jwt };
    },
    persist: true
});