// Capa de datos — abstrae Firestore o JSON local.
// API uniforme: list, get, create, update, remove, subscribe.

import { firebaseConfig, USE_FIREBASE } from './firebase-config.js';

const STORAGE_KEY = 'gestor-actividades-v1';

class LocalStore {
    constructor() {
        this.data = null;
        this.listeners = new Map();
    }

    async init() {
        const cached = localStorage.getItem(STORAGE_KEY);
        if (cached) {
            try { this.data = JSON.parse(cached); return; } catch { /* fallthrough */ }
        }
        const res = await fetch('data/seed.json');
        this.data = await res.json();
        this._persist();
    }

    _persist() { localStorage.setItem(STORAGE_KEY, JSON.stringify(this.data)); }

    _emit(col) {
        const arr = this.listeners.get(col) || [];
        arr.forEach(cb => cb(this.data[col]));
    }

    subscribe(col, cb) {
        if (!this.listeners.has(col)) this.listeners.set(col, []);
        this.listeners.get(col).push(cb);
        cb(this.data[col]);
        return () => {
            const arr = this.listeners.get(col);
            this.listeners.set(col, arr.filter(x => x !== cb));
        };
    }

    async list(col) { return [...(this.data[col] || [])]; }
    async get(col, id) { return (this.data[col] || []).find(x => x.id === id) || null; }

    async create(col, payload) {
        const prefix = col === 'temas' ? 'T' : col === 'actividades' ? 'A' : col === 'comentarios' ? 'C' : 'X';
        const max = (this.data[col] || []).reduce((m, x) => {
            const n = parseInt(String(x.id).split('-')[1] || '0', 10);
            return n > m ? n : m;
        }, 0);
        const id = `${prefix}-${String(max + 1).padStart(3, '0')}`;
        const item = { id, ...payload };
        this.data[col] = [...(this.data[col] || []), item];
        this._persist();
        this._emit(col);
        return item;
    }

    async update(col, id, patch) {
        this.data[col] = (this.data[col] || []).map(x => x.id === id ? { ...x, ...patch } : x);
        this._persist();
        this._emit(col);
        return this.get(col, id);
    }

    async remove(col, id) {
        this.data[col] = (this.data[col] || []).filter(x => x.id !== id);
        this._persist();
        this._emit(col);
    }

    async resetSeed() {
        localStorage.removeItem(STORAGE_KEY);
        await this.init();
        ['temas', 'actividades', 'comentarios', 'alertas'].forEach(c => this._emit(c));
    }
}

class FirebaseStore {
    constructor() { this.app = null; this.db = null; this.listeners = new Map(); }

    async init() {
        const { initializeApp } = await import('https://www.gstatic.com/firebasejs/10.12.0/firebase-app.js');
        const { getFirestore, collection, getDocs, doc, setDoc, updateDoc, deleteDoc, onSnapshot } =
            await import('https://www.gstatic.com/firebasejs/10.12.0/firebase-firestore.js');
        this.app = initializeApp(firebaseConfig);
        this.db = getFirestore(this.app);
        this._fs = { collection, getDocs, doc, setDoc, updateDoc, deleteDoc, onSnapshot };
    }

    async list(col) {
        const snap = await this._fs.getDocs(this._fs.collection(this.db, col));
        return snap.docs.map(d => ({ id: d.id, ...d.data() }));
    }

    async get(col, id) {
        const items = await this.list(col);
        return items.find(x => x.id === id) || null;
    }

    subscribe(col, cb) {
        const unsub = this._fs.onSnapshot(this._fs.collection(this.db, col), snap => {
            cb(snap.docs.map(d => ({ id: d.id, ...d.data() })));
        });
        return unsub;
    }

    async create(col, payload) {
        const prefix = col === 'temas' ? 'T' : col === 'actividades' ? 'A' : col === 'comentarios' ? 'C' : 'X';
        const items = await this.list(col);
        const max = items.reduce((m, x) => {
            const n = parseInt(String(x.id).split('-')[1] || '0', 10);
            return n > m ? n : m;
        }, 0);
        const id = `${prefix}-${String(max + 1).padStart(3, '0')}`;
        const item = { id, ...payload };
        await this._fs.setDoc(this._fs.doc(this.db, col, id), item);
        return item;
    }

    async update(col, id, patch) {
        await this._fs.updateDoc(this._fs.doc(this.db, col, id), patch);
        return this.get(col, id);
    }

    async remove(col, id) {
        await this._fs.deleteDoc(this._fs.doc(this.db, col, id));
    }
}

export const dataService = USE_FIREBASE ? new FirebaseStore() : new LocalStore();
export const dataSource = USE_FIREBASE ? 'Firebase Firestore' : 'Local (JSON mock)';
export const isOnline = USE_FIREBASE;

await dataService.init();
