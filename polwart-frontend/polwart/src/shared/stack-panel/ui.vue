<script setup lang="ts">

</script>

<template>
    <div class="stack-panel">
        <slot></slot>
    </div>
</template>

<style scoped>

.stack-panel {
    margin: 0 auto;

    display: flex;
    flex-direction: column;

    align-items: center;
    justify-content: center;

    gap: 20px;
}

</style>