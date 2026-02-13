<template>
  <div class="dice-game">
    <!-- Модальное окно ввода имени -->
    <PlayerNameModal v-if="!store.playerName"
                     @start-game="startGame" />

    <div v-else class="game-container">
      <!-- Заголовок и информация -->
      <GameHeader :player-name="store.playerName"
                  :turn-count="store.turnCount"
                  :reroll-count="store.rerollCount"
                  :max-rerolls="store.maxRerolls"
                  :remaining-rerolls="store.remainingRerolls"
                  :is-rolling-complete="store.isRollingComplete"
                  @reset-game="confirmReset"
                  @new-turn="startNewTurn" />

      <!-- Коллекции кубиков -->
      <div class="dice-collections">
        <!-- Активные кубики -->
        <DiceCollection title="Активные кубики"
                        :dice-collection="store.unlockedDice"
                        :is-frozen="false"
                        :isRollingComplete="store.isRollingComplete"
                        @freeze-dice="freezeDice"
                        @bind-new-roll="openBindingModal" />

        <!-- Замороженные кубики -->
        <DiceCollection title="Замороженные кубики"
                        :dice-collection="store.frozenDice"
                        :is-frozen="true"
                        :isRollingComplete="store.isRollingComplete"
                        :can-reroll="store.canReroll"
                        @unfreeze-dice="unfreezeDice" />
      </div>

      <!-- Панель бросков -->
      <RollPanel :can-reroll="store.canReroll"
                 :is-rolling-complete="store.isRollingComplete"
                 :new-roll-pairs="store.newRollPairs"
                 :remaining-rerolls="store.remainingRerolls"
                 :reroll-count="store.rerollCount"
                 :max-rerolls="store.maxRerolls"
                 @roll-dice="rollDice"
                 @complete-turn="completeTurn"
                 @perform-extra-roll="performExtraRoll"
                 @add-dice="showAddDiceModal = true" />

      <!-- Модальное окно добавления кубика -->
      <AddDiceModal v-if="showAddDiceModal"
                    @add-dice="handleAddDice"
                    @close="showAddDiceModal = false" />

      <!-- Модальное окно привязки NewRoll -->
      <BindNewRollModal v-if="showBindingModal"
                        :clergy-dice="selectedClergyDice"
                        :frozen-dice="store.frozenDice"
                        :can-reroll="store.canReroll"
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
  onMounted(async () => {
    // Загружаем сессию из cookies
    const hasSession = store.loadSessionFromCookies();

    if (hasSession) {
      try {
        // Если есть сессия, получаем стартовый набор
        await store.resetGame();
      } catch (error) {
        console.error('Failed to load game state:', error);
        // Если не удалось загрузить, очищаем сессию
        store.clearSession();
      }
    }
  });

  // Начало игры
  async function startGame(playerName) {
    try {
      await store.initializeGame(playerName);
      // Модальное окно закроется автоматически из-за v-if="!store.playerName"
    } catch (error) {
      console.error('Failed to start game:', error);
      alert('Не удалось начать игру. Пожалуйста, попробуйте снова.');
    }
  }

  // Подтверждение сброса игры
  function confirmReset() {
    if (confirm('Вы уверены, что хотите начать новую игру? Весь прогресс будет потерян.')) {
      store.resetGame();
    }
  }

  // Новый ход
  async function startNewTurn() {
    try {
      await store.startNewTurn();
    } catch (error) {
      console.error('Failed to start new turn:', error);
      alert('Не удалось начать новый ход.');
    }
  }

  // Бросок кубиков
  async function rollDice() {
    try {
      await store.rollDice();
    } catch (error) {
      console.error('Failed to roll dice:', error);
      alert(error.message || 'Не удалось выполнить бросок.');
    }
  }

  // Завершение хода
  function completeTurn() {
    store.completeTurn();
  }

  // Дополнительный бросок с NewRoll
  function performExtraRoll(pairId) {
    store.performExtraRoll(pairId);
  }

  // Заморозка кубика
  function freezeDice(dice) {
    store.freezeDice(dice);
  }

  // Разморозка кубика
  function unfreezeDice(dice) {
    store.unfreezeDice(dice);
  }

  // Добавление кубика
  async function handleAddDice(diceType) {
    try {
      await store.addDiceByType(diceType);
      showAddDiceModal.value = false;
    } catch (error) {
      console.error('Failed to add dice:', error);
      alert('Не удалось добавить кубик.');
    }
  }

  // Открытие модального окна для привязки NewRoll
  function openBindingModal(clergyDice) {
    selectedClergyDice.value = clergyDice;
    showBindingModal.value = true;
  }

  // Обработка привязки NewRoll
  function handleBindNewRoll({ clergyDice, lockedDice }) {
    try {
      store.bindNewRoll(clergyDice, lockedDice);
      showBindingModal.value = false;
    } catch (error) {
      console.error('Failed to bind NewRoll:', error);
      alert('Не удалось привязать NewRoll.');
    }
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
