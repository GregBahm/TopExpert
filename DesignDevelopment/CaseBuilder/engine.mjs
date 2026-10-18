export const STATS=['Exploitability','Focus','Interpersonal','Violence','Sly','Knowledge','Resilience','Otherness'];
const object=x=>x&&typeof x==='object'&&!Array.isArray(x);
export function validate(d){
 const need=(ok,msg)=>{if(!ok)throw Error(msg)};
 need(object(d)&&d.version===1,'Expected case version 1');
 need(/^[a-z0-9-]+$/.test(d.id),'Case ID: lowercase letters, numbers, hyphens');
 if(d.testSetup!==undefined){need(object(d.testSetup)&&Array.isArray(d.testSetup.investigatorIds)&&d.testSetup.investigatorIds.every(id=>typeof id==='string'&&/^[a-z0-9-]+$/.test(id)),'Test setup needs investigatorIds');need(new Set(d.testSetup.investigatorIds).size===d.testSetup.investigatorIds.length,'Test setup investigator IDs must be unique')}
 if(d.testSetup?.skillOverrides!==undefined){need(object(d.testSetup.skillOverrides),'Skill overrides must be an object');for(const [id,skills] of Object.entries(d.testSetup.skillOverrides)){need(/^[a-z0-9-]+$/.test(id)&&object(skills),'Invalid investigator skill override');for(const [skill,value] of Object.entries(skills))need(STATS.includes(skill)&&Number.isInteger(value)&&value>=0&&value<=6,'Skill overrides must be integers from 0 to 6')}}
 for(const k of ['title','opening','notes'])need(typeof d[k]==='string',k+' must be text');
 need(object(d.rule)&&Number.isInteger(d.rule.sides)&&d.rule.sides>=1&&d.rule.sides<=100,'Check die sides must be 1–100');
 need(Number.isFinite(d.rule.multiplier)&&d.rule.multiplier>=0,'Skill multiplier must be nonnegative');
 need(Array.isArray(d.investigators)&&d.investigators.length>0,'Add an investigator');
 const unique=(a,label)=>{need(a.every(x=>object(x)&&typeof x.id==='string'&&/^[a-z0-9-]+$/.test(x.id)),label+' IDs must use lowercase letters, numbers and hyphens');need(new Set(a.map(x=>x.id)).size===a.length,label+' IDs must be unique')};
 unique(d.investigators,'Investigator');
 for(const i of d.investigators){need(typeof i.name==='string'&&object(i.skills),'Investigator name/skills required');need(Number.isFinite(i.health)&&i.health>0,'Starting health must be positive');for(const [k,v] of Object.entries(i.skills))need(STATS.includes(k)&&Number.isFinite(v)&&v>=0,'Invalid skill '+k)}
 need(object(d.initial)&&object(d.initial.inventory)&&object(d.initial.resources)&&Array.isArray(d.initial.flags)&&d.initial.flags.every(f=>typeof f==='string'),'Initial inventory, resources, text flags required');
 const amounts=(o,label,nonnegative=false)=>{need(object(o),label+' must be an object');for(const v of Object.values(o))need(Number.isFinite(v)&&(!nonnegative||v>=0),label+' values must be numbers'+(nonnegative?' >= 0':''))};
 amounts(d.initial.inventory,'Inventory',true);amounts(d.initial.resources,'Resources',true);
 need(Array.isArray(d.opportunities)&&Array.isArray(d.endings),'Opportunities/endings must be lists');unique(d.opportunities,'Opportunity');unique(d.endings,'Ending');
 const ids=new Set(d.opportunities.map(o=>o.id)),ends=new Set(d.endings.map(e=>e.id));
 const effect=e=>{need(object(e)&&typeof e.text==='string'&&typeof e.notes==='string','Outcome text/notes required');for(const k of ['inventory','resources','skills']){if(e[k])amounts(e[k],k);if(k==='skills'&&e[k])for(const s of Object.keys(e[k]))need(STATS.includes(s),'Unknown outcome skill '+s)}for(const k of ['unlock','remove']){need(Array.isArray(e[k]),'Outcome '+k+' must be a list');for(const id of e[k])need(ids.has(id),'Unknown opportunity '+id)}for(const k of ['flagsAdd','flagsRemove'])need(Array.isArray(e[k])&&e[k].every(x=>typeof x==='string'),'Outcome '+k+' must be text list');if(e.health!==undefined)need(Number.isFinite(e.health),'Health delta must be numeric');if(e.ending)need(ends.has(e.ending),'Unknown ending '+e.ending)};
 for(const o of d.opportunities){for(const k of ['title','body','notes'])need(typeof o[k]==='string','Opportunity '+k+' required');need(typeof o.initial==='boolean'&&typeof o.repeatable==='boolean','Opportunity initial/repeatable must be boolean');need(['investigator','item'].includes(o.kind),'Action kind investigator or item');need(Number.isInteger(o.duration)&&o.duration>=1,'Duration must be positive turns');need(o.deadline===null||(Number.isInteger(o.deadline)&&o.deadline>=1),'Deadline is an absolute turn or null');need(object(o.requires),'Requires object required');for(const k of ['flags','notFlags','traits'])need(Array.isArray(o.requires[k])&&o.requires[k].every(x=>typeof x==='string'),'Requirements '+k+' must be text list');amounts(o.requires.inventory,'Required items',true);amounts(o.requires.resources,'Required resources',true);amounts(o.requires.skills,'Required skills',true);for(const s of Object.keys(o.requires.skills))need(STATS.includes(s),'Unknown required skill '+s);need(object(o.cost),'Cost required');amounts(o.cost.inventory,'Item costs',true);amounts(o.cost.resources,'Resource costs',true);if(o.kind==='item')need(Object.keys(o.requires.skills).length===0&&o.requires.traits.length===0,'Item actions cannot require investigator skills/traits');if(o.check){need(o.kind==='investigator','Item actions cannot have skill checks');need(Array.isArray(o.check.skills)&&o.check.skills.length>0&&o.check.skills.every(s=>STATS.includes(s)),'Check skills invalid');need(Number.isFinite(o.check.target)&&typeof o.check.notes==='string','Check target/notes required');amounts(o.check.itemBonuses,'Item bonuses')};effect(o.success);effect(o.failure);effect(o.expired)}
 for(const e of d.endings)for(const k of ['title','body','notes'])need(typeof e[k]==='string','Ending '+k+' required');return d;
}
export function start(d){return {turn:1,inventory:structuredClone(d.initial.inventory),resources:structuredClone(d.initial.resources),flags:[...d.initial.flags],investigators:structuredClone(d.investigators),active:d.opportunities.filter(o=>o.initial).map(o=>o.id),completed:[],ending:null,log:[d.opening]}}
export function available(d,s,o,actor){
 if(s.ending)return 'Case ended';if(!s.active.includes(o.id))return 'Unavailable';
 if(o.kind==='investigator'&&(!actor||actor.health<=0))return 'Choose a healthy investigator';
 const r=o.requires;
 if(r.flags.some(f=>!s.flags.includes(f))||r.notFlags.some(f=>s.flags.includes(f)))return 'Clue requirements unmet';
 for(const [k,v] of Object.entries(r.inventory))if((s.inventory[k]||0)<v)return 'Needs '+v+' '+k;
 for(const [k,v] of Object.entries(r.resources))if((s.resources[k]||0)<v)return 'Needs '+v+' '+k;
 for(const [k,v] of Object.entries(r.skills))if((actor?.skills[k]||0)<v)return 'Needs '+k+' '+v;
 if(r.traits.some(t=>!actor?.traits?.includes(t)))return 'Needs trait: '+r.traits.join(', ');
 for(const group of ['inventory','resources'])for(const [k,v] of Object.entries(o.cost[group]))if((s[group][k]||0)<v)return 'Cost: '+v+' '+k;
 return '';
}
function apply(s,e,actor){
 s.log.push(e.text);for(const group of ['inventory','resources'])for(const [k,v] of Object.entries(e[group]||{}))s[group][k]=Math.max(0,(s[group][k]||0)+v);
 if(actor){actor.health=Math.max(0,actor.health+(e.health||0));for(const [k,v] of Object.entries(e.skills||{}))actor.skills[k]=Math.max(0,(actor.skills[k]||0)+v)}
 s.flags=[...new Set([...s.flags.filter(f=>!e.flagsRemove.includes(f)),...e.flagsAdd])];
 s.active=[...new Set([...s.active.filter(id=>!e.remove.includes(id)),...e.unlock.filter(id=>!s.completed.includes(id))])];if(e.ending)s.ending=e.ending;
}
// Shared check modifiers for resolution and the displayed roll range.
export function checkTerms(definition,state,opportunity,actor,{costsPaid=false}={}){
 const components=opportunity.check.skills.map(name=>({name,value:actor.skills[name]||0}));
 const skill=components.reduce((sum,c)=>sum+c.value,0)*definition.rule.multiplier;
 const itemTerms=Object.entries(opportunity.check.itemBonuses).filter(([name])=>(state.inventory[name]||0)-(costsPaid?0:(opportunity.cost.inventory[name]||0))>0).map(([name,value])=>({name,value}));
 return {skill,bonus:itemTerms.reduce((sum,c)=>sum+c.value,0),components,itemTerms};
}
export function checkProbability(definition,state,opportunity,actor){
 const terms=checkTerms(definition,state,opportunity,actor);let successes=0;
 for(let die=1;die<=definition.rule.sides;die++)if(die+terms.skill+terms.bonus>=opportunity.check.target)successes++;
 return {...terms,successes,sides:definition.rule.sides,percent:Math.round(100*successes/definition.rule.sides)};
}
function inventorySnapshot(state){return {inventory:{...state.inventory},resources:{...state.resources}}}
function inventoryChanges(before,state){const changes=[];for(const type of ['inventory','resources'])for(const name of new Set([...Object.keys(before[type]),...Object.keys(state[type])])){const previous=before[type][name]||0,next=state[type][name]||0;if(previous!==next)changes.push({type,name,before:previous,after:next,delta:next-previous})}return changes}
export function advance(d,state,assignments,roll=()=>1+Math.floor(Math.random()*d.rule.sides)){
 const s=structuredClone(state);if(s.ending)throw Error('Restart to play again');s.turnResults=[];
 const seenActors=new Set(),seenOps=new Set();const planned=assignments.map(a=>{const o=d.opportunities.find(o=>o.id===a.opportunity);if(!o)throw Error('Unknown opportunity');const actor=s.investigators.find(i=>i.id===a.investigator);const why=available(d,s,o,actor);if(why)throw Error(why);if(seenOps.has(o.id))throw Error('Assign an opportunity once per turn');seenOps.add(o.id);if(o.kind==='investigator'){if(seenActors.has(actor.id))throw Error('One action per investigator each turn');seenActors.add(actor.id)}return {o,actor:o.kind==='item'?null:actor}});
 let elapsed=1;
 for(const {o,actor} of planned){if(s.ending)break;const why=available(d,s,o,actor);if(why){s.log.push(o.title+' skipped: '+why);continue}elapsed=Math.max(elapsed,o.duration);const beforeInventory=inventorySnapshot(s);for(const group of ['inventory','resources'])for(const [k,v] of Object.entries(o.cost[group]))s[group][k]-=v;
 let passed=true,total=null;if(o.check){const die=roll();if(!Number.isInteger(die)||die<1||die>d.rule.sides)throw Error('Invalid die roll');const {skill,bonus}=checkTerms(d,s,o,actor,{costsPaid:true});total=die+skill+bonus;passed=total>=o.check.target;s.log.push(`${actor.name}: ${o.title} — ${die} + skills ${skill} + items ${bonus} vs ${o.check.target}: ${passed?'success':'failure'}`)}else s.log.push((actor?.name||'Item action')+': '+o.title);
 if(!o.repeatable){s.active=s.active.filter(id=>id!==o.id);s.completed.push(o.id)}
 const outcome=passed?'success':'failure',effect=o[outcome],unlocks=effect.unlock.filter(id=>!s.completed.includes(id));apply(s,effect,actor);
 s.turnResults.push({opportunity:o.id,investigator:actor?.id,passed,total,outcome,text:effect.text,unlocks,changes:inventoryChanges(beforeInventory,s)});
 }
 s.turn+=elapsed;
 if(!s.ending)for(const o of d.opportunities){if(s.ending)break;if(s.active.includes(o.id)&&o.deadline!==null&&s.turn>o.deadline){const beforeInventory=inventorySnapshot(s);s.active=s.active.filter(id=>id!==o.id);s.completed.push(o.id);s.log.push(o.title+' expired after turn '+o.deadline);const unlocks=o.expired.unlock.filter(id=>!s.completed.includes(id));apply(s,o.expired,null);s.turnResults.push({opportunity:o.id,passed:false,total:null,outcome:'expired',text:o.expired.text,unlocks,changes:inventoryChanges(beforeInventory,s)})}}
 if(!assignments.length)s.log.push('Waited a turn.');return s;
}
