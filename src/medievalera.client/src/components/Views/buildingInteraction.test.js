import { test } from 'node:test'
import assert from 'node:assert/strict'
import { buildingColor, rotationDelta } from './buildingInteraction.js'

test('requested building groups share their colors on board and in catalog', () => {
  for (const group of [['wall', 'tower'], ['monastery', 'cathedral'], ['market', 'university'], ['farm', 'house'], ['cityHouse', 'guild']]) {
    assert.equal(buildingColor(group[0]), buildingColor(group[1]))
  }
  assert.equal(new Set(['wall', 'monastery', 'market', 'farm', 'cityHouse', 'hospital'].map(buildingColor)).size, 6)
})

test('finger rotation handles both directions across the angle boundary', () => {
  assert.equal(rotationDelta(170, -100), 90)
  assert.equal(rotationDelta(-170, 100), -90)
  assert.equal(rotationDelta(0, 90), 90)
  assert.equal(rotationDelta(90, 0), -90)
})
