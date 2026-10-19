import {test} from 'node:test';import assert from 'node:assert/strict';
import {advance} from './engine.mjs';
import {recordTurn,turnFlow} from './turn-history.mjs';
import {startFixture as start,makeFixture,opportunity,effect} from './test-fixture.mjs';

test('history freezes assignments and item changes, with topmost unlock origin despite reverse resolution order',()=>{
 const d=makeFixture();for(const o of d.opportunities){o.cost.resources={money:1};o.success=effect({inventory:{kit:1},unlock:['follow']})}
 d.opportunities.push(opportunity('follow',{initial:false}));const before=start(d);
 const prepared={assist:{opportunity:'assist',investigator:'helper',slots:{'resources:money':true}},inspect:{opportunity:'inspect',investigator:'analyst',slots:{'resources:money':true}}};
 const next=advance(d,before,[prepared.assist,prepared.inspect],()=>6),record=recordTurn(d,before,prepared,next);
 const graph=turnFlow(d,[record],next);assert.equal(graph.columns.length,2);assert.deepEqual(graph.edges,[{from:'0:inspect',to:'1:follow',outcome:'success'}]);
 assert.deepEqual(record.entries[0].events[0].changes,[{type:'inventory',name:'kit',before:1,after:2,delta:1},{type:'resources',name:'money',before:3,after:2,delta:-1}]);
 assert.equal(next.resources.money,2);assert.equal(record.entries[0].assignment.slots['resources:money'],true);
 before.investigators[0].name='Changed';prepared.inspect.slots={};next.turnResults[1].changes.length=0;
 assert.equal(record.entries[0].actor.name,'Analyst');assert.equal(record.entries[0].assignment.slots['resources:money'],true);assert.equal(record.entries[0].events[0].changes.length,2);
});

test('waiting records deadline inventory effects without inventing an assignment',()=>{
 const d=makeFixture();d.opportunities=[opportunity('deadline',{deadline:1,expired:effect({inventory:{parcel:2},unlock:['follow']})}),opportunity('follow',{initial:false})];
 const before=start(d),next=advance(d,before,[]),record=recordTurn(d,before,{},next);
 assert.equal(record.entries[0].actor,null);assert.equal(record.entries[0].events[0].outcome,'expired');assert.equal(record.entries[0].events[0].changes[0].delta,2);
 assert.deepEqual(turnFlow(d,[record],next).edges,[{from:'0:deadline',to:'1:follow',outcome:'expired'}]);
});
