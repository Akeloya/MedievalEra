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
  min-width: 0;
  margin: 12px 0;
  padding: 22px 26px 26px;
  color: #3f3020;
  background:
    radial-gradient(120% 90% at 12% 0%, rgba(255, 255, 255, 0.75) 0%, rgba(255, 255, 255, 0) 55%),
    linear-gradient(155deg, #f8f1df 0%, #efe3c7 55%, #e5d5b2 100%);
  border: 1px solid #c9b48c;
  border-radius: 12px;
  box-shadow: 0 8px 22px rgba(0, 0, 0, 0.35), inset 0 0 0 1px rgba(255, 255, 255, 0.4);
  box-sizing: border-box;
}

.scores-page h1 {
  margin: 0;
  padding-bottom: 12px;
  font-size: 1.9rem;
  color: #4a3218;
  border-bottom: 2px solid rgba(139, 111, 71, 0.35);
  text-shadow: 0 1px 0 rgba(255, 255, 255, 0.65);
}

.scores-page > button {
  margin: 18px 0 6px;
  padding: 10px 22px;
  font-family: var(--font-main);
  font-size: 1rem;
  font-weight: bold;
  letter-spacing: 0.3px;
  color: #fff8dc;
  background: linear-gradient(180deg, #8a6844 0%, #6d4c2f 100%);
  border: 2px solid #d2b48c;
  border-radius: 8px;
  box-shadow: 0 3px 8px rgba(0, 0, 0, 0.35), inset 0 1px 0 rgba(255, 255, 255, 0.2);
  cursor: pointer;
  transition: background 0.2s ease, box-shadow 0.2s ease, transform 0.2s ease;
}

.scores-page > button:hover {
  background: linear-gradient(180deg, #a07c4f 0%, #7d5836 100%);
  box-shadow: 0 5px 12px rgba(0, 0, 0, 0.4), inset 0 1px 0 rgba(255, 255, 255, 0.25);
  transform: translateY(-1px);
}

.scores-page > button:active {
  transform: translateY(1px);
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.35);
}

.scores-page > button:focus-visible {
  outline: 3px solid rgba(243, 146, 0, 0.6);
  outline-offset: 2px;
}

@media (max-width: 640px) {
  .scores-page {
    padding: 14px 12px 18px;
  }

  .scores-page h1 {
    font-size: 1.4rem;
  }
}
</style>
