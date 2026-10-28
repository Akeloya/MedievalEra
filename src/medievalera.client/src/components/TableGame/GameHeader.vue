<!-- components/GameHeader.vue -->
<template>
  <div class="game-header era-panel">
    <div class="header-container">
      <h1>Игрок: {{ playerName }}</h1>
      <div v-if="isRollingComplete" class="stat-item turn-complete">
        <span class="stat-label">Статус:</span>
        <span class="stat-value">Ход завершен</span>
      </div>
    </div>
    <div class="player-info">
      <div class="stats">
        <div class="stat-item">
          <span class="stat-label">Ход:</span>
          <span class="stat-value">{{ turnCount }}</span>
        </div>
        <div class="stat-item">
          <span class="stat-label">Бросков:</span>
          <span class="stat-value">{{ rerollCount }}/{{ maxRerolls }}</span>
        </div>
        <div class="stat-item">
          <span class="stat-label">Осталось:</span>
          <span class="stat-value">{{ remainingRerolls }}</span>
        </div>
      </div>
    </div>

    <div class="header-actions">
      <button @click="$emit('new-turn')"
              class="btn-new-turn era-button"
              :disabled="!isRollingComplete && rerollCount === 0"
              :title="getNewTurnTooltip">
        <span class="btn-icon">🎲</span>
        Новый ход
      </button>
      <button @click="$emit('reset-game')" class="btn-reset era-button">
        <span class="btn-icon">🔄</span>
        Сброс игры
      </button>
    </div>
    <slot></slot>
  </div>
</template>

<script setup>
    import { computed } from 'vue';

    const props = defineProps({
        playerName: {
            type: String,
            required: true
        },
        turnCount: {
            type: Number,
            required: true
        },
        rerollCount: {
            type: Number,
            required: true
        },
        maxRerolls: {
            type: Number,
            required: true
        },
        remainingRerolls: {
            type: Number,
            required: true
        },
        isRollingComplete: {
            type: Boolean,
            default: false
        }
    });

    defineEmits(['new-turn', 'reset-game']);

    const getNewTurnTooltip = computed(() => {
        if (props.isRollingComplete) {
            return 'Начать новый ход';
        }
        if (props.rerollCount === 0) {
            return 'Сделайте хотя бы один бросок, чтобы начать ход';
        }
        return 'Завершите текущий ход или используйте все броски';
    });

    // Дополнительная информация о текущем состоянии
    const turnStatus = computed(() => {
        if (props.isRollingComplete) {
            return 'Ход завершен';
        }
        if (props.rerollCount === 0) {
            return 'Ожидание броска';
        }
        return `Бросок ${props.rerollCount}/${props.maxRerolls}`;
    });
</script>

<style scoped>
    .game-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        flex-wrap: wrap;
        gap: 10px;
        padding: 14px;
    }

    .player-info {
        flex: 1;
    }

    .header-container h1 {
        margin: 0;
        font-size: 22px;
        overflow-wrap: anywhere;
        color: #333;
        font-weight: 600;
    }

    .stats {
        display: flex;
        gap: 8px;
        flex-wrap: wrap;
    }

    .stat-item {
        display: flex;
        flex-direction: row;
        align-items: center;
        gap: 6px;
        background: #f8f9fa;
        padding: 5px 8px;
        border-radius: 8px;
        min-width: 0;
    }

    .stat-label {
        font-size: 12px;
        color: #666;
        text-transform: uppercase;
        letter-spacing: 0.5px;
        margin-bottom: 0;
    }

    .stat-value {
        font-size: 18px;
        font-weight: bold;
        color: #333;
    }

    .turn-complete {
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);        
    }

        .turn-complete .stat-label,
        .turn-complete .stat-value {
            color: white;
        }

    .header-actions {
        display: flex;
        flex-wrap: wrap;
        gap: 12px;
    }

    .btn-new-turn, .btn-reset {
        display: flex;
        align-items: center;
        gap: 8px;
        padding: 8px 12px;
        font-size: 14px;
        font-weight: 600;
        cursor: pointer;
    }

    .btn-new-turn:disabled {
        opacity: 0.5;
        cursor: not-allowed;
    }

    .btn-icon {
        font-size: 18px;
    }

    .header-container {
      display: flex; /* Выстраиваем элементы в ряд */
      justify-content: space-between; /* Первый влево, второй вправо */
      align-items: center; /* Центрируем по вертикали, если высота разная */
      width: 100%;
      flex-wrap: wrap;
      gap: 8px;
    }
    /* Адаптивность */
    @media (max-width: 768px) {
        .game-header {
            flex-direction: column;
            align-items: stretch;
        }

        .header-container h1 {
            font-size: 24px;
            text-align: center;
        }

        .stats {
            justify-content: center;
        }

        .stat-item {
            min-width: 80px;
            padding: 8px 12px;
        }

        .stat-value {
            font-size: 20px;
        }

        .header-actions {
            justify-content: center;
        }

        .btn-new-turn, .btn-reset {
            flex: 1;
            justify-content: center;
        }
    }

    /* Анимации */
    @keyframes pulse {
        0% {
            box-shadow: 0 4px 6px rgba(102, 126, 234, 0.2);
        }

        50% {
            box-shadow: 0 8px 12px rgba(102, 126, 234, 0.4);
        }

        100% {
            box-shadow: 0 4px 6px rgba(102, 126, 234, 0.2);
        }
    }

    /* Стили для маленьких экранов */
    @media (max-width: 480px) {
        .stats {
            flex-direction: column;
            gap: 8px;
        }

        .stat-item {
            width: 100%;
            flex-direction: row;
            justify-content: space-between;
            align-items: center;
        }

        .stat-label {
            margin-bottom: 0;
        }

        .stat-value {
            font-size: 18px;
        }
    }
</style>
