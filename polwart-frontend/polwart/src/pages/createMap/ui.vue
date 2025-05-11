<script setup lang="ts">
import { StackPanel } from '@/shared/stack-panel';
import { Field } from '@/shared/field';
import { Button } from '@/shared/button';
import { Modal } from '@/widgets/modal';
import { Typography } from '@/shared/typography';
import { Checkbox } from '@/shared/checkbox';
import FileUpload, { type FileUploadSelectEvent } from 'primevue/fileupload';
import { ref } from 'vue';
import * as api from '@/features/api';
import { DateTimePicker } from '@/shared/datetimepicker';
import router from '@/app/router';

const submitAvailable = ref(false);

let mapImage: File | undefined;
let isPublic: boolean;
let timestampISO: string;

async function Submit() {
    if (!mapImage) return;
    let extension: string = (/(?:\.([^.]+))?$/.exec(mapImage.name) ?? [])[1] ?? '';
    let mapId: number = await api.Create(isPublic, timestampISO, extension);
    await api.MediaUpload(`mapBG_${mapId}.${extension}`, mapImage);
    router.push({name: 'edit', params: {mapId: mapId}});
}

</script>

<template>
    <Modal :isOpen="true">
        <StackPanel>
            <Field placeholder='Название' />
            <Checkbox :onChange="(value: boolean) => {isPublic = value;}" title='Открытый доступ' />
            <DateTimePicker :onChange="(value: string) => {timestampISO = value;}" />
            <FileUpload
                accept="image/*"
                mode="basic"
                :multiple="false"
                :maxFileSize="15 * 1000000"
                @select="(e: FileUploadSelectEvent) => {
                    if (Array.isArray(e.files))
                        mapImage = e.files[0];
                    else mapImage = e.files;

                    if(mapImage)
                        submitAvailable = true;
                    else submitAvailable = false;
                }"
                :auto="false" chooseLabel="Карта:"
            />

            <Button
                @click="() => {Submit();}"
            >
                Создать карту
            </Button>
        </StackPanel>
    </Modal>
</template>

<style scoped>

</style>