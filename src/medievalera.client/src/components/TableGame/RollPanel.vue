<!-- components/RollPanel.vue -->
<template>
  <div class="roll-panel">
    <div class="roll-info">
      <div v-if="hasExtraRolls" class="extra-rolls">
        <h4>✨ Дополнительные броски (NewRoll)</h4>
        <div class="pairs-list">
          <div v-for="pair in unusedPairs"
               :key="pair.id || pair.clergyId"
               class="pair-item">
            <div class="pair-info">
              <span class="pair-label">Кубик Духовенства</span>
              <button @click="$emit('perform-extra-roll', pair.clergyId)"
                      class="btn-extra"
                      :title="'Сделать дополнительный бросок'">
                🎲 Бросить еще раз
              </button>
            </div>
          </div>
        </div>
        <p v-if="unusedPairs.length === 0" class="no-pairs">
          Нет доступных дополнительных бросков
        </p>
      </div>
    </div>

    <div class="roll-actions">
      <button v-if="!isRollingComplete"
              @click="$emit('roll-dice')"
              :disabled="!canReroll"
              class="btn-roll"
              :class="{ 'btn-roll-warning': rerollCount === maxRerolls - 1 }">
        <span class="btn-icon">🎲</span>
        {{ rollButtonText }}
      </button>

      <button v-if="!isRollingComplete && !canReroll && rerollCount >= maxRerolls"
              @click="$emit('complete-turn')"
              class="btn-complete">
        <span class="btn-icon">⏹️</span>
        Завершить ход
      </button>

      <button @click="$emit('add-dice')"
              class="btn-add">
        <span class="btn-icon">➕</span>
        Получить кубик
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
    },
    rerollCount: {
      type: Number,
      default: 0
    },
    maxRerolls: {
      type: Number,
      default: 3
    }
  });

  defineEmits(['roll-dice', 'complete-turn', 'add-dice', 'perform-extra-roll']);

  // Неиспользованные пары для дополнительных бросков
  const unusedPairs = computed(() => {
    return props.newRollPairs.filter(pair => !pair.isUsed);
  });

  // Есть ли доступные дополнительные броски
  const hasExtraRolls = computed(() => {
    return unusedPairs.value.length > 0;
  });

  // Текст кнопки броска
  const rollButtonText = computed(() => {
    if (!props.canReroll) return 'Нет доступных бросков';
    if (props.remainingRerolls === 1) return '🎲 Последний бросок!';
    return `Бросить кубики (${props.remainingRerolls})`;
  });
</script>

