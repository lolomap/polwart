<script setup lang="ts">
import { Content } from '@/widgets/content';
import { Header } from '@/widgets/header';
import { Typography } from '@/shared/typography';
import { Field } from '@/shared/field';
import { Button } from '@/shared/button';
import { useEntityStore } from '@/entities';
import router from '@/app/router';
import type { UserModel } from '@/entities/user';
import { onMounted, ref } from 'vue';

const entityStore = useEntityStore();

let login_text = '';
let pass_text = ''

const onChangeAuth = (value: string) => login_text = value;
const onChangePass = (value: string) => pass_text = value;
const onSubmit = () => {
    const requestOptions = {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify(
            {
                "login": login_text,
                "password": pass_text
            }
        )
    };

    let user: UserModel = {
        id: 0,
        name: ''
    };
    fetch('https://localhost:7126/User/Auth', requestOptions)
        .then(response => response.json())
        .then(data => {
            user.id = data['user']['id'];
            user.name = data['user']['name'];
            console.log(user.name);

            entityStore.user = user;
            //router.push('/grades');
        });
};
</script>

<template>
    <Header />
    <div class="main__content">
        <Content>
            <div class="main__content_box">
                <Typography tag="h4">Login:</Typography>
                <Field ref="login_input"
                    class="auth_field" 
                    placeholder="login" 
                    :onChange="onChangeAuth"
                />

                <Typography tag="h4">Password</Typography>
                <Field ref="pass_input"
                    class="auth_field"
                    placeholder="password"
                    isSecret
                    :onChange="onChangePass"
                    :onEnter="onSubmit"
                />

                <Button 
                    class="auth_button" 
                    color="alert"
                    @click="onSubmit"
                >Авторизоваться</Button>
            </div>
        </Content>
    </div>
</template>

<style scoped>

.main__content_box {
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-direction: column;
    gap: 25px;
    color: var(--main-on-default)
}

.auth_field {
    width: 60%;
}

.auth_button {
    width: 250px;
}

</style>