import { test } from 'node:test'
import assert from 'node:assert/strict'
import { maskCells, shapePaths, canPlace, catalogGrid, catalogShape } from './buildingGeometry.js'

test('catalog computes shared dimensions and vertically centers different heights at one scale', () => {
  const grid = catalogGrid([{ mask: 'xx' }, { mask: 'xx\nxx\nxx' }, { mask: 'xxxx' }])
  assert.equal(grid.rows, 3)
  assert.equal(grid.columns, 4)
  assert.equal(grid.viewBox, '-1 -1 42 32')
  assert.ok(catalogShape('xx', grid).fill.startsWith('M0,10h10v10'))
  assert.ok(catalogShape('xx\nxx', grid).fill.startsWith('M0,5h10v10'))
  assert.ok(catalogShape('xx\nxx\nxx', grid).fill.startsWith('M0,0h10v10'))
  const larger = catalogGrid([{ mask: 'xxxxxx' }, { mask: 'xx\nxx\nxx' }])
  assert.equal(larger.columns, 6)
  assert.equal(larger.width, 62)
})

test('tower has one exterior outline without interior dividing edges', () => {
  const paths = shapePaths(maskCells('xx\nxx'), 10, 5, 5)
  assert.equal((paths.outline.match(/M/g) ?? []).length, 8)
  assert.ok(!paths.outline.includes('M10,0v10'))
})

test('concave masks rotate clockwise and preserve empty cells', () => {
  assert.deepEqual(maskCells('xx\nox', 90), [{ x: 1, y: 0 }, { x: 0, y: 1 }, { x: 1, y: 1 }])
  const field = Array.from({ length: 3 }, () => Array(3).fill(null))
  field[1][0] = 0
  assert.equal(canPlace(field, maskCells('xx\nox')), true)
  assert.equal(canPlace(field, maskCells('xx\nox', 90)), false)
  assert.equal(canPlace(field, [{ x: -1, y: 0 }]), false)
  assert.equal(canPlace(field, [{ x: 3, y: 0 }]), false)
  assert.equal(canPlace(field, [{ x: 0, y: 3 }]), false)
})

test('moving may reuse its own cells but never those of another building', () => {
  const field = [[null, 1, 1, 2]]
  assert.equal(canPlace(field, [{ x: 0, y: 0 }, { x: 1, y: 0 }], 1), true)
  assert.equal(canPlace(field, [{ x: 2, y: 0 }, { x: 3, y: 0 }], 1), false)
  assert.equal(canPlace(field, [{ x: 1, y: 0 }]), false)
})
