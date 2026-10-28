<template>
  <section class="roll-history era-panel" aria-label="История бросков">
    <h3>История бросков</h3>
    <p v-if="turns.length === 0" class="empty-history">Бросков пока нет</p>
    <div v-else class="history-list">
      <details v-for="(turn, index) in turns" :key="turn.number" class="turn-container" :open="index === 0">
        <summary>Ход {{ turn.number }}</summary>
        <div class="turn-content">
          <section v-for="(event, eventIndex) in turn.actions" :key="`actions-${eventIndex}`" class="actions-entry">
            <h4>Выбор игрока при завершении хода</h4>
            <HistoryDiceList :dice="event.changes" actions />
          </section>
          <template v-for="entry in turn.entries" :key="entry.roll">
            <section class="history-entry">
              <h4>Бросок {{ entry.roll }}{{ entry.isExtra ? ' (дополнительный)' : '' }}</h4>
              <HistoryDiceList :dice="entry.dice" />
            </section>
            <section v-if="entry.actionsBefore.length" class="actions-entry">
              <h4>Выбор игрока перед броском {{ entry.roll }}</h4>
              <HistoryDiceList :dice="entry.actionsBefore" actions />
            </section>
          </template>
        </div>
      </details>
    </div>
  </section>
</template>

<script setup>
import { computed } from 'vue';
import HistoryDiceList from './HistoryDiceList.vue';

const props = defineProps({
  entries: { type: Array, required: true },
  completedActions: { type: Array, default: () => [] }
});
const turns = computed(() => {
  const groups = new Map();
  const getTurn = number => {
    if (!groups.has(number)) groups.set(number, { number, entries: [], actions: [] });
    return groups.get(number);
  };
  for (const entry of props.entries) getTurn(entry.turn).entries.push(entry);
  for (const event of props.completedActions) getTurn(event.turn).actions.push(event);
  for (const turn of groups.values()) {
    turn.entries.sort((a, b) => b.roll - a.roll);
    turn.actions.reverse();
  }
  return [...groups.values()].sort((a, b) => b.number - a.number);
});
</script>

<style scoped>
.roll-history { min-width: 0; padding: 14px; }
h3 { margin: 0 0 10px; font-size: 18px; }
h4 { margin: 0 0 6px; font-size: 13px; }
.empty-history { margin: 0; color: #75634d; font-size: 13px; }
.history-list { max-height: 320px; overflow-y: auto; }
.turn-container { border: 1px solid #c9b48c; border-radius: 8px; background: #fffdf64d; }
.turn-container + .turn-container { margin-top: 10px; }
summary { padding: 8px 10px; cursor: pointer; font-weight: bold; background: #dcc79b80; }
summary:focus-visible { outline: 2px solid var(--era-orange); outline-offset: -2px; }
.turn-content { padding: 10px; display: grid; gap: 10px; }
.history-entry + .history-entry { padding-top: 10px; border-top: 1px solid #d8c6a2; }
.actions-entry { padding: 8px; border-left: 3px solid #8a6844; background: #e7d7b480; border-radius: 4px; }
</style>
