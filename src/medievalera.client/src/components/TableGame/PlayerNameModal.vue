<!-- components/PlayerNameModal.vue -->
<template>
  <div class="modal-overlay">
    <div class="modal-content era-panel">
      <h2>Добро пожаловать в игру!</h2>
      <p>Введите ваше имя, чтобы продолжить</p>

      <form @submit.prevent="handleSubmit">
        <input v-model="name"
               type="text"
               placeholder="Ваше имя"
               required
               class="name-input era-input" />

        <div class="modal-actions">
          <button type="submit" class="btn-start era-button">
            Начать игру
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
  import { ref } from 'vue';

  const emit = defineEmits(['start-game']);
  const name = ref('');

  function handleSubmit() {    
    if (name.value.trim()) {
      emit('start-game', name.value.trim());
    }
  }
</script>

<style scoped>
  .modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.7);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
  }

  .modal-content {
    max-width: 400px;
    width: 90%;
    text-align: center;
  }

  .name-input {
    width: 100%;
    padding: 12px;
    margin: 20px 0;
    border: 2px solid #ddd;
    border-radius: 6px;
    font-size: 16px;
  }

    .name-input:focus {
      border-color: #667eea;
      outline: none;
    }

  .btn-start {
    padding: 12px 24px;
    font-size: 16px;
    font-weight: bold;
    cursor: pointer;
  }
</style>
