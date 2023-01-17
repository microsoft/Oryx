import { test, expect } from '@playwright/test';

test('basic test', async ({ page }) => {
  await page.goto('/');
  await page.waitForSelector('div');
  await expect(page.locator('div')).toContainText('Hello World');
})