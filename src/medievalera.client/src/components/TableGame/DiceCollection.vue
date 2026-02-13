<!-- components/DiceCollection.vue -->
<template>
    <div class="dice-collection" :class="{ 'frozen-collection': isFrozen }">
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
                      :can-freeze="!isFrozen"
                      :can-unfreeze="isFrozen"
                      :isRollingComplete="isRollingComplete"
                      @freeze="$emit('freeze-dice', dice)"
                      @unfreeze="$emit('unfreeze-dice', dice)"
                      @bind-new-roll="$emit('bind-new-roll', dice)" />
        </div>
    </div>
</template>

<script setup>
    import DiceCard from './DiceCard.vue';

    defineProps({
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
      }
    });

    defineEmits(['freeze-dice', 'unfreeze-dice', 'bind-new-roll']);
</script>

<style scoped>
    .dice-collection {
        background: white;
        border-radius: 12px;
        padding: 20px;
        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    }

    .collection-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 20px;
        padding-bottom: 10px;
        border-bottom: 2px solid #f0f0f0;
    }

        .collection-header h3 {
            margin: 0;
            color: #333;
        }

    .count {
        background: #667eea;
        color: white;
        padding: 4px 12px;
        border-radius: 20px;
        font-size: 14px;
    }

    .dice-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
        gap: 20px;
    }

    .frozen-collection {
        background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
    }

    .empty-state {
        text-align: center;
        padding: 40px;
        color: #999;
        font-style: italic;
    }
</style>
