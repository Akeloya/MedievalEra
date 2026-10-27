<template>
  <section class="history">
    <label class="search">
      Поиск по игроку
      <input v-model.trim="query" type="search" placeholder="Введите имя игрока">
    </label>

    <p v-if="loading" role="status">Загружаем результаты...</p>
    <p v-else-if="error" class="error" role="alert">{{ error }}</p>
    <p v-else-if="filteredGames.length === 0">Игры не найдены.</p>

    <div v-else class="table-scroll">
      <table>
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
                  <span>{{ winner.title }} </span><strong>{{ winner.playerName }}</strong>
                  <span v-if="!winner.resultAccepted" class="pending-warning">
                    — игрок еще не принял этот результат
                  </span>
                </li>
              </ul>
            </td>
            <td>{{ formatDate(game.createdAt) }}</td>
            <td>{{ game.gameType === 'online' ? 'Онлайн' : 'Настольная' }}</td>
            <td>
              <ul class="participants">
                <li v-for="player in game.players" :key="player.playerName">
                  <span>{{ player.title }} </span><strong>{{ player.playerName }}</strong>
                  <span v-if="!player.resultAccepted" class="pending-warning">
                    (ожидает подтверждения)
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
import { computed, onMounted, ref } from 'vue'

const games = ref([])
const query = ref('')
const loading = ref(true)
const error = ref('')

const filteredGames = computed(() => {
  const search = query.value.toLocaleLowerCase()
  if (!search) return games.value
  return games.value.filter(game =>
    game.players.some(player => player.playerName.toLocaleLowerCase().includes(search))
  )
})

function winners(game) {
  return game.players.filter(player => player.hasTitle)
}

function formatDate(value) {
  return new Date(value).toLocaleString()
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
</script>

<style scoped>
.history {
  margin: 12px 0 0;
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
  padding: 9px 12px;
  font-family: var(--font-main);
  font-size: 1rem;
  color: #3f3020;
  background: #fffdf6;
  border: 2px solid #c9b48c;
  border-radius: 8px;
  box-shadow: inset 0 2px 4px rgba(90, 64, 32, 0.14);
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

.search input::placeholder {
  color: #9c8a6f;
  font-style: italic;
}

.search input:focus {
  outline: none;
  border-color: var(--era-orange);
  box-shadow:
    0 0 0 3px rgba(243, 146, 0, 0.25),
    inset 0 2px 4px rgba(90, 64, 32, 0.12);
}

.table-scroll {
  overflow-x: auto;
  background: #fffdf7;
  border: 1px solid #c9b48c;
  border-radius: 8px;
}

table {
  width: 100%;
  border-collapse: collapse;
  color: #3f3020;
}

th,
td {
  padding: 10px 12px;
  border: 1px solid #d8c6a2;
  text-align: left;
  vertical-align: top;
}

th {
  background: linear-gradient(180deg, #e7d7b4 0%, #dcc79b 100%);
  color: #4a3218;
  font-weight: bold;
}

tbody tr:nth-child(even) {
  background: rgba(246, 238, 219, 0.6);
}

.winners,
.participants {
  margin: 0;
  padding-left: 18px;
}

.pending-warning {
  color: #8c4a00;
  font-size: 0.9em;
}

.error {
  color: #b00020;
}
</style>
