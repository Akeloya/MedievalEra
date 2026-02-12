// stores/gameStore.js
import { defineStore } from "pinia";
import { ref, computed } from "vue";
import diceApi from "./diceApi";
import cookies from "js-cookie";
import DiceResources from "./DiceResources";

export const useGameStore = defineStore("game", () => {
  // Состояние
  const playerName = ref('');
  const sessionId = ref(null);
  const allDice = ref([]);
  const frozenDice = ref([]);
  const unlockedDice = ref([]);
  const rerollCount = ref(0);
  const turnCount = ref(0);
  const maxRerolls = 3;
  const hasStarterKit = ref(false);
  const newRollPairs = ref([]);
  const isRollingComplete = ref(false);
  const diceIdCounter = ref(0);

  // Геттеры
  const canReroll = computed(() => {
    return rerollCount.value < maxRerolls && !isRollingComplete.value;
  });

  const remainingRerolls = computed(() => {
    return Math.max(0, maxRerolls - rerollCount.value);
  });

  // Загрузка сессии из cookies
  function loadSessionFromCookies() {
    const savedSession = cookies.get('diceGameSession');
    if (savedSession) {
      try {
        const session = JSON.parse(savedSession);
        sessionId.value = session.sessionId;
        playerName.value = session.playerName;
        turnCount.value = session.turnCount || 0;
        return true;
      } catch (e) {
        console.error('Error loading session:', e);
        return false;
      }
    }
    return false;
  }

  // Сохранение сессии в cookies
  function saveSessionToCookies() {
    const session = {
      sessionId: sessionId.value,
      playerName: playerName.value,
      turnCount: turnCount.value,
      timestamp: new Date().getTime()
    };
    cookies.set('diceGameSession', JSON.stringify(session), { expires: 7 });
  }

  // Очистка сессии
  function clearSession() {
    cookies.remove('diceGameSession');
    playerName.value = '';
    sessionId.value = null;
    turnCount.value = 0;
  }

  // Генерация ID для кубика
  function generateDiceId() {
    diceIdCounter.value++;
    return `dice_${Date.now()}_${diceIdCounter.value}`;
  }

  // Инициализация новой игры
  async function initializeGame(name) {
    playerName.value = name;
    sessionId.value = generateSessionId();
    diceIdCounter.value = 0;
    await resetGame();
    saveSessionToCookies();
  }

  // Сброс игры
  async function resetGame() {
    try {
      const starterKit = await diceApi.getStarterKit();
      allDice.value = starterKit.map(dice => ({
        ...dice,
        id: generateDiceId(),
        currentFace: null,
        // Добавляем методы, если их нет
        roll: dice.roll || (() => ({ values: {} })),
        lock: dice.lock || (() => { }),
        unlock: dice.unlock || (() => { })
      }));
      frozenDice.value = [];
      unlockedDice.value = [...allDice.value];
      rerollCount.value = 0;
      isRollingComplete.value = false;
      newRollPairs.value = [];
      hasStarterKit.value = true;

      turnCount.value++;
      saveSessionToCookies();
    } catch (error) {
      console.error('Failed to reset game:', error);
      throw error;
    }
  }

  // Инициализация новой игры
  async function initializeGame(name) {
    playerName.value = name;
    sessionId.value = generateSessionId();
    await resetGame();
    saveSessionToCookies();
  }

  // Сброс игры
  async function resetGame() {
    try {
      const starterKit = await diceApi.getStarterKit();
      allDice.value = starterKit;
      frozenDice.value = [];
      unlockedDice.value = [...starterKit];
      rerollCount.value = 0;
      isRollingComplete.value = false;
      newRollPairs.value = [];
      hasStarterKit.value = true;

      // Увеличиваем счетчик ходов при полном сбросе
      turnCount.value++;
      saveSessionToCookies();
    } catch (error) {
      console.error("Failed to reset game:", error);
      throw error;
    }
  }

  // Добавление кубика по типу
  async function addDiceByType(type) {
    try {
      const dice = await diceApi.getDiceByType(type);
      allDice.value.push(dice);
      unlockedDice.value.push(dice);
      return dice;
    } catch (error) {
      console.error(`Failed to get dice type ${type}:`, error);
      throw error;
    }
  }

  // Бросок всех незамороженных кубиков
  async function rollDice() {
    if (!canReroll.value) {
      throw new Error("No rerolls remaining");
    }

    // Бросаем каждый незамороженный кубик
    unlockedDice.value.forEach(dice => {
      dice.currentFace = dice.roll();

      // Проверяем на Skull
      if (hasResource(dice.currentFace, DiceResources.Skull)) {
        freezeDice(dice);
      }

      // Проверяем на NewRoll для кубиков Clergy
      if (dice.diceType === 3 && hasResource(dice.currentFace, DiceResources.NewRoll)) {
        // Будет обработано позже
      }
    });

    rerollCount.value++;

    if (rerollCount.value === maxRerolls) {
      await handleAfterLastRoll();
    }
  }

  // Заморозка кубика
  function freezeDice(dice) {
    const index = unlockedDice.value.findIndex(d => d.id === dice.id);
    if (index !== -1) {
      dice.lock(dice.currentFace);
      unlockedDice.value.splice(index, 1);
      frozenDice.value.push(dice);
    }
  }

  // Разморозка кубика
  function unfreezeDice(dice) {
    const index = frozenDice.value.findIndex(d => d.id === dice.id);
    if (index !== -1) {
      dice.unlock();
      frozenDice.value.splice(index, 1);
      unlockedDice.value.push(dice);
    }
  }

  // Обработка после последнего броска
  async function handleAfterLastRoll() {
    // Обрабатываем кубики Clergy с NewRoll
    const clergyWithNewRoll = unlockedDice.value.filter(dice =>
      dice.diceType === 3 &&
      dice.currentFace &&
      hasResource(dice.currentFace, DiceResources.NewRoll)
    );

    if (clergyWithNewRoll.length > 0) {
      // Замораживаем обычные кубики
      frozenDice.value = [...frozenDice.value, ...unlockedDice.value.filter(d => d.diceType !== 3)];
      unlockedDice.value = unlockedDice.value.filter(d => d.diceType === 3);
    } else {
      // Если нет NewRoll, завершаем ход
      completeTurn();
    }
  }

  // Привязка NewRoll к замороженному кубику
  function bindNewRoll(clergyDice, lockedDice) {
    if (!clergyDice || !lockedDice) {
      console.error('Invalid dice for binding');
      return;
    }

    if (!hasResource(clergyDice.currentFace, DiceResources.NewRoll)) {
      console.error('Clergy dice does not have NewRoll');
      return;
    }

    if (!frozenDice.value.find(d => d.id === lockedDice.id)) {
      console.error('Selected dice is not frozen');
      return;
    }

    const pair = {
      id: `pair_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      clergyId: clergyDice.id,
      lockedId: lockedDice.id,
      isUsed: false,
      createdAt: new Date().toISOString()
    };

    newRollPairs.value.push(pair);

    // Удаляем clergy из unlocked
    const clergyIndex = unlockedDice.value.findIndex(d => d.id === clergyDice.id);
    if (clergyIndex !== -1) {
      unlockedDice.value.splice(clergyIndex, 1);
    }

    // Размораживаем выбранный кубик
    unfreezeDice(lockedDice);
  }

  // Дополнительный бросок с NewRoll
  function performExtraRoll(pairId) {
    const pair = newRollPairs.value.find(p =>
      !p.isUsed && p.clergyId === pairId
    );

    if (pair) {
      const clergyDice = allDice.value.find(d => d.id === pair.clergyId);
      const lockedDice = allDice.value.find(d => d.id === pair.lockedId);

      // Перебрасываем оба кубика
      clergyDice.currentFace = clergyDice.roll();
      lockedDice.currentFace = lockedDice.roll();

      pair.isUsed = true;

      // Проверяем результат
      if (!hasResource(clergyDice.currentFace, DiceResources.NewRoll)) {
        // Если NewRoll не выпал, пара замораживается
        freezeDice(clergyDice);
        freezeDice(lockedDice);
      }
      // Если выпал - можно привязать снова
    }
  }

  // Завершение хода
  function completeTurn() {
    // Замораживаем все оставшиеся кубики
    unlockedDice.value.forEach(dice => {
      if (dice.currentFace) {
        dice.lock(dice.currentFace);
      }
    });

    frozenDice.value = [...frozenDice.value, ...unlockedDice.value];
    unlockedDice.value = [];
    isRollingComplete.value = true;

    // Увеличиваем счетчик ходов
    turnCount.value++;
    rerollCount.value = 0;
    saveSessionToCookies();
  }

  // Начать новый ход
  async function startNewTurn() {
    if (!hasStarterKit.value) {
      await resetGame();
    } else {
      // Сбрасываем состояние для нового хода
      allDice.value.forEach(dice => {
        dice.unlock();
        dice.currentFace = null;
      });

      frozenDice.value = [];
      unlockedDice.value = [...allDice.value];
      rerollCount.value = 0;
      isRollingComplete.value = false;
      newRollPairs.value = [];

      turnCount.value++;
      saveSessionToCookies();
    }
  }

  // Вспомогательные функции
  function generateSessionId() {
    return "session_" + Date.now() + "_" + Math.random().toString(36).substr(2, 9);
  }

  function hasResource(face, resource) {
    return face && face.values && face.values[resource] > 0;
  }

  return {
    // Состояние
    playerName,
    sessionId,
    allDice,
    frozenDice,
    unlockedDice,
    rerollCount,
    turnCount,
    maxRerolls,
    hasStarterKit,
    newRollPairs,
    isRollingComplete,

    // Геттеры
    canReroll,
    remainingRerolls,

    // Методы
    initializeGame,
    resetGame,
    addDiceByType,
    rollDice,
    freezeDice,
    unfreezeDice,
    bindNewRoll,
    performExtraRoll,
    completeTurn,
    startNewTurn,
    loadSessionFromCookies,
    saveSessionToCookies,
    clearSession
  };
});
