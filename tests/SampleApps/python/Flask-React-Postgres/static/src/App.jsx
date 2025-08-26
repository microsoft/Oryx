import React, { useState, useEffect } from 'react';
import axios from 'axios';

function App() {
    const [message, setMessage] = useState('');
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        // Fetch data from Flask backend
        axios.get('/api/hello')
            .then(response => {
                setMessage(response.data.message || 'Hello from Flask!');
                setLoading(false);
            })
            .catch(err => {
                console.error('Error fetching data:', err);
                setError('Failed to connect to Flask backend');
                setLoading(false);
            });
    }, []);

    return (
        <div style={{
            padding: '2rem',
            fontFamily: 'Arial, sans-serif',
            maxWidth: '600px',
            margin: '0 auto'
        }}>
            <h1>Flask + React Sample App</h1>

            {loading && <p>Loading...</p>}
            {error && <p style={{ color: 'red' }}>{error}</p>}
            {message && !loading && !error && (
                <div>
                    <p><strong>Message from Flask backend:</strong></p>
                    <p style={{
                        padding: '1rem',
                        backgroundColor: '#f0f0f0',
                        borderRadius: '4px'
                    }}>
                        {message}
                    </p>
                </div>
            )}

            <hr style={{ margin: '2rem 0' }} />

            <h2>What this sample demonstrates:</h2>
            <ul>
                <li>Flask serving a simple REST API endpoint</li>
                <li>React frontend making HTTP requests to Flask</li>
                <li>Basic error handling and loading states</li>
            </ul>
        </div>
    );
}

export default App;