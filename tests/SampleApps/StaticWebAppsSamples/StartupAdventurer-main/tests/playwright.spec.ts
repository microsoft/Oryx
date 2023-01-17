import { test, expect } from '@playwright/test';

test('basic test', async ({ page }) => {
  await page.goto('/');
  await page.waitForSelector('h2');
  await expect(page.locator('h2')).toContainText('Create your own');
})