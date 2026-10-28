<!-- components/DiceCollection.vue -->
<template>
    <div class="dice-collection era-panel" :class="{ 'frozen-collection': isFrozen }">
        <div class="collection-header">
            <h3>{{ title }}</h3>
            <span class="count">{{ diceCollection.length }} кубиков</span>
        </div>

        <div v-if="diceCollection.length === 0" class="empty-state">
            <p>Нет кубиков</p>
        </div>

        <div v-else class="dice-grid">
            <DiceCard v-for="dice in diceCollection"
                      :key="dice.id"
                      :dice="dice"
                      :is-frozen="isFrozen"
                      :can-freeze="actionsEnabled && !isFrozen"
                      :can-unfreeze="actionsEnabled && isFrozen"
                      :isRollingComplete="isRollingComplete"
                      :can-reroll="canReroll"
                      @freeze="$emit('freeze-dice', dice)"
                      @unfreeze="$emit('unfreeze-dice', dice)"
                      @bind-new-roll="$emit('bind-new-roll', dice)" />
        </div>
    </div>
</template>

<script setup>
    import DiceCard from './DiceCard.vue';

    defineProps({
        actionsEnabled: {
            type: Boolean,
            default: true
        },
        title: {
            type: String,
            required: true
        },
        diceCollection: {
            type: Array,
            required: true
        },
        isFrozen: {
            type: Boolean,
            default: false
      },
      isRollingComplete: {
        type: Boolean,
        default: false
      },
      canReroll: {
        type: Boolean,
        default: true
      }
    });

    defineEmits(['freeze-dice', 'unfreeze-dice', 'bind-new-roll']);
</script>

<style scoped>
    .dice-collection {
        min-width: 0;
        min-height: 190px;
        padding: 14px;
    }

    .collection-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        flex-wrap: wrap;
        gap: 12px;
        margin-bottom: 12px;
        padding-bottom: 8px;
        border-bottom: 2px solid #f0f0f0;
    }

        .collection-header h3 {
            margin: 0;
            color: #333;
        }

    .count {
        background: #dcc79b;
        color: #4a3218;
        padding: 4px 12px;
        border-radius: 20px;
        font-size: 14px;
    }

    .dice-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(90px, 1fr));
        gap: 28px 20px;
        padding-bottom: 16px;
    }

    .empty-state {
        text-align: center;
        padding: 12px;
        color: #999;
        font-style: italic;
    }

    .empty-state p {
        margin: 0;
    }
</style>
