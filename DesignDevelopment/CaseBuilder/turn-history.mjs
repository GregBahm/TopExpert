// Immutable presentation records. No history edits feed back into gameplay.
export function recordTurn(definition,before,prepared,after){
 const entries=definition.opportunities.filter(o=>before.active.includes(o.id)).map(o=>{
  const assignment=structuredClone(prepared[o.id]||{slots:{}}),events=(after.turnResults||[]).filter(event=>event.opportunity===o.id);
  const actor=before.investigators.find(actor=>actor.id===assignment.investigator);
  return {opportunity:o.id,assignment,actor:actor?structuredClone(actor):null,events:structuredClone(events)};
 });
 // An opportunity can unlock and expire during the same multi-turn action.
 for(const event of after.turnResults||[])if(!entries.some(entry=>entry.opportunity===event.opportunity))entries.push({opportunity:event.opportunity,assignment:{slots:{}},actor:null,events:structuredClone(after.turnResults.filter(other=>other.opportunity===event.opportunity))});
 return {number:before.turn,entries};
}
export function turnFlow(definition,history,state){
 const current={number:state.turn,current:true,entries:state.ending?[]:definition.opportunities.filter(o=>state.active.includes(o.id)).map(o=>({opportunity:o.id}))};
 const columns=[...history,current],edges=[],seen=new Set(),sources=new Map();
 columns.forEach((column,index)=>{
  const previous=new Set((columns[index-1]?.entries||[]).map(entry=>entry.opportunity));
  column.entries.forEach(entry=>{
   if(previous.has(entry.opportunity))edges.push({from:(index-1)+':'+entry.opportunity,to:index+':'+entry.opportunity,outcome:'continuation'});
   if(!seen.has(entry.opportunity)&&sources.has(entry.opportunity))edges.push({...sources.get(entry.opportunity),to:index+':'+entry.opportunity});
   seen.add(entry.opportunity);
  });
  // Entries are in board order, so the topmost actual unlock wins even if
  // assignments were prepared or resolved in a different order.
  column.entries.forEach(entry=>{for(const event of entry.events||[])for(const id of event.unlocks||[])if(!seen.has(id)&&!sources.has(id))sources.set(id,{from:index+':'+entry.opportunity,outcome:event.outcome})});
 });
 return {columns,edges};
}
