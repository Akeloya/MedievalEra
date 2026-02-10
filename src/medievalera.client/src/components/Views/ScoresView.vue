<template>
  <Score></Score>
</template>
<script lang="js">
  import { defineComponent } from 'vue';
  import Score from '@/components/Score/ScoreTable.vue'

  export default defineComponent({
    components: {
      Score
    },
        data() {
            return {
                loading: false,
                post: null
            };
        },
    });
</script>
