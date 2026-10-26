<template>
  <section class="scores-page">
    <h1>Результаты прошедших игр</h1>
    <GameHistory />
    <button type="button" @click="showScoreForm = !showScoreForm">
      {{ showScoreForm ? 'Скрыть форму' : 'Добавить результат' }}
    </button>
    <ScoreTable v-if="showScoreForm" />
  </section>
</template>

<script setup>
import { ref } from 'vue'
import GameHistory from '@/components/Score/GameHistory.vue'
import ScoreTable from '@/components/Score/ScoreTable.vue'

const showScoreForm = ref(false)
</script>

<style scoped>
.scores-page {
  width: 100%;
}

.scores-page > button {
  margin: 16px 5px;
  padding: 8px 14px;
}
</style>
