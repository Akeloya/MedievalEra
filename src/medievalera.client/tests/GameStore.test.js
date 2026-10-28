import assert from 'node:assert/strict';
import { test } from 'node:test';
import { createPinia, setActivePinia } from 'pinia';
import { Dice } from '../src/components/TableGame/Dice.js';
import { useGameStore } from '../src/components/TableGame/GameStore.js';
import diceApi from '../src/components/TableGame/diceApi.js';

function makeDice(id, values, diceType = 1) {
  return new Dice({ id, diceType, faces: [{ values, choose: false }] });
}

function historyStore() {
  setActivePinia(createPinia());
  const store = useGameStore();
  store.turnCount = 1;
  store.allDice = [makeDice('first', { meal: 3 }), makeDice('second', { wood: 3 }), makeDice('third', { stone: 2 })];
  store.unlockedDice = [...store.allDice];
  return store;
}

test('manual toggles cancel and commit only at a roll or completion', async () => {
  const store = historyStore();
  await store.rollDice();
  const first = store.allDice[0];
  store.freezeDice(first);
  store.unfreezeDice(first);
  assert.equal(store.rollHistory.length, 1);
  await store.rollDice();
  assert.deepEqual(store.rollHistory[1].actionsBefore, []);
  store.freezeDice(first);
  store.unfreezeDice(first);
  store.freezeDice(first);
  assert.equal(store.completedTurnActions.length, 0);
  store.completeTurn();
  assert.deepEqual(store.completedTurnActions[0].changes.map(die => [die.id, die.frozen]), [['first', true]]);
  store.completeTurn();
  assert.equal(store.completedTurnActions.length, 1);
});

test('unfreezing and refreezing a previously saved die cancel', async () => {
  const store = historyStore();
  await store.rollDice();
  const first = store.allDice[0];
  store.freezeDice(first);
  await store.rollDice();
  assert.deepEqual(store.rollHistory[1].actionsBefore.map(die => [die.id, die.frozen]), [['first', true]]);
  store.unfreezeDice(first);
  store.freezeDice(first);
  store.completeTurn();
  assert.equal(store.completedTurnActions.length, 0);
});

test('unfreeze records the old face and automatic skull locks are excluded', async () => {
  const store = historyStore();
  store.allDice[1].faces = [{ values: { skull: 1 }, choose: false }];
  await store.rollDice();
  const first = store.allDice[0];
  store.freezeDice(first);
  await store.rollDice();
  store.unfreezeDice(first);
  first.faces = [{ values: { wood: 3 }, choose: false }];
  await store.rollDice();
  const entry = store.rollHistory[2];
  assert.deepEqual(entry.actionsBefore.map(die => [die.id, die.frozen]), [['first', false]]);
  assert.deepEqual(entry.actionsBefore[0].face.values, { meal: 3 });
  assert.deepEqual(entry.dice.find(die => die.id === 'first').face.values, { wood: 3 });
  assert.equal(store.completedTurnActions.length, 0);
});

test('retained dice keep their origin and sort behind newer results', async () => {
  const store = historyStore();
  await store.rollDice();
  store.freezeDice(store.allDice[0]);
  await store.rollDice();
  assert.deepEqual(store.rollHistory[1].dice.map(die => [die.id, die.originRoll]), [['second', 2], ['third', 2], ['first', 1]]);
  store.freezeDice(store.allDice[1]);
  await store.rollDice();
  assert.deepEqual(store.rollHistory[2].dice.map(die => [die.id, die.originRoll]), [['third', 3], ['second', 2], ['first', 1]]);
});

test('acquired dice wait for the next turn and then roll with the existing dice', async (t) => {
  setActivePinia(createPinia());
  const store = useGameStore();
  const existing = makeDice('existing', { meal: 3 });
  store.allDice = [existing];
  store.unlockedDice = [existing];
  store.hasStarterKit = true;
  await store.rollDice();
  store.completeTurn();

  const acquired = [makeDice('new-first', { wood: 3 }), makeDice('new-second', { stone: 2 })];
  const getDice = t.mock.method(diceApi, 'getDiceByType', async () => acquired.shift());
  const first = await store.addDiceByType(1);
  const second = await store.addDiceByType(1);

  assert.equal(getDice.mock.callCount(), 2);
  assert.deepEqual(store.newDice.map(die => die.id), ['new-first', 'new-second']);
  assert.equal(store.allDice.length, 3);
  assert.equal(store.unlockedDice.length, 0);
  assert.deepEqual(store.frozenDice.map(die => die.id), ['existing']);
  assert.equal(first.currentFace, null);
  assert.equal(second.currentFace, null);
  assert.equal(first.canBeLocked, false);
  await assert.rejects(store.rollDice(), /Нет доступных бросков/);

  await store.startNewTurn();

  assert.equal(store.newDice.length, 0);
  assert.equal(store.frozenDice.length, 0);
  assert.deepEqual(store.unlockedDice.map(die => die.id), ['existing', 'new-first', 'new-second']);
  assert.ok(store.unlockedDice.every(die => die.currentFace === null && !die.isLocked));
  await store.rollDice();
  assert.ok(store.unlockedDice.every(die => die.currentFace === die.faces[0] && die.canBeLocked));
});

