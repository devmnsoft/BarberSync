const api = 'http://localhost:3001';
async function loadServices(){ const res = await fetch(`${api}/services`); return res.json(); }
window.loadServices = loadServices;
