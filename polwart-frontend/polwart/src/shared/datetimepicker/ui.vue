<script lang="ts" setup>
import { computed, ref } from 'vue';

interface Props {
    onChange?: (value: string) => void;
    disabled?: boolean;
    size?: 'm' | 'l';
    value?: string;
}

const props = defineProps<Props>();
const { 
    disabled = false, 
    size = 'm',
    value = '',
    onChange = () => {},
} = props;

const rValue = computed(() => props.value);
</script>

<template>
    <div :class="['field', `disabled_${disabled}`, `size_${size}`]">
        <slot name="label"></slot>
        <div class="field__container">
            <input 
                @input="(input) => {if(input.target) onChange((input.target as HTMLInputElement).value)}"
                class="field__input"
                type="datetime-local"
                :disabled="disabled"
                :value="rValue"
            />
        </div>
    </div>
</template>

<style scoped>

.field:deep(.typography) {
    color: var(--grayscale-hard)
}

.field__container {
    position: relative;
}

.field__input {
    border-radius: 4px;
    border: 1px solid var(--grayscale-light);
    background: var(--main-surface);
    color: var(--main-on-surface);
    width: 100%;
}

.field__input::placeholder {
    color: var(--grayscale-hard);
}

.field.size_m .field__input {
    padding: 8px 16px;
    font-size: 16px;
}

.field.size_l .field__input {
    padding: 12px 16px;
    font-size: 24px;
}

</style>