import { PlaywrightTestConfig } from '@playwright/test';
const config: PlaywrightTestConfig = {
    use: {
        trace: 'on',
        baseURL: "http://localhost:3000/"
    },
    webServer: {
        command: 'npm run start',
        port: 3000,
        timeout: 120 * 1000,
        reuseExistingServer: !process.env.CI,
    },
    reporter: [
        ['html', { outputFolder: 'pw-report' }], 
        ['json', { outputFolder: 'pw-report', outputFile: 'report.json' }]
    ],
};
export default config;