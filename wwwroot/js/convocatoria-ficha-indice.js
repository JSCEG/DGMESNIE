(function () {
    'use strict';

    // Índice de la ficha Mixtos II con la misma anatomía del índice PAM: grupos numerados
    // (pam-index__group) y entradas con líder punteado (pam-index__entry) en dos columnas.
    // Capacidad por página en filas; un encabezado de grupo ocupa más que una entrada.
    const ROWS_PER_PAGE = 33;
    const GROUP_ROWS = 1.3;
    const pad = value => String(value).padStart(2, '0');

    // Recalcular después de insertar obras, evaluaciones y resultados territoriales.
    // No guardar destinos por posiciones anteriores a la carga del análisis.
    function actualizar(deck) {
        const template = deck?.querySelector('[data-convocatoria-index]');
        if (!template) return;
        const doc = deck.ownerDocument;
        deck.querySelectorAll('[data-convocatoria-index-continuation]').forEach(slide => slide.remove());

        // Cada lámina hereda el grupo de la lámina estática anterior (data-index-group);
        // así las láminas generadas por JS (obras, evaluaciones, INEGI, permisos) quedan
        // bajo la sección que las origina sin declararlo en cada una.
        let group = null;
        const entries = [];
        const dividers = new Map(); // portadillas de sección: encabezado del grupo en el índice, no entrada
        Array.from(deck.querySelectorAll('[data-slide]')).forEach(slide => {
            if (slide.dataset.indexGroup) group = slide.dataset.indexGroup;
            if (slide === template || slide.classList.contains('pam-slide--cover') || slide.classList.contains('pam-slide--back')) return;
            if (slide.classList.contains('pam-slide--divider')) { if (group && !dividers.has(group)) dividers.set(group, slide); return; }
            entries.push({ slide, group: group || 'Contenido' });
        });
        const groups = [];
        entries.forEach(entry => { if (!groups.includes(entry.group)) groups.push(entry.group); });

        const rows = [];
        groups.forEach((name, position) => {
            const items = entries.filter(entry => entry.group === name);
            rows.push({ kind: 'group', name, number: pad(position + 1), first: dividers.get(name) || items[0].slide, rows: GROUP_ROWS });
            items.forEach(entry => rows.push({ kind: 'entry', slide: entry.slide, group: name, number: pad(position + 1), rows: 1 }));
        });

        // Paginación: un encabezado nunca queda huérfano al final de una página y un grupo
        // que continúa en la siguiente página repite su encabezado.
        const pages = [[]];
        let used = 0;
        rows.forEach(row => {
            const needed = row.kind === 'group' ? row.rows + 1 : row.rows;
            if (used + needed > ROWS_PER_PAGE && pages.at(-1).length) {
                pages.push([]);
                used = 0;
                if (row.kind === 'entry') {
                    pages.at(-1).push({ kind: 'group', name: row.group, number: row.number, first: row.slide, rows: GROUP_ROWS, continued: true });
                    used += GROUP_ROWS;
                }
            }
            pages.at(-1).push(row);
            used += row.rows;
        });

        const indexes = [template];
        for (let page = 1; page < pages.length; page++) {
            const continuation = template.cloneNode(true);
            continuation.removeAttribute('data-convocatoria-index');
            continuation.setAttribute('data-convocatoria-index-continuation', '');
            continuation.classList.remove('is-active');
            continuation.setAttribute('aria-hidden', 'true');
            indexes.at(-1).after(continuation);
            indexes.push(continuation);
        }

        // Numeración de pantalla y botón "volver al índice" en el pie de cada lámina.
        const slides = Array.from(deck.querySelectorAll('[data-slide]'));
        slides.forEach((slide, index) => {
            slide.dataset.screenLabel = pad(index + 1);
            const footer = slide.querySelector('.pam-lamina-foot__meta') || slide.querySelector('.pam-lamina-foot > span:last-child');
            if (!footer) return;
            footer.classList.add('pam-lamina-foot__meta');
            footer.replaceChildren();
            if (!indexes.includes(slide)) {
                const home = doc.createElement('button');
                home.type = 'button';
                home.className = 'pam-home';
                home.dataset.goto = String(Math.max(0, slides.indexOf(template)));
                home.title = 'Volver al índice';
                home.setAttribute('aria-label', home.title);
                home.innerHTML = '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M3 11.5 12 4l9 7.5"></path><path d="M5.5 10v9h13v-9"></path></svg>';
                footer.append(home);
            }
            footer.append(doc.createTextNode(pad(index + 1)));
        });

        indexes.forEach((slide, page) => {
            slide.dataset.label = page ? `Índice · ${page + 1}` : 'Índice';
            slide.querySelector('[data-index-title]').textContent = pages.length > 1 ? `Índice · ${page + 1} de ${pages.length}` : 'Índice';
            const container = slide.querySelector('[data-index-entries]');
            container.replaceChildren();
            pages[page].forEach(row => {
                const button = doc.createElement('button');
                button.type = 'button';
                if (row.kind === 'group') {
                    button.className = 'pam-index__group';
                    button.dataset.gotoLabel = row.first.dataset.screenLabel;
                    const number = doc.createElement('b');
                    number.textContent = row.number;
                    const name = doc.createElement('span');
                    name.textContent = row.continued ? `${row.name} · continúa` : row.name;
                    button.append(number, name);
                } else {
                    button.className = 'pam-index__entry';
                    button.dataset.gotoLabel = row.slide.dataset.screenLabel;
                    const label = doc.createElement('span');
                    label.textContent = row.slide.dataset.label || 'Contenido';
                    const number = doc.createElement('small');
                    number.textContent = row.slide.dataset.screenLabel;
                    button.append(label, doc.createElement('i'), number);
                }
                container.append(button);
            });
        });
    }

    window.convocatoriaFichaIndice = { actualizar };
})();
