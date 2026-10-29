<template>
  <section class="lobby">
    <template v-if="loadingProfile">
      <p role="status">Проверяем авторизацию...</p>
    </template>
    <template v-else-if="!profile">
      <h1>Игровое лобби</h1>
      <p>Войдите в профиль, чтобы найти игроков или создать игру.</p>
      <div class="auth-switch">
        <button class="era-button" type="button" :aria-pressed="mode === 'login'" @click="mode = 'login'">Войти</button>
        <button class="era-button" type="button" :aria-pressed="mode === 'register'" @click="mode = 'register'">Создать профиль</button>
      </div>
      <form class="auth-form" @submit.prevent="authenticate">
        <label>Имя игрока<input v-model.trim="credentials.name" class="era-input" maxlength="50" autocomplete="username" required></label>
        <label>Пароль<input v-model="credentials.password" class="era-input" type="password" minlength="8" maxlength="128" :autocomplete="mode === 'login' ? 'current-password' : 'new-password'" required></label>
        <button class="era-button" type="submit" :disabled="busy">{{ busy ? 'Подождите...' : mode === 'login' ? 'Войти' : 'Зарегистрироваться' }}</button>
      </form>
      <p v-if="error" class="lobby-error" role="alert">{{ error }}</p>
      <RouterLink class="era-button" :to="{ name: 'profile' }">Перейти в профиль</RouterLink>
    </template>
    <template v-else>
      <header class="lobby-heading">
        <div><h1>Игровое лобби</h1><p>Вы вошли как <strong>{{ profile.name }}</strong></p></div>
        <button class="era-button" type="button" @click="refresh" :disabled="busy">Обновить</button>
      </header>
      <div class="lobby-stats" aria-live="polite">
        <article><strong>{{ lobby.waitingPlayers }}</strong><span>ожидают игры</span></article>
        <article><strong>{{ lobby.playingPlayers }}</strong><span>уже играют</span></article>
        <article><strong>{{ lobby.games.length }}</strong><span>игр в лобби</span></article>
      </div>
      <section class="create-game">
        <h2>Создать игру</h2>
        <p>Выберите комплект правил и формат игры.</p>
        <label>Версия игры
          <select v-model="version" class="era-input">
            <option value="base">Базовая</option>
            <option value="rivers-and-roads">Реки и дороги</option>
          </select>
        </label>
        <label class="opponents-choice"><span>Играть против</span>
          <select v-model="modeChoice" class="era-input">
            <option value="sir-philipp">Сэр Филипп</option>
            <option value="1">1 игрок</option>
            <option value="2">2 игрока</option>
            <option value="3">3 игрока</option>
          </select>
        </label>
        <label v-if="modeChoice !== 'sir-philipp'" class="bot-toggle">
          <input v-model="allowBots" type="checkbox"> Возможна игра против бота
        </label>
        <button class="era-button" type="button" :disabled="busy || myGame" @click="createGame">Зарегистрировать игру</button>
      </section>
      <p v-if="myGame" class="my-game-note">Вы участвуете в игре №{{ myGame.id }}.</p>
      <p v-if="error" class="lobby-error" role="alert">{{ error }}</p>
      <p v-if="loading" role="status">Обновляем список игр...</p>
      <section class="games-section">
        <h2>Ожидают игроков</h2>
        <p v-if="waitingGames.length === 0" class="empty-state">Нет игр, ожидающих игроков.</p>
        <div class="game-list">
          <article v-for="game in waitingGames" :key="game.id" class="game-card">
            <div class="game-card-heading"><h3>Игра №{{ game.id }}</h3><span class="game-status waiting">Ожидание</span></div>
            <p>Версия: <strong>{{ versionName(game.version) }}</strong></p>
            <p>Игроки: {{ game.players.length }} / {{ game.maxPlayers }}</p>
            <ul class="player-list"><li v-for="(player, index) in game.players" :key="`${game.id}-${index}`">{{ player.name }}<span v-if="player.id === game.ownerId"> · организатор</span><span v-if="player.isComputer"> · компьютер</span></li></ul>
            <div class="game-actions">
              <button v-if="game.ownerId === profile.id" class="era-button" type="button" :disabled="busy" @click="startGame(game)">Начать игру</button>
              <button v-else-if="game.players.some(player => player.isWaiting) && !myGame" class="era-button" type="button" :disabled="busy" @click="joinGame(game.id)">Присоединиться</button>
              <button v-if="myGame?.id === game.id" class="era-button secondary" type="button" :disabled="busy" @click="leaveGame(game.id)">Покинуть игру</button>
            </div>
          </article>
        </div>
      </section>
      <section class="games-section">
        <h2>Идут сейчас</h2>
        <p v-if="playingGames.length === 0" class="empty-state">Нет начавшихся игр.</p>
        <div class="game-list">
          <article v-for="game in playingGames" :key="game.id" class="game-card">
            <div class="game-card-heading"><h3>Игра №{{ game.id }}</h3><span class="game-status playing">Идёт игра</span></div>
            <p>Версия: <strong>{{ versionName(game.version) }}</strong></p>
            <p>Участники: {{ game.players.length }} / {{ game.maxPlayers }}</p>
            <ul class="player-list"><li v-for="(player, index) in game.players" :key="`${game.id}-${index}`">{{ player.name }}<span v-if="player.isComputer"> · компьютер</span></li></ul>
          </article>
        </div>
      </section>
    </template>
  </section>
