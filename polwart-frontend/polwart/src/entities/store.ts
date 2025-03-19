import { ref, reactive, watch } from 'vue';
import { defineStore } from 'pinia';
import { type User } from '@/entities/user';

export const useEntityStore = defineStore('entity', {
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