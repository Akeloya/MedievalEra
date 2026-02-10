<template>
  <MenuGrid />
</template>
<script lang="js">
  import { defineComponent } from 'vue';

  import MenuGrid from '@/components/MenuGrid.vue'

  export default defineComponent({
    components: {
      MenuGrid
    },
        data() {
            return {
                loading: false,
                post: null
            };
        },       
    });
</script>
