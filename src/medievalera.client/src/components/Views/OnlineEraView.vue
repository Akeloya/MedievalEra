<template>
  <section class="online-era">
    <h1>Эра средневековья</h1>
    <p v-if="loading" role="status">Загрузка планшета…</p>
    <p v-if="playerDeck" class="player-name">Планшет игрока {{ playerDeck.player.name }}</p>
    <div v-if="playerDeck" class="board-frame" role="img" :aria-label="boardDescription">
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

        <g class="lane-icons">
          <rect x="8" y="48" width="18" height="16" rx="2" /><text x="17" y="61">{{ diceIcons.goods }}</text>
          <rect x="8" y="65" width="18" height="16" rx="2" /><text x="17" y="78">{{ diceIcons.stone }}</text>
          <rect x="8" y="82" width="18" height="16" rx="2" /><text x="17" y="95">{{ diceIcons.wood }}</text>
          <rect x="8" y="99" width="18" height="16" rx="2" /><text x="17" y="112">{{ diceIcons.meal }}</text>
        </g>
        <g class="top-lane">
          <template v-for="(resource, row) in materialResources" :key="resource.resource">
            <template v-for="value in resource.maxValue + 1" :key="value">
              <circle :cx="34 + (value - 1) * 23" :cy="56 + row * 17" r="3" />
              <text :x="34 + (value - 1) * 23" :y="64 + row * 17">{{ value - 1 }}</text>
            </template>
            <circle class="resource-marker" :cx="34 + resource.count * 23" :cy="56 + row * 17" r="5" :style="{ fill: resource.color }">
              <title>{{ resource.label }}: {{ resource.count }}</title>
            </circle>
          </template>
        </g>

        <g class="side-lane">
          <rect x="298" y="8" width="48" height="111" rx="3" />
          <rect v-for="(step, index) in roundSteps" :key="step.name" x="302" :y="10 + index * 18" width="20" height="17" rx="2" />
          <text v-for="(step, index) in roundSteps" :key="`${step.name}-icon`" x="312" :y="23 + index * 18">{{ step.icon }}</text>
          <circle v-for="dot in sideDots" :key="`side-${dot}`" cx="332" :cy="19 + dot * 18" r="3.3" />
        </g>
        <rect x="15" y="135" width="272" height="273" rx="4" class="main-field" />
        <circle v-for="dot in fieldDots" :key="`field-${dot}`" :cx="29 + (dot % 14) * 18.6" :cy="150 + Math.floor(dot / 14) * 18.6" r="3.8" class="field-dot" />
        <g v-for="building in placedBuildings" :key="building.id" class="placed-building">
          <title>{{ building.name }}</title>
          <rect v-for="cell in building.cells" :key="`${cell.x}-${cell.y}`"
            :x="21 + cell.x * 18.6" :y="142 + cell.y * 18.6" width="16" height="16" rx="2"
            :fill="building.type === 'tower' ? '#b5bbc4' : '#be8d55'" />
          <text v-if="building.cells.length" :x="29 + building.cells[0].x * 18.6" :y="154 + building.cells[0].y * 18.6">{{ building.type === 'tower' ? '♜' : '⌂' }}</text>
        </g>
        <g class="right-rail">
          <rect x="299" y="135" width="45" height="273" rx="3" />
          <text class="score-header" x="307" y="145">{{ diceIcons.culture }}</text>
          <text class="score-header" x="336" y="145">{{ diceIcons.skull }}</text>
          <template v-for="position in scorePositions" :key="`score-${position}`">
            <circle cx="307" :cy="163 + (position - 1) * 9.95" r="2.6" />
            <text class="score-number" x="321.5" :y="165 + (position - 1) * 9.95">{{ position }}</text>
            <circle cx="336" :cy="163 + (position - 1) * 9.95" r="2.6" />
          </template>
          <text class="score-number" x="321.5" y="155">0</text>
          <circle v-for="resource in scoreResources" :key="resource.resource" class="resource-marker"
            :cx="resource.resource === 'culture' ? 307 : 336" :cy="153.05 + resource.count * 9.95" r="4.2" :style="{ fill: resource.color }">
            <title>{{ resource.label }}: {{ resource.count }}</title>
          </circle>
        </g>
      </svg>
    </div>
    <p v-if="playerDeck" class="building-summary">Построено: {{ placedBuildings.map(building => building.name).join(', ') || 'нет зданий' }}</p>
    <p v-if="error" class="error" role="alert">{{ error }}</p>
    <button v-if="!leaveOptions" class="era-button leave-button" type="button" :disabled="leaving" @click="askToLeave">
      {{ leaving ? 'Выходим...' : 'Покинуть игру' }}
    </button>
    <div v-else class="leave-confirmation" role="group" aria-labelledby="leave-question" :aria-busy="leaving">
      <p id="leave-question">{{ leaveQuestion }}</p>
      <label v-if="leaveOptions.hasOtherPlayers && leaveOptions.isOwner" class="successor-choice">
        Передать управление игроку
        <select v-model="successorId" :disabled="leaving">
          <option v-for="player in leaveOptions.successors" :key="player.id" :value="player.id">{{ player.name }}</option>
        </select>
      </label>
      <div class="leave-actions">
        <button class="era-button" type="button" :disabled="leaving" @click="leaveOptions = null">Продолжить игру</button>
        <template v-if="!leaveOptions.hasOtherPlayers">
          <button v-if="leaveOptions.canDiscard" class="era-button" type="button" :disabled="leaving" @click="leaveGame('discard')">Не сохранять результат</button>
          <button class="era-button" type="button" :disabled="leaving" @click="leaveGame('calculate')">Рассчитать как есть</button>
        </template>
        <button v-else class="era-button" type="button" :disabled="leaving || (leaveOptions.isOwner && !successorId)" @click="leaveGame('keep')">{{ leaving ? 'Выходим…' : 'Выйти из игры' }}</button>
      </div>
    </div>
  </section>
