import { chromium } from 'playwright';
import path from 'node:path';
import url from 'node:url';

const file = path.resolve('planes/deck-dashboard-territorial/_preview/todas.html');
const b = await chromium.launch();
const p = await b.newPage({ viewport: { width: 1760, height: 1000 } });
await p.goto(url.pathToFileURL(file).href);
await p.waitForTimeout(2500);
const res = await p.evaluate(() => {
  const out = [];
  document.querySelectorAll('body > div').forEach(card => {
    const name = card.firstElementChild.textContent.trim();
    const root = card.lastElementChild;
    if (!root) return out.push(name + ' :: SIN ROOT');
    const rr = root.getBoundingClientRect();
    let bottom = 0, right = 0;
    root.querySelectorAll('*').forEach(el => {
      const r = el.getBoundingClientRect();
      if (r.height === 0 && r.width === 0) return;
      bottom = Math.max(bottom, r.bottom - rr.bottom);
      right = Math.max(right, r.right - rr.right);
    });
    out.push(name + ' :: h=' + Math.round(rr.height) +
             ' abajo=' + Math.round(bottom) + ' der=' + Math.round(right));
  });
  return out.join('\n');
});
console.log(res);
await b.close();
