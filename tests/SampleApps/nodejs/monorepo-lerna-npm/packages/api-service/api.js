const express = require('express');
const { formatMessage, getTimestamp } = require('@monorepo/shared-utils');

const app = express();
const PORT = process.env.PORT || 4000;

app.use(express.json());

app.get('/api/data', (req, res) => {
    const message = formatMessage('api service is working!');
    const timestamp = getTimestamp();

    res.json({
        data: [
            { id: 1, name: 'Item 1' },
            { id: 2, name: 'Item 2' },
            { id: 3, name: 'Item 3' }
        ],
        message,
        timestamp
    });
});

app.get('/api/health', (req, res) => {
    res.json({ status: 'OK', service: 'api-service' });
});

app.listen(PORT, () => {
    console.log(`API service running on http://localhost:${PORT}`);
    console.log('Available endpoints:');
    console.log('  GET /api/data - Get sample data');
    console.log('  GET /api/health - Health check');
});