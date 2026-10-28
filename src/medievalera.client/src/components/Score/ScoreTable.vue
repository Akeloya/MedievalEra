<template>
  <div class="table-container">
    <form @submit.prevent="saveScore">
    <p class="game-type-note">
      Тип игры: <strong>Настольная</strong>
    </p>
    <table class="custom-table era-table">
      <thead>
        <tr>
          <th>Здание / результат</th>
          <th>Очки</th>
          <th v-for="player in players" :key="player.playerNumber" class="header-group">
            <label :for="`player-name-${player.playerNumber}`">Игрок {{ player.playerNumber }}</label>
            <div class="player-name-control">
              <input :id="`player-name-${player.playerNumber}`"
                     v-model.trim="player.playerName"
                     :class="['player-name', 'era-input', `player-status-${playerNameStatuses[player.playerNumber] ?? 'unknown'}`]"
                     maxlength="50"
                     required>
              <button v-if="player.playerNumber === players.length && players.length < 4"
                      type="button"
                      class="era-button add-player-button"
                      aria-label="Добавить игрока"
                      title="Добавить игрока"
                      @click="addPlayer">
                +
              </button>
            </div>
          </th>
        </tr>
      </thead>

      <tbody>
        <tr v-for="building in buildings" :key="building.key" class="row-color-blue">
          <td>{{ building.label }}</td>
          <td>
            <span class="score-indicator">
              <strong>{{ building.points }}</strong>
              <ScoreIcon :name="buildingIcon(building.key)" />
            </span>
          </td>
          <td v-for="player in players" :key="player.playerNumber">
            <div class="building-inputs">
              <label v-if="building.key === 'Tower'" class="wall-completed-toggle" title="Стена завершена">
                <span class="building-input-field wall-completion-field">
                  <span class="building-watermark" aria-hidden="true">🏰</span>
                  <input v-model="player.wallCompleted" class="wall-completion-checkbox" type="checkbox"
                         aria-label="Стена завершена" @change="onWallCompletedChange(player)">
                </span>
                <span>Стена завершена</span>
              </label>
              <label title="Всего построено">
                <span class="building-input-field">
                  <span class="building-watermark" aria-hidden="true">🔨</span>
                  <input class="era-input" v-model.number="player.buildings[building.key].count"
                         type="number" min="0" step="1" :max="availableFor(player, building)"
                         aria-label="Общее количество построенных зданий"
                         @input="syncInsideWallCount(player, building)" required>
                </span>
              </label>
              <label v-if="player.wallCompleted && building.key !== 'Tower'" title="В стене">
                <span class="building-input-field">
                  <span class="building-watermark" aria-hidden="true">🏰</span>
                  <input class="era-input" :value="player.buildings[building.key].insideWallCount"
                         type="number" min="0" step="1"
                         :max="maxInsideWallCount(player, building)"
                         aria-label="Количество зданий в стене"
                         @input="updateInsideWallCount(player, building, $event.target.value)" required>
                </span>
              </label>
            </div>
            <strong>{{ buildingTotal(player, building) }}</strong>
          </td>
        </tr>

        <tr v-for="bonus in quantityBonuses" :key="bonus.key" class="row-color-yellow">
          <td>{{ bonus.label }}</td>
          <td>
            <span class="score-indicator">
              <strong>{{ bonus.points }}</strong>
              <ScoreIcon v-if="bonus.icon" :name="bonus.icon" />
              <span v-else-if="bonus.key === 'guildResources'" class="resource-icons" aria-label="Еда, камень, древесина и товары">
                <span v-for="resource in guildResourceIcons" :key="resource">{{ diceIcons[resource] }}</span>
              </span>
              <span v-else-if="bonus.key === 'universityCultureLevel'" class="resource-icons" aria-label="Культура">
                {{ diceIcons.culture }}
              </span>
            </span>
          </td>
          <td v-for="player in players" :key="player.playerNumber">
            <label v-if="bonus.key !== 'cathedralDiceCount' && bonusAvailable(player, bonus.key)">
              {{ bonus.inputLabel }}
              <input class="era-input" v-model.number="player[bonus.key]"
                     type="number" min="0" step="1"
                     :max="bonus.key === 'marketBonus' ? builtCount(player, 'Market') * 8 : undefined" required>
            </label>
            <strong>{{ bonusValue(player, bonus.key) }}</strong>
          </td>
        </tr>

        <tr class="row-color-green">
          <td>Наибольшая область, окруженная стеной</td>
          <td>
            <span class="score-indicator"><strong>+10</strong><ScoreIcon name="point" /></span>
            <small>лидеру(ам)</small>
          </td>
          <td v-for="player in players" :key="player.playerNumber">
            <label v-if="player.wallCompleted">
              Точек в области
              <input class="era-input" v-model.number="player.walledAreaSize"
                     type="number" min="0" step="1" required>
            </label>
            <span v-else>—</span>
            <strong>Бонус: {{ walledAreaBonus(player) }}</strong>
          </td>
        </tr>
        <tr class="row-color-green">
          <td>Наиболее развитая культура</td>
          <td>
            <span class="score-indicator"><strong>+5</strong><ScoreIcon name="book" /></span>
            <small>лидеру(ам)</small>
          </td>
          <td v-for="player in players" :key="player.playerNumber">
            <strong>Бонус: {{ cultureBonus(player) }}</strong>
          </td>
        </tr>

        <tr class="row-color-white">
          <td>Промежуточный итог</td>
          <td></td>
          <td v-for="player in players" :key="player.playerNumber">
            <strong>{{ intermediateTotal(player) }}</strong>
          </td>
        </tr>

        <tr class="row-color-green">
          <td>Очки бедствий</td>
          <td>
            <span class="score-indicator"><strong>−1</strong><span class="resource-icons" aria-label="Череп">{{ diceIcons.skull }}</span></span>
          </td>
          <td v-for="player in players" :key="player.playerNumber">
            <input class="era-input" v-model.number="player.disasterPoints"
                   type="number" min="0" step="1" required>
          </td>
        </tr>

        <tr class="row-color-white">
          <td>Финальный итог</td>
          <td></td>
          <td v-for="player in players" :key="player.playerNumber">
            <strong>{{ finalTotal(player) }}</strong>
          </td>
        </tr>
        <tr class="row-color-white">
          <td>Очки / титул / комментарий</td>
          <td></td>
          <td v-for="player in players" :key="player.playerNumber">
            <div class="title-cell">
              <strong class="player-title">
                {{ hasTitle(player) ? '👑 ' : '' }}{{ titleForScore(finalTotal(player)).title }}
              </strong>
              <span>{{ titleForScore(finalTotal(player)).comment }}</span>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
    <button class="era-button" type="submit" :disabled="saving || !buildingsLoaded">
      {{ saving ? 'Сохранение...' : 'Сохранить результаты' }}
    </button>
    <p v-if="message" :class="{ error: isError }" role="status">{{ message }}</p>
    <p v-if="titlesError" class="error" role="status">{{ titlesError }}</p>
    <p v-if="buildingsError" class="error" role="status">{{ buildingsError }}</p>
    </form>
  </div>
