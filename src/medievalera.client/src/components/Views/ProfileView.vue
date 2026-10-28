<template>
  <section class="profile">
    <h1>Профиль игрока</h1>

    <p v-if="loading" role="status">Загрузка профиля...</p>
    <template v-else-if="profile">
      <p>Вы вошли как <strong>{{ profile.name }}</strong>.</p>
      <button class="era-button" type="button" @click="logout">Выйти</button>
      <section class="profile-settings">
        <form v-if="settingsMode === 'name'" @submit.prevent="changeName">
          <h2>Сменить имя игрока</h2>
          <label>
            Новое имя
            <input class="era-input" v-model.trim="newName" maxlength="50" autocomplete="username" required>
          </label>
          <label>
            Текущий пароль
            <input class="era-input" v-model="nameCurrentPassword" type="password" maxlength="128" autocomplete="current-password" required>
          </label>
          <div class="settings-actions">
            <button class="era-button" type="submit" :disabled="updatingProfile">
              {{ updatingProfile ? 'Сохранение...' : 'Сменить имя' }}
            </button>
            <button class="era-button" type="button" @click="settingsMode = 'password'">
              Сменить пароль
            </button>
          </div>
        </form>

        <form v-else @submit.prevent="changePassword">
          <h2>Сменить пароль</h2>
          <label>
            Новый пароль
            <span class="password-input-wrap">
              <input class="era-input" v-model="newPassword" :type="showNewPassword ? 'text' : 'password'" minlength="8" maxlength="128" autocomplete="new-password" required>
              <button class="password-visibility" type="button" :aria-label="showNewPassword ? 'Скрыть пароль' : 'Показать пароль'" @click="showNewPassword = !showNewPassword">
                {{ showNewPassword ? 'Скрыть' : 'Показать' }}
              </button>
            </span>
          </label>
          <label>
            Подтверждение пароля
            <span class="password-input-wrap">
              <input
                class="era-input"
                :class="{ 'password-mismatch': passwordConfirmationError }"
                :aria-invalid="passwordConfirmationError"
                v-model="confirmPassword"
                :type="showConfirmPassword ? 'text' : 'password'"
                minlength="8"
                maxlength="128"
                autocomplete="new-password"
                required
              >
              <button class="password-visibility" type="button" :aria-label="showConfirmPassword ? 'Скрыть пароль' : 'Показать пароль'" @click="showConfirmPassword = !showConfirmPassword">
                {{ showConfirmPassword ? 'Скрыть' : 'Показать' }}
              </button>
            </span>
          </label>
          <label>
            Старый пароль
            <input class="era-input" v-model="passwordCurrentPassword" type="password" maxlength="128" autocomplete="current-password" required>
          </label>
          <p v-if="passwordConfirmationError" class="error password-error" role="alert">
            Подтверждение не совпадает с новым паролем.
          </p>
          <div class="settings-actions">
            <button class="era-button" type="submit" :disabled="updatingProfile">
              {{ updatingProfile ? 'Сохранение...' : 'Сменить пароль' }}
            </button>
            <button class="era-button" type="button" @click="settingsMode = 'name'">
              Сменить имя
            </button>
          </div>
        </form>
      </section>
      <section class="profile-statistics">
        <h2>Статистика игрока</h2>
        <p>Игр проведено: <strong>{{ profile.statistics.gamesPlayed }}</strong></p>
        <p v-if="profile.statistics.wins > 0">
          Победил в {{ profile.statistics.wins }} из {{ profile.statistics.gamesPlayed }}
        </p>
        <p v-else>Пока еще не побеждал</p>
        <p>Чаще всего становился — <strong>{{ profile.statistics.mostCommonTitle || '—' }}</strong></p>
        <h3>Становился:</h3>
        <ul v-if="profile.statistics.titles.length" class="title-statistics">
          <li v-for="title in profile.statistics.titles" :key="title.title">
            {{ title.title }} — {{ title.count }} {{ timesWord(title.count) }}
          </li>
        </ul>
        <p v-else>Титулов пока нет.</p>
      </section>
      <h2>Результаты, ожидающие подтверждения</h2>
      <p v-if="profile.pendingResults.length === 0">Нет результатов, ожидающих подтверждения.</p>
      <ul v-else class="pending-results">
        <li v-for="result in profile.pendingResults" :key="result.id">
          <span>
            Игра №{{ result.gameId }} от {{ formatDate(result.createdAt) }}:
            {{ result.title }} {{ result.playerName }}{{ result.hasTitle ? ' — победитель' : '' }}.
          </span>
          <button class="era-button" type="button" :disabled="acceptingId === result.id" @click="acceptResult(result.id)">
            {{ acceptingId === result.id ? 'Подтверждение...' : 'Принять результат' }}
          </button>
        </li>
      </ul>
      <div class="section-heading">
        <h2>Последние 5 результатов</h2>
        <RouterLink class="era-button" :to="{ name: 'results', query: { player: profile.name } }">
          Посмотреть все
        </RouterLink>
      </div>
      <p v-if="profile.recentResults.length === 0">Пока нет сохранённых результатов игр.</p>
      <ol v-else class="recent-results">
        <li v-for="result in profile.recentResults" :key="result.id" class="recent-result">
          <div class="result-heading">
            <strong>Игра №{{ result.gameId }}</strong>
            <time>{{ formatDate(result.createdAt) }}</time>
            <span>{{ result.gameType === 'online' ? 'Онлайн' : 'Настольная' }}</span>
          </div>
          <p class="result-score">
            {{ result.finalTotal }} очков — статус «{{ result.title }}»
            <span v-if="result.hasTitle">· Победитель</span>
          </p>
          <p class="result-acceptance">
            {{ result.resultAccepted ? 'Результат подтверждён' : 'Результат ожидает подтверждения' }}
          </p>
          <div v-if="result.buildings.length" class="result-buildings">
            <strong>Построенные здания:</strong>
            <ul>
              <li v-for="building in result.buildings" :key="building.name">
                {{ building.name }} — {{ building.count + building.insideWallCount }}
                <span v-if="building.insideWallCount > 0">({{ building.insideWallCount }} в стене)</span>
              </li>
            </ul>
          </div>
          <p v-else class="result-buildings">Здания не указаны.</p>
        </li>
      </ol>
    </template>
    <template v-else>
      <div class="auth-switch">
        <button class="era-button" type="button" :aria-pressed="mode === 'login'" @click="mode = 'login'">Войти</button>
        <button class="era-button" type="button" :aria-pressed="mode === 'register'" @click="mode = 'register'">Создать профиль</button>
      </div>
      <form @submit.prevent="submitCredentials">
        <label>
          Имя игрока
          <input class="era-input" v-model.trim="name" maxlength="50" autocomplete="username" required>
        </label>
        <label>
          Пароль
          <input
            class="era-input"
            v-model="password"
            type="password"
            minlength="8"
            maxlength="128"
            :autocomplete="mode === 'login' ? 'current-password' : 'new-password'"
            required
          >
        </label>
        <button class="era-button" type="submit" :disabled="submitting">
          {{ submitting ? 'Подождите...' : (mode === 'login' ? 'Войти' : 'Зарегистрироваться') }}
        </button>
      </form>
    </template>
    <p v-if="message" :class="{ error: isError }" role="status">{{ message }}</p>
  </section>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'

