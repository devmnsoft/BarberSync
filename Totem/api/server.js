const express = require('express');
const app = express();
app.use(express.json());
app.get('/health', (_, res) => res.json({ status: 'ok', context: 'totem-api' }));
app.post('/checkin', (req, res) => res.status(201).json({ message: 'Check-in registrado', payload: req.body }));
app.listen(4000, () => console.log('Totem API on :4000'));
