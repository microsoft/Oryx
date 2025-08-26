const express = require('express');
const { formatMessage, getTimestamp } = require('@monorepo/shared-utils');

const app = express();
const PORT = process.env.PORT || 3000;

app.get('/', (req, res) => {
    const message = formatMessage('hello from monorepo web app!');
    const timestamp = getTimestamp();

    res.json({
        message,
        timestamp,
        info: 'This is a Lerna monorepo with Yarn workspaces demo'
    });
});

app.get('/health', (req, res) => {
    res.json({ status: 'OK', service: 'web-app' });
});

app.listen(PORT, () => {
    console.log(`Web app running on http://localhost:${PORT}`);
    console.log('Available endpoints:');
    console.log('  GET / - Main endpoint');
    console.log('  GET /health - Health check');
});