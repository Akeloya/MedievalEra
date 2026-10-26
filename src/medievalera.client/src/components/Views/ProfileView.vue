<template>
  <section class="profile">
    <h1>Профиль игрока</h1>

    <p v-if="loading" role="status">Загрузка профиля...</p>
    <template v-else-if="profile">
      <p>Вы вошли как <strong>{{ profile.name }}</strong>.</p>
      <button type="button" @click="logout">Выйти</button>
      <h2>Результаты, ожидающие подтверждения</h2>
      <p v-if="profile.pendingResults.length === 0">Нет результатов, ожидающих подтверждения.</p>
      <ul v-else class="pending-results">
        <li v-for="result in profile.pendingResults" :key="result.id">
          <span>
            Игра №{{ result.gameId }} от {{ formatDate(result.createdAt) }}:
            {{ result.title }} {{ result.playerName }}{{ result.hasTitle ? ' — победитель' : '' }}.
          </span>
          <button type="button" :disabled="acceptingId === result.id" @click="acceptResult(result.id)">
            {{ acceptingId === result.id ? 'Подтверждение...' : 'Принять результат' }}
          </button>
        </li>
      </ul>
    </template>
    <template v-else>
      <div class="auth-switch">
        <button type="button" :aria-pressed="mode === 'login'" @click="mode = 'login'">Войти</button>
        <button type="button" :aria-pressed="mode === 'register'" @click="mode = 'register'">Создать профиль</button>
      </div>
      <form @submit.prevent="submitCredentials">
        <label>
          Имя игрока
          <input v-model.trim="name" maxlength="50" autocomplete="username" required>
        </label>
        <label>
          Пароль
          <input
            v-model="password"
            type="password"
            minlength="8"
            maxlength="128"
            :autocomplete="mode === 'login' ? 'current-password' : 'new-password'"
            required
          >
        </label>
        <button type="submit" :disabled="submitting">
          {{ submitting ? 'Подождите...' : (mode === 'login' ? 'Войти' : 'Зарегистрироваться') }}
        </button>
      </form>
    </template>
    <p v-if="message" :class="{ error: isError }" role="status">{{ message }}</p>
  </section>
</template>

<script setup>
import { onMounted, ref } from 'vue'

const profile = ref(null)
const loading = ref(true)
const submitting = ref(false)
const acceptingId = ref(null)
const name = ref('')
const password = ref('')
const mode = ref('login')
const message = ref('')
const isError = ref(false)

function formatDate(value) {
  return new Date(value).toLocaleString()
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
  max-width: 760px;
  margin: 0 auto;
  padding: 12px;
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

.profile input {
  padding: 8px;
}

.profile button {
  padding: 8px 12px;
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

.pending-results li {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 10px;
}

.error {
  color: #b00020;
}
</style>
