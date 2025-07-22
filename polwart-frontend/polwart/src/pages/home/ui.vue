<script setup lang='ts'>
import { StackPanel } from '@/shared/stack-panel';
import { Field } from '@/shared/field';
import { Button } from '@/shared/button';
import { Modal } from '@/widgets/modal';
import { Typography } from '@/shared/typography';
import { Login } from '@/features/api';
import { ref } from 'vue';
import router from '@/app/router';

</script>

<template>
    <Modal :isOpen="true">
        <StackPanel>
            
            <Button
                @click="() => {
                    router.push({name: 'auth'});
                }"
            >
                Войти в систему
            </Button>
            <Typography tag='h4'>ИЛИ</Typography>
            <Button
                @click="() => {
                    router.push({name: 'edit', params: {mapId: 10}});
                }"
            >
                Открыть демонстрационную карту как Гость
            </Button>
        </StackPanel>
    </Modal>
</template>

<style scoped>
</style>