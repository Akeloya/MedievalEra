<!-- components/DiceCard.vue -->
<template>
  <div class="dice-card"
       :class="{
             frozen: isFrozen,
             selectable: isSelectable
         }"
       :style="{ backgroundColor: diceColor }">

    <!-- Индикаторы (только иконки, без текста) -->
    <div v-if="isFrozen" class="indicator frozen-icon">❄️</div>
    <div v-if="hasNewRoll" class="indicator newroll-icon">⚡</div>

    <!-- Ресурсы на грани -->
    <div class="resources-area" :class="getLayoutClass()">
      <template v-for="(resource, index) in displayResources" :key="index">
        <!-- Разделитель для choose (по диагонали) -->
        <div v-if="showDivider && index === 0"
             class="divider diagonal"
             :class="{ 'divider-active': selectedIndex === 0 }">
        </div>
        <div v-if="showDivider && index === 1"
             class="divider diagonal"
             :class="{ 'divider-active': selectedIndex === 1 }">
        </div>

        <!-- Группа иконок ресурса -->
        <div class="resource-group"
             :class="{
                         'selectable': isSelectable,
                         'selected': selectedIndex === index
                     }"
             @click.stop="selectResource(index, resource.type)">

          <div v-for="i in resource.count"
               :key="i"
               class="resource-icon"
               :class="`resource-${resource.type.toLowerCase()}`"
               :title="`${resource.type}: ${resource.count}`">
            {{ getIcon(resource.type) }}
          </div>

          <!-- Счетчик если больше 4 иконок -->
          <span v-if="resource.count > 4" class="resource-counter">
            {{ resource.count }}
          </span>
        </div>
      </template>
    </div>

    <!-- Кнопки действий -->
    <div v-if="showActions" class="action-buttons">
      <button v-if="canFreeze && !isFrozen"
              @click="$emit('freeze')"
              class="action-btn freeze"
              title="Заморозить">
        ❄️
      </button>
      <button v-if="canUnfreeze && isFrozen"
              @click="$emit('unfreeze')"
              class="action-btn unfreeze"
              title="Разморозить">
        🔥
      </button>
      <button v-if="canBindNewRoll && hasNewRoll && !isFrozen"
              @click="$emit('bind-new-roll')"
              class="action-btn bind"
              title="Перебросить">
        ⚡
      </button>
    </div>
  </div>
</template>

<script setup>
  import { computed, ref } from 'vue';

  const props = defineProps({
    dice: {
      type: Object,
      required: true
    },
    isFrozen: {
      type: Boolean,
      default: false
    },
    canFreeze: {
      type: Boolean,
      default: false
    },
    canUnfreeze: {
      type: Boolean,
      default: false
    },
    canBindNewRoll: {
      type: Boolean,
      default: false
    }
  });

  const emit = defineEmits(['freeze', 'unfreeze', 'bind-new-roll', 'resource-selected']);

  const selectedIndex = ref(null);

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
    'newroll': '🖋️'
  };

  const showActions = computed(() => props.dice.canBeLocked);
  // Определяем текущую грань
  const currentFace = computed(() => {
    return props.dice.currentFace || (props.dice.faces && props.dice.faces[0]);
  });

  // Цвет кубика (фон всего кубика)
  const diceColor = computed(() => {
    const colors = {
      1: '#8B4513', // Peasant - коричневый
      2: '#4A90E2', // Citizen - синий
      3: '#9B59B6', // Clergy - фиолетовый
      4: '#F1C40F'  // Nobility - золотой
    };
    return props.dice.color || colors[props.dice.diceType] || '#95A5A6';
  });

  // Форматирование ресурсов
  const displayResources = computed(() => {
    if (!currentFace.value || !currentFace.value.values) return [];

    return Object.entries(currentFace.value.values)
      .filter(([_, count]) => count > 0)
      .map(([type, count]) => ({
        type,
        count
      }));
  });

  // Показывать ли разделитель
  const showDivider = computed(() => {
    return currentFace.value?.choose === true && displayResources.value.length === 2;
  });

  // Доступен ли выбор
  const isSelectable = computed(() => {
    return currentFace.value?.choose === true && displayResources.value.length > 0;
  });

  // Проверка на NewRoll
  const hasNewRoll = computed(() => {
    return currentFace.value?.values?.['newroll'] > 0;
  });

  // Определение класса расположения
  function getLayoutClass() {
    const count = displayResources.value.length;
    if (count === 1) return 'layout-single';
    if (count === 2) return 'layout-diagonal';
    if (count === 3) return 'layout-triangle';
    return 'layout-grid';
  }

  // Иконка для ресурса
  function getIcon(resource) {
    return icons[resource] || '•';
  }

// Отслеживание выбранного ресурса
const selectedResource = defineModel('selectedResource', {
    type: String,
    default: null
  });

  // Выбор ресурса
  function selectResource(index, resourceType) {
    if (!isSelectable.value) return;

    if (selectedResource.value === resourceType) {
      selectedResource.value = null;
      emit('resource-selected', null);
    } else {
      selectedResource.value = resourceType;

      // Сохраняем выбор в самой грани
      if (currentFace.value) {
        currentFace.value.selectedResource = resourceType;
      }

      emit('resource-selected', {
        dice: props.dice,
        resource: resourceType,
        face: currentFace.value
      });
    }
  }
