<!-- components/DiceCard.vue -->
<template>
    <div class="dice-card"
         :style="{ borderColor: dice.color }"
         :class="{ frozen: isFrozen }">
        <div class="dice-header">
            <div class="dice-type" :style="{ backgroundColor: dice.color }">
                {{ dice.name || dice.diceType }}
            </div>
            <div class="dice-actions">
                <button v-if="canFreeze && dice.currentFace"
                        @click="$emit('freeze')"
                        class="btn-freeze"
                        title="Заморозить">
                    ❄️
                </button>
                <button v-if="canUnfreeze"
                        @click="$emit('unfreeze')"
                        class="btn-unfreeze"
                        title="Разморозить">
                    🔥
                </button>
                <button v-if="dice.diceType === 3 && dice.currentFace && hasNewRoll"
                        @click="$emit('bind-new-roll')"
                        class="btn-bind"
                        title="Привязать NewRoll">
                    ⚡
                </button>
            </div>
        </div>

        <!-- Отображение выпавшей грани -->
        <div v-if="dice.currentFace" class="dice-face">
            <div class="face-label">Выпало:</div>
            <div class="resources">
                <div v-for="(value, resource) in dice.currentFace.values"
                     :key="resource"
                     class="resource-item"
                     :class="getResourceClass(resource)">
                    {{ resource }}: {{ value }}
                </div>
                <div v-if="dice.currentFace.choose" class="choose-badge">
                    Выбор
                </div>
            </div>
        </div>

        <div v-else class="not-rolled">
            Не брошен
        </div>
    </div>
</template>

<script setup>
    import { computed } from 'vue';

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
        }
    });

    defineEmits(['freeze', 'unfreeze', 'bind-new-roll']);

    const hasNewRoll = computed(() => {
        return props.dice.currentFace?.values?.['NewRoll'] > 0;
    });

    function getResourceClass(resource) {
        const classes = {
            'resource': true,
            [`resource-${resource.toLowerCase()}`]: true
        };
        return classes;
    }
</script>

<style scoped>
    .dice-card {
        background: white;
        border: 3px solid #ddd;
        border-radius: 12px;
        padding: 16px;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        transition: all 0.3s ease;
    }

        .dice-card:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
        }

        .dice-card.frozen {
            opacity: 0.9;
            background: #f8f9fa;
            border-style: dashed;
        }

    .dice-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 12px;
    }

    .dice-type {
        padding: 4px 12px;
        border-radius: 20px;
        color: white;
        font-size: 14px;
        font-weight: bold;
    }

    .dice-actions {
        display: flex;
        gap: 8px;
    }

    .btn-freeze, .btn-unfreeze, .btn-bind {
        border: none;
        background: none;
        font-size: 18px;
        cursor: pointer;
        padding: 4px;
        transition: transform 0.2s;
    }

        .btn-freeze:hover, .btn-unfreeze:hover, .btn-bind:hover {
            transform: scale(1.2);
        }

    .dice-face {
        border-top: 1px solid #eee;
        padding-top: 12px;
    }

    .face-label {
        font-size: 12px;
        color: #666;
        margin-bottom: 8px;
    }

    .resources {
        display: flex;
        flex-wrap: wrap;
        gap: 8px;
    }

    .resource-item {
        padding: 4px 8px;
        border-radius: 4px;
        font-size: 12px;
        font-weight: bold;
        color: white;
    }

    .resource-goods {
        background: #28a745;
    }

    .resource-stone {
        background: #6c757d;
    }

    .resource-wood {
        background: #fd7e14;
    }

    .resource-meal {
        background: #dc3545;
    }

    .resource-skull {
        background: #343a40;
    }

    .resource-culture {
        background: #007bff;
    }

    .resource-attack {
        background: #ffc107;
        color: black;
    }

    .resource-defence {
        background: #17a2b8;
    }

    .resource-building {
        background: #20c997;
    }

    .resource-newroll {
        background: #6610f2;
    }

    .choose-badge {
        background: #ffc107;
        color: black;
        padding: 4px 8px;
        border-radius: 4px;
        font-size: 12px;
        font-weight: bold;
    }

    .not-rolled {
        color: #999;
        font-style: italic;
        text-align: center;
        padding: 20px 0;
    }
</style>