</template>

<script setup>
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { diceIcons } from '@/components/TableGame/DiceResources.js'

const route = useRoute()
const router = useRouter()
const leaving = ref(false)
const leaveOptions = ref(null)
const successorId = ref(null)
const leaveQuestion = computed(() => {
  if (!leaveOptions.value?.hasOtherPlayers) return leaveOptions.value?.canDiscard
    ? 'Точно завершить игру? Можно не сохранять результат или рассчитать его по текущему состоянию.'
    : 'Точно завершить игру? Результаты ушедших игроков сохранятся. Рассчитать игру по текущему состоянию?'
  if (leaveOptions.value.isOwner) {
    const successor = leaveOptions.value.successors.find(player => player.id === successorId.value)
    return `Точно выйти из игры? Ваши результаты сохранятся. Управление игрой будет передано игроку ${successor?.name ?? 'из списка ниже'}.`
  }
  return 'Точно выйти из игры? Ваши результаты сохранятся, а остальные игроки продолжат партию.'
})
const error = ref('')
const loading = ref(false)
const playerDeck = ref(null)
const resourceStyles = {
  goods: { label: 'Товары', color: '#62c9ed' },
  stone: { label: 'Камень', color: '#9299a2' },
  wood: { label: 'Дерево', color: '#88502c' },
  meal: { label: 'Еда', color: '#ffda32' },
  culture: { label: 'Культура', color: '#ffffff' },
  skull: { label: 'Черепа', color: '#f58b2b' }
}
const resources = computed(() => Object.entries(resourceStyles).flatMap(([key, style]) => {
  const resource = playerDeck.value?.resources.find(item => item.resource === key)
  return resource ? [{ ...resource, ...style }] : []
}))
const materialResources = computed(() => resources.value.filter(resource => !['culture', 'skull'].includes(resource.resource)))
const scoreResources = computed(() => resources.value.filter(resource => ['culture', 'skull'].includes(resource.resource)))
const placedBuildings = computed(() => (playerDeck.value?.buildings ?? []).map(building => ({
  ...building,
  cells: playerDeck.value.field.flatMap((row, y) => row.flatMap((id, x) => id === building.id ? [{ x, y }] : []))
})))
const boardDescription = computed(() => `Планшет игрока ${playerDeck.value?.player.name}. ${resources.value.map(resource => `${resource.label}: ${resource.count}`).join(', ')}. Построено: ${placedBuildings.value.map(building => building.name).join(', ')}`)

