import { test, expect } from '@playwright/test'

test('should navigate to the react page', async ({ page }) => {
    // Start from the index page (the baseURL is set via the webServer in the playwright.config.ts)
    await page.goto('/')
    // Find an element with the text 'About Page' and click on it
    await page.click('text=React')
    // The new url should be "/project/facebook-react/" (baseURL is used there)
    await expect(page).toHaveURL('/project/facebook-react/')
    // The new page should contain an h3 with "You can deploy..."
    await expect(page.locator('.project')).toContainText('Home')
})

test('should have stats', async ({ page }) => {
    // Start from the index page (the baseURL is set via the webServer in the playwright.config.ts)
    await page.goto('/project/facebook-react/')
    // The new page should contain stats, there should be more than 100k starts for react
    const numberOfStars = await page.innerText('.stats-details:first-of-type div p')
    expect(Number(numberOfStars)).toBeGreaterThan(100000);
})

test('goes back to home page', async ({ page }) => {
    // Start from the index page (the baseURL is set via the webServer in the playwright.config.ts)
    await page.goto('/project/nuxt-nuxtjs/')
    // Find an element with the text 'Home' and click on it
    await page.click('text=Home')
    // The new page should be the home page and contain an h1 with 'What Can I Deploy to Static Apps?' 
    await expect(page).toHaveURL('/')
})