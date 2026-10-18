import {test} from 'node:test';import assert from 'node:assert/strict';import fs from 'node:fs/promises';import os from 'node:os';import path from 'node:path';
import {validate,start,available,advance} from './engine.mjs';import serverModule from './server.cjs';
import {selectedInvestigatorIds,testRoster} from './test-setup.mjs';
const sample=JSON.parse(await fs.readFile(new URL('./cases/the-opening-act.json',import.meta.url),'utf8'));
test('sample gates, branches, checks, items, health, deadlines and isolated runtime',()=>{
 validate(sample);const original=JSON.stringify(sample),act=(s,op,investigator='avery',die=6)=>advance(sample,s,[{opportunity:op,investigator}],()=>die);
 let s=start(sample);assert.match(available(sample,s,sample.opportunities.find(o=>o.id==='rent')),/Unavailable/);
 s=act(s,'interview','avery',1);assert.ok(s.active.includes('interview'));assert.equal(s.turn,2);
 s=act(s,'interview');assert.ok(s.flags.includes('music-clue'));assert.ok(!s.active.includes('interview'));
 s=act(s,'borrow','avery',1);assert.equal(s.investigators[0].health,2);assert.equal(s.inventory.amplifier,undefined);
 s=act(s,'rent',null);assert.equal(s.resources.money,0);assert.equal(s.inventory.amplifier,1);assert.ok(!s.active.includes('borrow'));
 s=act(s,'observe');const fork=structuredClone(s);s=act(s,'badly');assert.equal(s.ending,'bad-review');assert.equal(s.inventory['alien-review'],1);
 assert.equal(act(fork,'properly','red',6).ending,'encore');assert.equal(act(fork,'properly','avery',1).ending,'bad-review');
 let timeout=act(start(sample),'archives','vanderhorten');assert.equal(timeout.inventory['old-review'],1);for(let i=0;i<7;i++)timeout=advance(sample,timeout,[]);assert.equal(timeout.ending,'abandoned');
 const override=start(sample);override.investigators[0].skills.Focus=99;override.inventory.amplifier=10;assert.equal(JSON.stringify(sample),original);
 assert.throws(()=>advance(sample,start(sample),[{opportunity:'interview',investigator:'avery'},{opportunity:'archives',investigator:'avery'}]),/One action/);
});
test('atomic file save/reload, conflict protection and dynamically added case',async t=>{
 const root=await fs.mkdtemp(path.join(os.tmpdir(),'top-expert-case-builder-'));await fs.mkdir(path.join(root,'cases'));await fs.writeFile(path.join(root,'cases',sample.id+'.json'),JSON.stringify(sample));
 const server=await serverModule.createServer(root);await new Promise(r=>server.listen(0,'127.0.0.1',r));t.after(async()=>{await new Promise(r=>server.close(r));await fs.rm(root,{recursive:true,force:true})});
 const url='http://127.0.0.1:'+server.address().port+'/api/cases/',read=async id=>(await fetch(url+id)).json();const body=await read(sample.id),edited=structuredClone(body.case);edited.notes+=' persistence checked';
 const put=async(id,d,revision)=>fetch(url+id,{method:'PUT',headers:{'Content-Type':'application/json'},body:JSON.stringify({case:d,revision})});
 assert.equal((await put(sample.id,edited,body.revision)).status,200);assert.equal((await read(sample.id)).case.notes,edited.notes);assert.deepEqual(JSON.parse(await fs.readFile(path.join(root,'cases',sample.id+'.json'),'utf8')),edited);
 assert.equal((await put(sample.id,sample,body.revision)).status,409);const invalid=structuredClone(edited);invalid.opportunities[0].success.unlock=['missing'];assert.equal((await put(sample.id,invalid,(await read(sample.id)).revision)).status,400);
 const configured=structuredClone(edited);configured.testSetup={investigatorIds:['avery','joan'],skillOverrides:{avery:{Focus:5}}};assert.equal((await put(sample.id,configured,(await read(sample.id)).revision)).status,200);assert.deepEqual((await read(sample.id)).case.testSetup,configured.testSetup);assert.equal((await read(sample.id)).case.opening,sample.opening);
 const fresh={...sample,id:'new-after-start'};assert.equal((await put(fresh.id,fresh,null)).status,200);assert.equal((await read(fresh.id)).case.id,fresh.id);assert.equal((await fs.readdir(path.join(root,'cases'))).filter(n=>n.endsWith('.tmp')).length,0);
 assert.deepEqual((await read(fresh.id)).case.testSetup,sample.testSetup);
});
test('documented roster and per-case test setup isolate runtime overrides',async()=>{
 const catalog=JSON.parse(await fs.readFile(new URL('./assets/investigators/catalog.json',import.meta.url),'utf8'));assert.equal(catalog.length,24);assert.deepEqual(selectedInvestigatorIds(sample,catalog),['avery','vanderhorten']);assert.deepEqual(catalog.filter(p=>!p.statsSpecified).map(p=>p.name),['Gabriel','Scooter','Xavier']);assert.deepEqual(catalog.find(p=>p.id==='ms-robinson').skills,{Interpersonal:1,Sly:2});
 const configured=structuredClone(sample);configured.testSetup={investigatorIds:['joan','audrey']};validate(configured);const team=testRoster(configured,catalog);assert.deepEqual(team.map(i=>i.id),['audrey','joan']);assert.equal(team.find(i=>i.id==='joan').skills.Interpersonal,3);const state=start({...configured,investigators:team});state.investigators[0].skills.Violence=99;assert.equal(testRoster(configured,catalog)[0].skills.Violence,2);assert.deepEqual(selectedInvestigatorIds(sample,catalog),['avery','vanderhorten']);configured.testSetup.investigatorIds=[];assert.deepEqual(testRoster(configured,catalog),[]);
 const overridden=structuredClone(sample);overridden.testSetup={investigatorIds:['avery'],skillOverrides:{avery:{Interpersonal:5,Focus:0}}};validate(overridden);const effective=testRoster(overridden,catalog);assert.equal(effective[0].skills.Interpersonal,5);assert.equal(catalog.find(p=>p.id==='avery').skills.Interpersonal,2);const checked=advance(overridden,start({...overridden,investigators:effective}),[{opportunity:'interview',investigator:'avery'}],()=>1);assert.ok(checked.flags.includes('music-clue'));delete overridden.testSetup.skillOverrides.avery.Interpersonal;assert.equal(testRoster(overridden,catalog)[0].skills.Interpersonal,2);overridden.testSetup.skillOverrides.avery.Focus=6;assert.throws(()=>validate(overridden),/0 to 5/);
});