</template>

<script setup>
import { onMounted, reactive, ref, watch } from 'vue'
import ScoreIcon from './ScoreIcon.vue'
import { diceIcons } from '@/components/TableGame/DiceResources'

const emit = defineEmits(['saved'])

const buildings = ref([])
const buildingsLoaded = ref(false)
const buildingsError = ref('')

onMounted(async () => {
  try {
    const response = await fetch('/api/Buildings')
    if (!response.ok) throw new Error(`Ошибка сервера: ${response.status}`)
    const gameSet = await response.json()
    buildings.value = gameSet
    for (const player of players) initializePlayerBuildings(player)
    buildingsLoaded.value = true
    refreshAutomaticBonuses()
  } catch {
    buildingsError.value = 'Не удалось загрузить игровой набор. Обновите страницу, чтобы повторить попытку.'
  }
})

const quantityBonuses = [
  { key: 'marketBonus', label: 'Бонус рынка', inputLabel: 'Очки', points: 8, icon: 'chessboard' },
  { key: 'guildResources', label: 'Гильдия: оставшиеся ресурсы', inputLabel: 'Ресурсы', points: 1 },
  { key: 'universityCultureLevel', label: 'Университет: уровень культуры', inputLabel: 'Уровень', points: 1 },
  { key: 'cathedralDiceCount', label: 'Собор: количество кубиков', inputLabel: 'Кубики', points: 1, icon: 'die' },
  { key: 'cultureIcons', label: 'Культура: количество иконок', inputLabel: 'Иконки', points: 1, icon: 'book' }
]

