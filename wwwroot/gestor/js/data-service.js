/**
 * data-service.js — ApiStore
 * Reemplaza el FirebaseStore/LocalStore del prototipo.
 * Mantiene la misma interfaz pública: list, get, create, update, remove, subscribe.
 */

const BASE = '/Gestor/Api';

const DATE_RX = /^\d{4}-\d{2}-\d{2}/;

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
    temas: `${BASE}/Temas`,
    actividades: `${BASE}/Actividades`,
    usuarios: `${BASE}/Usuarios`,
};

// Suscriptores por colección
const _subs = {};

function _notify(col) {
    (_subs[col] ?? []).forEach(cb => cb());
}

function toDateOnly(value) {
    if (!value) return '';
    const txt = String(value);
    const match = txt.match(DATE_RX);
    return match ? match[0] : txt;
}

function normalizeTema(raw = {}) {
    return {
        id: String(raw.id ?? raw.temaId ?? ''),
        clave: raw.clave ?? '',
        tema: raw.tema ?? '',
        descripcion: raw.descripcion ?? '',
        categoria: raw.categoria ?? '',
        prioridad: raw.prioridad ?? 'Media',
        estatus: raw.estatus ?? 'Activo',
        responsablePrincipalId: raw.responsablePrincipalId ?? null,
        responsablePrincipal: raw.responsableNombre ?? raw.responsablePrincipal ?? '',
        fechaInicio: toDateOnly(raw.fechaInicio),
        fechaCompromiso: toDateOnly(raw.fechaCompromiso),
        avanceGeneral: Number(raw.avanceGeneral ?? 0),
        ligaSharePoint: raw.ligaSharePoint ?? '',
        comentariosEjecutivos: raw.comentariosEjecutivos ?? '',
        fechaUltimaActualizacion: toDateOnly(raw.fechaUltimaActualizacion),
        corresponsablesIds: Array.isArray(raw.corresponsables)
            ? raw.corresponsables.map(x => Number(x.idUsuario)).filter(Number.isFinite)
            : Array.isArray(raw.corresponsablesIds)
                ? raw.corresponsablesIds.map(Number).filter(Number.isFinite)
                : []
    };
}

function normalizeActividad(raw = {}) {
    return {
        id: String(raw.id ?? raw.actividadId ?? ''),
        clave: raw.clave ?? '',
        temaId: String(raw.temaId ?? ''),
        temaNombre: raw.temaNombre ?? '',
        actividad: raw.actividad ?? '',
        descripcion: raw.descripcion ?? '',
        responsableId: raw.responsableId ?? null,
        responsable: raw.responsableNombre ?? raw.responsable ?? '',
        fechaInicio: toDateOnly(raw.fechaInicio),
        fechaCompromiso: toDateOnly(raw.fechaCompromiso),
        estatus: raw.estatus ?? 'Pendiente',
        prioridad: raw.prioridad ?? 'Media',
        avance: Number(raw.avance ?? 0),
        bloqueada: Boolean(raw.bloqueada),
        motivoBloqueo: raw.motivoBloqueo ?? '',
        evidenciaUrl: raw.evidenciaUrl ?? '',
        comentarios: raw.comentarios ?? '',
        fechaUltimaActualizacion: toDateOnly(raw.fechaUltimaActualizacion),
        corresponsablesIds: Array.isArray(raw.corresponsables)
            ? raw.corresponsables.map(x => Number(x.idUsuario)).filter(Number.isFinite)
            : Array.isArray(raw.corresponsablesIds)
                ? raw.corresponsablesIds.map(Number).filter(Number.isFinite)
                : []
    };
}

function normalizeUsuario(raw = {}) {
    return {
        idUsuario: Number(raw.idUsuario ?? 0),
        nombre: raw.nombre ?? '',
        correo: raw.correo ?? '',
        cargo: raw.cargo ?? ''
    };
}

export class ApiStore {
    constructor() {
        this._usersCache = null;
    }

    async _listUsersRaw() {
        if (this._usersCache) return this._usersCache;
        const users = await apiFetch(ENDPOINTS.usuarios);
        this._usersCache = Array.isArray(users) ? users.map(normalizeUsuario) : [];
        return this._usersCache;
    }