test('resetting the game discards pending new dice', async (t) => {
  setActivePinia(createPinia());
  const store = useGameStore();
  t.mock.method(diceApi, 'getDiceByType', async () => makeDice('new', { wood: 3 }));
  t.mock.method(diceApi, 'getStarterKit', async () => [makeDice('starter', { meal: 3 })]);
  await store.addDiceByType(1);

  await store.resetGame();

  assert.equal(store.newDice.length, 0);
  assert.equal(store.rollHistory.length, 0);
  assert.equal(store.turnCount, 1);
  assert.deepEqual(store.allDice.map(die => die.id), ['starter']);
  assert.deepEqual(store.unlockedDice.map(die => die.id), ['starter']);
});

test('history keeps snapshots, retained dice, and consecutive turn numbers', async (t) => {
  setActivePinia(createPinia());
  const store = useGameStore();
  t.mock.method(diceApi, 'getStarterKit', async () => [
    makeDice('first', { meal: 3 }), makeDice('second', { wood: 3 })
  ]);
  await store.resetGame();
  await store.rollDice();
  store.freezeDice(store.unlockedDice[0]);
  store.unlockedDice[0].faces = [{ values: { stone: 2 }, choose: false }];
  await store.rollDice();

  assert.deepEqual(store.rollHistory.map(entry => [entry.turn, entry.roll]), [[1, 1], [1, 2]]);
  assert.deepEqual(store.rollHistory[0].dice[1].face.values, { wood: 3 });
  assert.deepEqual(store.rollHistory[1].dice.map(die => die.wasRolled), [true, false]);
  store.allDice[0].currentFace.values.meal = 99;
  assert.equal(store.rollHistory[0].dice[0].face.values.meal, 3);
  store.completeTurn();
  assert.equal(store.turnCount, 1);
  await store.startNewTurn();
  await store.rollDice();
  assert.deepEqual(store.rollHistory.map(entry => [entry.turn, entry.roll]), [[1, 1], [1, 2], [2, 1]]);
});

test('history records additional rolls separately', () => {
  setActivePinia(createPinia());
  const store = useGameStore();
  const clergy = makeDice('clergy', { meal: 1 }, 3);
  const target = makeDice('target', { wood: 3 });
  store.turnCount = 1;
  store.allDice = [clergy, target];
  store.unlockedDice = [target];
  store.newRollPairs = [{ id: 'pair', clergyId: 'clergy', lockedId: 'target', isUsed: false }];

  store.performExtraRoll('pair');

  assert.equal(store.rollHistory.length, 1);
  assert.equal(store.rollHistory[0].isExtra, true);
  assert.deepEqual(store.rollHistory[0].dice.map(die => die.wasRolled), [true, true]);
  assert.deepEqual(store.rollHistory[0].dice[1].face.values, { wood: 3 });
});

for (const skullPositions of [[0], [1], [0, 1]]) {
  test(`rolls every die when skulls occur at positions ${skullPositions}`, async () => {
    setActivePinia(createPinia());
    const store = useGameStore();
    const dice = Array.from({ length: 4 }, (_, index) =>
      makeDice(`dice-${index}`, skullPositions.includes(index) ? { skull: 1 } : { meal: 3 })
    );
    store.allDice = dice;
    store.unlockedDice = [...dice];

    await store.rollDice();

    assert.equal(store.rerollCount, 1);
    assert.equal(store.frozenDice.length, skullPositions.length);
    for (const die of store.allDice) {
      assert.equal(die.currentFace, die.faces[0]);
      assert.equal(die.canBeLocked, !die.currentFace.values.skull);
    }
    const mealDice = [...store.unlockedDice];
    for (const die of mealDice) store.freezeDice(die);
    assert.equal(store.unlockedDice.length, 0);
    assert.ok(mealDice.every(die => die.isLocked));
  });
}

