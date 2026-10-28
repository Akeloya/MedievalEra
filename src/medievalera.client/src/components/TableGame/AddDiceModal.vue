<!-- components/AddDiceModal.vue -->
<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-content era-panel">
      <h3>Получить новый кубик</h3>

      <div class="dice-types">
        <button v-for="type in diceTypes"
                :key="type.value"
                @click="$emit('add-dice', type.value)"
                class="dice-type-btn era-button">
          {{ type.label }}
        </button>
      </div>

      <button @click="$emit('close')" class="btn-close era-button">
        Закрыть
      </button>
    </div>
  </div>
</template>

<script setup>
  const diceTypes = [
    { value: 1, label: 'Крестьянин' },
    { value: 2, label: 'Горожанин' },
    { value: 3, label: 'Духовенство' },
    { value: 4, label: 'Знать' }
  ];

  defineEmits(['add-dice', 'close']);
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
    max-width: 500px;
    width: 90%;
  }

  .dice-types {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 12px;
    margin: 20px 0;
  }

  .dice-type-btn {
    padding: 12px;
    cursor: pointer;
  }

  .btn-close {
    padding: 10px 20px;
    cursor: pointer;
    margin-top: 20px;
  }
</style>
