/* Interaction coverage beyond catalog rendering. Uses the generated handbook web package. */
const assert=require('node:assert/strict');
const path=require('node:path');
const fs=require('node:fs');
const {chromium}=require('playwright');
const base=process.argv[2]||'http://127.0.0.1:8767/';
(async()=>{
 const browser=await chromium.launch({executablePath:process.env.CHROMIUM_PATH||undefined,args:['--no-sandbox']});
 try{
  for(const touch of [false,true]){
   const context=await browser.newContext({viewport:{width:touch?390:1280,height:900},hasTouch:touch,isMobile:touch});
   const page=await context.newPage(),errors=[];page.on('pageerror',e=>errors.push(e.message));
   await page.goto(base);
   await page.setContent('<link rel="stylesheet" href="'+base+'web/dist/tokens.css"><link rel="stylesheet" href="'+base+'web/dist/veilbreak.css"><div id="app" class="vb-root" style="width:390px;height:844px;position:relative"></div>');
   if(process.env.PREVIEW_RUNTIME_DIR){for(const file of ['react.production.min.js','react-dom.production.min.js'])await page.addScriptTag({path:path.join(process.env.PREVIEW_RUNTIME_DIR,file)});}
   else{await page.addScriptTag({url:'https://cdnjs.cloudflare.com/ajax/libs/react/18.3.1/umd/react.production.min.js'});await page.addScriptTag({url:'https://cdnjs.cloudflare.com/ajax/libs/react-dom/18.3.1/umd/react-dom.production.min.js'});}
   await page.addScriptTag({url:base+'web/dist/veilbreak.js'});
   await page.evaluate(()=>{const h=React.createElement;window.effects=Array.from({length:12},(_,i)=>({id:'e'+i,definition:{id:'status'+i,title:'Effect '+i,description:'A complete custom effect '+i,icon:'barrier',scope:'actor'},remainingSeconds:i+1,stacks:i+1,activation:i%2?'conditional':'active'}));window.mount=ReactDOM.createRoot(document.getElementById('app'));mount.render(h(Veilbreak.EffectStrip,{effects}));});
   const overflow=page.getByRole('button',{name:'All effects · 12'});await overflow.waitFor();if(touch)await overflow.tap();else await overflow.hover();
   await page.locator('.vb-effect-list li').first().waitFor();assert.equal(await page.locator('.vb-effect-list li').count(),12);
   if(!touch){await page.locator('.vb-effect-list li').last().hover();await page.waitForTimeout(220);assert.equal(await page.locator('.vb-effect-list').count(),1);}
   const nested=page.locator('.vb-effect-list').getByRole('button',{name:'Effect 0',exact:true});if(touch)await nested.tap();else await nested.hover();await page.locator('.vb-effect-detail').waitFor();if(!touch){await page.locator('.vb-effect-detail').hover();await page.waitForTimeout(220);}assert.equal(await page.locator('.vb-effect-list').count(),1,'Parent list must stay open while inspecting nested details');
   await page.keyboard.press('Escape');await page.waitForTimeout(80);assert.equal(await page.locator('.vb-effect-detail').count(),0);assert.equal(await page.locator('.vb-effect-list').count(),1);await page.keyboard.press('Escape');await page.waitForTimeout(100);assert.equal(await page.locator('.vb-effect-list').count(),0);
   if(touch)await overflow.tap();else await overflow.hover();await page.locator('.vb-effect-list').waitFor();await page.locator('#app').click({position:{x:350,y:800}});await page.waitForTimeout(100);assert.equal(await page.locator('.vb-effect-list').count(),0,'Outside press closes overflow');
   for(const state of ['active','conditional','ready','spent','expired']){await page.evaluate(state=>mount.render(React.createElement(Veilbreak.EffectBadge,{view:Object.assign({},effects[0],{activation:state})})),state);await page.waitForTimeout(30);assert.equal(await page.locator('.vb-effect-badge').first().getAttribute('data-effect-state'),state);}
   await page.evaluate(()=>{const h=React.createElement;function Test(){const [open,setOpen]=React.useState(true);return h(React.Fragment,null,h('button',{id:'behind',type:'button'},'Behind'),h(Veilbreak.DrawerOverlay,{title:'Effects',open,onDismiss:()=>setOpen(false)},h('button',null,'Inside')));}mount.render(h(Test));});
   await page.getByRole('dialog',{name:'Effects'}).waitFor();assert(await page.locator('#behind').evaluate(e=>e.inert));
   await page.keyboard.press('Escape');await page.waitForTimeout(60);assert(await page.locator('#behind').evaluate(e=>e.inert),'Coverage released before close finished');await page.waitForTimeout(450);assert.equal(await page.getByRole('dialog').count(),0);assert.equal(await page.locator('#behind').evaluate(e=>e.inert),false);
   // Perks deal in order; rebinds retain DOM identity and don't replay existing choices.
   await page.evaluate(()=>{window.picks=[];window.cards=[0,1,2].map(i=>({id:'p'+i,name:'Perk '+i,tag:'Common',icon:'barrier',effect:'An authored effect',next:'+8%',stack:0,max:5}));window.draw=()=>mount.render(React.createElement(Veilbreak.PerkDraftView,{view:{wave:1,choices:cards,pick:0,squad:[]},onPick:i=>picks.push(i)}));draw();});
   const choices=page.locator('.vb-perk-card');await choices.first().waitFor();await page.waitForTimeout(40);
   const pending=await choices.last().evaluate(e=>e.inert);assert(pending,'Later choices should wait for their deal slot');
   await choices.first().click();assert.deepEqual(await page.evaluate(()=>picks),[0]);
   await page.waitForTimeout(650);await page.evaluate(()=>{window.first=document.querySelector('.vb-perk-card');cards[0].name='Updated';draw();});await page.waitForTimeout(30);
   assert(await page.evaluate(()=>first===document.querySelector('.vb-perk-card')));assert.equal(await choices.first().evaluate(e=>e.getAnimations().length),0);
   // Reduced motion cancels spatial movement, including an already active deal.
   await page.evaluate(()=>{document.getElementById('app').classList.add('vb-reduced-motion');cards=cards.map((p,i)=>Object.assign({},p,{id:'reduced'+i}));draw();});await page.waitForTimeout(200);
   assert.equal(await choices.last().evaluate(e=>e.inert),false);assert.equal(await choices.last().evaluate(e=>e.getAnimations().length),0);
   for(const width of [320,390]){
    await page.evaluate(width=>{var app=document.getElementById('app');app.style.width=width+'px';app.classList.toggle('vb-compact',width<380);mount.render(React.createElement(Veilbreak.BattleHudView,{hud:{wave:1},operatives:[0,1,2,3].map(i=>({id:'op'+i,name:'Operative',hp:1000,maxHp:1000,stability:100,maxStability:100,effects,skills:[{label:'A',cost:10},{label:'S',cost:20,slot:'S'}]}))}));},width);
    await page.locator('.vb-battle__card').first().waitFor();await page.waitForTimeout(100);
    const fit=await page.locator('.vb-battle__card').evaluateAll(cards=>cards.map(card=>{var b=card.getBoundingClientRect(),effect=card.querySelector('.vb-effect-strip__more').getBoundingClientRect(),skills=Array.from(card.querySelectorAll('.vb-skill')).map(e=>e.getBoundingClientRect());return{target:effect.height>=44,effectInside:effect.bottom<=b.bottom+1,skillsInside:skills.every(s=>s.bottom<=b.bottom+1&&s.right<=b.right+1)};}));
    assert(fit.every(c=>c.target&&c.effectInside&&c.skillsInside),'Battle card fit at '+width+': '+JSON.stringify(fit));
   }
   await page.evaluate(()=>mount.render(React.createElement(Veilbreak.BattleHudView,{reveal:.5})));await page.waitForTimeout(40);assert.equal(await page.locator('.vb-board__top').evaluate(e=>Number(e.style.opacity)),.5);
   await page.evaluate(()=>mount.render(React.createElement(Veilbreak.BattleHudView,{shown:true})));await page.waitForTimeout(200);assert.equal(await page.locator('.vb-board__top').evaluate(e=>Number(getComputedStyle(e).opacity)),1,'HUD can hand off from scrubbed deployment to presence');
   assert.deepEqual(errors,[]);console.log(JSON.stringify({touch,effects:12,hoverTransfer:true,drawerExit:true,keyedRebind:true,deal:true,reducedMotion:true}));await context.close();
  }
 }finally{await browser.close();}
})().catch(e=>{console.error(e);process.exit(1)});
