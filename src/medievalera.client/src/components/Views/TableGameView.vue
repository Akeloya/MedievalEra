<template>
    <DiceGame />
</template>
<script lang="js">
    import { defineComponent } from 'vue';
    import DiceGame from '@/components/TableGame/DiceGame.vue'

    export default defineComponent({
        components: {
            DiceGame
        },
        data() {
            return {
                loading: false,
                post: null
            };
        },
    });
</script>
