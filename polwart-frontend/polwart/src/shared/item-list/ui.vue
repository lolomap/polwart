<script setup lang="ts">
import { Container } from '@/shared/container';
import { Typography } from '@/shared/typography';

interface Props {
    items?: Array<Object>;
}
const props = defineProps<Props>();
const {
    items = [],
} = props;
</script>

<template>
    <Container class="list-container" v-for="item in items">
        <Container class="list-item">
            <Container
                v-for="property in Object.entries(item)"
                class="list-item-prop"
            >
                <img v-if="typeof property[1] == 'string' && property[1].split(':')[0] == 'img'"
                    :src="property[1].split(':')[1]" />
                <Typography tag="p" v-else>{{ property[1] }}</Typography>
            </Container>
        </Container>
    </Container>
</template>

<style scoped>

.list-container {
    display: flex;
    align-items: center;
    flex-direction: column;
}

.list-item {
    background: var(--slot-background);
    border-radius: 4px;
    /*height: 90px;*/
    display: flex;
    align-items: center;
    gap: 15px;
    padding: 15px;
    color: var(--main-on-default)
}

</style>