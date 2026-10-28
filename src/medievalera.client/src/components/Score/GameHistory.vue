<template>
  <section class="history">
    <section v-if="topPlayers.length" class="top-player-statistics">
      <h2>Статистика топ 4 игроков</h2>
      <div class="top-player-cards">
        <article v-for="player in topPlayers" :key="player.playerName" class="top-player-card">
          <h3>{{ player.playerName }}</h3>
          <p>Игр проведено: <strong>{{ player.statistics.gamesPlayed }}</strong></p>
          <p v-if="player.statistics.wins > 0">
            Победил в {{ player.statistics.wins }} из {{ player.statistics.gamesPlayed }}
          </p>
          <p v-else>Пока еще не побеждал</p>
          <p>Чаще всего становился — <strong>{{ player.statistics.mostCommonTitle || '—' }}</strong></p>
          <h4>Становился:</h4>
          <ul>
            <li v-for="title in player.statistics.titles" :key="title.title">
              {{ title.title }} — {{ title.count }} {{ timesWord(title.count) }}
            </li>
          </ul>
        </article>
      </div>
    </section>

    <label class="search">
      Поиск по игроку
      <input class="era-input" v-model.trim="query" type="search" placeholder="Введите имя игрока">
    </label>

    <p v-if="loading" role="status">Загружаем результаты...</p>
    <p v-else-if="error" class="error" role="alert">{{ error }}</p>
    <p v-else-if="filteredGames.length === 0">Игры не найдены.</p>

    <div v-else class="table-scroll">
      <table class="era-table">
        <thead>
          <tr>
            <th>Победитель</th>
            <th>Дата игры</th>
            <th>Тип игры</th>
            <th>Участники</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="game in filteredGames" :key="game.id">
            <td>
              <ul class="winners">
                <li v-for="winner in winners(game)" :key="winner.playerName">
                  <span class="player-title">{{ winner.title }}</span>
                  <strong class="player-name">{{ winner.playerName }}</strong>
                </li>
              </ul>
            </td>
            <td>{{ formatDate(game.createdAt) }}</td>
            <td>{{ game.gameType === 'online' ? 'Онлайн' : 'Настольная' }}</td>
            <td>
              <ul class="participants">
                <li v-for="player in game.players" :key="player.playerName">
                  <span class="player-title">{{ player.title }}</span>
                  <strong class="player-name">{{ player.playerName }}</strong>
                  <span v-if="!player.resultAccepted"
                        class="pending-indicator"
                        title="Игрок ещё не принял этот результат"
                        aria-label="Игрок ещё не принял этот результат">
                    ?
                  </span>
                </li>
              </ul>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>

<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()

const props = defineProps({
  refreshKey: {
    type: Number,
    default: 0
  }
})

const games = ref([])
const query = ref(typeof route.query.player === 'string' ? route.query.player : '')
const loading = ref(true)
const error = ref('')

const filteredGames = computed(() => {
  const search = query.value.toLocaleLowerCase()
  if (!search) return games.value
  return games.value.filter(game =>
    game.players.some(player => player.playerName.toLocaleLowerCase().includes(search))
  )
})

const topPlayers = computed(() => {
  const playersByName = new Map()
  for (const game of games.value) {
    for (const player of game.players) {
      const key = player.playerName.toLocaleLowerCase()
      const existing = playersByName.get(key)
      if (!existing || player.statistics.gamesPlayed > existing.statistics.gamesPlayed) {
        playersByName.set(key, { playerName: player.playerName, statistics: player.statistics })
      }
    }
  }

  return [...playersByName.values()]
    .sort((first, second) =>
      second.statistics.gamesPlayed - first.statistics.gamesPlayed ||
      first.playerName.localeCompare(second.playerName)
    )
    .slice(0, 4)
})

function winners(game) {
  return game.players.filter(player => player.hasTitle)
}

function formatDate(value) {
  return new Date(value).toLocaleString()
}

function timesWord(count) {
  const lastTwoDigits = count % 100
  if (lastTwoDigits >= 11 && lastTwoDigits <= 14) return 'раз'
  const lastDigit = count % 10
  if (lastDigit === 1) return 'раз'
  if (lastDigit >= 2 && lastDigit <= 4) return 'раза'
  return 'раз'
}

async function loadGames() {
  loading.value = true
  error.value = ''
  try {
    const response = await fetch('/api/GameScore')
    if (!response.ok) {
      throw new Error(`Не удалось загрузить результаты (ошибка ${response.status}).`)
    }
    games.value = await response.json()
  } catch (loadError) {
    error.value = loadError.message
  } finally {
    loading.value = false
  }
}

onMounted(loadGames)
watch(() => props.refreshKey, loadGames)
watch(() => route.query.player, player => {
  query.value = typeof player === 'string' ? player : ''
})
</script>

<style scoped>
.history {
  margin: 12px 0 0;
}

.top-player-statistics {
  margin-bottom: 18px;
}

.top-player-statistics h2 {
  margin-bottom: 12px;
  color: #4a3218;
}

.top-player-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(min(100%, 230px), 1fr));
  gap: 12px;
}

.top-player-card {
  padding: 14px 16px;
  color: #3f3020;
  background: rgba(255, 253, 246, 0.72);
  border: 1px solid #d8c6a2;
  border-radius: 8px;
}

.top-player-card h3 {
  margin-bottom: 6px;
  color: #4a3218;
  font-size: 1.1rem;
  font-weight: bold;
}

.top-player-card h4 {
  margin-top: 8px;
  color: #4a3218;
  font-weight: bold;
}

.top-player-card ul {
  padding-left: 20px;
}

.search {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 10px;
  margin: 16px 0;
  font-weight: bold;
  color: #4a3218;
}

.search input {
  width: 100%;
  max-width: 320px;
}

.table-scroll {
  overflow-x: auto;
  background: #fffdf7;
  border: 1px solid #c9b48c;
  border-radius: 8px;
}

.winners,
.participants {
  margin: 0;
  padding-left: 18px;
}

.player-title {
  margin-right: 8px;
  color: #765f40;
}

.player-name {
  white-space: nowrap;
}

.pending-indicator {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 17px;
  height: 17px;
  margin-left: 5px;
  color: #8c4a00;
  border: 1px solid currentColor;
  border-radius: 50%;
  font-size: 12px;
  font-weight: bold;
  line-height: 1;
  cursor: help;
}

.error {
  color: #b00020;
}
</style>
