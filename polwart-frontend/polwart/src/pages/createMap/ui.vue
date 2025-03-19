<script setup lang="ts">
import { StackPanel } from '@/shared/stack-panel';
import { Field } from '@/shared/field';
import { Button } from '@/shared/button';
import { Modal } from '@/widgets/modal';
import { Typography } from '@/shared/typography';
import { Checkbox } from '@/shared/checkbox';
</script>

<template>
    <Modal>
        <StackPanel>
            <Field placeholder='Название' />
            <Checkbox title='Открытый доступ' />
            
            <Button>Создать карту</Button>
        </StackPanel>
    </Modal>
</template>

<style scoped>

</style>