const guildResourceIcons = ['meal', 'stone', 'wood', 'goods']
const buildingIcons = {
  Tower: 'tower',
  House: 'house',
  Farm: 'farm',
  Church: 'church',
  Monastery: 'monastery',
  Cathedral: 'cathedral',
  CityHouse: 'city-house',
  Guild: 'guild',
  Market: 'market',
  Hospital: 'hospital',
  Sawmill: 'sawmill',
  University: 'university'
}

function buildingIcon(key) {
  return buildingIcons[key] ?? 'point'
}

const buildingForBonus = {
  marketBonus: 'Market',
  guildResources: 'Guild',
  universityCultureLevel: 'University'
}

function createPlayer(playerNumber) {
  return {
    playerNumber,
    playerName: `Игрок ${playerNumber}`,
    buildings: {},
    marketBonus: 0,
    guildResources: 0,
    universityCultureLevel: 0,
    cathedralDiceCount: 0,
    cultureIcons: 0,
    walledAreaSize: 0,
    disasterPoints: 0,
    wallCompleted: false
  }
}

function initializePlayerBuildings(player) {
  player.buildings = Object.fromEntries(buildings.value.map(building => [
    building.key, { count: 0, insideWallCount: 0 }
  ]))
}

const players = reactive([createPlayer(1), createPlayer(2)])
const playerNameStatuses = reactive({})
let nameStatusRequestId = 0
let nameStatusTimer

function addPlayer() {
  if (players.length >= 4) return

  const player = createPlayer(players.length + 1)
  if (buildingsLoaded.value) initializePlayerBuildings(player)
  players.push(player)
}

watch(() => players.map(player => ({ number: player.playerNumber, name: player.playerName })), entries => {
  const requestId = ++nameStatusRequestId
  clearTimeout(nameStatusTimer)
  nameStatusTimer = setTimeout(async () => {
    const statuses = await Promise.all(entries.map(async ({ number, name }) => {
      const normalizedName = name.trim()
      if (!normalizedName) return [number, 'unknown']

      try {
        const response = await fetch(`/api/PlayerProfile/name-status?name=${encodeURIComponent(normalizedName)}`)
        if (!response.ok) return [number, 'unknown']
        const result = await response.json()
        return [number, result.status]
      } catch {
        return [number, 'unknown']
      }
    }))

    if (requestId !== nameStatusRequestId) return
    for (const [number, status] of statuses) playerNameStatuses[number] = status
  }, 300)
}, { immediate: true, deep: true })

const saving = ref(false)
const message = ref('')
const isError = ref(false)
const scoreTitles = ref([])
const titlesError = ref('')

onMounted(async () => {
  try {
    const response = await fetch('/api/GameScore/titles')
    if (!response.ok) throw new Error(`Ошибка сервера: ${response.status}`)
    scoreTitles.value = await response.json()
  } catch {
    titlesError.value = 'Не удалось загрузить титулы. Обновите страницу, чтобы повторить попытку.'
  }
})

function buildingTotal(player, building) {
  const score = player.buildings[building.key]
  const insideWallCount = building.key === 'Tower'
    ? Number(score.count || 0)
    : player.wallCompleted ? Number(score.insideWallCount || 0) : 0
  return (building.points ?? 0) * (Number(score.count || 0) + insideWallCount)
}

function syncInsideWallCount(player, building) {
  const score = player.buildings[building.key]
  score.insideWallCount = Math.min(Number(score.insideWallCount || 0), Number(score.count || 0))
}

function maxInsideWallCount(player, building) {
  const score = player.buildings[building.key]
  return Math.max(0, Number(score.insideWallCount || 0) + availableFor(player, building) - Number(score.count || 0))
}

