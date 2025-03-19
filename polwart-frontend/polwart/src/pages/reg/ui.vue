<script setup lang="ts">
import { StackPanel } from '@/shared/stack-panel';
import { Field } from '@/shared/field';
import { Button } from '@/shared/button';
import { Modal } from '@/widgets/modal';
import { Typography } from '@/shared/typography';
</script>

<template>
    <Modal>
        <StackPanel>
            <Field placeholder="Логин" />
            <Field placeholder="Пароль"
                :isSecret="true"
            />
            <Field placeholder="Подтверждение"
                :isSecret="true"
            />
            
            <Button>Зарегистрироваться</Button>
            <Typography tag="h4">ИЛИ</Typography>
            <a href="/auth"><Typography tag="p">Войти в учетную запись</Typography></a>
        </StackPanel>
    </Modal>
</template>

<style scoped>
</style>