<style scoped>
  .roll-panel {
    background: white;
    border-radius: 16px;
    padding: 24px;
    margin-top: 24px;
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    flex-wrap: wrap;
    gap: 20px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  }

  .roll-info {
    display: flex;
    flex-direction: column;
    gap: 16px;
    flex: 1;
  }

  .remaining-rolls {
    font-size: 18px;
    color: #2c3e50;
    background: #f8f9fa;
    padding: 12px 20px;
    border-radius: 12px;
    display: inline-block;
  }

    .remaining-rolls strong {
      color: #667eea;
      font-size: 24px;
      margin-left: 8px;
    }

  .extra-rolls {
    background: linear-gradient(135deg, #f3e5f5 0%, #e1bee7 100%);
    padding: 20px;
    border-radius: 12px;
    border-left: 6px solid #9b59b6;
  }

    .extra-rolls h4 {
      margin: 0 0 16px 0;
      font-size: 16px;
      color: #4a148c;
      display: flex;
      align-items: center;
      gap: 8px;
    }

      .extra-rolls h4::before {
        content: '⚡';
        font-size: 20px;
      }

  .pairs-list {
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .pair-item {
    background: white;
    padding: 12px 16px;
    border-radius: 10px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
    transition: all 0.3s ease;
  }

    .pair-item:hover {
      transform: translateX(4px);
      box-shadow: 0 4px 8px rgba(155, 89, 182, 0.2);
    }

  .pair-info {
    display: flex;
    justify-content: space-between;
    align-items: center;
    flex-wrap: wrap;
    gap: 12px;
  }

  .pair-label {
    font-size: 14px;
    color: #666;
    display: flex;
    align-items: center;
    gap: 6px;
  }

    .pair-label::before {
      content: '🎲';
      font-size: 16px;
    }

  .btn-extra {
    background: linear-gradient(135deg, #9b59b6 0%, #8e44ad 100%);
    color: white;
    border: none;
    padding: 8px 16px;
    border-radius: 20px;
    font-size: 14px;
    font-weight: 600;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 6px;
    transition: all 0.3s ease;
  }

    .btn-extra:hover {
      transform: scale(1.05);
      box-shadow: 0 4px 12px rgba(155, 89, 182, 0.4);
    }

    .btn-extra:active {
      transform: scale(0.98);
    }

  .no-pairs {
    color: #666;
    font-style: italic;
    margin: 8px 0 0 0;
    font-size: 14px;
  }

  .roll-actions {
    display: flex;
    gap: 12px;
    flex-wrap: wrap;
    align-items: center;
  }

  .btn-roll, .btn-complete, .btn-add {
    border: none;
    padding: 14px 28px;
    border-radius: 12px;
    font-size: 16px;
    font-weight: bold;
    cursor: pointer;
    transition: all 0.3s ease;
    display: flex;
    align-items: center;
    gap: 8px;
    white-space: nowrap;
  }

  .btn-icon {
    font-size: 20px;
  }

  .btn-roll {
    background: linear-gradient(135deg, #28a745 0%, #20c997 100%);
    color: white;
    box-shadow: 0 4px 12px rgba(40, 167, 69, 0.3);
  }

    .btn-roll:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 6px 16px rgba(40, 167, 69, 0.4);
    }

  .btn-roll-warning {
    background: linear-gradient(135deg, #ffc107 0%, #fd7e14 100%);
    box-shadow: 0 4px 12px rgba(255, 193, 7, 0.3);
    animation: pulse 1.5s infinite;
  }

  .btn-roll:disabled {
    opacity: 0.5;
    cursor: not-allowed;
    transform: none;
    box-shadow: none;
  }

  .btn-complete {
    background: linear-gradient(135deg, #ffc107 0%, #e0a800 100%);
    color: #2c3e50;
    box-shadow: 0 4px 12px rgba(255, 193, 7, 0.3);
  }

    .btn-complete:hover {
      transform: translateY(-2px);
      box-shadow: 0 6px 16px rgba(255, 193, 7, 0.4);
    }

  .btn-add {
    background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
    color: white;
    box-shadow: 0 4px 12px rgba(0, 123, 255, 0.3);
  }

    .btn-add:hover {
      transform: translateY(-2px);
      box-shadow: 0 6px 16px rgba(0, 123, 255, 0.4);
    }

  @keyframes pulse {
    0% {
      box-shadow: 0 4px 12px rgba(255, 193, 7, 0.3);
    }

    50% {
      box-shadow: 0 8px 20px rgba(255, 193, 7, 0.6);
    }

    100% {
      box-shadow: 0 4px 12px rgba(255, 193, 7, 0.3);
    }
  }

  /* Адаптивность */
  @media (max-width: 768px) {
    .roll-panel {
      flex-direction: column;
      padding: 20px;
    }

    .roll-info {
      width: 100%;
    }

    .roll-actions {
      width: 100%;
      justify-content: stretch;
    }

    .btn-roll, .btn-complete, .btn-add {
      flex: 1;
      justify-content: center;
      padding: 12px 20px;
      font-size: 14px;
    }

    .pair-info {
      flex-direction: column;
      align-items: flex-start;
    }

    .btn-extra {
      width: 100%;
      justify-content: center;
    }
  }

  @media (max-width: 480px) {
    .roll-actions {
      flex-direction: column;
    }

    .btn-roll, .btn-complete, .btn-add {
      width: 100%;
    }
  }
</style>