function updateInsideWallCount(player, building, value) {
  const score = player.buildings[building.key]
  const current = Number(score.insideWallCount || 0)
  const requested = Math.max(0, Math.floor(Number(value) || 0))

  if (requested > current) {
    const capacity = Math.max(0, availableFor(player, building) - Number(score.count || 0))
    const increase = Math.min(requested - current, capacity)
    score.insideWallCount = current + increase
    score.count = Number(score.count || 0) + increase
  } else {
    score.insideWallCount = Math.min(requested, Number(score.count || 0))
  }
}

function onWallCompletedChange(player) {
  if (player.wallCompleted) return

  player.walledAreaSize = 0
  for (const score of Object.values(player.buildings)) score.insideWallCount = 0
}

function builtCount(player, key) {
  return Number(player.buildings[key]?.count || 0)
}

function bonusAvailable(player, key) {
  if (key === 'cathedralDiceCount') return builtCount(player, 'Cathedral') > 0
  const buildingKey = buildingForBonus[key]
  return !buildingKey || builtCount(player, buildingKey) > 0
}

function cathedralDiceCount(player) {
  if (!bonusAvailable(player, 'cathedralDiceCount')) return 0
  return buildings.value.reduce((total, building) =>
    total + (Number(building.diceType) !== 0 ? builtCount(player, building.key) : 0), 0)
}

function bonusValue(player, key) {
  if (key === 'cathedralDiceCount') return cathedralDiceCount(player)
  if (!bonusAvailable(player, key)) return 0
  if (key === 'marketBonus') return Math.min(Number(player.marketBonus || 0), builtCount(player, 'Market') * 8)
  return Number(player[key] || 0)
}

function refreshAutomaticBonuses() {
  for (const player of players) {
    for (const [bonusKey, buildingKey] of Object.entries(buildingForBonus)) {
      if (builtCount(player, buildingKey) === 0) player[bonusKey] = 0
    }
    player.marketBonus = Math.min(Number(player.marketBonus || 0), builtCount(player, 'Market') * 8)
  }
}

watch(() => players.map(player => [
  player.buildings.Market?.count,
  player.buildings.Guild?.count,
  player.buildings.University?.count,
  player.buildings.Cathedral?.count
]), refreshAutomaticBonuses, { deep: true })

function availableFor(player, building) {
  const usedByOthers = players.filter(candidate => candidate !== player)
    .reduce((total, candidate) => total + Number(candidate.buildings[building.key].count || 0), 0)
  return Math.max(0, building.quantity - usedByOthers)
}

function intermediateTotal(player) {
  const buildingsTotal = buildings.value.reduce(
    (total, building) => total + buildingTotal(player, building),
    0
  )
  return buildingsTotal +
    bonusValue(player, 'marketBonus') +
    bonusValue(player, 'guildResources') +
    bonusValue(player, 'universityCultureLevel') +
    cathedralDiceCount(player) +
    Number(player.cultureIcons || 0) +
    cultureBonus(player) +
    walledAreaBonus(player)
}

function finalTotal(player) {
  return intermediateTotal(player) - Number(player.disasterPoints || 0)
}

function hasTitle(player) {
  return finalTotal(player) === Math.max(...players.map(finalTotal))
}

function awardForHighest(metric, player, points) {
  const highest = Math.max(...players.map(candidate => Number(candidate[metric] || 0)))
  return highest > 0 && Number(player[metric] || 0) === highest ? points : 0
}

function cultureBonus(player) {
  return awardForHighest('cultureIcons', player, 5)
}

function walledAreaBonus(player) {
  if (!player.wallCompleted) return 0
  return awardForHighest('walledAreaSize', player, 10)
}

function titleForScore(score) {
  return scoreTitles.value.find(range => range.maxScore === null || score <= range.maxScore)
    ?? { title: '—', comment: '' }
}

