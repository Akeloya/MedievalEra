<!-- components/BindNewRollModal.vue -->
<template>
    <div class="modal-overlay" @click.self="$emit('close')">
        <div class="modal-content">
            <h3>Привязать NewRoll к замороженному кубику</h3>

            <div class="clergy-info">
                <div class="info-label">Кубик Clergy с NewRoll:</div>
                <DiceCard :dice="clergyDice"
                          :is-frozen="false"
                          :can-freeze="false"
                          class="compact-card" />
            </div>

            <div class="binding-section">
                <div class="section-label">Выберите замороженный кубик для привязки:</div>

                <div v-if="frozenDice.length === 0" class="empty-state">
                    <p>Нет доступных замороженных кубиков</p>
                    <button @click="$emit('close')" class="btn-close">
                        Закрыть
                    </button>
                </div>

                <div v-else class="frozen-dice-list">
                    <div v-for="dice in frozenDice"
                         :key="dice.id"
                         class="frozen-dice-item"
                         :class="{ selected: selectedDice && selectedDice.id === dice.id }"
                         @click="selectDice(dice)">
                        <DiceCard :dice="dice"
                                  :is-frozen="true"
                                  :can-unfreeze="false"
                                  class="compact-card" />
                        <div class="selection-indicator" v-if="selectedDice && selectedDice.id === dice.id">
                            ✓ Выбран
                        </div>
                    </div>
                </div>
            </div>

            <div class="modal-actions" v-if="selectedDice">
                <button @click="handleBind"
                        class="btn-bind"
                        :disabled="!selectedDice">
                    Привязать и выполнить бросок
                </button>
                <button @click="$emit('close')" class="btn-cancel">
                    Отмена
                </button>
            </div>

            <div class="info-text">
                <p>После привязки:</p>
                <ul>
                    <li>Выбранный замороженный кубик разморозится</li>
                    <li>Оба кубика будут переброшены</li>
                    <li>Если на кубике Clergy снова выпадет NewRoll, вы сможете повторить привязку</li>
                    <li>Если NewRoll не выпадет, оба кубика заморозятся</li>
                </ul>
            </div>
        </div>
    </div>
</template>

<script setup>
    import { ref } from 'vue';
    import DiceCard from './DiceCard.vue';

    const props = defineProps({
        clergyDice: {
            type: Object,
            required: true
        },
        frozenDice: {
            type: Array,
            required: true
        }
    });

    const emit = defineEmits(['bind', 'close']);
    const selectedDice = ref(null);

    function selectDice(dice) {
        selectedDice.value = dice;
    }

    function handleBind() {
        if (selectedDice.value) {
            emit('bind', {
                clergyDice: props.clergyDice,
                lockedDice: selectedDice.value
            });
        }
    }
</script>

<style scoped>
    .modal-overlay {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(0, 0, 0, 0.7);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 1100;
        padding: 20px;
    }

    .modal-content {
        background: white;
        padding: 30px;
        border-radius: 12px;
        max-width: 600px;
        width: 100%;
        max-height: 90vh;
        overflow-y: auto;
    }

        .modal-content h3 {
            margin-top: 0;
            margin-bottom: 20px;
            color: #333;
            text-align: center;
        }

    .clergy-info {
        background: linear-gradient(135deg, #f3e5f5 0%, #e1bee7 100%);
        padding: 15px;
        border-radius: 8px;
        margin-bottom: 20px;
    }

    .info-label {
        font-size: 14px;
        font-weight: bold;
        color: #4a148c;
        margin-bottom: 10px;
    }

    .binding-section {
        margin-bottom: 20px;
    }

    .section-label {
        font-size: 16px;
        font-weight: bold;
        color: #333;
        margin-bottom: 15px;
    }

    .frozen-dice-list {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
        gap: 15px;
        max-height: 300px;
        overflow-y: auto;
        padding: 5px;
    }

    .frozen-dice-item {
        border: 2px solid transparent;
        border-radius: 12px;
        cursor: pointer;
        transition: all 0.3s;
        position: relative;
    }

        .frozen-dice-item:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
        }

        .frozen-dice-item.selected {
            border-color: #667eea;
            background: #f0f3ff;
        }

    .selection-indicator {
        position: absolute;
        top: 10px;
        right: 10px;
        background: #667eea;
        color: white;
        padding: 4px 8px;
        border-radius: 4px;
        font-size: 12px;
        font-weight: bold;
    }

    .compact-card {
        margin: 0;
    }

    .empty-state {
        text-align: center;
        padding: 30px;
        background: #f8f9fa;
        border-radius: 8px;
        color: #666;
    }

    .modal-actions {
        display: flex;
        gap: 12px;
        justify-content: center;
        margin-top: 20px;
    }

    .btn-bind {
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        color: white;
        border: none;
        padding: 12px 24px;
        border-radius: 6px;
        font-size: 16px;
        font-weight: bold;
        cursor: pointer;
        transition: all 0.3s;
    }

        .btn-bind:hover:not(:disabled) {
            transform: scale(1.05);
            box-shadow: 0 4px 8px rgba(102, 126, 234, 0.3);
        }

        .btn-bind:disabled {
            opacity: 0.5;
            cursor: not-allowed;
        }

    .btn-cancel {
        background: #6c757d;
        color: white;
        border: none;
        padding: 12px 24px;
        border-radius: 6px;
        font-size: 16px;
        font-weight: bold;
        cursor: pointer;
        transition: background 0.3s;
    }

        .btn-cancel:hover {
            background: #5a6268;
        }

    .btn-close {
        background: #6c757d;
        color: white;
        border: none;
        padding: 10px 20px;
        border-radius: 6px;
        cursor: pointer;
        margin-top: 20px;
    }

    .info-text {
        margin-top: 20px;
        padding: 15px;
        background: #f8f9fa;
        border-radius: 8px;
        font-size: 14px;
        color: #666;
        border-left: 4px solid #17a2b8;
    }

        .info-text p {
            margin-top: 0;
            margin-bottom: 10px;
            font-weight: bold;
            color: #17a2b8;
        }

        .info-text ul {
            margin: 0;
            padding-left: 20px;
        }

        .info-text li {
            margin-bottom: 5px;
        }
</style>