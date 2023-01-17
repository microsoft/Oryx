import { PlaywrightTestConfig } from '@playwright/test';
const config: PlaywrightTestConfig = {
    testDir: "./tests",
    use: {
        baseURL: "http://localhost:3000/",
        trace: 'on'
    },
    reporter: [
      ['html', { outputFolder: 'pw-report' }], 
      ['json', { outputFolder: 'pw-report', outputFile: 'report.json' }]
    ],
    webServer: {
        command: 'npm run start',
        port: 3000,
        timeout: 120 * 1000,
        reuseExistingServer: !process.env.CI,
    },
};
export default config;