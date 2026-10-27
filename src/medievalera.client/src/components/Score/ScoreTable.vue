<template>
  <div class="table-container">
    <form @submit.prevent="saveScore">
    <p class="game-type-note">
      Тип игры: <strong>Настольная</strong>
    </p>
    <table class="custom-table">
      <thead>
        <tr>
          <th>Здание / результат</th>
          <th>Очки</th>
          <th v-for="player in players" :key="player.playerNumber" class="header-group">
            <label :for="`player-name-${player.playerNumber}`">Игрок {{ player.playerNumber }}</label>
            <input :id="`player-name-${player.playerNumber}`"
                   v-model.trim="player.playerName"
                   class="player-name"
                   maxlength="50"
                   required>
          </th>
        </tr>
      </thead>

      <tbody>
        <tr v-for="building in buildings" :key="building.key" class="row-color-blue">
          <td>{{ building.label }}</td>
          <td>{{ building.points || 'Не учитывается' }}</td>
          <td v-for="player in players" :key="player.playerNumber">
            <div class="building-inputs">
              <label>
                Построено
                <input v-model.number="player.buildings[building.key].count"
                       type="number" min="0" step="1" required>
              </label>
              <label>
                В стене
                <input v-model.number="player.buildings[building.key].insideWallCount"
                       type="number" min="0" step="1"
                       :max="player.buildings[building.key].count" required>
              </label>
            </div>
            <strong>{{ buildingTotal(player, building) }}</strong>
          </td>
        </tr>

        <tr v-for="bonus in quantityBonuses" :key="bonus.key" class="row-color-yellow">
          <td>{{ bonus.label }}</td>
          <td>1 за единицу</td>
          <td v-for="player in players" :key="player.playerNumber">
            <label>
              {{ bonus.inputLabel }}
              <input v-model.number="player[bonus.key]"
                     type="number" min="0" step="1" required>
            </label>
            <strong>{{ player[bonus.key] || 0 }}</strong>
          </td>
        </tr>

        <tr class="row-color-green">
          <td>Наибольшая область, окруженная стеной</td>
          <td>+10 очков лидеру(ам)</td>
          <td v-for="player in players" :key="player.playerNumber">
            <label>
              Точек в области
              <input v-model.number="player.walledAreaSize"
                     type="number" min="0" step="1" required>
            </label>
            <strong>Бонус: {{ walledAreaBonus(player) }}</strong>
          </td>
        </tr>
        <tr class="row-color-green">
          <td>Наиболее развитая культура</td>
          <td>+5 очков лидеру(ам)</td>
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
          <td>вычитаются</td>
          <td v-for="player in players" :key="player.playerNumber">
            <input v-model.number="player.disasterPoints"
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
    <button type="submit" :disabled="saving">
      {{ saving ? 'Сохранение...' : 'Сохранить результаты' }}
    </button>
    <p v-if="message" :class="{ error: isError }" role="status">{{ message }}</p>
    </form>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue'

const buildings = [
  { id: 3, key: 'Tower', label: 'Башня', points: 1 },
  { id: 4, key: 'House', label: 'Дом', points: 1 },
  { id: 5, key: 'Farm', label: 'Ферма', points: 1 },
  { id: 6, key: 'Church', label: 'Церковь', points: 1 },
  { id: 7, key: 'Monastery', label: 'Монастырь', points: 4 },
  { id: 8, key: 'Cathedral', label: 'Собор', points: 5 },
  { id: 9, key: 'CityHouse', label: 'Городской дом', points: 1 },
  { id: 10, key: 'Guild', label: 'Гильдия', points: 3 },
  { id: 11, key: 'Market', label: 'Рынок', points: 2 },
  { id: 12, key: 'Hospital', label: 'Больница', points: 3 },
  { id: 13, key: 'Sawmill', label: 'Лесопилка', points: 2 },
  { id: 14, key: 'University', label: 'Университет', points: 4 }
]

const quantityBonuses = [
  { key: 'marketBonus', label: 'Бонус рынка', inputLabel: 'Очки' },
  { key: 'guildResources', label: 'Гильдия: оставшиеся ресурсы', inputLabel: 'Ресурсы' },
  { key: 'universityCultureLevel', label: 'Университет: уровень культуры', inputLabel: 'Уровень' },
  { key: 'cathedralDiceCount', label: 'Собор: количество кубиков', inputLabel: 'Кубики' },
  { key: 'cultureIcons', label: 'Культура: количество иконок', inputLabel: 'Иконки' }
]

const players = reactive(Array.from({ length: 4 }, (_, index) => ({
  playerNumber: index + 1,
  playerName: `Игрок ${index + 1}`,
  buildings: Object.fromEntries(buildings.map(building => [
    building.key,
    { count: 0, insideWallCount: 0 }
  ])),
  marketBonus: 0,
  guildResources: 0,
  universityCultureLevel: 0,
  cathedralDiceCount: 0,
  cultureIcons: 0,
  walledAreaSize: 0,
  disasterPoints: 0
})))

