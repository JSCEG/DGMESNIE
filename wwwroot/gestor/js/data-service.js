/**
 * data-service.js — ApiStore
 * Reemplaza el FirebaseStore/LocalStore del prototipo.
 * Mantiene la misma interfaz pública: list, get, create, update, remove, subscribe.
 */

const BASE = '/Gestor/Api';

function csrfToken() {
    return document.querySelector('meta[name="csrf-token"]')?.content ?? '';
}

async function apiFetch(url, opts = {}) {
    const method = (opts.method ?? 'GET').toUpperCase();
    const headers = { 'Content-Type': 'application/json', ...(opts.headers ?? {}) };

    if (['POST', 'PUT', 'DELETE', 'PATCH'].includes(method)) {
        headers['RequestVerificationToken'] = csrfToken();
    }

    const res = await fetch(url, { ...opts, headers });
    if (!res.ok) {
        const body = await res.text().catch(() => '');
        throw new Error(`[${res.status}] ${url} — ${body}`);
    }
    // DELETE 200/204 may return empty body
    const ct = res.headers.get('content-type') ?? '';
    return ct.includes('application/json') ? res.json() : null;
}

// Colección → endpoint
const ENDPOINTS = {
    temas:       `${BASE}/Temas`,
    actividades: `${BASE}/Actividades`,
    usuarios:    `${BASE}/Usuarios`,
};

// Suscriptores por colección
const _subs = {};

function _notify(col) {
    (_subs[col] ?? []).forEach(cb => cb());
}

export class ApiStore {
    // list(col, params) — GET /Gestor/Api/{Col}?...
    async list(col, params = {}) {
        const ep = ENDPOINTS[col];
        if (!ep) throw new Error(`Colección desconocida: ${col}`);
        const qs = new URLSearchParams(params).toString();
        const url = qs ? `${ep}?${qs}` : ep;
        return apiFetch(url);
    }

    // get(col, id)
    async get(col, id) {
        const ep = ENDPOINTS[col];
        if (!ep) throw new Error(`Colección desconocida: ${col}`);
        return apiFetch(`${ep}/${id}`);
    }

    // create(col, payload)
    async create(col, payload) {
        const ep = ENDPOINTS[col];
        if (!ep) throw new Error(`Colección desconocida: ${col}`);
        const result = await apiFetch(ep, {
            method: 'POST',
            body: JSON.stringify(payload),
        });
        _notify(col);
        return result;
    }

    // update(col, id, patch) — maps to PUT (full update)
    async update(col, id, patch) {
        const ep = ENDPOINTS[col];
        if (!ep) throw new Error(`Colección desconocida: ${col}`);
        const result = await apiFetch(`${ep}/${id}`, {
            method: 'PUT',
            body: JSON.stringify(patch),
        });
        _notify(col);
        return result;
    }

    // remove(col, id)
    async remove(col, id) {
        const ep = ENDPOINTS[col];
        if (!ep) throw new Error(`Colección desconocida: ${col}`);
        await apiFetch(`${ep}/${id}`, { method: 'DELETE' });
        _notify(col);
    }

    // subscribe(col, cb) — invocado por app.js para escuchar cambios.
    // Estrategia sencilla: registra callback; se llama después de cada mutación.
    subscribe(col, cb) {
        if (!_subs[col]) _subs[col] = [];
        _subs[col].push(cb);
        // Retorna función de limpieza compatible con el patrón del prototipo
        return () => {
            _subs[col] = _subs[col].filter(f => f !== cb);
        };
    }
}

export const dataService = new ApiStore();
export const dataSource  = 'Base de datos NSIE';
export const isOnline    = true;
