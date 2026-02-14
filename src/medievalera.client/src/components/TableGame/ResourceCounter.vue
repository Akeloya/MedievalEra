<!-- components/ResourceCounter.vue -->
<template>
  <div class="resource-counter-panel">
    <!-- Заголовок с информацией о статусе -->
    <div class="counter-header">
      <h3>📊 Собранные ресурсы</h3>
      <div class="turn-status">
        <span class="status-badge" :class="{ 'can-complete': canCompleteTurn }">
          {{ turnStatusText }}
        </span>
      </div>
    </div>

    <!-- Предупреждения о незавершенных выборах -->
    <div v-if="pendingChoices.length > 0" class="pending-choices-warning">
      <div class="warning-icon">⚠️</div>
      <div class="warning-text">
        <strong>Требуется выбор:</strong>
        <span v-for="dice in pendingChoices" :key="dice.id" class="pending-dice">
          Кубик {{ dice.name }}
        </span>
      </div>
    </div>

    <!-- Подсчет ресурсов -->
    <div class="resources-grid" v-else>
      <div v-for="(count, resource) in resourceTotals"
           :key="resource"
           class="resource-row"
           :class="{ 'has-resource': count > 0 }">

        <div class="resource-icon-cell">
          <span class="resource-icon" :class="`resource-${resource.toLowerCase()}`">
            {{ getIcon(resource) }}
          </span>
        </div>

        <div class="resource-name">{{ getResourceName(resource) }}</div>

        <div class="resource-count">
          <span class="count-value">{{ count }}</span>
          <div class="count-bar" :style="{ width: getBarWidth(count) + '%' }"></div>
        </div>
      </div>
    </div>

    <!-- Кнопка завершения хода -->
    <div class="turn-actions" v-if="!isTurnCompleted">
      <button class="btn-complete-turn"
              :class="{ 'btn-pulse': canCompleteTurn }"
              :disabled="!canCompleteTurn"
              @click="completeTurn">
        <span class="btn-icon">✅</span>
        Завершить ход
        <span v-if="canCompleteTurn" class="btn-hint">(доступно)</span>
      </button>
    </div>
  </div>
</template>

<script setup>
  import { computed } from 'vue';
  import {Dice} from './Dice'


  const props = defineProps({
    frozenDice: {
      type: Dice[0],
      required: true
    },
    unlockedDice: {
      type: Dice[0],
      required: true
    },
    isRollingComplete: {
      type: Boolean,
      default: false
    },
    isTurnCompleted: {
      type: Boolean,
      default: true
    }
  });

  const emit = defineEmits(['complete-turn']);

  // Иконки ресурсов
  const icons = {
    'goods': '📦',
    'stone': '⛰️',
    'wood': '🌲',
    'meal': '🌾',
    'skull': '💀',
    'culture': '📜',
    'attack': '⚔️',
    'defence': '🛡️',
    'building': '🔨',
    'NewRoll': '🖋️'
  };

  // Названия ресурсов
  const resourceNames = {
    'goods': 'Товары',
    'stone': 'Камень',
    'wood': 'Древесина',
    'meal': 'Еда',
    'skull': 'Черепа',
    'culture': 'Культура',
    'attack': 'Атака',
    'defence': 'Защита',
    'building': 'Строительство',
    'NewRoll': 'Переброс'
  };

  // Получить иконку ресурса
  function getIcon(resource) {
    return icons[resource] || '•';
  }

  // Получить название ресурса
  function getResourceName(resource) {
    return resourceNames[resource] || resource;
  }

  // Найти кубики с невыбранными ресурсами (choose: true)
  const pendingChoices = computed(() => {
    return props.frozenDice.filter(dice => {
      const face = dice.currentFace;
      return face?.choose === true && !face.selectedResource;
    });
  });

  // Максимальное количество ресурсов для шкалы
  const maxResourceCount = computed(() => {
    const counts = Object.values(resourceTotals.value);
    return Math.max(...counts, 1);
  });

  // Подсчет всех ресурсов из замороженных кубиков
  const resourceTotals = computed(() => {
    const totals = {};

    props.frozenDice.forEach(dice => {
      const face = dice.currentFace;
      if (!face || !face.values) return;

      // Если есть choose и выбран конкретный ресурс
      if (face.choose === true && face.selectedResource) {
        const count = face.values[face.selectedResource] || 0;
        totals[face.selectedResource] = (totals[face.selectedResource] || 0) + count;
      }
      // Если обычная грань (без выбора)
      else if (!face.choose) {
        Object.entries(face.values).forEach(([resource, count]) => {
          if (count > 0) {
            totals[resource] = (totals[resource] || 0) + count;
          }
        });
      }
      // Если есть choose, но выбор не сделан - не учитываем
    });

    return totals;
  });

  // Можно ли завершить ход
  const canCompleteTurn = computed(() => {
    // Если есть активные кубики - нельзя
    if (props.unlockedDice.length > 0) {
      return false;
    }

    // Если есть кубики с невыбранными ресурсами - нельзя
    if (pendingChoices.value.length > 0) {
      return false;
    }

    // Если есть замороженные кубики - можно
    return props.frozenDice.length > 0;
  });

  // Текст статуса хода
  const turnStatusText = computed(() => {
    if (props.unlockedDice.length > 0) {
      return `🎲 Активных кубиков: ${props.unlockedDice.length}`;
    }
    if (pendingChoices.value.length > 0) {
      return `⚠️ Выберите ресурсы (${pendingChoices.value.length})`;
    }
    if (props.frozenDice.length > 0) {
      return '✅ Можно завершить ход';
    }
    return '⏳ Нет кубиков';
  });

  // Ширина полосы прогресса
  function getBarWidth(count) {
    return (count / maxResourceCount.value) * 100;
  }

  // Завершение хода
  function completeTurn() {
    if (canCompleteTurn.value) {
      emit('complete-turn');
    }
  }
