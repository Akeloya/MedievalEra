<template>
  <ul class="results">
    <li v-for="(die, dieIndex) in dice" :key="die.id" class="die-result" :class="{ 'origin-boundary': !actions && dieIndex > 0 && dice[dieIndex - 1].originRoll !== die.originRoll }">
      <span class="die-marker" :style="{ backgroundColor: die.color }" aria-hidden="true"></span>
      <span class="die-name">{{ die.name || typeNames[die.diceType] || 'Кубик' }}</span>
      <span v-if="actions" class="action-label">{{ die.frozen ? 'Заморожен' : 'Разморожен' }}</span>
      <span v-else class="origin">Бросок {{ die.originRoll }}</span>
      <span v-if="die.face" class="face-result">
        <template v-for="([resource, count], index) in Object.entries(die.face.values).filter(([, count]) => count > 0)" :key="resource">
          {{ index && die.face.choose ? ' / ' : '' }}<span :title="resourceNames[resource] || resource">{{ diceIcons[resource] || resource }} ×{{ count }}</span>
        </template>
      </span>
      <span v-if="!actions && !die.wasRolled" class="held" title="Сохранён с предыдущего броска">❄️</span>
    </li>
  </ul>
</template>

<script setup>
import { diceIcons } from './DiceResources.js';
defineProps({ dice: { type: Array, required: true }, actions: Boolean });
const typeNames = { 1: 'Крестьянин', 2: 'Горожанин', 3: 'Духовенство', 4: 'Знать' };
const resourceNames = {
  goods: 'Товары', stone: 'Камень', wood: 'Древесина', meal: 'Еда', skull: 'Черепа',
  culture: 'Культура', attack: 'Атака', defence: 'Защита', building: 'Строительство', newroll: 'Переброс'
};
</script>

<style scoped>
.results { list-style: none; margin: 0; padding: 0; display: grid; gap: 4px; }
.die-result { display: flex; align-items: center; gap: 6px; font-size: 12px; flex-wrap: wrap; }
.origin-boundary { margin-top: 4px; padding-top: 6px; border-top: 1px dashed #c9b48c; }
.die-marker { width: 10px; height: 10px; border-radius: 3px; flex-shrink: 0; border: 1px solid #947b59; }
.die-name { overflow-wrap: anywhere; }
.face-result { margin-left: auto; display: inline-flex; gap: 4px; flex-wrap: wrap; }
.origin, .action-label { color: #75634d; font-size: 10px; }
.held { font-size: 10px; }
</style>
