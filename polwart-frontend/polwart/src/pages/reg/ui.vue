<script setup lang="ts">
import { StackPanel } from '@/shared/stack-panel';
import { Field } from '@/shared/field';
import { Button } from '@/shared/button';
import { Modal } from '@/widgets/modal';
import { Typography } from '@/shared/typography';
import { ref } from 'vue';
import { Register } from '@/features/api';
import router from '@/app/router';

const login = ref('');
const pass = ref('');
const passConfirm = ref('');
</script>

<template>
    <Modal :isOpen="true">
        <StackPanel>
            <Field placeholder="Логин" @change="(text:string) => {login = text;}" />
            <Field placeholder="Пароль"
                @change="(text:string) => {pass = text;}"
                :isSecret="true"
            />
            <Field placeholder="Подтверждение"
                @change="(text:string) => {passConfirm = text;}"
                :isSecret="true"
            />
            
            <Button
                :disabled="login == '' || pass == '' || pass != passConfirm"
                @click="async () => {
                    const status = await Register(login, pass);
                    if (status)
                        router.push({name: 'create'});
                }"
            >
                Зарегистрироваться
            </Button>
            <Typography tag="h4">ИЛИ</Typography>
            <a href="/auth"><Typography tag="p">Войти в учетную запись</Typography></a>
        </StackPanel>
    </Modal>
</template>

<style scoped>
</style>