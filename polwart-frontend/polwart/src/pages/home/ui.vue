<script setup lang='ts'>
import { StackPanel } from '@/shared/stack-panel';
import { Button } from '@/shared/button';
import { Modal } from '@/widgets/modal';
import { Typography } from '@/shared/typography';
import router from '@/app/router';
import { usePersistentStore } from '@/entities/store';

const storage = usePersistentStore();

if (storage.lastMap != -1)
    router.push({name: 'edit', params: {mapId: storage.lastMap}});

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