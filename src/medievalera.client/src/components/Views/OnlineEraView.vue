<template>
  <section class="online-era">
    <p v-if="loading" role="status">Загрузка планшета…</p>
    <div v-if="playerDeck" class="construction-layout" :class="{ 'carrying-building': selectedBuilding }">
      <aside class="building-catalog" aria-label="Здания">
        <div class="rotation-hint" title="← / → — повернуть здание; Esc — отменить" aria-label="Поворот здания клавишами стрелок">
          <kbd>←</kbd><span aria-hidden="true">↻</span><kbd>→</kbd>
        </div>
        <button v-for="item in buildingCatalog" :key="item.type + item.mask" type="button"
          class="building-option" :class="{ selected: selectedBuilding === item }"
          :title="item.name" :aria-label="`${item.name}, осталось ${item.remaining}`"
          :disabled="!isOwnBoard || placing || item.remaining === 0" :draggable="false"
          @click="clickCatalog(item)" @pointerdown="startPointerDrag($event, item)" @dragstart.prevent>
          <svg :viewBox="buildingGrid.viewBox" :style="{ width: `${buildingGrid.width}px`, height: `${buildingGrid.height}px` }" aria-hidden="true">
            <path :d="catalogPaths(item).fill" :fill="buildingColor(item.type)" />
            <path :d="catalogPaths(item).outline" class="building-outline" />
          </svg>
          <span class="building-count">{{ item.remaining }}</span>
        </button>
      </aside>
    <div class="board-column">
      <nav class="board-player-panel" aria-label="Просмотр планшетов игроков">
        <button v-for="deck in otherPlayerDecks" :key="deck.player.id" type="button"
          :disabled="placing" :aria-pressed="viewedPlayerId === deck.player.id"
          :title="viewedPlayerId === deck.player.id ? 'Вернуться к своему планшету' : `Посмотреть планшет: ${deck.player.name}`"
          @click="toggleViewedPlayer(deck.player.id)">{{ deck.player.name }}</button>
      </nav>
    <div class="board-frame" :aria-label="boardDescription">
      <svg ref="boardSvg" class="game-board" viewBox="0 0 360 430" xmlns="http://www.w3.org/2000/svg"
        @mousedown.prevent @selectstart.prevent @dragstart.prevent
        :style="{ touchAction: selectedBuilding ? 'none' : 'auto' }"
        @touchstart="startTouch" @touchmove="moveTouch" @touchend="endTouch" @touchcancel="cancelTouch"
        @pointermove="updateHover" @pointerleave="hoverCell = null" @click="clickBoard">
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
          <rect x="8" y="67" width="18" height="16" rx="2" /><text x="17" y="80">{{ diceIcons.stone }}</text>
          <rect x="8" y="86" width="18" height="16" rx="2" /><text x="17" y="99">{{ diceIcons.wood }}</text>
          <rect x="8" y="105" width="18" height="16" rx="2" /><text x="17" y="118">{{ diceIcons.meal }}</text>
        </g>
        <g class="top-lane">
          <template v-for="(resource, row) in materialResources" :key="resource.resource">
            <template v-for="value in resource.maxValue + 1" :key="value">
              <circle :cx="34 + (value - 1) * 23" :cy="56 + row * 19" r="3" />
              <text :x="34 + (value - 1) * 23" :y="67 + row * 19">{{ value - 1 }}</text>
            </template>
            <circle class="resource-marker" :cx="34 + resource.count * 23" :cy="56 + row * 19" r="5" :style="{ fill: resource.color }">
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
        <g v-for="building in placedBuildings" :key="building.id" class="placed-building"
          :class="{ moving: movingBuildingId === building.id, movable: isOwnBoard && !building.locked && !building.isInsideWall }"
          @pointerdown.stop="startPlacedDrag($event, building)"
          @click.stop @dblclick.stop.prevent="removeBuilding(building)">
          <title>{{ building.name }}</title>
          <path :d="shapePaths(building.cells).fill" :fill="buildingColor(building.type)" />
          <path :d="shapePaths(building.cells).outline" class="building-outline" />
        </g>
        <g v-if="previewCells.length" class="placement-preview" :class="{ invalid: !validPlacement }">
          <path :d="shapePaths(previewCells).fill" :fill="validPlacement ? buildingColor(selectedBuilding.type) : '#e35d4f'" />
          <path :d="shapePaths(previewCells).outline" class="building-outline" />
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
    </div>
      <aside class="action-history" aria-label="История действий">
        <div class="history-filters" role="group" aria-label="Фильтр игроков">
          <button type="button" :aria-pressed="allPlayersSelected" @click="selectAllPlayers">Все</button>
          <button v-for="slot in historyPlayerSlots" :key="slot.player.id" type="button"
            :title="slot.player.name" :aria-pressed="selectedPlayerIds.includes(slot.player.id)"
            @click="toggleHistoryPlayer(slot.player.id)">{{ slot.label }}</button>
        </div>
        <ol class="history-list">
          <li v-for="entry in filteredHistory" :key="entry.id">
            <div class="history-meta"><strong>{{ entry.playerName }}</strong><span>Раунд {{ entry.round }}</span></div>
            <p>{{ entry.description }}</p>
            <div v-for="(die, index) in entry.dice" :key="index" class="history-die" :title="die.name">
              <span v-for="([resource, count], resourceIndex) in Object.entries(die.values)" :key="resource">
                {{ resourceIndex ? (die.choose ? ' / ' : ' + ') : '' }}{{ diceIcons[resource.toLowerCase()] || resource }} ×{{ count }}
              </span>
            </div>
          </li>
        </ol>
        <p v-if="!filteredHistory.length" class="history-empty">Пока нет действий</p>
      </aside>
    </div>
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
import { computed, ref, watch, onMounted, onUnmounted } from 'vue'
import { maskCells, shapePaths, canPlace, cellSize, catalogGrid, catalogShape } from './buildingGeometry.js'
import { buildingColor, rotationDelta } from './buildingInteraction.js'
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
const playerDecks = ref([])
const viewedPlayerId = ref(null)
const viewedDeck = computed(() => playerDecks.value.find(deck => deck.player.id === viewedPlayerId.value) ?? playerDeck.value)
const isOwnBoard = computed(() => viewedPlayerId.value === null || viewedPlayerId.value === playerDeck.value?.player.id)
const otherPlayerDecks = computed(() => playerDecks.value.filter(deck => !deck.hasLeft && deck.player.id !== playerDeck.value?.player.id))
function toggleViewedPlayer(id) {
  if (placing.value) return
  cancelPlacement()
  viewedPlayerId.value = viewedPlayerId.value === id ? null : id
}
const buildingCatalog = ref([])
const selectedBuilding = ref(null)
const movingBuildingId = ref(null)
let grabOffset = { x: 0, y: 0 }
const rotation = ref(0)
const hoverCell = ref(null)
const boardSvg = ref(null)
const placing = ref(false)
const history = ref([])
const historyPlayers = ref([])
const selectedPlayerIds = ref([])
const historyPlayerSlots = computed(() => {
  const me = historyPlayers.value.find(player => player.id === playerDeck.value?.player.id)
  const others = historyPlayers.value.filter(player => player.id !== me?.id)
  return [...(me ? [{ label: 'Я', player: me }] : []), ...others.map((player, index) => ({ label: String(index + 1), player }))]
})
const allPlayersSelected = computed(() => historyPlayers.value.length > 0 && historyPlayers.value.every(player => selectedPlayerIds.value.includes(player.id)))
const filteredHistory = computed(() => history.value.filter(entry => selectedPlayerIds.value.includes(entry.playerId)).slice().reverse())
function selectAllPlayers() { selectedPlayerIds.value = historyPlayers.value.map(player => player.id) }
function toggleHistoryPlayer(id) {
  selectedPlayerIds.value = selectedPlayerIds.value.includes(id)
    ? selectedPlayerIds.value.filter(value => value !== id) : [...selectedPlayerIds.value, id]
}
function updateHistory(board) {
  // An older polling response must not replace the result of a completed action.
  if ((board.history?.length ?? 0) < history.value.length) return
  history.value = board.history ?? []
  playerDecks.value = board.players
  if (viewedPlayerId.value !== null && !board.players.some(deck => deck.player.id === viewedPlayerId.value && !deck.hasLeft)) viewedPlayerId.value = null
  historyPlayers.value = board.players.filter(item => !item.hasLeft).map(item => item.player)
}
let pollingHistory = false
async function refreshHistory() {
  if (!playerDeck.value || pollingHistory) return
  const gameId = route.query.gameId
  pollingHistory = true
  try {
    const response = await fetch(`/api/lobby/games/${gameId}/board`)
    if (response.ok) {
      const board = await response.json()
      if (route.query.gameId !== gameId || (board.history?.length ?? 0) < history.value.length) return
      updateHistory(board)
      if (!placing.value) {
        buildingCatalog.value = board.buildingCatalog ?? []
        if (selectedBuilding.value) {
          if (movingBuildingId.value !== null) {
            const ownDeck = board.players.find(item => item.player.id === playerDeck.value?.player.id)
            const moving = ownDeck?.buildings.find(item => item.id === movingBuildingId.value)
            if (!moving || moving.locked || moving.isInsideWall) cancelPlacement()
          } else {
            const selected = buildingCatalog.value.find(item => item.type === selectedBuilding.value.type && item.mask === selectedBuilding.value.mask)
            if (!selected || selected.remaining <= 0) cancelPlacement()
            else selectedBuilding.value = selected
          }
        }
        playerDeck.value = board.players.find(item => item.player.id === playerDeck.value?.player.id) ?? playerDeck.value
      }
    }
  } catch { /* Keep the last history until the next refresh. */ }
  finally { pollingHistory = false }
}
const previewCells = computed(() => selectedBuilding.value && hoverCell.value
  ? maskCells(selectedBuilding.value.mask, rotation.value).map(cell => ({ x: cell.x + hoverCell.value.x, y: cell.y + hoverCell.value.y })) : [])
