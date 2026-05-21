const api = 'http://localhost:3001';

async function request(path) {
  const response = await fetch(`${api}${path}`);
  if (!response.ok) {
    throw new Error(`Falha ao consultar ${path}: ${response.status}`);
  }
  return response.json();
}

export async function loadTotemSnapshot() {
  const [services, operations] = await Promise.all([
    request('/services'),
    request('/futuristic-automation/operations-snapshot')
  ]);

  return {
    services,
    operations,
    gamification: {
      progressLabel: 'Cliente VIP Ouro',
      nextReward: 'Cashback de 15% em combo premium',
      pointsToNextLevel: 120
    }
  };
}

window.loadTotemSnapshot = loadTotemSnapshot;
