<!-- components/AddDiceModal.vue -->
<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-content">
      <h3>Получить новый кубик</h3>

      <div class="dice-types">
        <button v-for="type in diceTypes"
                :key="type.value"
                @click="$emit('add-dice', type.value)"
                class="dice-type-btn">
          {{ type.label }}
        </button>
      </div>

      <button @click="$emit('close')" class="btn-close">
        Закрыть
      </button>
    </div>
  </div>
</template>

<script setup>
  const diceTypes = [
    { value: 1, label: 'Peasant (Крестьянин)' },
    { value: 2, label: 'Citizen (Горожанин)' },
    { value: 3, label: 'Clergy (Духовенство)' },
    { value: 4, label: 'Nobility (Знать)' }
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
    background: white;
    padding: 30px;
    border-radius: 12px;
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
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    border: none;
    padding: 12px;
    border-radius: 6px;
    cursor: pointer;
    transition: transform 0.3s;
  }

    .dice-type-btn:hover {
      transform: scale(1.05);
    }

  .btn-close {
    background: #6c757d;
    color: white;
    border: none;
    padding: 10px 20px;
    border-radius: 6px;
    cursor: pointer;
    margin-top: 20px;
  }
</style>