const validPlacement = computed(() => !!playerDeck.value && canPlace(playerDeck.value.field, previewCells.value, movingBuildingId.value))
const buildingGrid = computed(() => catalogGrid(buildingCatalog.value))
function catalogPaths(item) { return catalogShape(item.mask, buildingGrid.value) }
function selectBuilding(item) {
  if (!isOwnBoard.value || placing.value || item.remaining <= 0) return
  movingBuildingId.value = null
  grabOffset = { x: 0, y: 0 }
  selectedBuilding.value = item; rotation.value = 0; error.value = ''
}
let pointerDrag = null
function boardCell(event) {
  const matrix = boardSvg.value?.getScreenCTM()
  if (!matrix) return null
  const point = new DOMPoint(event.clientX, event.clientY).matrixTransform(matrix.inverse())
  return point.x >= 19.7 && point.x < 280.1 && point.y >= 140.7 && point.y < 401.1
    ? { x: Math.round((point.x - 29) / cellSize), y: Math.round((point.y - 150) / cellSize) } : null
}
function startPlacedDrag(event, building) {
  if (!isOwnBoard.value || placing.value || building.locked || building.isInsideWall || event.button !== 0) return
  const cell = boardCell(event)
  if (!cell) return
  event.preventDefault()
  cancelPlacement()
  pointerDrag = { id: event.pointerId, x: event.clientX, y: event.clientY, moved: false, building,
    offset: { x: cell.x - Math.min(...building.cells.map(item => item.x)), y: cell.y - Math.min(...building.cells.map(item => item.y)) } }
}
function clickCatalog(item) { if (Date.now() >= ignoreClickUntil && selectedBuilding.value !== item) selectBuilding(item) }
function startPointerDrag(event, item) {
  if (!isOwnBoard.value || placing.value || item.remaining <= 0) return
  if (event.pointerType === 'touch') return
  if (event.button !== 0 || placing.value) return
  event.preventDefault()
  if (selectedBuilding.value !== item) selectBuilding(item)
  pointerDrag = { id: event.pointerId, x: event.clientX, y: event.clientY, moved: false }
}
function movePointerDrag(event) {
  if (!pointerDrag || event.pointerId !== pointerDrag.id) return
  pointerDrag.moved ||= Math.hypot(event.clientX - pointerDrag.x, event.clientY - pointerDrag.y) > 4
  if (pointerDrag.building && pointerDrag.moved && movingBuildingId.value === null) {
    selectedBuilding.value = pointerDrag.building
    movingBuildingId.value = pointerDrag.building.id
    rotation.value = 0
    grabOffset = pointerDrag.offset
    error.value = ''
  }
  updateHover(event)
}
function endPointerDrag(event) {
  if (!pointerDrag || event.pointerId !== pointerDrag.id) return
  const moved = pointerDrag.moved
  pointerDrag = null
  ignoreClickUntil = Date.now() + 500
  if (moved && selectedBuilding.value) {
    updateHover(event)
    if (hoverCell.value) dropBuilding(event)
    else if (movingBuildingId.value !== null) cancelPlacement()
  }
}
function rotateBuilding(direction = 1) { if (!placing.value) rotation.value = (rotation.value + direction * 90 + 360) % 360 }
function cancelPlacement() {
  if (!placing.value) {
    pointerDrag = null; selectedBuilding.value = null; hoverCell.value = null
    movingBuildingId.value = null; grabOffset = { x: 0, y: 0 }
  }
}
function handleKey(event) {
  if (!selectedBuilding.value || /INPUT|TEXTAREA|SELECT/.test(event.target.tagName)) return
  if (['ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown'].includes(event.key)) {
    event.preventDefault()
    rotateBuilding(['ArrowLeft', 'ArrowDown'].includes(event.key) ? -1 : 1)
  }
  if (event.key === 'Escape') cancelPlacement()
}
let historyTimer
watch(selectedBuilding, value => document.body.classList.toggle('carrying-building', !!value))
onMounted(() => {
  window.addEventListener('keydown', handleKey, true)
  window.addEventListener('pointermove', movePointerDrag)
  window.addEventListener('pointerup', endPointerDrag)
  window.addEventListener('pointercancel', cancelPlacement)
  window.addEventListener('blur', cancelPlacement)
  historyTimer = window.setInterval(refreshHistory, 3000)
})
onUnmounted(() => {
  window.removeEventListener('keydown', handleKey, true)
  window.removeEventListener('pointermove', movePointerDrag)
  window.removeEventListener('pointerup', endPointerDrag)
  window.removeEventListener('pointercancel', cancelPlacement)
  window.removeEventListener('blur', cancelPlacement)
  window.clearInterval(historyTimer)
  document.body.classList.remove('carrying-building')
})
let touchAngle = null
let touchRotation = 0
let usedRotationGesture = false
let ignoreClickUntil = 0
function fingerAngle(touches) {
  return Math.atan2(touches[1].clientY - touches[0].clientY, touches[1].clientX - touches[0].clientX) * 180 / Math.PI
}
function startTouch(event) {
  if (pointerDrag?.building) return
  if (!selectedBuilding.value || placing.value) return
  event.preventDefault()
  if (event.touches.length >= 2) {
    touchAngle = fingerAngle(event.touches)
    touchRotation = rotation.value
    usedRotationGesture = true
  } else {
    usedRotationGesture = false
    updateHover(event.touches[0])
  }
}
function moveTouch(event) {
  if (pointerDrag?.building) return
  if (!selectedBuilding.value || placing.value) return
  event.preventDefault()
  if (event.touches.length >= 2 && touchAngle !== null) {
    const delta = rotationDelta(touchAngle, fingerAngle(event.touches))
    rotation.value = ((touchRotation + Math.round(delta / 90) * 90) % 360 + 360) % 360
  } else if (!usedRotationGesture && event.touches.length === 1) updateHover(event.touches[0])
}
function endTouch(event) {
  if (pointerDrag?.building) return
  if (!selectedBuilding.value || placing.value) return
  event.preventDefault()
  ignoreClickUntil = Date.now() + 700
  if (event.touches.length) return
  touchAngle = null
  if (!usedRotationGesture && event.changedTouches.length) dropBuilding(event.changedTouches[0])
  usedRotationGesture = false
}
function cancelTouch() { touchAngle = null; usedRotationGesture = false; hoverCell.value = null; ignoreClickUntil = Date.now() + 700 }
function clickBoard(event) { if (Date.now() >= ignoreClickUntil) dropBuilding(event) }
function updateHover(event) {
  if (!isOwnBoard.value || !selectedBuilding.value || placing.value) return
  const cell = boardCell(event)
  hoverCell.value = cell ? { x: cell.x - grabOffset.x, y: cell.y - grabOffset.y } : null
}
async function dropBuilding(event) {
  if (!isOwnBoard.value || !selectedBuilding.value || placing.value) return
  if (selectedBuilding.value.remaining <= 0) { cancelPlacement(); return }
  updateHover(event)
  if (!hoverCell.value) return
  if (!validPlacement.value) {
    error.value = 'Здание пересекается с другим зданием или выходит за поле.'
    if (movingBuildingId.value !== null) cancelPlacement()
    return
  }
  placing.value = true
  error.value = ''
  const gameId = route.query.gameId
  try {
    const moving = movingBuildingId.value !== null
    const response = await fetch(`/api/lobby/games/${gameId}/buildings${moving ? `/${movingBuildingId.value}` : ''}`, {
      method: moving ? 'PUT' : 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ type: selectedBuilding.value.type, mask: selectedBuilding.value.mask,
        rotation: moving ? (selectedBuilding.value.rotation + rotation.value) % 360 : rotation.value, ...hoverCell.value })
    })
    const result = await response.json()
    if (!response.ok) throw new Error(result.message || 'Не удалось разместить здание.')
    if (route.query.gameId !== gameId) return
    playerDeck.value = result.players.find(item => item.player.id === playerDeck.value.player.id)
    buildingCatalog.value = result.buildingCatalog
    updateHistory(result)
    selectedBuilding.value = null
    hoverCell.value = null
    movingBuildingId.value = null
    grabOffset = { x: 0, y: 0 }
  } catch (cause) { error.value = cause.message }
  finally {
    placing.value = false
    if (movingBuildingId.value !== null) cancelPlacement()
  }
}
async function removeBuilding(building) {
  if (!isOwnBoard.value || placing.value || !playerDeck.value?.allowToRemoveBuilding || building.locked || building.id === 0 || building.isInsideWall) return
  cancelPlacement()
  placing.value = true
  error.value = ''
  const gameId = route.query.gameId
  const playerId = playerDeck.value.player.id
  try {
    const response = await fetch(`/api/lobby/games/${gameId}/buildings/${building.id}`, { method: 'DELETE' })
    if (!response.ok) {
      const result = await response.json().catch(() => null)
      throw new Error(result?.message || 'Не удалось удалить здание.')
    }
    const board = await response.json()
    if (route.query.gameId !== gameId) return
    playerDeck.value = board.players.find(item => item.player.id === playerId)
    buildingCatalog.value = board.buildingCatalog
    updateHistory(board)
  } catch (cause) { error.value = cause.message }
  finally { placing.value = false }
}
const resourceStyles = {
  goods: { label: 'Товары', color: '#62c9ed' },
  stone: { label: 'Камень', color: '#9299a2' },
  wood: { label: 'Дерево', color: '#88502c' },
  meal: { label: 'Еда', color: '#ffda32' },
  culture: { label: 'Культура', color: '#ffffff' },
  skull: { label: 'Черепа', color: '#f58b2b' }
}
const resources = computed(() => Object.entries(resourceStyles).flatMap(([key, style]) => {
  const resource = viewedDeck.value?.resources.find(item => item.resource === key)
  return resource ? [{ ...resource, ...style }] : []
}))
const materialResources = computed(() => resources.value.filter(resource => !['culture', 'skull'].includes(resource.resource)))
const scoreResources = computed(() => resources.value.filter(resource => ['culture', 'skull'].includes(resource.resource)))
const placedBuildings = computed(() => (viewedDeck.value?.buildings ?? []).map(building => ({
  ...building,
  cells: viewedDeck.value.field.flatMap((row, y) => row.flatMap((id, x) => id === building.id ? [{ x, y }] : []))
})))
const boardDescription = computed(() => `Планшет игрока ${viewedDeck.value?.player.name}. ${resources.value.map(resource => `${resource.label}: ${resource.count}`).join(', ')}. Построено: ${placedBuildings.value.map(building => building.name).join(', ')}`)