</script>

<style scoped>
  .dice-card {
    position: relative;
    width: 90px;
    height: 90px;
    border: 2px solid rgba(0, 0, 0, 0.2);
    border-radius: 12px;
    padding: 6px;
    box-shadow: 0 2px 6px rgba(0, 0, 0, 0.2);
    transition: all 0.2s ease;
    box-sizing: border-box;
  }

    .dice-card.frozen {
      opacity: 0.7;
      filter: grayscale(0.3);
      border-style: dashed;
      border-color: #3498DB;
    }

    .dice-card.selectable {
      cursor: pointer;
    }

  /* Индикаторы */
  .indicator {
    position: absolute;
    width: 22px;
    height: 22px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 14px;
    border-radius: 50%;
    z-index: 10;
    box-shadow: 0 1px 3px rgba(0,0,0,0.2);
  }

  .frozen-icon {
    top: -6px;
    right: -6px;
    background: #3498DB;
    color: white;
  }

  .newroll-icon {
    bottom: -6px;
    right: -6px;
    background: #9B59B6;
    color: white;
  }

  /* Область ресурсов */
  .resources-area {
    position: relative;
    width: 100%;
    height: 100%;
    display: flex;
  }

  /* Расположения */
  .layout-single {
    display: flex;
    justify-content: center;
    align-items: center;
  }

  .layout-diagonal {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 8px;
  }

    .layout-diagonal .resource-group:first-child {
      align-self: flex-start;
    }

    .layout-diagonal .resource-group:last-child {
      align-self: flex-end;
    }

  .layout-triangle {
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    padding: 8px;
  }

    .layout-triangle .resource-group:first-child {
      align-self: flex-start;
    }

    .layout-triangle .resource-group:nth-child(2) {
      align-self: center;
    }

    .layout-triangle .resource-group:last-child {
      align-self: flex-end;
    }

  .layout-grid {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 4px;
    align-items: center;
    justify-items: center;
  }

  /* Группа ресурсов */
  .resource-group {
    position: relative;
    display: flex;
    flex-wrap: wrap;
    gap: 2px;
    border-radius: 8px;
    transition: all 0.2s ease;
    background: rgba(255, 255, 255, 0.2);
    backdrop-filter: blur(1px);
  }

    .resource-group.selectable {
      cursor: pointer;
    }

      .resource-group.selectable:hover {
        background: rgba(255, 255, 255, 0.4);
        transform: scale(1.02);
      }

    .resource-group.selected {
      background: rgba(255, 215, 0, 0.4);
      box-shadow: 0 0 0 2px gold;
    }

  /* Иконка ресурса */
  .resource-icon {
    width: 20px;
    height: 20px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 14px;
    border-radius: 4px;
    background: white;
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.15);
  }

  /* Цвета для иконок (только фон иконки, не кубика) */
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

  /* Счетчик ресурсов */
  .resource-counter {
    position: absolute;
    top: -2px;
    right: -2px;
    background: #667eea;
    color: white;
    font-size: 8px;
    font-weight: bold;
    padding: 1px 3px;
    border-radius: 8px;
    min-width: 12px;
    text-align: center;
  }

  /* Разделитель (по диагонали) */
  .divider {
    position: absolute;
    width: 1px;
    height: 60px;
    background: rgba(0, 0, 0, 0.4);
    transform-origin: center;
    z-index: 5;
  }

    .divider.diagonal {
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%) rotate(45deg);
    }

      .divider.diagonal:last-child {
        transform: translate(-50%, -50%) rotate(-45deg);
      }

    .divider.divider-active {
      background: gold;
      box-shadow: 0 0 4px gold;
      width: 2px;
    }

  /* Кнопки действий */
  .action-buttons {
    position: absolute;
    bottom: -16px;
    left: 50%;
    transform: translateX(-50%);
    display: flex;
    gap: 4px;
    background: white;
    padding: 4px 8px;
    border-radius: 20px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.15);
    z-index: 20;
  }

  .action-btn {
    border: none;
    background: none;
    font-size: 14px;
    cursor: pointer;
    padding: 2px;
    border-radius: 50%;
    width: 24px;
    height: 24px;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: all 0.2s ease;
  }

    .action-btn.freeze:hover {
      background: #3498DB;
      color: white;
    }

    .action-btn.unfreeze:hover {
      background: #E74C3C;
      color: white;
    }

    .action-btn.bind:hover {
      background: #9B59B6;
      color: white;
    }

  /* Адаптивность */
  @media (max-width: 768px) {
    .dice-card {
      width: 100px;
      height: 100px;
      padding: 4px;
    }

    .resource-icon {
      width: 18px;
      height: 18px;
      font-size: 12px;
    }

    .indicator {
      width: 18px;
      height: 18px;
      font-size: 12px;
    }

    .action-buttons {
      bottom: -14px;
      padding: 3px 6px;
    }

    .action-btn {
      width: 20px;
      height: 20px;
      font-size: 12px;
    }
  }
</style>
