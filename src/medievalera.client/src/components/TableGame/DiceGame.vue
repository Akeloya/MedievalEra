<!-- components/DiceGame.vue -->
<template>
    <div class="dice-game">
        <!-- Модальное окно ввода имени -->
        <PlayerNameModal v-if="!playerName"
                         @start-game="startGame" />

        <div v-else class="game-container">
            <!-- Заголовок и информация -->
            <GameHeader :player-name="playerName"
                        :turn-count="turnCount"
                        :reroll-count="rerollCount"
                        :max-rerolls="maxRerolls"
                        :can-reroll="canReroll"
                        @reset-game="confirmReset"
                        @new-turn="startNewTurn" />

            <!-- Коллекции кубиков -->
            <div class="dice-collections">
                <!-- Активные кубики -->
                <DiceCollection title="Активные кубики"
                                :dice-collection="unlockedDice"
                                :is-frozen="false"
                                @freeze-dice="freezeDice"
                                @bind-new-roll="openBindingModal" />

                <!-- Замороженные кубики -->
                <DiceCollection title="Замороженные кубики"
                                :dice-collection="frozenDice"
                                :is-frozen="true"
                                @unfreeze-dice="unfreezeDice" />
            </div>

            <!-- Панель бросков -->
            <RollPanel :can-reroll="canReroll"
                       :is-rolling-complete="isRollingComplete"
                       :new-roll-pairs="newRollPairs"
                       @roll-dice="rollDice"
                       @complete-turn="completeTurn"
                       @perform-extra-roll="performExtraRoll" />

            <!-- Модальное окно добавления кубика -->
            <AddDiceModal v-if="showAddDiceModal"
                          @add-dice="handleAddDice"
                          @close="showAddDiceModal = false" />

            <!-- Модальное окно привязки NewRoll -->
            <BindNewRollModal v-if="showBindingModal"
                              :clergy-dice="selectedClergyDice"
                              :frozen-dice="frozenDice"
                              @bind="handleBindNewRoll"
                              @close="showBindingModal = false" />
        </div>
    </div>
</template>

<script setup>
    import { ref, onMounted } from 'vue';
    import { useGameStore } from './gameStore';
    import PlayerNameModal from './PlayerNameModal.vue';
    import GameHeader from './GameHeader.vue';
    import DiceCollection from './DiceCollection.vue';
    import RollPanel from './RollPanel.vue';
    import AddDiceModal from './AddDiceModal.vue';
    import BindNewRollModal from './BindNewRollModal.vue';

    const store = useGameStore();
    const showAddDiceModal = ref(false);
    const showBindingModal = ref(false);
    const selectedClergyDice = ref(null);

    // Загрузка сессии при монтировании
    onMounted(() => {
        if (store.loadSessionFromCookies()) {
            // Загружаем данные игры из хранилища
            // Здесь можно добавить API для загрузки состояния
        }
    });

    // Начало игры
    async function startGame(playerName) {
        await store.initializeGame(playerName);
    }

    // Подтверждение сброса игры
    function confirmReset() {
        if (confirm('Вы уверены, что хотите начать новую игру? Весь прогресс будет потерян.')) {
            store.resetGame();
        }
    }

    // Добавление кубика
    async function handleAddDice(diceType) {
        await store.addDiceByType(diceType);
        showAddDiceModal.value = false;
    }

    // Открытие модального окна для привязки NewRoll
    function openBindingModal(clergyDice) {
        selectedClergyDice.value = clergyDice;
        showBindingModal.value = true;
    }

    // Обработка привязки NewRoll
    function handleBindNewRoll({ clergyDice, lockedDice }) {
        store.bindNewRoll(clergyDice, lockedDice);
        showBindingModal.value = false;
    }
</script>

<style scoped>
    .dice-game {
        min-height: 100vh;
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        padding: 20px;
    }

    .game-container {
        max-width: 1400px;
        margin: 0 auto;
    }

    .dice-collections {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 20px;
        margin: 20px 0;
    }

    @media (max-width: 768px) {
        .dice-collections {
            grid-template-columns: 1fr;
        }
    }
</style>