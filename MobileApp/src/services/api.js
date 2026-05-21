export const API_URL = process.env.EXPO_PUBLIC_API_URL ?? 'http://localhost:5000';

async function httpGet(path) {
  const response = await fetch(`${API_URL}${path}`);
  if (!response.ok) {
    throw new Error(`Request failed for ${path} with status ${response.status}`);
  }

  return response.json();
}

export const mobileApi = {
  getOperationsSnapshot: () => httpGet('/api/futuristic-automation/operations-snapshot'),
  getAppointments: () => httpGet('/api/appointments'),
  getLoyaltyAccounts: () => httpGet('/api/loyalty/accounts')
};