watch(() => route.query.gameId, async (gameId, _, onCleanup) => {
  const controller = new AbortController()
  onCleanup(() => controller.abort())
  playerDeck.value = null
  leaveOptions.value = null
  error.value = ''
  loading.value = true
  try {
    if (!gameId || !/^\d+$/.test(String(gameId))) throw new Error('Не указана игра. Откройте партию из лобби.')
    const [profileResponse, boardResponse] = await Promise.all([
      fetch('/api/PlayerProfile/me', { signal: controller.signal }),
      fetch(`/api/lobby/games/${gameId}/board`, { signal: controller.signal })
    ])
    if (profileResponse.status === 401 || boardResponse.status === 401) throw new Error('Войдите в профиль, чтобы открыть планшет.')
    if (!profileResponse.ok || !boardResponse.ok) throw new Error('Не удалось загрузить планшет игрока.')
    const [profile, board] = await Promise.all([profileResponse.json(), boardResponse.json()])
    if (controller.signal.aborted) return
    const deck = board.players.find(item => item.player.id === profile.id)
    if (!deck) throw new Error('Ваш планшет в этой игре не найден.')
    playerDeck.value = deck
  } catch (cause) {
    if (!controller.signal.aborted) error.value = cause.message
  } finally {
    if (!controller.signal.aborted) loading.value = false
  }
}, { immediate: true })
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

async function askToLeave() {
  leaving.value = true
  error.value = ''
  try {
    const response = await fetch(`/api/lobby/games/${route.query.gameId}/leave-options`)
    if (response.status === 404) { await router.push({ name: 'online' }); return }
    if (!response.ok) throw new Error('Не удалось загрузить варианты выхода из игры.')
    leaveOptions.value = await response.json()
    successorId.value = leaveOptions.value.successors[0]?.id ?? null
  } catch (cause) {
    error.value = cause.message
  } finally {
    leaving.value = false
  }
}

async function leaveGame(resolution) {
  leaving.value = true
  error.value = ''
  try {
    const query = new URLSearchParams({ resolution })
    if (leaveOptions.value?.isOwner && successorId.value) query.set('successorId', successorId.value)
    const response = await fetch(`/api/lobby/games/${route.query.gameId}/players/me?${query}`, { method: 'DELETE' })
    if (!response.ok && response.status !== 404) {
      const message = await response.json().catch(() => null)
      if (response.status === 409) await askToLeave()
      throw new Error(message?.message ?? (typeof message === 'string' ? message : `Не удалось покинуть игру (HTTP ${response.status}).`))
    }
    await router.push({ name: resolution === 'calculate' && response.ok ? 'results' : 'online' })
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
.game-board .resource-marker { stroke: #514633; stroke-width: 1.2; }
.placed-building rect { stroke: #5c4b37; stroke-width: 1.2; }
.placed-building text { fill: #39342e; font: bold 15px Georgia, serif; text-anchor: middle; }
.player-name, .building-summary { color: #f5deb3; }
.leave-button { margin-top: 4px; }
.leave-confirmation { max-width: 650px; margin: 4px auto; padding: 16px; border: 1px solid #d2b48c; border-radius: 10px; background: rgba(44, 39, 29, .85); }
.leave-confirmation p { margin: 0 0 14px; }
.leave-actions { display: flex; justify-content: center; flex-wrap: wrap; gap: 10px; }
.successor-choice { display: flex; justify-content: center; align-items: center; flex-wrap: wrap; gap: 8px; margin-bottom: 14px; }
.successor-choice select { padding: 7px; color: #fff8dc; background: #3c3529; border: 1px solid #d2b48c; border-radius: 5px; }
.error { color: #ffb3a9; }
</style>
