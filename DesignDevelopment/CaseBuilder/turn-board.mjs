import {available,advance} from './engine.mjs';

// Slots adapt existing requirements/costs; optional check bonuses stay optional.
export function requiredSlots(opportunity){
 const slots=[];
 if(opportunity.kind==='investigator')slots.push({key:'investigator',type:'investigator',label:'Investigator'});
 for(const type of ['inventory','resources']){
  const names=new Set([...Object.keys(opportunity.requires[type]),...Object.keys(opportunity.cost[type])]);
  for(const name of names){const quantity=Math.max(opportunity.requires[type][name]||0,opportunity.cost[type][name]||0);if(quantity>0)slots.push({key:type+':'+name,type,name,quantity,consumed:opportunity.cost[type][name]||0,label:name+' × '+quantity})}
 }
 if(!slots.length)slots.push({key:'ready',type:'ready',label:'Prepare action'});
 return slots;
}
export function reservations(definition,prepared,type,name,exceptOpportunity=null,exceptSlot=null){
 let count=0;for(const entry of Object.values(prepared)){const o=definition.opportunities.find(o=>o.id===entry.opportunity);if(!o)continue;for(const slot of requiredSlots(o)){if(entry.opportunity===exceptOpportunity&&slot.key===exceptSlot)continue;if(slot.type===type&&slot.name===name&&entry.slots?.[slot.key])count+=slot.quantity}}
 return count;
}
export function placeCard(definition,state,prepared,opportunityId,slotKey,payload){
 if(state.ending)throw Error('Case ended. Restart to test again.');
 const opportunity=definition.opportunities.find(o=>o.id===opportunityId);
 if(!opportunity||!state.active.includes(opportunityId))throw Error('Opportunity unavailable');
 const slot=requiredSlots(opportunity).find(s=>s.key===slotKey);if(!slot)throw Error('Unknown slot');
 if(slot.type!==payload?.type||(slot.name!==undefined&&slot.name!==payload.name))throw Error('This slot needs '+slot.label);
 const result=structuredClone(prepared);
 if(slot.type==='investigator'){
  const actor=state.investigators.find(i=>i.id===payload.id);if(!actor||actor.health<=0)throw Error('Choose a healthy investigator from this test team');
  for(const entry of Object.values(result))if(entry.investigator===payload.id)delete entry.investigator;
 }else if(slot.type!=='ready'){
  if(payload.fromOpportunity&&payload.fromSlot){const previous=result[payload.fromOpportunity],previousOp=definition.opportunities.find(o=>o.id===payload.fromOpportunity),previousSlot=previousOp&&requiredSlots(previousOp).find(s=>s.key===payload.fromSlot);if(!previousSlot||previousSlot.type!==slot.type||previousSlot.name!==slot.name||!previous?.slots?.[previousSlot.key])throw Error('The source slot is empty');delete previous.slots[previousSlot.key]}
  const reserved=reservations(definition,result,slot.type,slot.name,opportunityId,slotKey);
  if((state[slot.type][slot.name]||0)-reserved<slot.quantity)throw Error('Not enough unassigned '+slot.name+' (needs '+slot.quantity+')');
 }
 const entry=result[opportunityId]??{opportunity:opportunityId,slots:{}};result[opportunityId]=entry;
 if(slot.type==='investigator')entry.investigator=payload.id;else entry.slots[slotKey]=true;
 return result;
}
export function clearSlot(prepared,opportunity,slot){const result=structuredClone(prepared),entry=result[opportunity];if(entry){if(slot==='investigator')delete entry.investigator;else delete entry.slots[slot]}return result}
export function canPlaceCard(definition,state,prepared,opportunityId,slotKey,payload){
 try{
  const o=definition.opportunities.find(o=>o.id===opportunityId);if(!o)return false;
  if(o.requires.flags.some(f=>!state.flags.includes(f))||o.requires.notFlags.some(f=>state.flags.includes(f)))return false;
  if(payload?.type==='investigator'){const actor=state.investigators.find(i=>i.id===payload.id);if(!actor||Object.entries(o.requires.skills).some(([k,v])=>(actor.skills[k]||0)<v)||o.requires.traits.some(t=>!actor.traits?.includes(t)))return false}
  placeCard(definition,state,prepared,opportunityId,slotKey,payload);return true;
 }catch{return false}
}
export function preparationReason(definition,state,opportunity,entry){
 if(!entry)return 'Not prepared';
 for(const slot of requiredSlots(opportunity))if(slot.type==='investigator'?!entry.investigator:!entry.slots?.[slot.key])return 'Needs '+slot.label;
 return available(definition,state,opportunity,state.investigators.find(i=>i.id===entry.investigator));
}
export function completePreparedTurn(definition,state,prepared,roll){
 const valid=[],skipped=[];
 for(const entry of Object.values(prepared)){
  const o=definition.opportunities.find(o=>o.id===entry.opportunity);if(!o)continue;
  // Completely empty entries left after moving/clearing cards are not attempts.
  if(!entry.investigator&&!Object.values(entry.slots||{}).some(Boolean))continue;
  const why=preparationReason(definition,state,o,entry);if(why)skipped.push(o.title+': '+why);else valid.push({opportunity:o.id,investigator:entry.investigator});
 }
 const next=advance(definition,state,valid,roll);
 if(skipped.length)next.log.push('Not resolved: '+skipped.join('; '));
 return {state:next,skipped,resolved:valid.map(a=>a.opportunity)};
}
