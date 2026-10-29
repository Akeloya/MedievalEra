<template>
  <section class="online-era">
    <h1>Эра средневековья</h1>
    <div class="board-frame" role="img" aria-label="Игровое поле: основная сетка точек, верхняя дорожка и боковые шкалы">
      <svg class="game-board" viewBox="0 0 360 430" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
        <defs>
          <linearGradient id="parchment" x1="0" y1="0" x2="0" y2="1">
            <stop offset="0" stop-color="#fff68a" />
            <stop offset="1" stop-color="#e8d76a" />
          </linearGradient>
          <linearGradient id="field" x1="0" y1="0" x2="0" y2="1">
            <stop offset="0" stop-color="#f4e98c" />
            <stop offset="1" stop-color="#eadb75" />
          </linearGradient>
        </defs>
        <rect x="2" y="2" width="356" height="426" rx="13" fill="url(#parchment)" stroke="#d5b942" stroke-width="2" />
        <text x="10" y="39" class="board-logo">ЭРА</text>
        <path d="M214 24c18-12 31 8 46-4 10-8 18-4 25 0M220 30c18-8 30 7 42-2 8-6 15-5 22-1" class="ornament" />

        <!-- Decorative resource lanes; red rule overlays and player pieces are intentionally absent. -->
        <g class="lane-icons">
          <rect x="8" y="48" width="18" height="16" rx="2" /><text x="17" y="61">{{ diceIcons.goods }}</text>
          <rect x="8" y="65" width="18" height="16" rx="2" /><text x="17" y="78">{{ diceIcons.stone }}</text>
          <rect x="8" y="82" width="18" height="16" rx="2" /><text x="17" y="95">{{ diceIcons.wood }}</text>
          <rect x="8" y="99" width="18" height="16" rx="2" /><text x="17" y="112">{{ diceIcons.meal }}</text>
        </g>
        <g class="top-lane">
          <circle v-for="dot in topDots" :key="`top-${dot}`" :cx="34 + dot.index * 23" :cy="56 + dot.row * 18" r="3.5" />
          <text v-for="number in topNumbers" :key="`number-${number.row}-${number.value}`" :x="34 + number.index * 23" :y="65 + number.row * 18">{{ number.value }}</text>
        </g>

        <g class="side-lane">
          <rect x="298" y="8" width="48" height="111" rx="3" />
          <rect v-for="(step, index) in roundSteps" :key="step.name" x="302" :y="10 + index * 18" width="20" height="17" rx="2" />
          <text v-for="(step, index) in roundSteps" :key="`${step.name}-icon`" x="312" :y="23 + index * 18">{{ step.icon }}</text>
          <circle v-for="dot in sideDots" :key="`side-${dot}`" cx="332" :cy="19 + dot * 18" r="3.3" />
        </g>
        <rect x="15" y="135" width="272" height="273" rx="4" class="main-field" />
        <circle v-for="dot in fieldDots" :key="`field-${dot}`" :cx="29 + (dot % 14) * 18.6" :cy="150 + Math.floor(dot / 14) * 18.6" r="3.8" class="field-dot" />
        <g class="right-rail">
          <rect x="299" y="135" width="45" height="273" rx="3" />
          <text class="score-header" x="307" y="151">{{ diceIcons.culture }}</text>
          <text class="score-header" x="336" y="151">{{ diceIcons.skull }}</text>
          <template v-for="position in scorePositions" :key="`score-${position}`">
            <circle cx="307" :cy="163 + (position - 1) * 9.95" r="2.6" />
            <text class="score-number" x="321.5" :y="165 + (position - 1) * 9.95">{{ position }}</text>
            <circle cx="336" :cy="163 + (position - 1) * 9.95" r="2.6" />
          </template>
        </g>
      </svg>
    </div>
    <p v-if="error" class="error" role="alert">{{ error }}</p>
    <button class="era-button leave-button" type="button" :disabled="leaving" @click="leaveGame">
      {{ leaving ? 'Выходим...' : 'Покинуть игру' }}
    </button>
  </section>
</template>

<script setup>
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { diceIcons } from '@/components/TableGame/DiceResources.js'

const route = useRoute()
const router = useRouter()
const leaving = ref(false)
const error = ref('')
const topDots = computed(() => [6, 7, 8, 10].flatMap((count, row) =>
  Array.from({ length: count }, (_, index) => ({ row, index }))
))
const topNumbers = computed(() => [6, 7, 10].flatMap((count, row) =>
  Array.from({ length: count }, (_, index) => ({ row, index, value: index + 1 }))
))
const sideDots = computed(() => Array.from({ length: 6 }, (_, index) => index))
const scorePositions = computed(() => Array.from({ length: 25 }, (_, index) => index + 1))
const fieldDots = computed(() => Array.from({ length: 196 }, (_, index) => index))
const roundSteps = [
  { name: 'Бросок кубиков', icon: '🎲' },
  { name: 'Сбор ресурсов', icon: '🤲' },
  { name: 'Содержание жителей', icon: diceIcons.meal },
  { name: 'Бедствия', icon: diceIcons.skull },
  { name: 'Строительство', icon: diceIcons.building },
  { name: 'Набег', icon: diceIcons.attack }
]

async function leaveGame() {
  leaving.value = true
  error.value = ''
  try {
    const response = await fetch(`/api/lobby/games/${route.query.gameId}/players/me`, { method: 'DELETE' })
    if (!response.ok && response.status !== 404) {
      throw new Error(`Не удалось покинуть игру (HTTP ${response.status}).`)
    }
    await router.push({ name: 'online' })
  } catch (cause) {
    error.value = cause.message
  } finally {
    leaving.value = false
  }
}
</script>

<style scoped>
.online-era { width: 100%; text-align: center; color: #fff8dc; }
.online-era h1 { margin-bottom: 12px; color: #f5deb3; }
.board-frame { width: min(100%, 650px); margin: 0 auto 16px; padding: 8px; border: 1px solid #d2b48c; border-radius: 14px; background: rgba(44, 39, 29, .72); box-shadow: 0 8px 24px rgba(0, 0, 0, .35); }
.game-board { display: block; width: 100%; height: auto; border-radius: 10px; }
.board-logo { fill: #897a43; font: bold 26px Georgia, serif; letter-spacing: 2px; }
.ornament { fill: none; stroke: #8d804a; stroke-width: 1.3; opacity: .8; }
.top-lane rect, .side-lane > rect, .right-rail > rect { fill: #f3e987; stroke: #d4c360; stroke-width: 1; }
.top-lane circle, .side-lane circle, .right-rail circle { fill: #898354; }
.top-lane text { fill: #5b5438; font: 5.5px Georgia, serif; text-anchor: middle; }
.right-rail .score-header { fill: #827443; font: 11px Georgia, serif; text-anchor: middle; }
.right-rail .score-number { fill: #5b5438; font: 5px Georgia, serif; text-anchor: middle; }
.lane-icons rect, .side-lane rect:not(:first-child), .right-rail rect:not(:first-child) { fill: #fff3a0; stroke: #d2c16b; }
.lane-icons text, .side-lane text, .right-rail text { fill: #827443; font: 11px Georgia, serif; text-anchor: middle; }
.main-field { fill: url(#field); stroke: #d0c16a; stroke-width: 1; }
.field-dot { fill: #898354; }
.leave-button { margin-top: 4px; }
.error { color: #ffb3a9; }
</style>
