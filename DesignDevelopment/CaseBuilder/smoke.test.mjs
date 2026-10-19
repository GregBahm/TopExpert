import {test} from 'node:test';import assert from 'node:assert/strict';import fs from 'node:fs/promises';import os from 'node:os';import path from 'node:path';
import {validate,available,advance} from './engine.mjs';import serverModule from './server.cjs';
import {selectedInvestigatorIds,testRoster} from './test-setup.mjs';
import {startFixture as start,makeFixture,fixtureCatalog,opportunity,effect} from './test-fixture.mjs';

test('gates, outcomes, deadlines and runtime state stay separate from definitions',()=>{
 const d=makeFixture(),inspect=d.opportunities[0];inspect.repeatable=true;inspect.success=effect({flagsAdd:['found'],unlock:['resolve'],remove:['inspect']});inspect.failure=effect({health:-1});
 d.endings=['passed','failed','timeout'].map(id=>({id,title:id,body:'Fixture ending',notes:''}));
 d.opportunities.push(opportunity('resolve',{initial:false,requires:{flags:['found'],notFlags:[],inventory:{},resources:{},skills:{},traits:[]},check:{skills:['Focus'],target:3,itemBonuses:{},notes:''},success:effect({inventory:{reward:1},ending:'passed'}),failure:effect({ending:'failed'})}),opportunity('deadline',{deadline:5,expired:effect({ending:'timeout'})}));
 validate(d);const original=JSON.stringify(d),act=(s,id,actor='analyst',die=6)=>advance(d,s,[{opportunity:id,investigator:actor}],()=>die);
 let s=start(d);assert.equal(available(d,s,d.opportunities.find(o=>o.id==='resolve')),'Unavailable');
 s=act(s,'inspect','analyst',1);assert.equal(s.investigators[0].health,2);assert.ok(s.active.includes('inspect'));s=act(s,'inspect');assert.ok(s.active.includes('resolve'));assert.ok(!s.active.includes('inspect'));
 const passed=act(s,'resolve');assert.equal(passed.ending,'passed');assert.equal(passed.inventory.reward,1);assert.equal(act(s,'resolve','helper',1).ending,'failed');
 let timeout=start(d);for(let i=0;i<5;i++)timeout=advance(d,timeout,[]);assert.equal(timeout.ending,'timeout');
 s.investigators[0].skills.Focus=99;s.inventory.extra=1;assert.equal(JSON.stringify(d),original);assert.throws(()=>advance(d,start(d),[{opportunity:'inspect',investigator:'analyst'},{opportunity:'assist',investigator:'analyst'}]),/One action/);
});

test('file saves reload, reject stale revisions and isolate per-case setup',async t=>{
 const fixture=makeFixture(),root=await fs.mkdtemp(path.join(os.tmpdir(),'top-expert-case-builder-'));await fs.mkdir(path.join(root,'cases'));await fs.writeFile(path.join(root,'cases',fixture.id+'.json'),JSON.stringify(fixture));
 const server=await serverModule.createServer(root);await new Promise(r=>server.listen(0,'127.0.0.1',r));t.after(async()=>{await new Promise(r=>server.close(r));await fs.rm(root,{recursive:true,force:true})});
 const url='http://127.0.0.1:'+server.address().port+'/api/cases/',read=async id=>(await fetch(url+id)).json(),put=async(id,d,revision)=>fetch(url+id,{method:'PUT',headers:{'Content-Type':'application/json'},body:JSON.stringify({case:d,revision})});
 const body=await read(fixture.id),edited=structuredClone(body.case);edited.notes='Updated fixture note';assert.equal((await put(fixture.id,edited,body.revision)).status,200);assert.deepEqual(JSON.parse(await fs.readFile(path.join(root,'cases',fixture.id+'.json'),'utf8')),edited);
 assert.equal((await put(fixture.id,fixture,body.revision)).status,409);const invalid=structuredClone(edited);invalid.opportunities[0].success.unlock=['missing'];assert.equal((await put(fixture.id,invalid,(await read(fixture.id)).revision)).status,400);
 edited.testSetup={...edited.testSetup,investigatorIds:['analyst'],skillOverrides:{analyst:{Focus:6}}};assert.equal((await put(fixture.id,edited,(await read(fixture.id)).revision)).status,200);assert.deepEqual((await read(fixture.id)).case.testSetup,edited.testSetup);assert.equal((await read(fixture.id)).case.opening,fixture.opening);
 const other={...makeFixture(),id:'other-case'};assert.equal((await put(other.id,other,null)).status,200);assert.deepEqual((await read(other.id)).case.testSetup,makeFixture().testSetup);assert.equal((await fs.readdir(path.join(root,'cases'))).filter(n=>n.endsWith('.tmp')).length,0);
});

test('effective skills and selections are per case without changing baselines',()=>{
 const d=makeFixture();assert.deepEqual(selectedInvestigatorIds(d.testSetup,fixtureCatalog),['analyst','helper']);
 d.testSetup={...d.testSetup,investigatorIds:['analyst','unspecified'],skillOverrides:{analyst:{Focus:5}}};validate(d);const team=testRoster(d.testSetup,fixtureCatalog);assert.deepEqual(team.map(i=>i.id),['analyst']);assert.equal(team[0].skills.Focus,5);
 team[0].skills.Focus=99;assert.equal(testRoster(d.testSetup,fixtureCatalog)[0].skills.Focus,5);assert.equal(fixtureCatalog[0].skills.Focus,1);assert.equal(testRoster(makeFixture().testSetup,fixtureCatalog)[0].skills.Focus,1);
 delete d.testSetup.skillOverrides.analyst.Focus;assert.equal(testRoster(d.testSetup,fixtureCatalog)[0].skills.Focus,1);d.testSetup.investigatorIds=[];assert.deepEqual(testRoster(d.testSetup,fixtureCatalog),[]);d.testSetup.skillOverrides.analyst.Focus=7;assert.throws(()=>validate(d),/0 to 6/);
});
