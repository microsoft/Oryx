import { test, expect } from '@playwright/test';

test('basic manager', async ({ page }) => {
  await page.goto('/');
  await page.waitForSelector('strong');
  await expect(page.locator('strong')).toContainText('Todo manager')
}) 

test('login button', async ({ page }) => {
  await page.goto('/');
  await page.waitForSelector('text=Login to see todo items');
  await page.click('a:has-text("Login")');
  await expect(page).toHaveURL('/.auth/login/github');
}) 