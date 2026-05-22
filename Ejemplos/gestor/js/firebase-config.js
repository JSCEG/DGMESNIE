// Firebase config — pega tus credenciales aquí para activar Firestore.
// Mientras estén vacías, el data-service usa el JSON mock en data/seed.json.
//
// Pasos:
// 1) Crea proyecto en https://console.firebase.google.com
// 2) Habilita Firestore (modo producción) con colecciones: temas, actividades, comentarios, alertas
// 3) Copia config web aquí
// 4) Recarga la app.

export const firebaseConfig = {
    apiKey: "",
    authDomain: "",
    projectId: "",
    storageBucket: "",
    messagingSenderId: "",
    appId: ""
};

export const USE_FIREBASE = Boolean(firebaseConfig.projectId);
