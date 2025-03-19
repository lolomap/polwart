<script setup lang='ts'>
import { StackPanel } from '@/shared/stack-panel';
import { Field } from '@/shared/field';
import { Button } from '@/shared/button';
import { Modal } from '@/widgets/modal';
import { Typography } from '@/shared/typography';
</script>

<template>
    <Modal>
        <StackPanel>
            <Field placeholder='Логин' />
            <Field placeholder='Пароль'
                :isSecret='true'
            />
            
            <Button>Войти</Button>
            <Typography tag='h4'>ИЛИ</Typography>
            <a href='/reg'><Typography tag='p'>Создать учетную запись</Typography></a>
        </StackPanel>
    </Modal>
</template>

<style scoped>
</style>