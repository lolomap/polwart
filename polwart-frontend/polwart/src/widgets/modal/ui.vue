<script setup lang="ts">
import { computed } from 'vue';

interface Props {
    isOpen?: boolean;
}

const props = defineProps<Props>();
const { 
    isOpen = false
} = props;

const rIsOpen = computed(() => props.isOpen);
</script>

<template>
    <div :class="[`modal_${rIsOpen ? 'open' : 'closed'}`]">
        <div class="modal-inner">
            <slot></slot>
        </div>
    </div>
</template>

<style scoped>

.modal_closed {
    display: none;
}

.modal_open {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    z-index: 99;

    display: flex;
    align-items: center;
    justify-content: center;

    background-color: rgba(0, 0, 0, 0.2);
    backdrop-filter: blur(3px);
}

.modal-inner {
    background-color: white;
    padding: 32px;
    border-radius: 16px;

    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;

    gap: 16px;
}

</style>