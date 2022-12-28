import { test, expect } from '@playwright/test';

//checks to see if at least 1 of the h3s contains the text #30DaysOfSWA
test('#30DaysOfSWA test', async ({ page }) => {
  await page.goto('/');
  await page.waitForSelector('h3');
  const elements = await page.$$("h3");
  var hasText = false
  await Promise.all(elements.map(async (e)=>{
    const text = await e.innerText();
    if(text== '#30DaysOfSWA'){
        hasText = true;
    }
  }))
  await expect(hasText).toEqual(true);
})