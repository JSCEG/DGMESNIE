const { chromium } = require('playwright');
const path = require('path');

async function main() {
    console.log("Launching browser...");
    const browser = await chromium.launch({ headless: true });
    const context = await browser.newContext({
        viewport: { width: 1400, height: 900 },
        ignoreHTTPSErrors: true
    });
    const page = await context.newPage();

    const outputDir = 'C:\\Users\\User\\.gemini\\antigravity-ide\\brain\\fc5d0ab5-4709-4619-8b63-6f8983d396e2';
    const baseUrl = 'https://localhost:7105';

    try {
        console.log("1. Navigating to bypass login...");
        await page.goto(`${baseUrl}/Acceso/DevBypass/1`);
        await page.waitForTimeout(3000);
        console.log("Current URL:", page.url());
        console.log("Current Title:", await page.title());

        console.log("2. Navigating to Transparencia...");
        await page.goto('http://localhost:5154/Transparencia');
        await page.waitForTimeout(2000);
        console.log("Current URL:", page.url());
        console.log("Current Title:", await page.title());

        console.log("3. Clicking Folio header 'Folio: 340026100011826' on left sidebar...");
        const folioHeader = page.locator('div.folio-group-header:has-text("340026100011826")');
        
        // Check if folio header is present
        const count = await folioHeader.count();
        console.log(`Found ${count} folio header(s) for 340026100011826`);
        if (count === 0) {
            console.log("Page HTML content snippet:", (await page.content()).substring(0, 1000));
            throw new Error("Folio header not found!");
        }

        await folioHeader.click();
        await page.waitForTimeout(2500);

        console.log("4. Taking screenshot of reordered layout (stepper at top, details below)...");
        await page.screenshot({ path: path.join(outputDir, 'transparency_reorder_stepper.png') });

        console.log("5. Clicking on card 'SENER2603243' in the top sequence...");
        const cardNode = page.locator('.trace-step:has-text("SENER2603243")');
        const cardCount = await cardNode.count();
        console.log(`Found ${cardCount} cards for SENER2603243`);
        if (cardCount > 0) {
            await cardNode.click();
            await page.waitForTimeout(2000);
        } else {
            console.log("Card SENER2603243 not found in trace stepper!");
        }

        console.log("6. Taking screenshot of active selected card details...");
        await page.screenshot({ path: path.join(outputDir, 'transparency_reorder_active.png') });

        console.log("7. Clicking on 'Infografía' tab...");
        const infografiaTab = page.locator('#tab-btn-infografia');
        const infoTabCount = await infografiaTab.count();
        console.log(`Found ${infoTabCount} Infografía tabs`);
        if (infoTabCount > 0 && await infografiaTab.isVisible()) {
            await infografiaTab.click();
            await page.waitForTimeout(2000);
        } else {
            console.log("Infografia tab not visible or not found");
        }

        console.log("8. Taking screenshot of Infografía tab showing external link button below iframe...");
        await page.screenshot({ path: path.join(outputDir, 'transparency_reorder_infografia.png') });

        console.log("Tests completed successfully!");

    } catch (err) {
        console.error("Test failed with error:", err);
    } finally {
        await browser.close();
    }
}

main();
