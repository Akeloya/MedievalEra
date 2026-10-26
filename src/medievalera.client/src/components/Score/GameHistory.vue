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
  margin: 12px 5px;
}

.search {
  display: flex;
  align-items: center;
  gap: 10px;
  margin: 12px 0;
}

.search input {
  max-width: 320px;
  padding: 7px 10px;
}

.table-scroll {
  overflow-x: auto;
}

table {
  width: 100%;
  border-collapse: collapse;
}

th,
td {
  padding: 10px;
  border: 1px solid #aaa;
  text-align: left;
  vertical-align: top;
}

th {
  background: #e8e0cf;
}

.winners,
.participants {
  margin: 0;
  padding-left: 18px;
}

.pending-warning {
  color: #a34b00;
  font-size: 0.9em;
}

.error {
  color: #b00020;
}
</style>
