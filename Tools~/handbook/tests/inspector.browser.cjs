/* Optional browser regression suite. Requires playwright; the site is served
   separately. CHROMIUM_PATH and PREVIEW_RUNTIME_DIR support offline CI runners. */
const assert = require('node:assert/strict');
const path = require('node:path');
const { chromium } = require('playwright');
const base = process.argv[2] || 'http://127.0.0.1:8766/';
(async () => {
  const browser = await chromium.launch({headless:true, executablePath:process.env.CHROMIUM_PATH || undefined, args:['--no-sandbox']});
  try {
    const page = await browser.newPage({viewport:{width:1440,height:1000}});
    const errors=[]; page.on('pageerror',error=>errors.push(error.message));
    if (process.env.PREVIEW_RUNTIME_DIR) await page.route('https://cdnjs.cloudflare.com/**', route=>route.fulfill({
      path:path.join(process.env.PREVIEW_RUNTIME_DIR,route.request().url().includes('react-dom/')?'react-dom.production.min.js':'react.production.min.js'), contentType:'application/javascript'
    }));
    await page.goto(base+'#tokens'); // Initial routes without a preview must work.
    await page.locator('#view[data-page="tokens"]').waitFor();
    async function route(name) {
      await page.evaluate(name=>location.hash=name,name);
      await page.waitForFunction(name=>document.querySelector('#view').dataset.page===(name.startsWith('e-')?'entry':name),name);
      if (name.startsWith('e-')) await page.locator('#stage-host [data-inspect-node]').first().waitFor();
    }
    async function inspect() { const button=page.getByRole('button',{name:'Inspect',exact:true}); if(await button.getAttribute('aria-pressed')!=='true') await button.click(); await page.locator('#inspection-detail h3').waitFor(); }
    function field(name) { return page.locator('[data-property='+JSON.stringify(name)+']').last().locator('input:not([type="checkbox"]), select, textarea').first(); }
    async function choose(name,value) { await field(name).selectOption(JSON.stringify(value)); }
    async function noOverflow() { assert(await page.evaluate(()=>document.documentElement.scrollWidth<=document.documentElement.clientWidth+1),'Page-wide horizontal overflow'); }
    await route('e-button'); await inspect();
    assert.equal(await page.locator('#inspection-detail h3').textContent(),'VbButton');
    assert(await page.locator('#inspection-overlay').isVisible(),'Bounds overlay must actually be visible');
    const buttons=page.locator('#stage-host .vb-button');
    await buttons.first().click();
    await buttons.nth(2).focus(); await page.keyboard.press('Enter');
    assert.equal(await field('title').inputValue(),'SETTINGS');
    await buttons.first().dispatchEvent('pointerdown');
    for(const variant of ['primary','secondary','light','danger','ghost']) {
      await choose('variant',variant);
      await page.waitForFunction(variant=>document.querySelector('#stage-host .vb-button').classList.contains('vb-button--'+variant),variant);
    }
    // Picking does not activate a component, including disabled controls.
    await field('blocked').fill('Not enough Power'); await field('blocked').press('Tab');
    await page.waitForFunction(()=>document.querySelector('#stage-host .vb-button').disabled);
    await buttons.nth(1).dispatchEvent('pointerdown');
    assert((await field('blocked').inputValue()).length>0);
    await buttons.first().dispatchEvent('pointerdown');
    // Simulated pseudo-states and CSS property overrides update real styles.
    await page.locator('#inspection-state-active').check();
    await page.waitForFunction(()=>getComputedStyle(document.querySelector('#stage-host .vb-button')).scale==='0.97');
    await page.locator('#inspection-state-active').uncheck();
    await page.locator('#inspection-state-hover').check();
    assert.equal(await buttons.first().getAttribute('data-inspect-hover'),'');
    await page.waitForFunction(()=>Math.abs(Number(document.getElementById('inspection-selection').getAttribute('width'))-document.querySelector('#stage-host .vb-button').getBoundingClientRect().width)<1);
    await page.getByText('Preview CSS custom properties',{exact:true}).click();
    const cut=page.getByLabel('--vb-cut',{exact:true}); await cut.fill('23'); await cut.press('Tab');
    await page.waitForFunction(()=>getComputedStyle(document.querySelector('#stage-host .vb-button')).getPropertyValue('--vb-cut').trim()==='23');
    // Rendered attributes can be set and removed; CSS definitions remain visible.
    await page.getByText('Rendered HTML attributes',{exact:true}).click();
    await page.getByLabel('Preview HTML attribute',{exact:true}).fill('aria-label');
    await page.getByLabel('Attribute value',{exact:true}).fill('Inspector example');
    await page.getByRole('button',{name:'Apply attribute',exact:true}).click();
    assert.equal(await buttons.first().getAttribute('aria-label'),'Inspector example');
    await page.getByLabel('Remove attribute',{exact:true}).check();
    await page.getByRole('button',{name:'Apply attribute',exact:true}).click();
    assert.equal(await buttons.first().getAttribute('aria-label'),null);
    await page.getByText('Style definitions',{exact:true}).click();
    assert(await page.locator('pre[aria-label="CSS definition"]').count()>0);
    await page.getByText('Style definitions',{exact:true}).locator('..').locator('details > summary').first().click();
    assert(await page.getByRole('region',{name:'CSS definition',exact:true}).count()>0);
    await page.context().grantPermissions(['clipboard-read','clipboard-write'],{origin:new URL(base).origin});
    await page.locator('#inspection-copy').click();
    assert.equal(JSON.parse(await page.evaluate(()=>navigator.clipboard.readText())).variant,'ghost');
    await page.locator('#inspection-reset').click();
    await page.waitForFunction(()=>document.querySelector('#stage-host .vb-button').classList.contains('vb-button--primary') && !document.querySelector('#stage-host .vb-button').disabled);
    await page.keyboard.press('Escape');
    assert(await page.locator('#preview-inspector').isHidden());
    assert(await page.locator('#inspection-overlay').isHidden());
    await route('e-skill-button'); await inspect();
    const skillStates=await page.evaluate(()=>JSON.parse(document.getElementById('inspection-data').textContent).components.SkillButton.properties.view.properties.state.values);
    for(const state of skillStates) {
      await choose('view.state',state);
      await page.waitForFunction(state=>document.querySelector('#stage-host .vb-skill').classList.contains('vb-skill--'+state),state);
    }
    // Nested component instances retain a complete view-model schema.
    await route('e-battle-hud-view'); await inspect();
    const option=page.locator('#inspection-tree option').filter({hasText:'SkillButton'}).first();
    await page.locator('#inspection-tree').selectOption(await option.getAttribute('value'));
    await choose('view.state','cooldown');
    assert.equal(await field('view.state').inputValue(),'"cooldown"');
    // Measured overlay bounds use screen coordinates for scaled portrait boards.
    const rect=await page.locator('#inspection-selection').evaluate(el=>({width:+el.getAttribute('width'),height:+el.getAttribute('height')}));
    assert(rect.width>0 && rect.height>0);
    // All entry routes still render, and inspector rebinding/cleanup survives navigation.
    const entries=await page.evaluate(()=>JSON.parse(document.getElementById('hb-data').textContent).entries);
    let checked=0;
    for(const width of [390,1280]) {
      await page.setViewportSize({width,height:900});
      for(const entry of entries) {
        await route('e-'+entry.id); await noOverflow();
        if(await page.getByRole('button',{name:'Inspect',exact:true}).count()) {
          await inspect(); await noOverflow();
          assert(await page.locator('#inspection-tree option').count()>0,entry.id+' has no component instances');
          assert.equal(await page.locator('#stage-host').getByText('Preview could not render this value.',{exact:false}).count(),0,entry.id);
          await page.keyboard.press('Escape');
        }
        checked++;
      }
    }
    // Representative open-inspector accessibility audit, excluding component mirrors.
    {
      await page.addScriptTag({path:process.env.PREVIEW_RUNTIME_DIR?path.join(process.env.PREVIEW_RUNTIME_DIR,'axe.min.js'):require.resolve('axe-core/axe.min.js')});
      for(const width of [390,1280]) {
        await page.setViewportSize({width,height:900}); await route('e-button'); await inspect();
        const violations=await page.evaluate(async()=>{
          const result=await axe.run({exclude:[['[data-preview]']]},{runOnly:{type:'tag',values:['wcag2a','wcag2aa','wcag21aa']}});
          return result.violations.map(v=>({id:v.id,nodes:v.nodes.map(n=>n.target)}));
        });
        assert.deepEqual(violations,[],'Inspector accessibility at '+width);
      }
    }
    assert.deepEqual(errors,[]);
    console.log(JSON.stringify({entryChecks:checked,skillStates:skillStates.length,errors:0}));
  } finally { await browser.close(); }
})().catch(error=>{console.error(error);process.exit(1)});
