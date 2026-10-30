export const cellSize = 18.6

export function catalogGrid(buildings, size = 10) {
  const dimensions = buildings.map(({ mask }) => {
    const rows = mask.split('\n')
    return { columns: Math.max(...rows.map(row => row.length)), rows: rows.length }
  })
  const columns = Math.max(1, ...dimensions.map(item => item.columns))
  const rows = Math.max(1, ...dimensions.map(item => item.rows))
  const width = columns * size + 2, height = rows * size + 2
  return { columns, rows, size, width, height, viewBox: `-1 -1 ${width} ${height}` }
}

export function catalogShape(mask, grid) {
  const rows = mask.split('\n').length
  return shapePaths(maskCells(mask), grid.size, grid.size / 2,
    (grid.rows - rows + 1) * grid.size / 2)
}

export function maskCells(mask, rotation = 0) {
  let rows = mask.split('\n').map(row => [...row])
  for (let angle = 0; angle < rotation; angle += 90) {
    rows = rows[0].map((_, x) => rows.map(row => row[x]).reverse())
  }
  return rows.flatMap((row, y) => row.flatMap((cell, x) => cell === 'x' ? [{ x, y }] : []))
}

// Fill adjacent cells without seams; stroke only exposed edges, including holes.
export function shapePaths(cells, size = cellSize, originX = 29, originY = 150) {
  const occupied = new Set(cells.map(({ x, y }) => `${x},${y}`))
  let fill = '', outline = ''
  for (const { x, y } of cells) {
    const left = originX + (x - .5) * size, top = originY + (y - .5) * size
    fill += `M${left},${top}h${size}v${size}h${-size}Z`
    if (!occupied.has(`${x},${y - 1}`)) outline += `M${left},${top}h${size}`
    if (!occupied.has(`${x + 1},${y}`)) outline += `M${left + size},${top}v${size}`
    if (!occupied.has(`${x},${y + 1}`)) outline += `M${left},${top + size}h${size}`
    if (!occupied.has(`${x - 1},${y}`)) outline += `M${left},${top}v${size}`
  }
  return { fill, outline }
}

export function canPlace(field, cells, ignoredBuildingId = null) {
  return cells.length > 0 && cells.every(({ x, y }) =>
    y >= 0 && y < field.length && x >= 0 && x < field[y].length &&
    (field[y][x] == null || (ignoredBuildingId !== null && field[y][x] === ignoredBuildingId)))
}
