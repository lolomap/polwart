import { ref, reactive, watch } from 'vue';
import { defineStore } from 'pinia';
import { type User } from '@/entities/user';
import { type Map } from '@/entities/Map/map';
import * as jsonpatch from 'jsonpatch';

export const useSessionStore = defineStore('session', {
    state: () => {
        return {
            mapData: null as Map | null
        }
    },

    actions: {
        patch(patchData: jsonpatch.JSONPatch[]) {
            this.mapData = jsonpatch.apply_patch(this.mapData, patchData);
        }
    }
});

// Use this store for persistent data like User credentials
export const usePersistentStore = defineStore('persistent', {
    state: () => {
        const __user__: User = {
            id: -1,
            name: 'Blank',
            login: '',
            password: ''
        };
        const user = reactive(__user__);

        return { user, };
    },
    persist: true
});