const saving = ref(false)
const message = ref('')
const isError = ref(false)

function buildingTotal(player, building) {
  const score = player.buildings[building.key]
  return building.points * (Number(score.count || 0) + Number(score.insideWallCount || 0))
}

function intermediateTotal(player) {
  const buildingsTotal = buildings.reduce(
    (total, building) => total + buildingTotal(player, building),
    0
  )
  return buildingsTotal +
    Number(player.marketBonus || 0) +
    Number(player.guildResources || 0) +
    Number(player.universityCultureLevel || 0) +
    Number(player.cathedralDiceCount || 0) +
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
  return awardForHighest('walledAreaSize', player, 10)
}

function titleForScore(score) {
  if (score < 45) return { title: 'Раб', comment: 'Терпимо. Продолжайте возделывать землю.' }
  if (score <= 64) return { title: 'Крестьянин', comment: 'Неплохое начало. Вы хотя бы свободны.' }
  if (score <= 79) return { title: 'Торговец', comment: 'Хорошее предложение. Повысим ставки?' }
  if (score <= 89) return { title: 'Рыцарь', comment: 'Отличный турнир. Сумеете лучше в следующий раз?' }
  if (score <= 94) return { title: 'Знать', comment: 'Величественное представление!' }
  if (score <= 99) return { title: 'Монарх', comment: 'Королевское достижение!' }
  return { title: 'Папа', comment: 'Ваши кубики, несомненно, были благословлены!' }
}

async function saveScore() {
  if (players.some(player =>
    buildings.some(building =>
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
      buildings: buildings.map(building => ({
        building: building.id,
        count: Number(player.buildings[building.key].count || 0),
        insideWallCount: Number(player.buildings[building.key].insideWallCount || 0)
      })),
      marketBonus: Number(player.marketBonus || 0),
      guildResources: Number(player.guildResources || 0),
      universityCultureLevel: Number(player.universityCultureLevel || 0),
      cathedralDiceCount: Number(player.cathedralDiceCount || 0),
      cultureIcons: Number(player.cultureIcons || 0),
      walledAreaSize: Number(player.walledAreaSize || 0),
      disasterPoints: Number(player.disasterPoints || 0)
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
    border: 1px solid #c9b48c;
    padding: 8px 12px;
    text-align: center;
    vertical-align: middle;
  }
  .custom-table input[type="number"],
  .custom-table input.player-name {
    box-sizing: border-box;
    padding: 5px 6px;
    font-family: var(--font-main);
    font-size: 13px;
    color: #3f3020;
    background: #fffdf6;
    border: 1px solid #c2ab80;
    border-radius: 5px;
    transition: border-color 0.2s ease, box-shadow 0.2s ease;
  }
  .custom-table input[type="number"] {
    max-width: 80px;
    width: 100%;
  }
  .custom-table input.player-name {
    display: block;
    margin-top: 6px;
    max-width: 150px;
    width: 100%;
  }
  .custom-table input:focus {
    outline: none;
    border-color: var(--era-orange);
    box-shadow: 0 0 0 2px rgba(243, 146, 0, 0.28);
  }
  .building-inputs {
    display: flex;
    justify-content: center;
    gap: 8px;
  }
  .building-inputs label,
  .custom-table td label {
    display: inline-flex;
    align-items: center;
    flex-direction: column;
    gap: 4px;
    font-size: 11px;
  }
  .custom-table td strong {
    display: block;
    margin-top: 4px;
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
    padding: 10px 22px;
    font-family: var(--font-main);
    font-size: 1rem;
    font-weight: bold;
    letter-spacing: 0.3px;
    color: #fff8dc;
    background: linear-gradient(180deg, #55703a 0%, #3d5128 100%);
    border: 2px solid #d2b48c;
    border-radius: 8px;
    box-shadow: 0 3px 8px rgba(0, 0, 0, 0.35), inset 0 1px 0 rgba(255, 255, 255, 0.2);
    cursor: pointer;
    transition: background 0.2s ease, box-shadow 0.2s ease, transform 0.2s ease;
  }
  form > button:hover:not(:disabled) {
    background: linear-gradient(180deg, #648345 0%, #4a6230 100%);
    box-shadow: 0 5px 12px rgba(0, 0, 0, 0.4), inset 0 1px 0 rgba(255, 255, 255, 0.25);
    transform: translateY(-1px);
  }
  form > button:active:not(:disabled) {
    transform: translateY(1px);
    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.35);
  }
  form > button:focus-visible {
    outline: 3px solid rgba(243, 146, 0, 0.6);
    outline-offset: 2px;
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
