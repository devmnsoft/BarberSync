import React from 'react';
import { createRoot } from 'react-dom/client';
import App from '../../App.jsx';
import './totem.css';

export const API_PREFIX = import.meta.env.VITE_KIOSK_API_PREFIX || '/KioskApi';
export const DEVICE_CODE = new URLSearchParams(location.search).get('deviceCode') || 'KIOSK-001';

export class KioskError extends Error {
  constructor(message, traceId) { super(message); this.traceId = traceId; }
}

export async function kioskRequest(path, { method = 'GET', body, signal } = {}) {
  let response;
  try {
    response = await fetch(`${API_PREFIX}${path}`, {
      method, signal, credentials: 'same-origin',
      headers: { Accept: 'application/json', ...(body ? { 'Content-Type': 'application/json' } : {}) },
      body: body ? JSON.stringify(body) : undefined
    });
  } catch (error) {
    throw new KioskError(navigator.onLine ? 'Não foi possível conectar. Chame um atendente.' : 'Totem offline. Sua operação não foi enviada.');
  }
  const payload = await response.json().catch(() => null);
  const traceId = payload?.traceId || response.headers.get('x-trace-id');
  if (!response.ok || payload?.success === false) throw new KioskError(payload?.message || 'Não foi possível concluir esta etapa.', traceId);
  return payload?.data ?? payload?.items ?? payload;
}

export const kioskApi = {
  branches: signal => kioskRequest(`/branches?deviceCode=${encodeURIComponent(DEVICE_CODE)}`, { signal }),
  services: (branchId, signal) => kioskRequest(`/services?deviceCode=${encodeURIComponent(DEVICE_CODE)}&branchId=${encodeURIComponent(branchId)}`, { signal }),
  identify: phone => kioskRequest('/client/find-by-phone', { method: 'POST', body: { phone, deviceCode: DEVICE_CODE } }),
  register: input => kioskRequest('/client/quick-register', { method: 'POST', body: { ...input, deviceCode: DEVICE_CODE } }),
  professionals: (branchId, serviceId) => kioskRequest(`/professionals?deviceCode=${encodeURIComponent(DEVICE_CODE)}&branchId=${encodeURIComponent(branchId)}&serviceId=${encodeURIComponent(serviceId)}`),
  slots: (branchId, serviceId, professionalId, date) => kioskRequest(`/availability?deviceCode=${encodeURIComponent(DEVICE_CODE)}&branchId=${encodeURIComponent(branchId)}&serviceId=${encodeURIComponent(serviceId)}&professionalId=${encodeURIComponent(professionalId)}&date=${date}`),
  checkIn: body => kioskRequest('/check-in', { method: 'POST', body: { ...body, deviceCode: DEVICE_CODE } }),
  preOrder: body => kioskRequest('/pre-orders', { method: 'POST', body: { ...body, deviceCode: DEVICE_CODE } }),
  flow: (method, body) => kioskRequest('/flow', { method, body })
};

createRoot(document.getElementById('root')).render(React.createElement(App, { api: kioskApi }));
