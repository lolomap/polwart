import { ref, reactive, watch } from 'vue';
import { defineStore } from 'pinia';
import { type UserModel } from '@/entities/user';

export const useEntityStore = defineStore('entity', {
    state: () => {
        const __user__: UserModel = {
            id: -1,
            name: 'Blank'
        };
        const user = reactive(__user__);

        return { user, };
    },
    persist: true
});