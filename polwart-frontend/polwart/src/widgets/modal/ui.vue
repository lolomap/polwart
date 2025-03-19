<script setup lang="ts">
import { Field } from '@/shared/field';
import { StackPanel } from '@/shared/stack-panel';
import { Button } from '@/shared/button';

</script>

<template>
    <div class="modal">
        <div class="modal-inner">
            <slot></slot>
        </div>
    </div>
</template>

<style scoped>

.modal {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    z-index: 99;

    background-color: rgba(0, 0, 0, 0.2);

    display: flex;
    align-items: center;
    justify-content: center;
}

.modal-inner {
    background-color: white;
    padding: 32px;
    border-radius: 16px;
}

</style>