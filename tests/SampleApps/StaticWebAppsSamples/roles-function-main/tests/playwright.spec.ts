import { test, expect } from '@playwright/test';

test('basic test', async ({ page }) => {
  await page.goto('/');
  await page.locator('text=All users').click()
  await expect(page.locator('h1')).toContainText('Anonymous');
})