watch(() => route.query.gameId, async (gameId, _, onCleanup) => {
  const controller = new AbortController()
  onCleanup(() => controller.abort())
  playerDeck.value = null
  playerDecks.value = []
  viewedPlayerId.value = null
  buildingCatalog.value = []
  history.value = []
  historyPlayers.value = []
  selectedPlayerIds.value = []
  cancelPlacement()
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
    buildingCatalog.value = board.buildingCatalog ?? []
    updateHistory(board)
    selectAllPlayers()
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
.game-board > :not(.main-field):not(.field-dot):not(.placed-building) { pointer-events: none; }
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
.placed-building.movable { cursor: grab; touch-action: none; }
.placed-building.moving { opacity: .2; }
.game-board .resource-marker { stroke: #514633; stroke-width: 1.2; }
.construction-layout { display: flex; align-items: flex-start; justify-content: center; gap: 16px; }
.rotation-hint { display: flex; align-items: center; justify-content: center; gap: 8px; min-height: 30px; color: #ffe39b; }
.rotation-hint kbd { padding: 1px 6px; border: 1px solid #aa9467; border-radius: 3px; background: #49402e; font-size: 17px; }
:global(body.carrying-building), :global(body.carrying-building *) { cursor: grabbing !important; user-select: none; }
.action-history { flex: 0 1 230px; min-width: 150px; height: min(790px, 80vh); display: flex; flex-direction: column; padding: 8px; border: 1px solid #d2b48c; border-radius: 10px; background: rgba(44, 39, 29, .85); text-align: left; }
.history-filters { display: flex; gap: 3px; margin-bottom: 8px; }
.history-filters button { flex: 1; padding: 5px 3px; color: #fff8dc; border: 1px solid #aa9467; border-radius: 4px; background: #49402e; cursor: pointer; }
.history-filters button[aria-pressed="true"] { background: #92723e; border-color: #ffe39b; }
.history-filters button:disabled { opacity: .35; cursor: default; }
.history-list { list-style: none; padding: 0; margin: 0; overflow-y: auto; font-size: 13px; }
.history-list li { padding: 8px 0; border-bottom: 1px solid #756346; }
.history-meta { display: flex; flex-wrap: wrap; justify-content: space-between; gap: 4px; color: #e4c998; font-size: 12px; }
.history-list p { margin: 4px 0; }
.history-die { display: inline-block; margin: 2px; padding: 3px; border-radius: 3px; background: #514431; }
.history-empty { color: #d2b48c; font-size: 13px; }
.building-catalog { flex: 0 0 120px; max-height: 790px; overflow-y: auto; display: flex; flex-direction: column; gap: 3px; padding: 5px; border: 1px solid #d2b48c; border-radius: 10px; background: #30291f; }
.building-option { display: flex; flex-direction: row; align-items: center; gap: 6px; min-height: 38px; padding: 4px 6px; color: #fff8dc; background: #49402e; border: 1px solid #aa9467; border-radius: 4px; cursor: grab; user-select: none; }
.building-option.selected { border-color: #ffe36a; background: #66572e; }
.building-option:disabled { opacity: .45; cursor: default; pointer-events: none; }
.building-option svg { flex: 0 0 auto; pointer-events: none; }
.building-count { flex: 0 0 22px; text-align: right; font-variant-numeric: tabular-nums; }
.building-outline { fill: none; stroke: #5c4b37; stroke-width: 1.2; stroke-linejoin: round; }
.placement-preview { pointer-events: none; opacity: .85; }
.board-column { min-width: 0; flex: 0 1 650px; }
.board-player-panel { display: flex; justify-content: center; width: 100%; gap: 0; margin-bottom: 8px; min-height: 38px; padding: 4px 0; border: 1px solid #d2b48c; border-radius: 8px; background: rgba(44, 39, 29, .85); box-sizing: border-box; }
.board-player-panel button { flex: 0 0 25%; max-width: 25%; padding: 6px 2px; border: 1px solid #aa9467; border-radius: 4px; background: #49402e; color: #fff8dc; cursor: pointer; overflow-wrap: anywhere; }
.board-player-panel button[aria-pressed="true"] { border-color: #ffe39b; background: #92723e; }
.construction-layout .board-frame { box-sizing: border-box; width: 100%; margin: 0 0 16px; }
@media (max-width: 600px) {
  .construction-layout { gap: 6px; flex-wrap: wrap; }
  .building-catalog { flex-basis: 104px; padding: 3px; max-height: 65vh; }
  .board-column { flex-basis: calc(100% - 134px); }
  .action-history { flex-basis: 100%; height: 280px; }
  .building-option { font-size: 12px; padding: 4px; }
}
.player-name, .building-summary { color: #f5deb3; }
.leave-button { margin-top: 4px; }
.leave-confirmation { max-width: 650px; margin: 4px auto; padding: 16px; border: 1px solid #d2b48c; border-radius: 10px; background: rgba(44, 39, 29, .85); }
.leave-confirmation p { margin: 0 0 14px; }
.leave-actions { display: flex; justify-content: center; flex-wrap: wrap; gap: 10px; }
.successor-choice { display: flex; justify-content: center; align-items: center; flex-wrap: wrap; gap: 8px; margin-bottom: 14px; }
.successor-choice select { padding: 7px; color: #fff8dc; background: #3c3529; border: 1px solid #d2b48c; border-radius: 5px; }
.error { color: #ffb3a9; }
</style>
