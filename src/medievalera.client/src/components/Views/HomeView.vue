<template>
  <div class="view-wrapper">
    <!-- Если мы находимся ровно на том уровне, где нужно меню -->
    <MenuGrid v-if="isMenuRoute" />
    <router-view v-else />
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import MenuGrid from '@/components/MenuGrid.vue'

const route = useRoute()

//Смотрим - это меню или уже конечная.
const isMenuRoute = computed(() => {
  return route.path == '/' || route.matched.at(-1)?.children?.length > 0
})
</script>