</template>

<script setup>
import { computed, onMounted, onUnmounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const router = useRouter()
const route = useRoute()

const profile = ref(null)
const loadingProfile = ref(true)
const loading = ref(false)
const busy = ref(false)
const mode = ref('login')
const version = ref('base')
const modeChoice = ref('1')
const allowBots = ref(true)
const error = ref('')
const credentials = reactive({ name: '', password: '' })
const lobby = reactive({ waitingPlayers: 0, playingPlayers: 0, games: [] })
const waitingGames = computed(() => lobby.games.filter(game => game.status === 'waiting'))
const playingGames = computed(() => lobby.games.filter(game => game.status === 'playing'))
const myGame = computed(() => lobby.games.find(game => game.players.some(player => player.id === profile.value?.id)))
let refreshTimer

async function loadProfile() {
  loadingProfile.value = true
  try {
    const response = await fetch('/api/PlayerProfile/me')
    if (response.status === 401) { profile.value = null; return }
    if (!response.ok) throw new Error('Не удалось загрузить профиль.')
    profile.value = await response.json()
  } catch (cause) { error.value = cause.message }
  finally { loadingProfile.value = false }
}

async function authenticate() {
  busy.value = true; error.value = ''
  try {
    const response = await fetch(`/api/PlayerProfile/${mode.value}`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(credentials)
    })
    if (!response.ok) throw new Error((await response.text()) || 'Не удалось выполнить вход.')
    credentials.password = ''
    await loadProfile()
    await refresh()
  } catch (cause) { error.value = cause.message }
  finally { busy.value = false }
}

async function refresh() {
  if (!profile.value) return
  loading.value = true
  try {
    const response = await fetch('/api/lobby')
    if (response.status === 401) { profile.value = null; return }
    if (!response.ok) throw new Error(`Не удалось загрузить список игр (HTTP ${response.status}). Проверьте подключение и авторизацию.`)
    Object.assign(lobby, await response.json())
    error.value = ''
    if (route.name === 'online' && myGame.value?.status === 'playing')
      await router.push({ name: 'online-era', query: { gameId: myGame.value.id } })
  } catch (cause) { error.value = cause.message }
  finally { loading.value = false }
}

async function mutate(url, method = 'POST', body, options = {}) {
  busy.value = true; error.value = ''
  try {
    const response = await fetch(url, {
      method,
      ...(body ? { headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) } : {})
    })
    if (!response.ok) {
      let message = await response.text()
      try { message = JSON.parse(message).message || message } catch { /* Plain text server response. */ }
      throw new Error(message || 'Не удалось выполнить действие.')
    }
    const payload = response.status === 204 ? null : await response.json()
    await refresh()
    if (options.enterWhenPlaying && payload?.status === 'playing' && route.name !== 'online-era')
      await router.push({ name: 'online-era', query: { gameId: payload.id } })
    return payload
  } catch (cause) { error.value = cause.message }
  finally { busy.value = false }
}

