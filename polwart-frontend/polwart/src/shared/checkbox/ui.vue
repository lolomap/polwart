<script lang="ts" setup>
import { Typography } from '../typography';

interface Props {
    onChange?: (value: string) => void;
    title?: string;
    isMirrored?: boolean;
    disabled?: boolean;
    size?: 'm' | 'l';
    value?: boolean;
}

const props = defineProps<Props>();
const { 
    title: placeholder = '', 
    isMirrored = false,
    disabled = false, 
    size = 'm',
    value = false,
    onChange = () => {},
} = props;
</script>

<template>
    <div :class="['checkbox', `disabled_${disabled}`, `size_${size}`]">
        <p v-if="!isMirrored">{{ title }}</p>

        <div class="checkbox__container">
            <input 
                @input="(input) => {if(input.target) onChange((input.target as HTMLInputElement).value)}"
                class="checkbox__input" 
                :placeholder="placeholder" 
                type="checkbox" 
                :disabled="disabled"
                :value="value"
            />
        </div>

        <p v-if="isMirrored">{{ title }}</p>
    </div>
</template>

<style scoped>

.checkbox.size_l input[type=checkbox]
{
  /* Double-sized Checkboxes */
  -ms-transform: scale(3); /* IE */
  -moz-transform: scale(3); /* FF */
  -webkit-transform: scale(3); /* Safari and Chrome */
  -o-transform: scale(3); /* Opera */
  transform: scale(3);
  padding: 20px;
}

.checkbox.size_m input[type=checkbox]
{
  /* Double-sized Checkboxes */
  -ms-transform: scale(2); /* IE */
  -moz-transform: scale(2); /* FF */
  -webkit-transform: scale(2); /* Safari and Chrome */
  -o-transform: scale(2); /* Opera */
  transform: scale(2);
  padding: 10px;
}

.checkbox {
    display: flex;
    gap: 32px;
}

.checkbox__container {
    position: relative;
}

.checkbox.size_m {
    padding: 8px 16px;
    font-size: 16px;
}

.checkbox.size_l {
    padding: 12px 16px;
    font-size: 24px;
}

</style>