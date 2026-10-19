import {test} from 'node:test';import assert from 'node:assert/strict';
import {validate} from './engine.mjs';import {testRoster} from './test-setup.mjs';
import {requiredSlots,placeCard,clearSlot,canPlaceCard,preparationReason,completePreparedTurn,reservations} from './turn-board.mjs';
import {startFixture as start,makeFixture,fixtureCatalog,opportunity,effect} from './test-fixture.mjs';

test('investigators occupy one place and use effective skills without mutating definitions',()=>{
 const d=makeFixture();d.testSetup={...d.testSetup,investigatorIds:['analyst','helper'],skillOverrides:{analyst:{Focus:5}}};
 const state=start(d),before=JSON.stringify({d,state});let prepared={};
 prepared=placeCard(d,state,prepared,'inspect','investigator',{type:'investigator',id:'analyst'});assert.equal(preparationReason(d,state,d.opportunities[0],prepared.inspect),'');
 prepared=placeCard(d,state,prepared,'assist','investigator',{type:'investigator',id:'analyst'});assert.equal(prepared.inspect.investigator,undefined);assert.equal(prepared.assist.investigator,'analyst');
 prepared=placeCard(d,state,prepared,'assist','investigator',{type:'investigator',id:'helper'});assert.equal(prepared.assist.investigator,'helper');prepared=clearSlot(prepared,'assist','investigator');assert.equal(prepared.assist.investigator,undefined);
 assert.equal(canPlaceCard(d,state,prepared,'inspect','investigator',{type:'resources',name:'money'}),false);assert.equal(canPlaceCard(d,state,prepared,'inspect','investigator',{type:'investigator',id:'analyst'}),true);
 assert.equal(state.investigators.find(i=>i.id==='analyst').skills.Focus,5);prepared=placeCard(d,state,prepared,'inspect','investigator',{type:'investigator',id:'analyst'});
 const result=completePreparedTurn(d,state,prepared,()=>1);assert.ok(result.state.flags.includes('found'));assert.deepEqual(result.resolved,['inspect']);assert.equal(JSON.stringify({d,state}),before);
});

test('items reject mismatches, reserve quantities and consume costs once',()=>{
 const d=makeFixture();d.testSetup.inventory={kit:2,wrong:1};const action=opportunity('use-kit',{kind:'item',requires:{flags:[],notFlags:[],inventory:{kit:2},resources:{},skills:{},traits:[]},cost:{inventory:{kit:1},resources:{money:2}},success:effect({inventory:{reward:1}})});
 d.opportunities=[action,{...structuredClone(action),id:'other-kit'}];validate(d);const state=start(d);
 assert.deepEqual(requiredSlots(action).map(s=>[s.type,s.name,s.quantity]),[['inventory','kit',2],['resources','money',2]]);
 let prepared={};assert.equal(canPlaceCard(d,state,prepared,'use-kit','inventory:kit',{type:'inventory',name:'wrong'}),false);assert.equal(canPlaceCard(d,state,prepared,'use-kit','inventory:kit',{type:'inventory',name:'kit'}),true);assert.equal(canPlaceCard(d,state,prepared,'use-kit','resources:money',{type:'investigator',id:'analyst'}),false);
 prepared=placeCard(d,state,prepared,'use-kit','inventory:kit',{type:'inventory',name:'kit'});assert.equal(reservations(d,prepared,'inventory','kit'),2);assert.throws(()=>placeCard(d,state,prepared,'other-kit','inventory:kit',{type:'inventory',name:'kit'}),/Not enough/);
 const incomplete=completePreparedTurn(d,state,prepared);assert.equal(incomplete.state.inventory.kit,2);assert.equal(incomplete.state.resources.money,4);assert.equal(incomplete.skipped.length,1);
 prepared=placeCard(d,state,prepared,'other-kit','inventory:kit',{type:'inventory',name:'kit',fromOpportunity:'use-kit',fromSlot:'inventory:kit'});assert.equal(prepared['use-kit'].slots['inventory:kit'],undefined);prepared=placeCard(d,state,prepared,'other-kit','resources:money',{type:'resources',name:'money'});
 const result=completePreparedTurn(d,state,prepared);assert.equal(result.state.inventory.kit,1);assert.equal(result.state.inventory.reward,1);assert.equal(result.state.resources.money,2);
 const twice=completePreparedTurn(d,result.state,prepared);assert.equal(twice.state.inventory.reward,1);assert.equal(twice.state.resources.money,2);assert.equal(state.inventory.kit,2);
 const bonus=makeFixture().opportunities[0];bonus.check.itemBonuses={optional:3};assert.ok(!requiredSlots(bonus).some(s=>s.name==='optional'));
});