function createGame() {
  const solo = modeChoice.value === 'sir-philipp'
  return mutate('/api/lobby/games', 'POST', {
    version: version.value,
    mode: solo ? 'sir-philipp' : 'players',
    opponentsCount: solo ? 1 : Number(modeChoice.value),
    allowBots: solo ? false : allowBots.value
  }, { enterWhenPlaying: true })
}
function joinGame(id) { return mutate(`/api/lobby/games/${id}/join`) }
function startGame(game) {
  const hasEmptySeats = game.players.some(player => player.isWaiting)
  const fillWithBots = hasEmptySeats && game.allowBots && window.confirm('Заполнить свободные места ботами?')
  if (hasEmptySeats && !game.allowBots) {
    error.value = 'Дождитесь игроков: для этой игры заполнение ботами отключено.'
    return
  }
  if (hasEmptySeats && !fillWithBots) return
  return mutate(`/api/lobby/games/${game.id}/start?fillWithBots=${fillWithBots}`, 'POST', undefined, { enterWhenPlaying: true })
}
function leaveGame(id) { return mutate(`/api/lobby/games/${id}/players/me`, 'DELETE') }
function versionName(value) { return value === 'rivers-and-roads' ? 'Реки и дороги' : 'Базовая' }

onMounted(async () => {
  await loadProfile()
  if (profile.value) {
    await refresh()
    refreshTimer = window.setInterval(refresh, 5000)
  }
})
onUnmounted(() => window.clearInterval(refreshTimer))
</script>

<style scoped>
.lobby { width: 100%; max-width: 900px; color: #fff8dc; }
.lobby h1, .lobby h2, .lobby h3 { color: #f5deb3; }
.lobby-heading { display: flex; align-items: center; justify-content: space-between; gap: 16px; }
.lobby-heading h1 { margin-bottom: 4px; }
.auth-switch, .game-actions { display: flex; flex-wrap: wrap; gap: 10px; margin: 14px 0; }
.auth-form { display: grid; gap: 14px; max-width: 420px; margin: 20px 0; }
.auth-form label, .create-game label { display: grid; gap: 6px; }
.lobby-stats { display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px; margin: 20px 0; }
.lobby-stats article, .create-game, .game-card { padding: 16px; border: 1px solid #d2b48c; border-radius: 8px; background: rgba(30, 35, 39, .82); }
.lobby-stats article { display: grid; text-align: center; gap: 4px; }
.lobby-stats strong { color: #ffd700; font-size: 1.8rem; }
.create-game { display: grid; gap: 10px; }
.create-game h2, .create-game p { margin: 0; }
.create-game .era-input { max-width: 320px; }
.opponents-choice { display: flex !important; align-items: center; gap: 12px; }
.opponents-choice .era-input { max-width: 260px; }
.bot-toggle { display: flex !important; align-items: center; gap: 8px; }
.games-section { margin-top: 24px; }
.game-list { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 12px; }
.game-card-heading { display: flex; justify-content: space-between; align-items: center; gap: 8px; }
.game-card h3, .game-card p { margin: 8px 0; }
.game-status { padding: 4px 8px; border-radius: 12px; font-size: .85rem; }
.game-status.waiting { background: #755a20; }
.game-status.playing { background: #315c3a; }
.player-list { margin: 8px 0; padding-left: 20px; }
.player-list span { color: #d2b48c; }
.game-actions { margin-bottom: 0; }
.secondary { background: #554b40; }
.empty-state, .refresh-note { color: #d2b48c; }
.refresh-note { margin-top: 20px; font-size: .9rem; }
.my-game-note { color: #ffd700; }
.lobby-error { color: #ffb3a9; }
@media (max-width: 600px) { .lobby-stats { grid-template-columns: 1fr; } .lobby-heading { align-items: flex-start; } }
</style>