</script>

<style scoped>
  .resource-counter-panel {
    background: white;
    border-radius: 16px;
    padding: 20px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    min-width: 300px;
  }

  .counter-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 16px;
    padding-bottom: 12px;
    border-bottom: 2px solid #f0f0f0;
  }

    .counter-header h3 {
      margin: 0;
      color: #333;
      font-size: 18px;
    }

  .turn-status {
    display: flex;
    align-items: center;
  }

  .status-badge {
    padding: 4px 12px;
    border-radius: 20px;
    font-size: 12px;
    font-weight: 600;
    background: #f8f9fa;
    color: #666;
  }

    .status-badge.can-complete {
      background: #28a745;
      color: white;
      animation: subtle-pulse 2s infinite;
    }

  /* Предупреждения */
  .pending-choices-warning {
    background: #fff3cd;
    border: 1px solid #ffeeba;
    border-radius: 8px;
    padding: 12px;
    margin-bottom: 16px;
    display: flex;
    gap: 12px;
    align-items: flex-start;
  }

  .warning-icon {
    font-size: 20px;
  }

  .warning-text {
    flex: 1;
    font-size: 13px;
    color: #856404;
  }

    .warning-text strong {
      display: block;
      margin-bottom: 6px;
    }

  .pending-dice {
    display: inline-block;
    background: #ffeeba;
    padding: 2px 8px;
    border-radius: 12px;
    margin: 2px 4px 2px 0;
    font-size: 11px;
    font-weight: 600;
  }

  /* Сетка ресурсов */
  .resources-grid {
    display: flex;
    flex-direction: column;
    gap: 8px;
    margin-bottom: 20px;
    max-height: 300px;
    overflow-y: auto;
    padding-right: 4px;
  }

  .resource-row {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 8px 12px;
    background: #f8f9fa;
    border-radius: 8px;
    transition: all 0.2s ease;
  }

    .resource-row.has-resource {
      background: #e8f5e9;
    }

  .resource-icon-cell {
    width: 32px;
    display: flex;
    justify-content: center;
  }

  .resource-icon {
    width: 28px;
    height: 28px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 16px;
    border-radius: 6px;
    background: white;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  }

  /* Цвета для иконок */
  .resource-goods {
    background: #FFD700;
    color: #8B4513;
  }

  .resource-stone {
    background: #A9A9A9;
    color: white;
  }

  .resource-wood {
    background: #8B4513;
    color: #FFD700;
  }

  .resource-meal {
    background: #FFA500;
    color: white;
  }

  .resource-skull {
    background: #2C2C2C;
    color: white;
  }

  .resource-culture {
    background: #9B59B6;
    color: white;
  }

  .resource-attack {
    background: #E74C3C;
    color: white;
  }

  .resource-defence {
    background: #3498DB;
    color: white;
  }

  .resource-building {
    background: #27AE60;
    color: white;
  }

  .resource-newroll {
    background: #8E44AD;
    color: white;
  }

  .resource-name {
    flex: 1;
    font-size: 14px;
    font-weight: 500;
    color: #333;
  }

  .resource-count {
    position: relative;
    min-width: 60px;
    text-align: right;
    font-weight: 600;
    font-size: 16px;
    color: #2c3e50;
  }

  .count-value {
    position: relative;
    z-index: 2;
  }

  .count-bar {
    position: absolute;
    top: 0;
    right: 0;
    bottom: 0;
    background: rgba(40, 167, 69, 0.2);
    border-radius: 4px;
    z-index: 1;
    transition: width 0.3s ease;
  }

  /* Кнопка завершения хода */
  .turn-actions {
    margin-top: 16px;
    padding-top: 16px;
    border-top: 2px solid #f0f0f0;
  }

  .btn-complete-turn {
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    background: linear-gradient(135deg, #28a745 0%, #20c997 100%);
    color: white;
    border: none;
    padding: 14px 20px;
    border-radius: 10px;
    font-size: 16px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.3s ease;
    opacity: 0.7;
  }

    .btn-complete-turn:not(:disabled) {
      opacity: 1;
      box-shadow: 0 4px 12px rgba(40, 167, 69, 0.3);
    }

    .btn-complete-turn:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 6px 16px rgba(40, 167, 69, 0.4);
    }

    .btn-complete-turn:disabled {
      background: #6c757d;
      cursor: not-allowed;
      opacity: 0.5;
    }

    .btn-complete-turn.btn-pulse {
      animation: pulse-green 1.5s infinite;
    }

  .btn-icon {
    font-size: 18px;
  }

  .btn-hint {
    font-size: 12px;
    opacity: 0.9;
  }

  /* Анимации */
  @keyframes pulse-green {
    0% {
      box-shadow: 0 4px 12px rgba(40, 167, 69, 0.3);
    }

    50% {
      box-shadow: 0 8px 20px rgba(40, 167, 69, 0.6);
    }

    100% {
      box-shadow: 0 4px 12px rgba(40, 167, 69, 0.3);
    }
  }

  @keyframes subtle-pulse {
    0% {
      box-shadow: 0 0 0 0 rgba(40, 167, 69, 0.4);
    }

    70% {
      box-shadow: 0 0 0 6px rgba(40, 167, 69, 0);
    }

    100% {
      box-shadow: 0 0 0 0 rgba(40, 167, 69, 0);
    }
  }

  /* Скроллбар */
  .resources-grid::-webkit-scrollbar {
    width: 6px;
  }

  .resources-grid::-webkit-scrollbar-track {
    background: #f1f1f1;
    border-radius: 10px;
  }

  .resources-grid::-webkit-scrollbar-thumb {
    background: #888;
    border-radius: 10px;
  }

    .resources-grid::-webkit-scrollbar-thumb:hover {
      background: #555;
    }

  /* Адаптивность */
  @media (max-width: 768px) {
    .resource-counter-panel {
      padding: 16px;
      min-width: auto;
    }

    .resource-row {
      padding: 6px 10px;
    }

    .resource-icon {
      width: 24px;
      height: 24px;
      font-size: 14px;
    }

    .resource-name {
      font-size: 12px;
    }

    .resource-count {
      font-size: 14px;
      min-width: 50px;
    }

    .btn-complete-turn {
      padding: 12px 16px;
      font-size: 14px;
    }
  }
</style>
