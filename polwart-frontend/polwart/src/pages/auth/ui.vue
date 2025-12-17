<script setup lang='ts'>
import { StackPanel } from '@/shared/stack-panel';
import { Field } from '@/shared/field';
import { Button } from '@/shared/button';
import { Modal } from '@/widgets/modal';
import { Typography } from '@/shared/typography';
import { Login } from '@/features/api';
import { ref } from 'vue';
import router from '@/app/router';

const login = ref('');
const pass = ref('');

const regUrl = import.meta.env.BASE_URL + 'reg';
</script>

<template>
    <Modal :isOpen="true">
        <StackPanel>
            <Field placeholder='Логин' @change="(text:string) => {login = text;}"/>
            <Field placeholder='Пароль'
                @change="(text:string) => {pass = text;}"
                :isSecret='true'
            />
            
            <Button
                @click="async () => {
                    const status = await Login(login, pass);
                    if (status)
                        router.push({name: 'create'});
                }"
            >
                Войти
            </Button>
            <Typography tag='h4'>ИЛИ</Typography>
            <a :href="regUrl"><Typography tag='p'>Создать учетную запись</Typography></a>
        </StackPanel>
    </Modal>
</template>

<style scoped>
</style>