async function saveScore() {
  if (!buildingsLoaded.value) return
  const exceeded = buildings.value.find(building => players.reduce(
    (total, player) => total + Number(player.buildings[building.key].count || 0), 0
  ) > building.quantity)
  if (exceeded) {
    isError.value = true
    message.value = `Превышен запас здания «${exceeded.label}»: в наборе ${exceeded.quantity}.`
    return
  }
  if (players.some(player =>
    buildings.value.some(building =>
      player.buildings[building.key].insideWallCount > player.buildings[building.key].count
    )
  )) {
    isError.value = true
    message.value = 'Число зданий внутри стены не может превышать число построенных зданий.'
    return
  }

  saving.value = true
  message.value = ''
  isError.value = false

  const payload = {
    gameType: 'tabletop',
    players: players.map(player => ({
      playerNumber: player.playerNumber,
      playerName: player.playerName,
      buildings: buildings.value.map(building => ({
        building: building.id,
        count: Number(player.buildings[building.key].count || 0),
        insideWallCount: building.key === 'Tower'
          ? Number(player.buildings[building.key].count || 0)
          : player.wallCompleted ? Number(player.buildings[building.key].insideWallCount || 0) : 0
      })),
      marketBonus: bonusValue(player, 'marketBonus'),
      guildResources: Number(player.guildResources || 0),
      universityCultureLevel: Number(player.universityCultureLevel || 0),
      cathedralDiceCount: cathedralDiceCount(player),
      cultureIcons: Number(player.cultureIcons || 0),
      walledAreaSize: player.wallCompleted ? Number(player.walledAreaSize || 0) : 0,
      disasterPoints: Number(player.disasterPoints || 0),
      wallCompleted: player.wallCompleted
    }))
  }

  try {
    const response = await fetch('/api/GameScore', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    })
    if (!response.ok) {
      const details = await response.text()
      throw new Error(details || `Ошибка сервера: ${response.status}`)
    }

    const result = await response.json()
    message.value = `Результаты сохранены (игра №${result.id}).`
    emit('saved', result.id)
  } catch (error) {
    isError.value = true
    message.value = `Не удалось сохранить результаты: ${error.message}`
  } finally {
    saving.value = false
  }
}
</script>