const profile = ref(null)
const loading = ref(true)
const submitting = ref(false)
const acceptingId = ref(null)
const updatingProfile = ref(false)
const name = ref('')
const password = ref('')
const newName = ref('')
const nameCurrentPassword = ref('')
const passwordCurrentPassword = ref('')
const newPassword = ref('')
const confirmPassword = ref('')
const settingsMode = ref('name')
const showNewPassword = ref(false)
const showConfirmPassword = ref(false)
const mode = ref('login')
const message = ref('')
const isError = ref(false)
const passwordsMatch = computed(() => newPassword.value === confirmPassword.value)
const passwordConfirmationError = computed(() =>
  (newPassword.value.length > 0 || confirmPassword.value.length > 0) && !passwordsMatch.value
)

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

async function loadProfile() {
  loading.value = true
  try {
    const response = await fetch('/api/PlayerProfile/me')
    if (response.status === 401) {
      profile.value = null
      return
    }
    if (!response.ok) throw new Error(`Не удалось загрузить профиль (ошибка ${response.status}).`)
    profile.value = await response.json()
  } catch (error) {
    isError.value = true
    message.value = error.message
  } finally {
    loading.value = false
  }
}

async function submitCredentials() {
  submitting.value = true
  message.value = ''
  isError.value = false
  try {
    const response = await fetch(`/api/PlayerProfile/${mode.value}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name: name.value, password: password.value })
    })
    if (!response.ok) {
      const details = await response.text()
      throw new Error(details || `Не удалось выполнить вход (ошибка ${response.status}).`)
    }
    password.value = ''
    await loadProfile()
  } catch (error) {
    isError.value = true
    message.value = error.message
  } finally {
    submitting.value = false
  }
}

async function changeName() {
  updatingProfile.value = true
  message.value = ''
  isError.value = false

  const payload = {
    name: newName.value,
    currentPassword: nameCurrentPassword.value,
    acceptOfflineResults: false
  }

  try {
    let response = await fetch('/api/PlayerProfile/change-name', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    })

    if (response.status === 409) {
      const conflict = await response.json()
      if (conflict.code === 'offline_results_available') {
        if (!window.confirm(conflict.message)) {
          throw new Error('Это имя уже занято. Результаты игр не были присоединены.')
        }

        payload.acceptOfflineResults = true
        response = await fetch('/api/PlayerProfile/change-name', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(payload)
        })
      } else {
        throw new Error(conflict.message || 'Это имя уже занято другим игроком.')
      }
    }

    if (!response.ok) {
      const details = await response.text()
      throw new Error(details || `Не удалось изменить имя (ошибка ${response.status}).`)
    }

    profile.value.name = (await response.json()).name
    message.value = 'Имя игрока изменено.'
    newName.value = ''
    nameCurrentPassword.value = ''
  } catch (error) {
    isError.value = true
    message.value = error.message
  } finally {
    updatingProfile.value = false
  }
}

async function changePassword() {
  if (!passwordsMatch.value) {
    return
  }

  await updateProfile('/api/PlayerProfile/change-password', {
    currentPassword: passwordCurrentPassword.value,
    newPassword: newPassword.value
  }, 'Пароль изменён.')
  if (!isError.value) {
    passwordCurrentPassword.value = ''
    newPassword.value = ''
    confirmPassword.value = ''
  }
}

async function updateProfile(url, payload, successMessage) {
  updatingProfile.value = true
  message.value = ''
  isError.value = false
  try {
    const response = await fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    })
    if (!response.ok) {
      const details = await response.text()
      throw new Error(details || `Не удалось обновить профиль (ошибка ${response.status}).`)
    }
    if (url.endsWith('change-name')) {
      profile.value.name = (await response.json()).name
    }
    message.value = successMessage
  } catch (error) {
    isError.value = true
    message.value = error.message
  } finally {
    updatingProfile.value = false
  }
}

async function acceptResult(resultId) {
  acceptingId.value = resultId
  message.value = ''
  isError.value = false
  try {
    const response = await fetch(`/api/PlayerProfile/results/${resultId}/accept`, { method: 'POST' })
    if (!response.ok) {
      const details = await response.text()
      throw new Error(details || `Не удалось принять результат (ошибка ${response.status}).`)
    }
    await loadProfile()
  } catch (error) {
    isError.value = true
    message.value = error.message
  } finally {
    acceptingId.value = null
  }
}

async function logout() {
  const response = await fetch('/api/PlayerProfile/logout', { method: 'POST' })
  if (!response.ok) {
    isError.value = true
    message.value = `Не удалось выйти из профиля (ошибка ${response.status}).`
    return
  }
  profile.value = null
}

onMounted(loadProfile)
</script>

<style scoped>
.profile {
  width: 100%;
  max-width: 760px;
  margin: 12px auto;
  padding: 22px 26px 26px;
  color: #3f3020;
  background: radial-gradient(120% 90% at 12% 0%, rgba(255, 255, 255, 0.75) 0%, rgba(255, 255, 255, 0) 55%),
    linear-gradient(155deg, #f8f1df 0%, #efe3c7 55%, #e5d5b2 100%);
  border: 1px solid #c9b48c;
  border-radius: 12px;
  box-shadow: 0 8px 22px rgba(0, 0, 0, 0.35), inset 0 0 0 1px rgba(255, 255, 255, 0.4);
}

.profile h1 {
  margin-bottom: 14px;
  padding-bottom: 12px;
  color: #4a3218;
  border-bottom: 2px solid rgba(139, 111, 71, 0.35);
  text-shadow: 0 1px 0 rgba(255, 255, 255, 0.65);
}

.profile form,
.profile label {
  display: grid;
  gap: 8px;
}

.profile form {
  max-width: 360px;
  gap: 14px;
}

.profile-settings {
  display: grid;
  gap: 14px;
  margin: 24px 0;
}

.profile-statistics {
  margin: 24px 0;
  padding: 16px;
  background: rgba(255, 253, 246, 0.65);
  border: 1px solid #d8c6a2;
  border-radius: 8px;
}

.profile-statistics h2 {
  margin-bottom: 8px;
}

.profile-statistics h3 {
  margin-top: 12px;
  color: #4a3218;
}

.title-statistics {
  padding-left: 22px;
}

.settings-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.profile-settings form {
  max-width: none;
  width: min(100%, 420px);
  padding: 16px;
  background: rgba(255, 253, 246, 0.65);
  border: 1px solid #d8c6a2;
  border-radius: 8px;
}

.profile h2 {
  color: #4a3218;
}

.profile label {
  color: #4a3218;
  font-weight: bold;
}

.profile label .era-input {
  width: 100%;
}

.password-input-wrap {
  display: flex;
  align-items: center;
  gap: 8px;
}

.password-input-wrap .era-input {
  min-width: 0;
  flex: 1;
}

.password-mismatch {
  border-color: #b00020;
  box-shadow: 0 0 0 2px rgba(176, 0, 32, 0.18);
}

.password-visibility {
  padding: 4px 0;
  color: #6d4c2f;
  background: transparent;
  border: 0;
  font: inherit;
  text-decoration: underline;
  cursor: pointer;
}

.password-visibility:focus-visible {
  outline: 2px solid var(--era-orange);
  outline-offset: 2px;
}

.password-error {
  margin-top: -6px;
}

.profile form > button {
  justify-self: start;
}

.auth-switch {
  display: flex;
  gap: 8px;
  margin-bottom: 16px;
}

.pending-results {
  display: grid;
  gap: 12px;
  padding-left: 22px;
}

.recent-results {
  display: grid;
  gap: 14px;
  margin: 14px 0 24px;
  padding-left: 28px;
}

.section-heading {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 12px;
  margin-top: 20px;
}

.section-heading h2 {
  margin: 0;
}

.recent-result {
  padding: 14px 16px;
  background: rgba(255, 253, 246, 0.65);
  border: 1px solid #d8c6a2;
  border-radius: 8px;
}

.result-heading {
  display: flex;
  flex-wrap: wrap;
  gap: 8px 16px;
  color: #6d4c2f;
}

.result-score {
  margin-top: 6px;
  color: #4a3218;
  font-weight: bold;
}

.result-acceptance {
  color: #765f40;
  font-size: 0.9em;
}

.result-buildings {
  margin-top: 8px;
}

.result-buildings ul {
  display: flex;
  flex-wrap: wrap;
  gap: 4px 20px;
  padding-left: 20px;
}

.pending-results li {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 10px;
}

.error {
  color: #b00020;
}

@media (max-width: 640px) {
  .profile {
    padding: 14px 12px 18px;
  }
}
</style>
