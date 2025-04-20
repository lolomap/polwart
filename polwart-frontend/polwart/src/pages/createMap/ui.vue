<script setup lang="ts">
import { StackPanel } from '@/shared/stack-panel';
import { Field } from '@/shared/field';
import { Button } from '@/shared/button';
import { Modal } from '@/widgets/modal';
import { Typography } from '@/shared/typography';
import { Checkbox } from '@/shared/checkbox';
import FileUpload, { type FileUploadSelectEvent } from 'primevue/fileupload';
import { MediaUpload } from '@/features/api';
import { ref } from 'vue';

const submitAvailable = ref(false);

let mapImage: File | undefined;

function Submit() {
    if (!mapImage) return;

    MediaUpload(Date.now() + mapImage.name, mapImage);
}

</script>

<template>
    <Modal :isOpen="true">
        <StackPanel>
            <Field placeholder='Название' />
            <Checkbox title='Открытый доступ' />

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
                :auto="false" chooseLabel="Загрузить:"
            />

            <Button
                :disabled="!submitAvailable"
                @click="Submit"
            >
                Создать карту
            </Button>
        </StackPanel>
    </Modal>
</template>

<style scoped>

</style>