<style scoped>
  .table-container {
    overflow-x: auto;
    margin: 14px 0 0;
  }

  .custom-table {
    width: 100%;
    border-collapse: collapse;
    font-family: var(--font-main);
    font-size: 14px;
    color: #3f3020;
  }

  .custom-table th,
  .custom-table td {
    text-align: center;
    vertical-align: middle;
  }
  .custom-table input[type="number"] {
    box-sizing: border-box;
    max-width: 80px;
    width: 100%;
  }
  .custom-table input.player-name {
    display: block;
    margin-top: 6px;
    max-width: 150px;
    width: 100%;
  }
  .player-name-control {
    display: flex;
    align-items: flex-end;
    justify-content: center;
    gap: 6px;
  }
  .custom-table .player-name-control input.player-name {
    min-width: 0;
    margin-top: 6px;
  }
  .custom-table input.player-name.player-status-unknown {
    background-color: #fffdf6;
  }
  .custom-table input.player-name.player-status-offline {
    background-color: #dcecf8;
  }
  .custom-table input.player-name.player-status-registered {
    background-color: #e1f1dc;
  }
  .add-player-button {
    width: 34px;
    height: 34px;
    flex: 0 0 34px;
    padding: 0;
    font-family: Arial, sans-serif;
    font-size: 1.4rem;
    line-height: 1;
  }
  .building-inputs {
    display: flex;
    justify-content: center;
    gap: 8px;
  }
  .building-input-field {
    position: relative;
    display: inline-flex;
    width: 58px;
    flex: 0 0 58px;
  }
  .building-watermark {
    position: absolute;
    z-index: 1;
    top: 50%;
    left: 5px;
    opacity: 0.32;
    font-size: 15px;
    line-height: 1;
    transform: translateY(-50%);
    pointer-events: none;
  }
  .building-inputs label,
  .custom-table td label {
    display: inline-flex;
    align-items: center;
    flex-direction: column;
    gap: 4px;
    font-size: 11px;
  }
  .custom-table .building-inputs label {
    flex-direction: row;
    flex-shrink: 0;
  }
  .custom-table .building-inputs .building-input-field input[type="number"] {
    position: relative;
    z-index: 2;
    width: 100%;
    height: 34px;
    padding: 5px 5px 5px 23px;
    color: #2d2117;
    font-size: 14px;
    font-weight: bold;
    text-align: right;
    background: rgba(255, 253, 246, 0.88);
    border: 1px solid #bda77d;
    border-radius: 6px;
    box-shadow: inset 0 1px 3px rgba(90, 64, 32, 0.12);
  }
  .custom-table .building-inputs .building-input-field input[type="number"]:focus {
    border-color: var(--era-orange);
    box-shadow: 0 0 0 2px rgba(243, 146, 0, 0.28);
  }
  .custom-table .wall-completed-toggle {
    display: inline-flex;
    flex-direction: row;
    align-items: center;
    justify-content: center;
    gap: 8px;
    white-space: nowrap;
    cursor: pointer;
  }
  .wall-completion-field {
    height: 34px;
  }
  .wall-completion-checkbox {
    position: absolute;
    z-index: 2;
    inset: 0;
    width: 100%;
    height: 34px;
    margin: 0;
    appearance: none;
    background: rgba(255, 253, 246, 0.88);
    border: 1px solid #bda77d;
    border-radius: 6px;
    box-shadow: inset 0 1px 3px rgba(90, 64, 32, 0.12);
    cursor: pointer;
  }
  .wall-completion-checkbox:checked {
    background: rgba(225, 241, 220, 0.78);
  }
  .wall-completion-checkbox:checked::after {
    position: absolute;
    top: 50%;
    right: 6px;
    color: #3d5128;
    content: '✓';
    font-size: 19px;
    font-weight: bold;
    line-height: 1;
    transform: translateY(-55%);
  }
  .wall-completion-checkbox:focus-visible {
    outline: 2px solid var(--era-orange);
    outline-offset: 2px;
  }
  .custom-table td strong {
    display: block;
    margin-top: 4px;
  }
  .score-indicator {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 6px;
    white-space: nowrap;
  }
  .score-indicator > strong {
    margin: 0;
    color: #4a3218;
    font-size: 1rem;
  }
  .resource-icons {
    display: inline-flex;
    align-items: center;
    gap: 2px;
    font-size: 18px;
    line-height: 1;
    white-space: nowrap;
  }
  .title-cell {
    min-width: 130px;
  }
  .player-title {
    color: #8a5b00;
    font-size: 16px;
  }
  .title-cell span {
    display: block;
    font-size: 12px;
    margin-top: 4px;
  }
  form > button {
    margin-top: 16px;
    background: linear-gradient(180deg, #55703a 0%, #3d5128 100%);
  }
  form > button:hover:not(:disabled) {
    background: linear-gradient(180deg, #648345 0%, #4a6230 100%);
    transform: translateY(-1px);
  }
  form > button:active:not(:disabled) {
    transform: translateY(1px);
    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.35);
  }
  .game-type-note {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    margin: 4px 0 8px;
    font-weight: bold;
    color: #4a3218;
  }
  .game-type-note strong {
    color: #6d4c2f;
  }
  form > button:disabled {
    cursor: wait;
  }
  .error {
    color: #b00020;
  }
  .custom-table th.header-empty {
    border: none;
  }

  /* Заголовочные ячейки */
  .header-empty {
    background-color: transparent;    
  }

  .header-group {
    background: linear-gradient(180deg, #e7d7b4 0%, #dcc79b 100%);
    color: #4a3218;
    font-weight: bold;
    font-size: 16px;
  }

  /* Цвета строк */
  .row-color-blue {
    background-color: #e6f7ff; /* светло-голубой */
  }

  .row-color-yellow {
    background-color: #fffde7; /* светло-желтый */
  }

  .row-color-green {
    background-color: #e8f5e9; /* светло-зеленый */
  }

  .row-color-purple {
    background-color: #f3e5f5; /* светло-фиолетовый */
  }

  .row-color-white {
    background-color: #fff; /* белый */
  }

  .col-color-black{
    background-color: #000;
    color: #fff;
  }

  /* Объединенная ячейка */
  .merged-cell {
    background-color: #e1f5fe;
    font-weight: bold;
    vertical-align: middle;
    width: 50px;
  }

  .hidden-cell {
    display: none;
  }

  /* Альтернативный вариант для визуализации границ */
  .custom-table tbody tr:hover {
    background-color: rgba(243, 146, 0, 0.16);
  }

  /* Номера строк */
  .custom-table td:first-child {
    font-weight: bold;
    font-size: 12px;
    text-align: left;
    background-color: #f4ecd9;
    width: 200px;
  }
</style>