    async _resolveUserId(explicitId, displayName) {
        const numeric = Number(explicitId);
        if (Number.isFinite(numeric) && numeric > 0) return numeric;
        const name = String(displayName ?? '').trim().toLowerCase();
        if (!name) return null;
        const users = await this._listUsersRaw();
        const hit = users.find(u => u.nombre.trim().toLowerCase() === name);
        return hit ? hit.idUsuario : null;
    }

    async _toTemaPayload(payload = {}) {
        const responsablePrincipalId = await this._resolveUserId(payload.responsablePrincipalId, payload.responsablePrincipal);
        return {
            tema: payload.tema ?? '',
            descripcion: payload.descripcion || null,
            categoria: payload.categoria || null,
            prioridad: payload.prioridad ?? 'Media',
            estatus: payload.estatus ?? 'Activo',
            responsablePrincipalId,
            fechaInicio: payload.fechaInicio || null,
            fechaCompromiso: payload.fechaCompromiso || null,
            ligaSharePoint: payload.ligaSharePoint || null,
            comentariosEjecutivos: payload.comentariosEjecutivos || null,
            corresponsablesIds: Array.isArray(payload.corresponsablesIds)
                ? payload.corresponsablesIds.map(Number).filter(Number.isFinite)
                : []
        };
    }

    async _toActividadPayload(payload = {}) {
        const responsableId = await this._resolveUserId(payload.responsableId, payload.responsable);
        return {
            temaId: Number(payload.temaId),
            actividad: payload.actividad ?? '',
            descripcion: payload.descripcion || null,
            responsableId,
            fechaInicio: payload.fechaInicio || null,
            fechaCompromiso: payload.fechaCompromiso || null,
            estatus: payload.estatus ?? 'Pendiente',
            prioridad: payload.prioridad ?? 'Media',
            avance: Number(payload.avance ?? 0),
            bloqueada: Boolean(payload.bloqueada),
            motivoBloqueo: payload.motivoBloqueo || null,
            evidenciaUrl: payload.evidenciaUrl || null,
            comentarios: payload.comentarios || null,
            corresponsablesIds: Array.isArray(payload.corresponsablesIds)
                ? payload.corresponsablesIds.map(Number).filter(Number.isFinite)
                : []
        };
    }

    // list(col, params) — GET /Gestor/Api/{Col}?...
    async list(col, params = {}) {
        const ep = ENDPOINTS[col];
        if (!ep) throw new Error(`Colección desconocida: ${col}`);
        const qs = new URLSearchParams(params).toString();
        const url = qs ? `${ep}?${qs}` : ep;
        const result = await apiFetch(url);
        if (!Array.isArray(result)) return result;
        if (col === 'temas') return result.map(normalizeTema);
        if (col === 'actividades') return result.map(normalizeActividad);
        if (col === 'usuarios') return result.map(normalizeUsuario);
        return result;
    }

    // get(col, id)
    async get(col, id) {
        const ep = ENDPOINTS[col];
        if (!ep) throw new Error(`Colección desconocida: ${col}`);
        const item = await apiFetch(`${ep}/${id}`);
        if (col === 'temas') return normalizeTema(item);
        if (col === 'actividades') return normalizeActividad(item);
        if (col === 'usuarios') return normalizeUsuario(item);
        return item;
    }

    // create(col, payload)
    async create(col, payload) {
        const ep = ENDPOINTS[col];
        if (!ep) throw new Error(`Colección desconocida: ${col}`);
        let body = payload;
        if (col === 'temas') body = await this._toTemaPayload(payload);
        if (col === 'actividades') body = await this._toActividadPayload(payload);
        const result = await apiFetch(ep, {
            method: 'POST',
            body: JSON.stringify(body),
        });
        _notify(col);
        if (col === 'temas') return normalizeTema(result);
        if (col === 'actividades') return normalizeActividad(result);
        return result;
    }

    // update(col, id, patch) — maps to PUT (full update)
    async update(col, id, patch) {
        const ep = ENDPOINTS[col];
        if (!ep) throw new Error(`Colección desconocida: ${col}`);
        let body = patch;
        if (col === 'temas') body = await this._toTemaPayload(patch);
        if (col === 'actividades') body = await this._toActividadPayload(patch);
        const result = await apiFetch(`${ep}/${id}`, {
            method: 'PUT',
            body: JSON.stringify(body),
        });
        _notify(col);
        if (col === 'temas') return normalizeTema(result);
        if (col === 'actividades') return normalizeActividad(result);
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
export const dataSource = 'Base de datos NSIE';
export const isOnline = true;
