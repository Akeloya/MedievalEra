// stores/gameStore.js
import { defineStore } from "pinia";
import { ref, computed } from "vue";
import diceApi from "./diceApi";
import cookies from "js-cookie";
import DiceResources from "./DiceResources";
import { Dice } from './Dice';

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
  const isTurnCompleted = ref(false);

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

      // Убеждаемся, что каждый кубик - экземпляр Dice
      allDice.value = starterKit.map(dice => {
        if (dice instanceof Dice) {
          return dice;
        }
        return new Dice(dice);
      });

      frozenDice.value = [];
      unlockedDice.value = [...allDice.value];
      rerollCount.value = 0;
      isRollingComplete.value = false;
      newRollPairs.value = [];
      hasStarterKit.value = true;

      turnCount.value = 0;
      isTurnCompleted.value = false;
      saveSessionToCookies();
    } catch (error) {
      console.error('Failed to reset game:', error);
      throw error;
    }
  }

  // Добавление кубика по типу
  async function addDiceByType(type) {
    try {
      const dice = await diceApi.getDiceByType(type);
      // Убеждаемся, что это экземпляр Dice
      const newDice = dice instanceof Dice ? dice : new Dice(dice);
      allDice.value.push(newDice);
      unlockedDice.value.push(newDice);
      return newDice;
    } catch (error) {
      console.error(`Failed to get dice type ${type}:`, error);
      throw error;
    }
  }

  // Бросок всех незамороженных кубиков
  async function rollDice() {
    if (!canReroll.value) {
      throw new Error('Нет доступных бросков');
    }

    console.log('Rolling dice:', unlockedDice.value.length);

    // Бросаем каждый незамороженный кубик
    unlockedDice.value.forEach(dice => {
      if (dice instanceof Dice) {
        dice.currentFace = dice.roll();
        console.log(`Dice ${dice.id} rolled:`, dice.currentFace);

        // Проверяем на Skull
        if (hasResource(dice.currentFace, DiceResources.Skull)) {
          console.log(`Dice ${dice.id} has Skull, freezing`);
          freezeDice(dice);
        }
      } else {
        console.error('Invalid dice object:', dice);
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
      if (dice instanceof Dice) {
        dice.lock(dice.currentFace);
      }
      unlockedDice.value.splice(index, 1);
      frozenDice.value.push(dice);
      console.log(`Dice ${dice.id} frozen`);
    }
  }

  // Разморозка кубика
  function unfreezeDice(dice) {
    const index = frozenDice.value.findIndex(d => d.id === dice.id);
    if (index !== -1) {
      if (dice instanceof Dice) {
        dice.unlock();
      }
      frozenDice.value.splice(index, 1);
      unlockedDice.value.push(dice);
      console.log(`Dice ${dice.id} unfrozen`);
    }
  }
  // Обработка после последнего броска
  async function handleAfterLastRoll() {
    console.log('After last roll handling');

    // Обрабатываем кубики Clergy с NewRoll
    const clergyWithNewRoll = unlockedDice.value.filter(dice =>
      dice.diceType === 3 &&
      dice.currentFace &&
      hasResource(dice.currentFace, DiceResources.NewRoll)
    );

    if (clergyWithNewRoll.length > 0) {
      console.log(`Found ${clergyWithNewRoll.length} clergy dice with NewRoll`);
      // Замораживаем обычные кубики
      unlockedDice.value.forEach(dice => {
        if (dice.diceType !== 3) {
          freezeDice(dice);
        }
      });
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

    console.log('NewRoll pair created:', pair);
  }

  // Дополнительный бросок с NewRoll
  function performExtraRoll(pairId) {
    const pairIndex = newRollPairs.value.findIndex(p =>
      !p.isUsed && (p.clergyId === pairId || p.id === pairId)
    );

    if (pairIndex === -1) {
      console.error('Pair not found or already used');
      return;
    }

    const pair = newRollPairs.value[pairIndex];
    const clergyDice = allDice.value.find(d => d.id === pair.clergyId);
    const lockedDice = allDice.value.find(d => d.id === pair.lockedId);

    if (!clergyDice || !lockedDice) {
      console.error('Dice not found');
      return;
    }

    // Перебрасываем оба кубика
    if (clergyDice instanceof Dice) {
      clergyDice.currentFace = clergyDice.roll();
    }
    if (lockedDice instanceof Dice) {
      lockedDice.currentFace = lockedDice.roll();
    }

    pair.isUsed = true;

    console.log('Extra roll performed:', {
      clergy: clergyDice.currentFace,
      locked: lockedDice.currentFace
    });

    // Проверяем результат
    if (hasResource(clergyDice.currentFace, DiceResources.NewRoll)) {
      // Если NewRoll выпал снова, кубик остается в unlocked для повторной привязки
      if (!unlockedDice.value.find(d => d.id === clergyDice.id)) {
        unlockedDice.value.push(clergyDice);
      }
    } else {
      // Если NewRoll не выпал, пара замораживается
      freezeDice(clergyDice);
      freezeDice(lockedDice);
      // Удаляем использованную пару
      newRollPairs.value.splice(pairIndex, 1);
    }
  }

  // Завершение хода
  function completeTurn() {
    if (isTurnCompleted.value) {
      console.log('Turn already completed');
      return;
    }
    console.log('Completing turn');

    // Замораживаем все оставшиеся кубики
    unlockedDice.value.forEach(dice => {
      if (dice instanceof Dice && dice.currentFace) {
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
    isTurnCompleted.value = true;
    console.log('Turn completed');
  }

  // Начать новый ход
  async function startNewTurn() {
    isTurnCompleted.value = false;
    console.log('Starting new turn');

    if (!hasStarterKit.value) {
      await resetGame();
    } else {
      // Сбрасываем состояние для нового хода
      allDice.value.forEach(dice => {
        if (dice instanceof Dice) {
          dice.unlock();
          dice.currentFace = null;
        }
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
    isTurnCompleted,

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
