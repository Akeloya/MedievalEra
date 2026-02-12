<!-- components/RollPanel.vue -->
<template>
  <div class="roll-panel">
    <div class="roll-info">
      <div class="remaining-rolls">
        Осталось бросков: {{ remainingRerolls }}
      </div>

      <div v-if="newRollPairs.length > 0" class="extra-rolls">
        <h4>Дополнительные броски (NewRoll)</h4>
        <div class="pairs-list">
          <div v-for="pair in unusedPairs" :key="pair.clergyId" class="pair-item">
            <span>Кубик {{ pair.clergyId }} → </span>
            <button @click="$emit('perform-extra-roll', pair.clergyId)"
                    class="btn-extra">
              Сделать доп. бросок
            </button>
          </div>
        </div>
      </div>
    </div>

    <div class="roll-actions">
      <button v-if="!isRollingComplete"
              @click="$emit('roll-dice')"
              :disabled="!canReroll"
              class="btn-roll">
        {{ canReroll ? `Бросить кубики (${remainingRerolls})` : 'Нет бросков' }}
      </button>

      <button v-if="!isRollingComplete && !canReroll"
              @click="$emit('complete-turn')"
              class="btn-complete">
        Завершить броски
      </button>

      <button @click="$emit('add-dice')"
              class="btn-add">
        + Получить кубик
      </button>
    </div>
  </div>
</template>

<script setup>
  import { computed } from 'vue';

  const props = defineProps({
    canReroll: {
      type: Boolean,
      required: true
    },
    isRollingComplete: {
      type: Boolean,
      required: true
    },
    newRollPairs: {
      type: Array,
      default: () => []
    },
    remainingRerolls: {
      type: Number,
      default: 3
    }
  });

  defineEmits(['roll-dice', 'complete-turn', 'add-dice', 'perform-extra-roll']);

  const unusedPairs = computed(() => {
    return props.newRollPairs.filter(pair => !pair.isUsed);
  });
</script>

<style scoped>
  .roll-panel {
    background: white;
    border-radius: 12px;
    padding: 20px;
    margin-top: 20px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    flex-wrap: wrap;
    gap: 20px;
  }

  .roll-info {
    display: flex;
    flex-direction: column;
    gap: 10px;
  }

  .remaining-rolls {
    font-size: 18px;
    font-weight: bold;
    color: #667eea;
  }

  .extra-rolls {
    background: #f8f9fa;
    padding: 12px;
    border-radius: 8px;
  }

    .extra-rolls h4 {
      margin: 0 0 10px 0;
      font-size: 14px;
      color: #666;
    }

  .pairs-list {
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
  }

  .pair-item {
    background: white;
    padding: 8px 12px;
    border-radius: 6px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  }

  .btn-extra {
    background: #17a2b8;
    color: white;
    border: none;
    padding: 4px 12px;
    border-radius: 4px;
    cursor: pointer;
  }

  .roll-actions {
    display: flex;
    gap: 12px;
  }

  .btn-roll, .btn-complete, .btn-add {
    border: none;
    padding: 12px 24px;
    border-radius: 8px;
    font-size: 16px;
    font-weight: bold;
    cursor: pointer;
    transition: all 0.3s;
  }

  .btn-roll {
    background: #28a745;
    color: white;
  }

    .btn-roll:hover:not(:disabled) {
      background: #218838;
    }

    .btn-roll:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

  .btn-complete {
    background: #ffc107;
    color: black;
  }

    .btn-complete:hover {
      background: #e0a800;
    }

  .btn-add {
    background: #007bff;
    color: white;
  }

    .btn-add:hover {
      background: #0056b3;
    }
</style>
