import {recordTurn,turnFlow} from './turn-history.mjs';
import {STATS,validate,start,available,advance,checkProbability} from './engine.mjs';
import {selectedInvestigatorIds,baseInvestigatorForCase,investigatorForCase,testRoster} from './test-setup.mjs';
import {requiredSlots,reservations,placeCard,clearSlot,canPlaceCard,preparationReason,completePreparedTurn} from './turn-board.mjs';
const $=s=>document.querySelector(s),esc=s=>String(s??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
let doc,revision=null,dirty=false,saving=false,timer,raw={},problem='',storageProblem='',conflict=false,runtime=null,prepared={},generation=0,mode='dev';
let pickedCard=null,draggedCard=null,boardMessage='';
let hoveredTrayCard=null,hoveredBoardSlot=null;
let expandedFlowNodes=new Set(),turnHistory=[],flowObserver=null;
// Presentation-only order, retained across turns and test restarts for each case.
const trayOrders=new Map();
const trayCardKey=card=>card.type+':'+(card.id??card.name);
function trayOrder(type){const key=doc.id+':'+type;if(!trayOrders.has(key))trayOrders.set(key,[]);return trayOrders.get(key)}
function orderedTrayCards(cards,type){const order=trayOrder(type);for(const card of cards)if(!order.includes(card.key))order.push(card.key);return cards.sort((a,b)=>order.indexOf(a.key)-order.indexOf(b.key)).map(card=>card.html).join('')}
function reorderTrayCard(payload,type,target,after){const order=trayOrder(type),key=trayCardKey(payload),anchor=target&&trayCardKey(target);if(anchor===key)return;const previous=order.indexOf(key);if(previous!==-1)order.splice(previous,1);const index=anchor?order.indexOf(anchor):-1;order.splice(index<0?order.length:index+(after?1:0),0,key)}
function clearTrayInsertion(tray){tray.querySelectorAll('.insert-before,.insert-after').forEach(card=>card.classList.remove('insert-before','insert-after'))}
function resetTrayDragVisual(){hoveredTrayCard=null;hoveredBoardSlot=null;document.body.classList.remove('tray-card-dragging');document.querySelectorAll('.incompatible-slot,.incompatible-card,.drop-active,.insert-before,.insert-after').forEach(el=>{el.classList.remove('incompatible-slot','incompatible-card','drop-active','insert-before','insert-after');el.removeAttribute('aria-disabled')});document.querySelectorAll('#preview [data-drop-op] [aria-disabled]').forEach(el=>el.removeAttribute('aria-disabled'));document.querySelectorAll('.slot-skill-overlay').forEach(el=>el.remove())}
window.addEventListener('drop',()=>{resetTrayDragVisual();setTimeout(()=>{draggedCard=null;updateCompatibility()},0)},true);
window.addEventListener('dragend',()=>{draggedCard=null;resetTrayDragVisual()},true);
window.addEventListener('blur',resetTrayDragVisual);
window.addEventListener('keydown',event=>{if(event.key==='Escape')resetTrayDragVisual()});
document.addEventListener('visibilitychange',()=>{if(document.hidden)resetTrayDragVisual()});
let saveProblem='',serverProblem='';
let sourceRequest=0;
let investigatorCatalog=[],runDefinition=null,runSignature='';
async function loadCatalog(){const response=await fetch('/assets/investigators/catalog.json');if(!response.ok)throw Error('Investigator catalog unavailable');investigatorCatalog=await response.json()}
function skillStars(id,skill,value,baseline,disabled){const rating=Math.max(0,Math.min(6,Math.round(value))),overridden=value!==baseline;return '<div class="skill-rating" data-skill="'+skill+'" data-skill-control><span class="skill-name">'+esc(skill)+'</span><span class="skill-stars '+(overridden?'is-overridden':'')+'" role="group" aria-label="'+esc(skill)+' rating">'+[1,2,3,4,5,6].map(n=>'<button type="button" data-level="'+n+'" aria-label="'+esc(skill)+' '+n+' out of 6" aria-pressed="'+(rating===n)+'" '+(disabled?'disabled':'')+'><span aria-hidden="true" class="'+(n<=rating?'filled-star':'empty-star')+'">'+(n<=rating?'★':'☆')+'</span></button>').join('')+'</span></div>'}
function renderSetup(){
 const ids=new Set(selectedInvestigatorIds(doc,investigatorCatalog)),ordered=[...investigatorCatalog.filter(p=>p.statsSpecified!==false),...investigatorCatalog.filter(p=>p.statsSpecified===false)];
 $('#test-setup').innerHTML='<h2>Investigators</h2><p class="muted">Select the team for this case. Selections and skill overrides autosave. Restart Test to apply changes to an existing run.</p><div class="investigator-grid">'+ordered.map(p=>{const i=investigatorForCase(doc,p),base=baseInvestigatorForCase(doc,p),disabled=p.statsSpecified===false;return '<article class="investigator-card" data-investigator="'+p.id+'" data-selected="'+ids.has(p.id)+'" aria-disabled="'+disabled+'"><button type="button" class="investigator-select" aria-pressed="'+ids.has(p.id)+'" '+(disabled?'disabled':'')+'><img src="/assets/investigators/'+encodeURIComponent(p.file)+'" alt="'+esc(p.name)+'" width="95" height="95"><strong>'+esc(p.name)+'</strong></button><div class="investigator-stats">'+STATS.map(skill=>skillStars(p.id,skill,disabled?0:(i.skills[skill]||0),base.skills[skill]||0,disabled)).join('')+'</div>'+(disabled?'<em class="muted">Stats not specified; unavailable for tests.</em>':'')+'</article>'}).join('')+'</div>';
 $('#test-setup').querySelectorAll('[data-investigator]').forEach(card=>{const p=investigatorCatalog.find(p=>p.id===card.dataset.investigator);if(p.statsSpecified===false)return;card.onclick=event=>{if(event.target.closest('[data-skill-control]'))return;const selected=new Set(doc.testSetup?.investigatorIds??selectedInvestigatorIds(doc,investigatorCatalog));if(selected.has(p.id))selected.delete(p.id);else selected.add(p.id);doc.testSetup={...doc.testSetup,investigatorIds:[...selected]};changed();renderSetup();renderTray()};card.querySelectorAll('[data-level]').forEach(button=>button.onclick=event=>{event.stopPropagation();const skill=button.closest('[data-skill]').dataset.skill,baseline=baseInvestigatorForCase(doc,p).skills[skill]||0,current=investigatorForCase(doc,p).skills[skill]||0,clicked=Number(button.dataset.level),value=clicked===1&&current===1?0:clicked;const overrides=structuredClone(doc.testSetup?.skillOverrides||{}),skills={...overrides[p.id]};if(value===baseline)delete skills[skill];else skills[skill]=value;if(Object.keys(skills).length)overrides[p.id]=skills;else delete overrides[p.id];doc.testSetup={...doc.testSetup,investigatorIds:doc.testSetup?.investigatorIds??selectedInvestigatorIds(doc,investigatorCatalog),skillOverrides:overrides};changed();renderSetup();const selector='[data-level="'+clicked+'"]';$('#test-setup').querySelector('[data-investigator="'+p.id+'"] [data-skill="'+skill+'"] '+selector)?.focus()})});
 renderRuntimeOverrides();
}
async function refreshSource(){const request=++sourceRequest,id=doc?.sourcePitchId||doc?.source?.pitchId,box=$('#source-context');if(!box)return;box.hidden=true;box.textContent='';if(!id)return;try{const r=await fetch('/api/source-pitches/'+encodeURIComponent(id),{cache:'no-store'});if(!r.ok)throw Error('Source unavailable');const source=await r.json();if(request!==sourceRequest||box!==$('#source-context'))return;const premiseTitle=source.premise.title.replace(/\b\p{L}/gu,c=>c.toLocaleUpperCase());box.innerHTML=`<div class="pitch-section-heading"><h2><button type="button" class="pitch-toggle" aria-expanded="true" aria-controls="pitch-source-body">Pitch <span aria-hidden="true">▾</span></button></h2><a class="pitch-review-button" href="http://127.0.0.1:8765/?pitch=${encodeURIComponent(source.id)}" target="_blank" rel="noopener">Open In Pitch Review</a></div><div id="pitch-source-body"><h3>${esc(premiseTitle)}</h3><p class="muted">${esc(source.premise.description)}</p><h3>${esc(source.pitch.title)}</h3><p class="source-copy">${esc(source.pitch.text)}</p><h4>Notes</h4><p class="source-copy"><em>${esc(source.notes)||'—'}</em></p></div>`;const toggle=box.querySelector('.pitch-toggle'),body=box.querySelector('#pitch-source-body');toggle.onclick=()=>{const expanded=toggle.getAttribute('aria-expanded')!=='true';toggle.setAttribute('aria-expanded',String(expanded));body.hidden=!expanded;toggle.querySelector('span').textContent=expanded?'▾':'▸'};box.hidden=false}catch{if(request===sourceRequest&&box===$('#source-context')){box.textContent='Source pitch context unavailable.';box.hidden=false}}}
const key=id=>'top-expert-case-builder-draft:'+id;
function backup(){if(!doc)return;try{localStorage.setItem(key(doc.id),JSON.stringify({doc,revision,raw,updated:new Date().toISOString()}));storageProblem=''}catch{storageProblem='Browser draft backup unavailable. Keep this tab open until file save succeeds.'}showStatus()}
function showStatus(){const invalid=Object.keys(raw).length;const warning=serverProblem||(conflict?'File changed elsewhere — edits are not saving. Draft preserved.':invalid?'Invalid edits — changes are not saving. Draft preserved.':saveProblem)||(dirty?storageProblem:'');$('#save-status').textContent=warning;$('#save-status').hidden=!warning;$('#retry').hidden=!warning;$('#download').hidden=!warning;$('#retry').disabled=saving||invalid>0||conflict;if($('#conflict-actions'))$('#conflict-actions').hidden=!conflict;$('#error').hidden=!(problem||storageProblem);$('#error').textContent=[problem,storageProblem].filter(Boolean).join('\n')}
async function checkServer(){try{const r=await fetch('/api/cases',{cache:'no-store',signal:AbortSignal.timeout(3000)});if(!r.ok||!Array.isArray(await r.json()))throw Error('Unavailable');serverProblem=''}catch{serverProblem='Server unavailable — changes are not saving.'}showStatus()}
function changed(){dirty=true;generation++;problem='';backup();clearTimeout(timer);timer=setTimeout(save,300)}
async function save(){if(!doc||!dirty||saving||conflict||Object.keys(raw).length)return;try{validate(doc)}catch(e){problem=e.message;saveProblem='Invalid case — changes are not saving: '+e.message;showStatus();return}saving=true;showStatus();const snapshot=JSON.stringify(doc),base=revision;try{const r=await fetch('/api/cases/'+doc.id,{method:'PUT',headers:{'Content-Type':'application/json'},body:JSON.stringify({case:JSON.parse(snapshot),revision:base}),signal:AbortSignal.timeout(5000)});const result=await r.json();if(!r.ok){conflict=r.status===409;throw Error(result.error)}revision=result.revision;saveProblem='';serverProblem='';if(snapshot===JSON.stringify(doc)&&!Object.keys(raw).length){dirty=false;try{localStorage.removeItem(key(doc.id))}catch{}}else backup();problem=''}catch(e){problem='File not saved: '+e.message;saveProblem='Changes are not saving — '+e.message;backup()}finally{saving=false;showStatus();if(dirty&&!problem&&!conflict&&!Object.keys(raw).length)save()}}
function field(label,value,attrs='',large=false){return `<label>${esc(label)}${large?`<textarea ${attrs}>${esc(value)}</textarea>`:`<input ${attrs} value="${esc(value)}">`}</label>`}
function jsonBox(label,value,id){return field(label,raw[id]??JSON.stringify(value,null,2),`class="code" data-json="${id}"`,true)}
const effect=()=>({text:'',notes:'',inventory:{},resources:{},skills:{},health:0,flagsAdd:[],flagsRemove:[],unlock:[],remove:[],ending:null});
const newOp=id=>({id,title:'New opportunity',body:'What can the investigator do?',notes:'',kind:'investigator',initial:false,repeatable:false,duration:1,deadline:null,requires:{flags:[],notFlags:[],inventory:{},resources:{},skills:{},traits:[]},cost:{inventory:{},resources:{}},check:null,success:effect(),failure:effect(),expired:effect()});
function renderDev(){
 $('#dev').innerHTML=`<section id="source-context" class="source-context" aria-label="Source pitch" hidden></section><h2>Case Implementation</h2><div id="conflict-actions" ${conflict?'':'hidden'} class="card"><p>Your browser draft and disk file differ. Download the draft before choosing which version to continue.</p><button id="use-draft">Use this draft over current disk file</button><button id="use-disk">Load disk version (keep draft archive)</button></div>${field('Title',doc.title,'data-case-field="title"')}${field('Opening',doc.opening,'data-case-field="opening"',true)}${field('Case implementation notes',doc.notes,'data-case-field="notes"',true)}<details><summary>Starting team, inventory, resources & provisional check rule</summary><p>Check: d${doc.rule.sides} + sum of listed skills × ${doc.rule.multiplier} + bonuses from held items ≥ target. This formula is provisional and editable. Missing skills are 0.</p>${jsonBox('Check rule (sides, multiplier, notes)',doc.rule,'rule')}${jsonBox('Starting investigators (skills, health, traits, notes)',doc.investigators,'investigators')}${jsonBox('Starting inventory, resources and flags',doc.initial,'initial')}${jsonBox('Source pitch and review notes',doc.source,'source')}</details><h2>Opportunities <button id="add-op">Add opportunity</button></h2><p class="muted">Unlock controls board visibility. Requirements gate assignment. Costs apply on either result. Nonrepeatable actions leave the board on either result. Deadlines expire after the listed absolute turn.</p>${doc.opportunities.map((o,i)=>`<article class="card"><div class="row"><strong>${esc(o.id)}</strong><button data-remove="${i}">Remove opportunity</button></div>${field('Action title',o.title,`data-op="${i}" data-field="title"`)}${field('Player-facing body',o.body,`data-op="${i}" data-field="body"`,true)}${field('Implementation notes',o.notes,`data-op="${i}" data-field="notes"`,true)}<div class="row"><label>Slot type<select data-op="${i}" data-field="kind"><option value="investigator" ${o.kind==='investigator'?'selected':''}>Investigator</option><option value="item" ${o.kind==='item'?'selected':''}>Item only</option></select></label><label><input type="checkbox" data-op="${i}" data-field="initial" ${o.initial?'checked':''}> Starts on board</label><label><input type="checkbox" data-op="${i}" data-field="repeatable" ${o.repeatable?'checked':''}> Repeatable</label></div>${field('Check notes',o.check?.notes??'',`data-check-notes="${i}" ${o.check?'':'disabled'}`,true)}${['success','failure','expired'].map(k=>`<details><summary>${k==='expired'?'Deadline expiry':k} text & notes</summary>${field('Result text',o[k].text,`data-outcome="${i}" data-result="${k}" data-field="text"`,true)}${field('Notes',o[k].notes,`data-outcome="${i}" data-result="${k}" data-field="notes"`,true)}</details>`).join('')}<details><summary>All mechanics JSON — conditions, costs, check, transitions & effects</summary>${jsonBox('Complete opportunity (valid JSON saves immediately)',o,'op-'+i)}</details></article>`).join('')}<h2>Endings <button id="add-ending">Add ending</button></h2>${doc.endings.map((e,i)=>`<div class="card"><div class="row"><strong>${esc(e.id)}</strong><button data-remove-ending="${i}">Remove ending</button></div>${['title','body','notes'].map(k=>field(k,e[k],`data-ending="${i}" data-field="${k}"`,k!=='title')).join('')}</div>`).join('')}<details><summary>Entire case JSON</summary>${jsonBox('Advanced definition editor',doc,'entire')}<button id="refresh-editor">Rebuild structured controls from current valid definition</button></details>`;
 refreshSource();
 $('#dev').querySelectorAll('[data-json]').forEach(el=>el.addEventListener('focus',()=>{const id=el.dataset.json;if(Object.hasOwn(raw,id))return;const value=id==='entire'?doc:id.startsWith('op-')?doc.opportunities[Number(id.slice(3))]:doc[id];el.value=JSON.stringify(value,null,2)}));
 $('#dev').querySelectorAll('input,textarea,select').forEach(el=>el.addEventListener('input',()=>{
 if(el.dataset.json){const id=el.dataset.json;raw[id]=el.value;try{const value=JSON.parse(el.value),candidate=structuredClone(doc);if(id==='entire'){if(value.id!==doc.id)throw Error('Case ID must match the current filename');validate(value);doc=value}else{if(id.startsWith('op-'))candidate.opportunities[Number(id.slice(3))]=value;else candidate[id]=value;validate(candidate);doc=candidate}delete raw[id];problem=''}catch(e){problem='Editor '+id+': '+e.message}dirty=true;generation++;backup();clearTimeout(timer);timer=setTimeout(save,300);return}
 if(el.dataset.caseField)doc[el.dataset.caseField]=el.value;
 if(el.dataset.op!==undefined){const o=doc.opportunities[el.dataset.op];o[el.dataset.field]=el.type==='checkbox'?el.checked:el.value}
 if(el.dataset.checkNotes!==undefined)doc.opportunities[el.dataset.checkNotes].check.notes=el.value;
 if(el.dataset.outcome!==undefined)doc.opportunities[el.dataset.outcome][el.dataset.result][el.dataset.field]=el.value;
 if(el.dataset.ending!==undefined)doc.endings[el.dataset.ending][el.dataset.field]=el.value;
 // Keep invalid JSON drafts intact; structured changes cannot silently discard them.
 changed();
 }));
 $('#add-op').onclick=()=>{let n=1;while(doc.opportunities.some(o=>o.id==='action-'+n))n++;doc.opportunities.push(newOp('action-'+n));changed();renderDev()};
 $('#add-ending').onclick=()=>{let n=1;while(doc.endings.some(e=>e.id==='ending-'+n))n++;doc.endings.push({id:'ending-'+n,title:'New ending',body:'Case closed.',notes:''});changed();renderDev()};
 $('#dev').querySelectorAll('[data-remove]').forEach(el=>el.onclick=()=>{const id=doc.opportunities[el.dataset.remove].id;const refs=doc.opportunities.filter(o=>['success','failure','expired'].some(k=>o[k].unlock.includes(id)||o[k].remove.includes(id)));if(refs.length){problem='Remove references to '+id+' in outcomes first: '+refs.map(o=>o.id).join(', ');showStatus();return}if(Object.keys(raw).length){problem='Resolve JSON drafts before deleting';showStatus();return}doc.opportunities.splice(el.dataset.remove,1);changed();renderDev()});
 $('#dev').querySelectorAll('[data-remove-ending]').forEach(el=>el.onclick=()=>{const id=doc.endings[el.dataset.removeEnding].id;if(doc.opportunities.some(o=>['success','failure','expired'].some(k=>o[k].ending===id))){problem='Remove outcome references to '+id+' first';showStatus();return}doc.endings.splice(el.dataset.removeEnding,1);changed();renderDev()});
 $('#refresh-editor').onclick=renderDev;
 $('#use-draft').onclick=async()=>{try{const r=await fetch('/api/cases/'+doc.id);if(!r.ok)throw Error('Cannot read current disk file');const current=await r.json();localStorage.setItem(key(doc.id)+':disk-before-replace:'+Date.now(),JSON.stringify(current));revision=current.revision;conflict=false;backup();await save();renderDev()}catch(e){problem=e.message;showStatus()}};
 $('#use-disk').onclick=async()=>{try{localStorage.setItem(key(doc.id)+':archive:'+Date.now(),JSON.stringify({doc,raw,revision}));localStorage.removeItem(key(doc.id));dirty=false;raw={};await load(doc.id)}catch(e){problem=e.message;showStatus()}};
}
function portraitFor(id){return investigatorCatalog.find(p=>p.id===id)}
function checkDisplay(opportunity,actor,rule,resultTotal=null){
 const check=opportunity.check;if(!check)return '';
 const terms=actor?checkProbability(runDefinition,runtime,opportunity,actor):null;
 const minimum=resultTotal??(terms?1+terms.skill+terms.bonus:0),maximum=resultTotal??(terms?rule.sides+terms.skill+terms.bonus:0);
 const difficulty=Math.max(0,Math.ceil(check.target)),count=Math.max(difficulty,Math.ceil(maximum));
 const shape='M12 2 15.1 8.3 22 9.3 17 14.2 18.2 21.1 12 17.8 5.8 21.1 7 14.2 2 9.3 8.9 8.3Z';
 const stars=Array.from({length:count},(_,index)=>{
  const value=index+1,fill=actor||resultTotal!==null?(value<=minimum?'solid':value<=maximum?'possible':'empty'):'empty';
  return '<svg class="check-star '+fill+' '+(value<=difficulty?'required':'overflow')+'" viewBox="0 0 24 24" aria-hidden="true"><path class="star-fill" d="'+shape+'"/><path class="star-outline" d="'+shape+'"/></svg>';
 }).join('');
 const label=check.skills.join(' + '),description=label+': difficulty '+check.target+(resultTotal!==null?', rolled total '+resultTotal:actor?', guaranteed total '+minimum+', maximum total '+maximum:', no investigator assigned');
 return '<span class="check-rating" role="img" aria-label="'+esc(description)+'"><span class="check-skill">'+esc(label)+'</span><span class="check-stars">'+stars+'</span></span>';
}

function updateSkillOverlays(){
 document.querySelectorAll('.slot-skill-overlay').forEach(el=>el.remove());
 if(mode!=='preview'||!runtime||!hoveredBoardSlot)return;
 const o=runDefinition.opportunities.find(o=>o.id===hoveredBoardSlot.opportunity),slot=o&&requiredSlots(o).find(slot=>slot.key===hoveredBoardSlot.key);if(slot?.type!=='investigator'||!o.check)return;
 const cards=[...document.querySelectorAll('#investigator-tray .portrait-card[data-card]')];
 const occupied=document.querySelector(`#preview [data-drop-op="${o.id}"].portrait-card[data-card]`);if(occupied)cards.push(occupied);
 for(const card of cards){const payload=JSON.parse(card.dataset.card),actor=runtime.investigators.find(i=>i.id===payload.id);if(!actor)continue;card.insertAdjacentHTML('beforeend',`<span class="slot-skill-overlay ${o.check.skills.length>5?'many-skills':''}">${o.check.skills.map(skill=>`<span>${esc(skill)} +${actor.skills[skill]||0}</span>`).join('')}</span>`)}
}
function dropPayload(event){try{const text=event.dataTransfer.getData('application/x-top-expert-card')||event.dataTransfer.getData('text/plain');return text?JSON.parse(text):null}catch{return null}}
function updateCompatibility(){
 if(mode!=='preview'||!runtime||!runDefinition)return;
 const payload=draggedCard||hoveredTrayCard||pickedCard;
 document.querySelectorAll('#preview [data-drop-op]').forEach(target=>{const o=runDefinition.opportunities.find(o=>o.id===target.dataset.dropOp),slot=requiredSlots(o)[Number(target.dataset.dropSlot)],blocked=!!payload&&!canPlaceCard(runDefinition,runtime,prepared,o.id,slot.key,payload);target.classList.toggle('incompatible-slot',blocked);target.setAttribute('aria-disabled',String(blocked));target.querySelectorAll('[data-place-op]').forEach(button=>button.setAttribute('aria-disabled',String(blocked)))});
 document.querySelectorAll('#investigator-tray [data-card]').forEach(card=>{const blocked=!!hoveredBoardSlot&&!canPlaceCard(runDefinition,runtime,prepared,hoveredBoardSlot.opportunity,hoveredBoardSlot.key,JSON.parse(card.dataset.card));card.classList.toggle('incompatible-card',blocked)});
 updateSkillOverlays();
}
function cardLabel(payload){return payload?.type==='investigator'?runtime?.investigators.find(i=>i.id===payload.id)?.name||payload.id:payload?.name||''}
function cardMarkup(payload,content,extra='',attrs=''){return `<button type="button" class="candidate-card ${pickedCard&&JSON.stringify(pickedCard)===JSON.stringify(payload)?'picked':''} ${extra}" draggable="true" data-card="${esc(JSON.stringify(payload))}" title="${esc(cardLabel(payload))}" aria-label="Select ${esc(cardLabel(payload))}" ${attrs}>${content}</button>`}
function actorMarkup(actor,payload,extra='',attrs=''){const portrait=portraitFor(actor.id);return cardMarkup(payload,portrait?`<img src="/assets/investigators/${encodeURIComponent(portrait.file)}" alt="${esc(actor.name)}" draggable="false" width="86" height="86">`:'<span aria-hidden="true">?</span>','portrait-card '+extra,attrs)}

function itemMarkup(payload,quantity,attrs=''){return cardMarkup(payload,'<span class="item-name">'+esc(payload.name)+'</span><span class="item-quantity">x'+quantity+'</span>','item-card',attrs)}

function renderTray(){
 if(!runtime||!runDefinition)return;
 const selected=new Set(selectedInvestigatorIds(doc,investigatorCatalog));
 const actors=runtime.investigators.filter(i=>selected.has(i.id)&&i.health>0&&!Object.values(prepared).some(a=>a.investigator===i.id)).map(i=>({key:trayCardKey({type:'investigator',id:i.id}),html:actorMarkup(i,{type:'investigator',id:i.id})}));
 $('#investigator-portraits').innerHTML=orderedTrayCards(actors,'investigator');
 const items=[];for(const type of ['inventory','resources'])for(const [name,quantity] of Object.entries(runtime[type])){const remaining=quantity-reservations(runDefinition,prepared,type,name);if(remaining>0)items.push({key:trayCardKey({type,name}),html:itemMarkup({type,name},remaining)})}
 $('#inventory-items').innerHTML=orderedTrayCards(items,'inventory')||'<p class="muted">No items.</p>';
 $('#complete-turn').disabled=!!runtime.ending;
 bindCardSources($('#investigator-tray'));bindTrayReturn($('#investigator-portraits'),'investigator');bindTrayReturn($('#inventory-items'),'inventory');
}
function selectCard(payload){pickedCard=payload;boardMessage='';renderTray();updateCompatibility();const message=$('#board-message');if(message)message.textContent=boardMessage;const matching=[...document.querySelectorAll('[data-card]')].find(el=>el.dataset.card===JSON.stringify(payload));matching?.focus()}
function bindCardSources(root){root.querySelectorAll('[data-card]').forEach(button=>{const payload=JSON.parse(button.dataset.card);if(button.closest('#investigator-tray')){button.onpointerenter=button.onfocus=()=>{hoveredTrayCard=payload;updateCompatibility()};button.onpointerleave=button.onblur=()=>{if(hoveredTrayCard===payload)hoveredTrayCard=null;updateCompatibility()}}button.onclick=()=>{if(button.dataset.dropOp&&pickedCard&&JSON.stringify(pickedCard)!==JSON.stringify(payload)){const o=runDefinition.opportunities.find(o=>o.id===button.dataset.dropOp),slot=requiredSlots(o)[Number(button.dataset.dropSlot)];assignCard(o.id,slot.key,pickedCard)}else selectCard(payload)};button.ondragstart=event=>{if(payload.type==='investigator'&&runtime.investigators.find(i=>i.id===payload.id)?.health<=0){event.preventDefault();return}document.body.classList.toggle('tray-card-dragging',!!button.closest('#investigator-tray'));draggedCard=payload;updateCompatibility();event.dataTransfer.effectAllowed='move';event.dataTransfer.setData('application/x-top-expert-card',JSON.stringify(payload));event.dataTransfer.setData('text/plain',JSON.stringify(payload))};button.ondragend=()=>{draggedCard=null;resetTrayDragVisual();document.querySelectorAll('.drop-active').forEach(el=>el.classList.remove('drop-active'))}})}
function assignCard(opportunity,slot,payload){if(!canPlaceCard(runDefinition,runtime,prepared,opportunity,slot,payload))return;try{prepared=placeCard(runDefinition,runtime,prepared,opportunity,slot,payload);pickedCard=null;boardMessage='';renderPreview()}catch(e){boardMessage=e.message;const message=$('#board-message');if(message)message.textContent=boardMessage}}
function returnCard(payload,type){if(!payload)return;if(type==='investigator'&&payload.type==='investigator'){for(const entry of Object.values(prepared))if(entry.investigator===payload.id)prepared=clearSlot(prepared,entry.opportunity,'investigator')}else if(type==='inventory'&&['inventory','resources'].includes(payload.type)&&payload.fromOpportunity&&payload.fromSlot){prepared=clearSlot(prepared,payload.fromOpportunity,payload.fromSlot)}else return;pickedCard=null;draggedCard=null;boardMessage='';renderPreview()}
function bindTrayReturn(tray,type){
 const accepts=payload=>type==='investigator'?payload?.type==='investigator':['inventory','resources'].includes(payload?.type);
 const insertion=event=>{const card=event.target.closest('[data-card]');return {card:card&&tray.contains(card)?card:null,after:card?event.clientX>card.getBoundingClientRect().left+card.getBoundingClientRect().width/2:false}};
 tray.setAttribute('aria-label',type==='investigator'?'Investigator tray. Drag portraits to reorder or return them from the board.':'Inventory tray. Drag items to reorder or return them from the board.');
 tray.ondragenter=tray.ondragover=event=>{
  event.stopPropagation();const allowed=accepts(draggedCard||dropPayload(event));event.dataTransfer.dropEffect=allowed?'move':'none';tray.classList.toggle('drop-active',allowed);clearTrayInsertion(tray);
  if(allowed){event.preventDefault();const {card,after}=insertion(event);if(card&&trayCardKey(JSON.parse(card.dataset.card))!==trayCardKey(draggedCard||dropPayload(event)))card.classList.add(after?'insert-after':'insert-before')}
 };
 tray.ondragleave=event=>{if(!tray.contains(event.relatedTarget)){tray.classList.remove('drop-active');clearTrayInsertion(tray)}};
 tray.ondrop=event=>{
  event.stopPropagation();tray.classList.remove('drop-active');clearTrayInsertion(tray);const payload=dropPayload(event)||draggedCard;if(!accepts(payload)){event.dataTransfer.dropEffect='none';return}event.preventDefault();
  const {card,after}=insertion(event);reorderTrayCard(payload,type,card?JSON.parse(card.dataset.card):null,after);
  const onBoard=payload.type==='investigator'?Object.values(prepared).some(entry=>entry.investigator===payload.id):!!payload.fromOpportunity;
  if(onBoard)returnCard(payload,type);else{pickedCard=null;draggedCard=null;resetTrayDragVisual();renderTray();updateCompatibility()}
 };
 tray.onclick=event=>{if(!event.target.closest('[data-card]'))returnCard(pickedCard,type)};
 tray.onkeydown=event=>{if(event.target===tray&&['Enter',' '].includes(event.key)){event.preventDefault();returnCard(pickedCard,type)}};
}


function slotMarkup(o,slot,index){
 const entry=prepared[o.id],actor=runtime.investigators.find(i=>i.id===entry?.investigator),filled=slot.type==='investigator'?!!actor:!!entry?.slots?.[slot.key];
 const attrs=`data-drop-op="${o.id}" data-drop-slot="${index}"`,placeAttrs=`${attrs} data-place-op="${o.id}" data-place-slot="${index}"`;
 if(slot.type==='investigator')return filled?actorMarkup(actor,{type:'investigator',id:actor.id},'investigator-slot',attrs):`<button type="button" class="portrait-card investigator-slot empty-investigator-slot" ${placeAttrs} aria-label="Assign investigator to ${esc(o.title)}">Investigator</button>`;
 if(slot.type==='ready')return `<button type="button" class="item-slot ${filled?'slot-filled':'empty-item-slot'}" ${placeAttrs}>${filled?'Prepared':'Prepare action'}</button>`;
 if(filled)return itemMarkup({type:slot.type,name:slot.name,fromOpportunity:o.id,fromSlot:slot.key},slot.quantity,attrs);
 return `<button type="button" class="item-slot empty-item-slot" ${placeAttrs} aria-label="Assign ${esc(slot.name)} x${slot.quantity} to ${esc(o.title)}">${esc(slot.name)} x${slot.quantity}</button>`;
}

function bindSlots(){
 $('#preview').querySelectorAll('[data-drop-op]').forEach(target=>{const o=runDefinition.opportunities.find(o=>o.id===target.dataset.dropOp),slot=requiredSlots(o)[Number(target.dataset.dropSlot)],identity={opportunity:o.id,key:slot.key};target.onpointerenter=target.onfocusin=()=>{hoveredBoardSlot=identity;updateCompatibility()};target.onpointerleave=()=>{if(hoveredBoardSlot===identity)hoveredBoardSlot=null;updateCompatibility()};target.onfocusout=event=>{if(!target.contains(event.relatedTarget)&&hoveredBoardSlot===identity){hoveredBoardSlot=null;updateCompatibility()}};target.ondragenter=target.ondragover=event=>{event.stopPropagation();const payload=draggedCard||dropPayload(event),allowed=canPlaceCard(runDefinition,runtime,prepared,o.id,slot.key,payload);event.dataTransfer.dropEffect=allowed?'move':'none';target.classList.toggle('drop-active',allowed);hoveredBoardSlot=identity;updateCompatibility();if(allowed)event.preventDefault()};target.ondragleave=()=>{target.classList.remove('drop-active');if(hoveredBoardSlot===identity)hoveredBoardSlot=null;updateCompatibility()};target.ondrop=event=>{event.stopPropagation();target.classList.remove('drop-active');const payload=dropPayload(event)||draggedCard;if(!canPlaceCard(runDefinition,runtime,prepared,o.id,slot.key,payload)){event.dataTransfer.dropEffect='none';return}event.preventDefault();assignCard(o.id,slot.key,payload)}});
 $('#preview').querySelectorAll('[data-place-op]').forEach(button=>button.onclick=()=>{const o=runDefinition.opportunities.find(o=>o.id===button.dataset.placeOp),slot=requiredSlots(o)[Number(button.dataset.placeSlot)];if(slot.type==='ready'){if(!pickedCard)assignCard(o.id,slot.key,{type:'ready'})}else if(pickedCard)assignCard(o.id,slot.key,pickedCard);else{boardMessage='';$('#board-message').textContent=boardMessage}});

 bindCardSources($('#preview'));updateCompatibility();
}
function renderRuntimeOverrides(){
 if(!runtime||!$('#test-setup'))return;
 $('#test-setup').insertAdjacentHTML('beforeend',`<details class="runtime-overrides"><summary>Current test overrides</summary><p class="muted">Applies only to the running test. Does not save the case or Test Setup.</p>${jsonBox('Runtime investigators',runtime.investigators,'override-team')}${jsonBox('Runtime inventory',runtime.inventory,'override-inventory')}${jsonBox('Runtime resources',runtime.resources,'override-resources')}<button id="apply-overrides">Apply to current test</button></details>`);
 $('#apply-overrides').onclick=()=>{try{const team=JSON.parse($('[data-json="override-team"]').value),inventory=JSON.parse($('[data-json="override-inventory"]').value),resources=JSON.parse($('[data-json="override-resources"]').value);if(!Array.isArray(team)||team.some(i=>!Number.isFinite(i.health)||i.health<0))throw Error('Health must be nonnegative');validate({...runDefinition,investigators:team.length?team.map(i=>({...i,health:Math.max(.01,i.health)})):runDefinition.investigators,initial:{...runDefinition.initial,inventory,resources}});runtime={...runtime,investigators:team,inventory,resources,log:[...runtime.log,'Test-only overrides applied.']};prepared={};pickedCard=null;problem='';renderSetup()}catch(e){problem='Overrides not applied: '+e.message}showStatus()};
}
function opportunityMarkup(o,result,historyEntry=null){
 const d=runDefinition,s=runtime,interactive=result===undefined,entry=prepared[o.id],reason=interactive?preparationReason(d,s,o,entry):'',slots=requiredSlots(o),historicalSlots=historyEntry?slots.map(slot=>historicalSlotMarkup(slot,historyEntry)).join(''):'';
 return `<article class="card opportunity-card ${interactive?(!reason?'is-ready':''):result?(result.passed?'is-passed':'is-failed'):'is-unused'}" data-opportunity="${o.id}" aria-label="${esc(o.title)}: ${esc(interactive?(reason||'Ready'):result?(result.passed?'Passed':'Failed'):'Not chosen')}"><div class="opportunity-heading"><h3>${esc(o.title)}</h3></div><p>${esc(o.body)}</p>${o.deadline!==null?'<p class="muted opportunity-rules">Through turn '+o.deadline+'</p>':''}${o.requires.flags.length||o.requires.notFlags.length?'<p class="muted">Clues: '+esc([...o.requires.flags,...o.requires.notFlags.map(f=>'not '+f)].join(', '))+'</p>':''}${Object.keys(o.requires.skills).length||o.requires.traits.length?'<p class="muted">Investigator needs: '+esc([...Object.entries(o.requires.skills).map(([k,v])=>k+' '+v),...o.requires.traits].join(', '))+'</p>':''}<div class="opportunity-footer">${o.check&&(interactive||Number.isFinite(result?.total))?'<p class="muted opportunity-rules">'+checkDisplay(o,interactive?s.investigators.find(i=>i.id===entry?.investigator):null,d.rule,result?.total??null)+'</p>':''}${interactive?'<div class="opportunity-slots">'+slots.map((slot,index)=>slotMarkup(o,slot,index)).join('')+'</div>':(historicalSlots?'<div class="opportunity-slots history-slots">'+historicalSlots+'</div>':'')+(result?'<p class="opportunity-outcome">'+esc(result.text)+'</p>':'')}${historyEntry?inventoryChangeMarkup(historyEntry.events):''}</div></article>`;
}
function historicalSlotMarkup(slot,entry){
 if(slot.type==='investigator'){
  if(!entry.actor)return '';
  const portrait=portraitFor(entry.actor.id);return '<div class="portrait-card history-card" aria-label="Assigned '+esc(entry.actor.name)+'">'+(portrait?'<img src="/assets/investigators/'+encodeURIComponent(portrait.file)+'" alt="'+esc(entry.actor.name)+'" draggable="false">':esc(entry.actor.name))+'</div>';
 }
 const filled=entry.assignment.slots?.[slot.key];
 if(!filled)return '';
 if(slot.type==='ready')return '<div class="item-slot history-card">Prepared</div>';
 return '<div class="item-card history-card"><span class="item-name">'+esc(slot.name)+'</span><span class="item-quantity">x'+slot.quantity+'</span></div>';
}
function inventoryChangeMarkup(events){
 const changes=(events||[]).flatMap(event=>event.changes||[]);if(!changes.length)return '';
 return '<div class="inventory-changes" aria-label="Inventory changes">'+changes.map(change=>'<div class="item-card inventory-change '+(change.delta>0?'item-gained':'item-spent')+'" aria-label="'+esc(change.name+': '+change.before+' to '+change.after)+'"><span class="item-name">'+esc(change.name)+'</span><strong>'+(change.delta>0?'+':'')+change.delta+'</strong><span class="item-quantity">x'+change.before+' → x'+change.after+'</span></div>').join('')+'</div>';
}
function flowGraph(){return turnFlow(runDefinition,turnHistory,runtime)}
function flowNodeMarkup(node){
 const {opportunity:o,entry,current}=node,events=entry.events||[],outcome=events.find(event=>event.outcome!=='expired')||events[0];
 const result=outcome?{...outcome,text:events.map(event=>event.text).filter(Boolean).join('\n')}:null;
 const expanded=expandedFlowNodes.has(node.id),state=current?(preparationReason(runDefinition,runtime,o,prepared[o.id])?'is-active':'is-ready'):result?(result.passed?'is-passed':'is-failed'):'is-unused';
 const panel=expanded?opportunityMarkup(o,current?undefined:result,current?null:entry):'';
 return '<div class="flow-node '+state+'" data-flow-id="'+esc(node.id)+'"><button type="button" class="flow-toggle" data-flow-toggle="'+esc(node.id)+'" aria-expanded="'+expanded+'" aria-controls="flow-panel-'+esc(node.id)+'">'+esc(o.title)+'</button><div id="flow-panel-'+esc(node.id)+'" class="flow-panel" '+(expanded?'':'hidden')+'>'+panel+'</div></div>';
}
function updateStageBackground(){
 const stage=$('.case-flow-scroll'),current=stage?.querySelector('[data-current-turn]'),past=current?.previousElementSibling;
 if(!stage||!current)return;
 const boundary=past?.classList.contains('past-turn')?(past.getBoundingClientRect().right+current.getBoundingClientRect().left)/2-stage.getBoundingClientRect().left:0;
 stage.style.setProperty('--past-boundary',Math.max(0,Math.min(stage.clientWidth,boundary))+'px');
}
function drawFlowLines(){
 updateStageBackground();
 const canvas=$('#case-flow'),svg=$('#flow-lines');if(!canvas||!svg)return;
 const bounds=canvas.getBoundingClientRect(),{edges}=flowGraph(),elements=new Map([...canvas.querySelectorAll('[data-flow-id]')].map(el=>[el.dataset.flowId,el]));
 svg.setAttribute('viewBox','0 0 '+canvas.clientWidth+' '+canvas.clientHeight);
 svg.innerHTML=edges.map(edge=>{
  const from=elements.get(edge.from),to=elements.get(edge.to);if(!from||!to)return '';
  const a=from.getBoundingClientRect(),b=to.getBoundingClientRect(),x1=a.right-bounds.left,y1=a.top-bounds.top+Math.min(a.height/2,25),x2=b.left-bounds.left,y2=b.top-bounds.top+Math.min(b.height/2,25);
  const bend=Math.max(45,Math.abs(x2-x1)/2),back=x2<=x1;
  const path=back?'M '+x1+' '+y1+' C '+(x1+bend)+' '+(y1-55)+', '+(x2-bend)+' '+(y2-55)+', '+x2+' '+y2:'M '+x1+' '+y1+' C '+(x1+bend)+' '+y1+', '+(x2-bend)+' '+y2+', '+x2+' '+y2;
  const taken=edge.outcome!=='continuation';
  return '<path class="flow-link '+edge.outcome+(taken?' is-taken':'')+'" d="'+path+'"><title>'+esc((edge.outcome==='continuation'?'Same opportunity':edge.outcome[0].toUpperCase()+edge.outcome.slice(1))+': '+from.querySelector('.flow-toggle').textContent+' → '+to.querySelector('.flow-toggle').textContent)+'</title></path>';
 }).join('');
}
function openingCardMarkup(){
 const expanded=expandedFlowNodes.has('case-opening');
 return '<section class="flow-node opening-card"><button type="button" class="flow-toggle" data-flow-toggle="case-opening" aria-expanded="'+expanded+'" aria-controls="flow-panel-case-opening">'+esc(runDefinition.title)+'</button><div id="flow-panel-case-opening" class="flow-panel opening-copy" '+(expanded?'':'hidden')+'><p>'+esc(runDefinition.opening)+'</p></div></section>';
}
function flowMarkup(){
 const {columns}=flowGraph(),ending=runDefinition.endings.find(ending=>ending.id===runtime.ending);
 return '<div class="case-flow-scroll" tabindex="0" aria-label="Case stage. Drag to pan."><div id="case-flow" class="case-flow"><svg id="flow-lines" class="flow-lines" aria-label="Opportunity unlock history"></svg>'+columns.map((column,index)=>'<section class="flow-column '+(column.current?'current-turn':'past-turn')+'" aria-label="Turn '+column.number+'" '+(column.current?'data-current-turn':'')+'>'+(index===0?openingCardMarkup():'')+column.entries.map(entry=>flowNodeMarkup({id:index+':'+entry.opportunity,opportunity:runDefinition.opportunities.find(o=>o.id===entry.opportunity),entry,current:!!column.current})).join('')+(column.current?(ending?'<article class="card ending-card"><h3>'+esc(ending.title)+'</h3><p>'+esc(ending.body)+'</p></article>':!column.entries.length?'<p class="muted">No opportunities. Complete a turn to wait.</p>':''):'')+(column.current?'<button id="complete-turn" type="button">Complete turn</button>':'')+'</section>').join('')+'</div></div>';
}
function bindStagePan(stage){
 let gesture=null,suppressClick=false;
 stage.onpointerdown=event=>{
  if(!event.isPrimary||event.button!==0||event.target.closest('[data-card],[data-drop-op],input,textarea,select,a'))return;
  suppressClick=false;gesture={id:event.pointerId,x:event.clientX,y:event.clientY,left:stage.scrollLeft,top:stage.scrollTop,panning:false};
 };
 stage.onpointermove=event=>{
  if(!gesture||event.pointerId!==gesture.id)return;
  const dx=event.clientX-gesture.x,dy=event.clientY-gesture.y;
  if(!gesture.panning&&Math.hypot(dx,dy)<6)return;
  if(!gesture.panning){gesture.panning=true;stage.setPointerCapture(event.pointerId);stage.classList.add('is-panning')}
  event.preventDefault();stage.scrollLeft=gesture.left-dx;stage.scrollTop=gesture.top-dy;
 };
 const finish=event=>{
  if(!gesture||event.pointerId!==gesture.id)return;
  suppressClick=gesture.panning;gesture=null;stage.classList.remove('is-panning');
  if(stage.hasPointerCapture(event.pointerId))stage.releasePointerCapture(event.pointerId);
 };
 stage.onpointerup=stage.onpointercancel=finish;
 stage.onlostpointercapture=()=>{if(gesture?.panning){suppressClick=true;gesture=null;stage.classList.remove('is-panning')}};
 stage.addEventListener('click',event=>{if(suppressClick){suppressClick=false;event.preventDefault();event.stopImmediatePropagation()}},true);
 stage.ondragstart=event=>{if(!event.target.closest('[data-card]'))event.preventDefault()};
}
function bindFlow(){
 bindStagePan($('.case-flow-scroll'));$('.case-flow-scroll').onscroll=updateStageBackground;
 $('#preview').querySelectorAll('[data-flow-toggle]').forEach(button=>button.onclick=()=>{const id=button.dataset.flowToggle;if(expandedFlowNodes.has(id))expandedFlowNodes.delete(id);else expandedFlowNodes.add(id);renderPreview();$('#preview').querySelector('[data-flow-toggle="'+id+'"]')?.focus({preventScroll:true})});
 flowObserver?.disconnect();flowObserver=new ResizeObserver(drawFlowLines);flowObserver.observe($('#case-flow'));$('#case-flow').querySelectorAll('.flow-node').forEach(node=>flowObserver.observe(node));requestAnimationFrame(drawFlowLines);
}
function renderPreview(){
 const previousScroll=$('.case-flow-scroll'),scrollLeft=previousScroll?.scrollLeft||0,scrollTop=previousScroll?.scrollTop||0;
 draggedCard=null;resetTrayDragVisual();
 if(!runtime){try{validate(doc)}catch(e){$('#preview').innerHTML='<p>Fix the definition before testing: '+esc(e.message)+'</p>';return}runDefinition=structuredClone(doc);runtime=start({...runDefinition,investigators:testRoster(doc,investigatorCatalog)});prepared={};pickedCard=null;boardMessage='';expandedFlowNodes=new Set(['case-opening']);turnHistory=[];runSignature=JSON.stringify(doc)}
 const d=runDefinition,s=runtime,pending=runSignature!==JSON.stringify(doc);

 $('#preview').innerHTML='<section class="test-board" aria-label="Opportunity board">'+(pending?'<p class="setup-pending">Setup or definition changed. Restart test to apply; the current run is unchanged.</p>':'')+'<p id="board-message" role="status">'+esc(boardMessage)+'</p>'+flowMarkup()+'</section>';
 $('#restart').onclick=()=>{runtime=null;renderPreview()};
 $('#complete-turn').onclick=()=>{try{
  const result=completePreparedTurn(d,runtime,prepared);
  turnHistory.push(recordTurn(d,runtime,prepared,result.state));
  runtime=result.state;prepared={};pickedCard=null;boardMessage=result.skipped.length?'Not resolved: '+result.skipped.join('; '):'';renderPreview();const scroll=$('.case-flow-scroll');scroll.scrollLeft=scroll.scrollWidth;
 }catch(e){boardMessage=e.message;$('#board-message').textContent=boardMessage}};
 renderTray();bindSlots();bindFlow();const scroll=$('.case-flow-scroll');scroll.scrollLeft=scrollLeft;scroll.scrollTop=scrollTop;
}

async function list(){const r=await fetch('/api/cases');if(!r.ok)throw Error('Cannot list cases');const cases=await r.json();$('#case-select').innerHTML=cases.map(c=>`<option value="${esc(c.id)}">${esc(c.title)}</option>`).join('');return cases}
async function load(id){const r=await fetch('/api/cases/'+id);if(!r.ok)throw Error('Cannot load case');const body=await r.json();doc=body.case;revision=body.revision;raw={};dirty=false;conflict=false;problem='';try{const text=localStorage.getItem(key(id));if(text){const draft=JSON.parse(text);if(draft.doc.id!==id)throw Error('Draft ID mismatch');doc=draft.doc;raw=draft.raw||{};dirty=true;conflict=draft.revision!==revision;revision=conflict?draft.revision:revision;problem=conflict?'Recovered browser draft conflicts with the disk file. Compare in Dev before replacing.':'Recovered unsaved browser draft.'}}catch(e){problem='Draft recovery: '+e.message}runtime=null;prepared={};pickedCard=null;generation++;$('#case-select').value=id;renderDev();if(mode==='preview')renderPreview();if(mode==='setup')renderSetup();showStatus();if(dirty&&!conflict&&!Object.keys(raw).length)save()}
$('#case-select').onchange=async()=>{const next=$('#case-select').value;await save();if(dirty||saving){$('#case-select').value=doc.id;problem='Save or resolve this draft before switching cases';showStatus();return}try{await load(next)}catch(e){problem=e.message;showStatus()}};
for(const m of ['dev','preview','setup'])$('#'+m+'-mode').onclick=()=>{resetTrayDragVisual();mode=m;document.body.classList.toggle('preview-active',m==='preview');$('#investigator-tray').hidden=m!=='preview';$('#restart').hidden=m!=='preview';$('#dev').hidden=m!=='dev';$('#preview').hidden=m!=='preview';$('#test-setup').hidden=m!=='setup';for(const n of ['dev','preview','setup'])$('#'+n+'-mode').setAttribute('aria-pressed',String(n===m));if(m==='preview')renderPreview();else if(m==='setup')renderSetup();else renderDev()};
$('#retry').onclick=()=>{problem='';if(dirty)save();else checkServer()};$('#download').onclick=()=>{const a=document.createElement('a');a.href=URL.createObjectURL(new Blob([JSON.stringify({doc,revision,raw},null,2)],{type:'application/json'}));a.download=(doc?.id||'case')+'-draft.json';a.click();setTimeout(()=>URL.revokeObjectURL(a.href),1000)};
window.addEventListener('beforeunload',e=>{if(dirty){e.preventDefault();e.returnValue=''}});window.addEventListener('online',()=>{if(dirty&&!conflict)save()});
setInterval(checkServer,10000);
try{await loadCatalog();const cases=await list();if(cases.length)await load(cases[0].id);else{problem='No cases found. Add a case JSON file in cases/.';showStatus()}}catch(e){serverProblem='Server unavailable — changes are not saving.';problem='Cannot open Case Builder: '+e.message;showStatus()}
