// Builders for repetitive diagram parts. Load before deck.js so presenter clones include them.
// <svg data-dots="total,cols,pitch,radius"> fills each <g data-range="from,to"> with dots.
// <div class="cells" data-cells="pppmmm..."> draws the 16 declared instances grouped by type
// (or one type's instances when fewer codes are given):
// p pass, m missing value, x element not found, n no matching rule, i neutral instance.
(() => {
  const SVG = 'http://www.w3.org/2000/svg';
  document.querySelectorAll('svg[data-dots]').forEach((svg) => {
    const [, cols, pitch, r] = svg.dataset.dots.split(',').map(Number);
    svg.querySelectorAll('g[data-range]').forEach((g) => {
      const [from, to] = g.dataset.range.split(',').map(Number);
      for (let k = from; k < to; k++) {
        const c = document.createElementNS(SVG, 'circle');
        c.setAttribute('cx', (r + 2 + (k % cols) * pitch).toFixed(1));
        c.setAttribute('cy', (r + 2 + Math.floor(k / cols) * pitch).toFixed(1));
        c.setAttribute('r', r);
        g.append(c);
      }
    });
  });

  const GROUPS = [['A', 3], ['B', 3], ['C', 3], ['D', 3], ['ACT', 2], ['GWB', 2]];
  document.querySelectorAll('.cells[data-cells], .cells[data-heads]').forEach((row) => {
    const codes = (row.dataset.cells || '').split('');
    let k = 0;
    // A short code string is one type's instances; 16 codes or data-heads span all six types.
    const groups = codes.length && codes.length !== 16 ? [['', codes.length]] : GROUPS;
    groups.forEach(([name, n]) => {
      const group = document.createElement('span');
      group.className = 'g';
      if (row.dataset.heads !== undefined) {
        group.textContent = name;
        group.style.width = `calc(${n} * var(--cell) + ${n - 1} * var(--cgap))`;
      } else {
        for (let j = 0; j < n; j++) {
          const cell = document.createElement('span');
          cell.className = `cell ${codes[k++] || 'i'}`;
          group.append(cell);
        }
      }
      row.append(group);
    });
  });
})();
