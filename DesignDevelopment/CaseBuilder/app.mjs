import {recordTurn,turnFlow} from './turn-history.mjs';
import {STATS,GAME_RULES,validate,validateRuntime,start,available,advance,checkProbability} from './engine.mjs';
import {defaultTestSetup,selectedInvestigatorIds,baseInvestigator,testInvestigator,testRoster} from './test-setup.mjs';
import {requiredSlots,reservations,placeCard,clearSlot,canPlaceCard,preparationReason,completePreparedTurn} from './turn-board.mjs';
const $=s=>document.querySelector(s),esc=s=>String(s??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
let doc,revision=null,dirty=false,saving=false,timer,raw={},problem='',storageProblem='',conflict=false,runtime=null,prepared={},generation=0,mode='dev';
let pickedCard=null,draggedCard=null,boardMessage='';
let hoveredTrayCard=null,hoveredBoardSlot=null;
let expandedFlowNodes=new Set(),turnHistory=[],flowObserver=null;
let selectedDevOpportunity=null,devObserver=null,devSection='implementation';
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
 const ids=new Set(selectedInvestigatorIds(doc.testSetup,investigatorCatalog)),ordered=[...investigatorCatalog.filter(p=>p.statsSpecified!==false),...investigatorCatalog.filter(p=>p.statsSpecified===false)];
 $('#test-setup').innerHTML='<h2>Investigators</h2><p class="muted">Select the team for this case. Selections and skill overrides autosave. Restart Test to apply changes to an existing run.</p><div class="investigator-grid">'+ordered.map(p=>{const i=testInvestigator(doc.testSetup,p),base=baseInvestigator(p),disabled=p.statsSpecified===false;return '<article class="investigator-card" data-investigator="'+p.id+'" data-selected="'+ids.has(p.id)+'" aria-disabled="'+disabled+'"><button type="button" class="investigator-select" aria-pressed="'+ids.has(p.id)+'" '+(disabled?'disabled':'')+'><img src="/assets/investigators/'+encodeURIComponent(p.file)+'" alt="'+esc(p.name)+'" width="95" height="95"><strong>'+esc(p.name)+'</strong></button><div class="investigator-stats">'+STATS.map(skill=>skillStars(p.id,skill,disabled?0:(i.skills[skill]||0),base.skills[skill]||0,disabled)).join('')+'</div>'+(disabled?'<em class="muted">Stats not specified; unavailable for tests.</em>':'')+'</article>'}).join('')+'</div>';
 $('#test-setup').querySelectorAll('[data-investigator]').forEach(card=>{const p=investigatorCatalog.find(p=>p.id===card.dataset.investigator);if(p.statsSpecified===false)return;card.onclick=event=>{if(event.target.closest('[data-skill-control]'))return;const selected=new Set(doc.testSetup?.investigatorIds??selectedInvestigatorIds(doc.testSetup,investigatorCatalog));if(selected.has(p.id))selected.delete(p.id);else selected.add(p.id);doc.testSetup={...doc.testSetup,investigatorIds:[...selected]};changed();renderSetup();renderTray()};card.querySelectorAll('[data-level]').forEach(button=>button.onclick=event=>{event.stopPropagation();const skill=button.closest('[data-skill]').dataset.skill,baseline=baseInvestigator(p).skills[skill]||0,current=testInvestigator(doc.testSetup,p).skills[skill]||0,clicked=Number(button.dataset.level),value=clicked===1&&current===1?0:clicked;const overrides=structuredClone(doc.testSetup?.skillOverrides||{}),skills={...overrides[p.id]};if(value===baseline)delete skills[skill];else skills[skill]=value;if(Object.keys(skills).length)overrides[p.id]=skills;else delete overrides[p.id];doc.testSetup={...doc.testSetup,investigatorIds:doc.testSetup?.investigatorIds??selectedInvestigatorIds(doc.testSetup,investigatorCatalog),skillOverrides:overrides};changed();renderSetup();const selector='[data-level="'+clicked+'"]';$('#test-setup').querySelector('[data-investigator="'+p.id+'"] [data-skill="'+skill+'"] '+selector)?.focus()})});
 renderTestSupplies();renderRuntimeOverrides();
}
function renderTestSupplies(){
 const root=$('#test-setup');
 root.insertAdjacentHTML('beforeend','<section class="test-supply-section"><h2>Inventory & resources</h2><p class="muted">Starting supplies for this case’s test. Changes apply on Restart test.</p><div class="test-supply-groups">'+['inventory','resources'].map(type=>'<section class="test-supply-group"><h3>'+ (type==='inventory'?'Inventory':'Resources')+'</h3>'+Object.entries(doc.testSetup[type]).map(([name,quantity])=>'<div class="supply-row"><label>'+esc(name)+'<input type="number" min="0" step="1" data-supply-type="'+type+'" data-supply-name="'+esc(name)+'" value="'+quantity+'"></label><button type="button" data-remove-supply="'+type+'" data-supply-name="'+esc(name)+'" aria-label="Remove '+esc(name)+'">Remove</button></div>').join('')+'<div class="supply-add"><input placeholder="'+(type==='inventory'?'Item name':'Resource name')+'" aria-label="New '+type+' name" data-new-supply="'+type+'"><button type="button" data-add-supply="'+type+'">Add</button></div></section>').join('')+'</div></section>');
 root.querySelectorAll('[data-supply-type]').forEach(input=>input.oninput=()=>{const value=input.valueAsNumber;if(!Number.isFinite(value)||value<0){input.setCustomValidity('Enter a nonnegative quantity');input.reportValidity();return}input.setCustomValidity('');doc.testSetup[input.dataset.supplyType][input.dataset.supplyName]=value;changed()});
 root.querySelectorAll('[data-remove-supply]').forEach(button=>button.onclick=()=>{delete doc.testSetup[button.dataset.removeSupply][button.dataset.supplyName];changed();renderSetup()});
 root.querySelectorAll('[data-add-supply]').forEach(button=>button.onclick=()=>{const type=button.dataset.addSupply,input=root.querySelector('[data-new-supply="'+type+'"]'),name=input.value.trim();if(!name)return;if(!Object.hasOwn(doc.testSetup[type],name))doc.testSetup[type][name]=1;changed();renderSetup()});
}
async function refreshSource(){
 const request=++sourceRequest,id=doc?.sourcePitchId||doc?.source?.pitchId,box=$('#source-context');if(!box)return;
 box.hidden=false;box.textContent=id?'Loading pitch…':'This case is not linked to a pitch.';if(!id)return;
 try{const r=await fetch('/api/source-pitches/'+encodeURIComponent(id),{cache:'no-store'});if(!r.ok)throw Error('Source unavailable');const source=await r.json();if(request!==sourceRequest||box!==$('#source-context'))return;
 const premiseTitle=source.premise.title.replace(/\b\p{L}/gu,c=>c.toLocaleUpperCase());
 box.innerHTML=`<div class="pitch-section-heading"><h2>Pitch</h2><a class="pitch-review-button" href="http://127.0.0.1:8765/?pitch=${encodeURIComponent(source.id)}" target="_blank" rel="noopener">Open In Pitch Review</a></div><h3>${esc(premiseTitle)}</h3><p class="muted">${esc(source.premise.description)}</p><h3>${esc(source.pitch.title)}</h3><p class="source-copy">${esc(source.pitch.text)}</p><h4>Notes</h4><p class="source-copy"><em>${esc(source.notes)||'—'}</em></p>`;
 }catch{if(request===sourceRequest&&box===$('#source-context'))box.textContent='Source pitch context unavailable.'}
}

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
function wireTabKeys(root){
 root.onkeydown=event=>{
  const tabs=[...root.querySelectorAll('[role="tab"]')],current=tabs.indexOf(event.target);if(current<0)return;
  let next;if(event.key==='ArrowRight')next=(current+1)%tabs.length;else if(event.key==='ArrowLeft')next=(current+tabs.length-1)%tabs.length;else if(event.key==='Home')next=0;else if(event.key==='End')next=tabs.length-1;else return;
  event.preventDefault();tabs[next].click();tabs[next].focus();
 };
}
function organizeDevPanels(){
 const root=$('#dev'),pitch=$('#source-context'),settings=$('#dev-case-settings'),toolbar=root.querySelector('.dev-graph-toolbar'),graph=root.querySelector('.dev-graph-scroll');
 const tabs=document.createElement('div');tabs.className='secondary-tabs';tabs.setAttribute('role','tablist');tabs.setAttribute('aria-label','Dev sections');
 const items=[['pitch','Pitch',[pitch]],['settings','Case Settings',[settings]],['implementation','Implementation',[toolbar,graph]]];
 for(const [id,title,content] of items){
  const button=document.createElement('button');button.type='button';button.id='dev-'+id+'-tab';button.setAttribute('role','tab');button.setAttribute('aria-controls','dev-'+id+'-panel');button.dataset.devSection=id;button.textContent=title;tabs.append(button);
  const panel=document.createElement('section');panel.id='dev-'+id+'-panel';panel.className='dev-section-panel';panel.setAttribute('role','tabpanel');panel.setAttribute('aria-labelledby',button.id);content.forEach(el=>panel.append(el));root.append(panel);
  button.onclick=()=>{selectedDevOpportunity=null;devSection=id;applyDevSection();renderDevGraph()};
 }
 root.prepend(tabs);wireTabKeys(tabs);applyDevSection();
}
function applyDevSection(){
 $('#dev').querySelectorAll('[data-dev-section]').forEach(button=>{const active=button.dataset.devSection===devSection;button.setAttribute('aria-selected',String(active));button.tabIndex=active?0:-1;$('#dev-'+button.dataset.devSection+'-panel').hidden=!active});
 requestAnimationFrame(drawDevLines);
}
function renderDev(){
 $('#dev').innerHTML=`<section id="source-context" class="source-context" aria-label="Source pitch" hidden></section><section id="dev-case-settings" aria-label="Case settings"><h2>Case Settings</h2><div id="conflict-actions" ${conflict?'':'hidden'} class="card"><p>Your browser draft and disk file differ. Download the draft before choosing which version to continue.</p><button id="use-draft">Use this draft over current disk file</button><button id="use-disk">Load disk version (keep draft archive)</button></div>${field('Title',doc.title,'data-case-field="title"')}${field('Opening',doc.opening,'data-case-field="opening"',true)}${field('Case implementation notes',doc.notes,'data-case-field="notes"',true)}${field('Initial scene flags',doc.initialFlags.join(', '),'data-case-flags="true"')}<h2>Endings <button id="add-ending">Add ending</button></h2>${doc.endings.map((e,i)=>`<div class="card"><div class="row"><strong>${esc(e.id)}</strong><button data-remove-ending="${i}">Remove ending</button></div>${['title','body','notes'].map(k=>field(k,e[k],`data-ending="${i}" data-field="${k}"`,k!=='title')).join('')}</div>`).join('')}</section><div class="dev-graph-toolbar"><h2>Case Implementation</h2><button id="add-op">Add opportunity</button></div><div class="dev-graph-scroll" tabindex="0" aria-label="Case dependency graph. Drag to pan."><div id="dev-graph" class="case-flow"><svg id="dev-lines" class="flow-lines" aria-label="Case outcome dependencies"></svg></div></div>`;
 organizeDevPanels();renderDevGraph();refreshSource();
 bindDevInputs($('#dev-case-settings'));
 $('#add-op').onclick=()=>{const id=crypto.randomUUID();doc.opportunities.push(newOp(id));selectedDevOpportunity=id;changed();renderDev()};
 $('#add-ending').onclick=()=>{let n=1;while(doc.endings.some(e=>e.id==='ending-'+n))n++;doc.endings.push({id:'ending-'+n,title:'New ending',body:'Case closed.',notes:''});changed();renderDev()};
 $('#dev').querySelectorAll('[data-remove-ending]').forEach(el=>el.onclick=()=>{const id=doc.endings[el.dataset.removeEnding].id;if(doc.opportunities.some(o=>['success','failure','expired'].some(k=>o[k].ending===id))){problem='Remove outcome references to '+id+' first';showStatus();return}doc.endings.splice(el.dataset.removeEnding,1);changed();renderDev()});
 $('#use-draft').onclick=async()=>{try{const r=await fetch('/api/cases/'+doc.id);if(!r.ok)throw Error('Cannot read current disk file');const current=await r.json();localStorage.setItem(key(doc.id)+':disk-before-replace:'+Date.now(),JSON.stringify(current));revision=current.revision;conflict=false;backup();await save();renderDev()}catch(e){problem=e.message;showStatus()}};
 $('#use-disk').onclick=async()=>{try{localStorage.setItem(key(doc.id)+':archive:'+Date.now(),JSON.stringify({doc,raw,revision}));localStorage.removeItem(key(doc.id));dirty=false;raw={};await load(doc.id)}catch(e){problem=e.message;showStatus()}};
}
function bindDevInputs(root){
 root.querySelectorAll('[data-add-check],[data-remove-check]').forEach(button=>button.onclick=()=>{const adding=button.dataset.addCheck!==undefined,index=Number(adding?button.dataset.addCheck:button.dataset.removeCheck),design=editInteraction(doc.opportunities[index]);if(adding){const attribute=STATS.find(attribute=>!design.checks.some(check=>check.attribute===attribute))??STATS[0];design.checks.push({attribute,difficulty:5})}else if(design.checks.length>1)design.checks.splice(Number(button.dataset.checkIndex),1);changed();renderDevGraph()});
 root.querySelectorAll('input,textarea,select').forEach(el=>el.addEventListener('input',()=>{
 if(el.dataset.interaction!==undefined){const design=editInteraction(doc.opportunities[el.dataset.interaction]),name=el.dataset.interactionField;let value=el.type==='checkbox'?el.checked:el.value;if(el.type==='number'){value=el.valueAsNumber;const minimum=name==='requiredInvestigators'?1:0;if(!Number.isInteger(value)||value<minimum){el.setCustomValidity('Enter a whole number of at least '+minimum);el.reportValidity();return}el.setCustomValidity('')}design[name]=value;changed();if(el.type==='checkbox')renderDevGraph();return}
 if(el.dataset.checkDesign!==undefined){const check=editInteraction(doc.opportunities[el.dataset.checkDesign]).checks[Number(el.dataset.checkIndex)];let value=el.value;if(el.dataset.checkField==='difficulty'){value=el.valueAsNumber;if(!Number.isInteger(value)||value<1){el.setCustomValidity('Enter a whole number of at least 1');el.reportValidity();return}el.setCustomValidity('')}check[el.dataset.checkField]=value;changed();return}
 if(el.dataset.deadlineToggle!==undefined){const index=el.dataset.deadlineToggle,fields=root.querySelector('[data-deadline-fields="'+index+'"]'),input=fields.querySelector('[data-deadline-turns]');doc.opportunities[index].deadline=el.checked?Number(input.value):null;fields.hidden=!el.checked;changed();return}
 if(el.dataset.deadlineTurns!==undefined){const value=el.valueAsNumber;if(!Number.isInteger(value)||value<1){el.setCustomValidity('Enter a whole number of turns, at least 1');el.reportValidity();return}el.setCustomValidity('');doc.opportunities[el.dataset.deadlineTurns].deadline=value}
 if(el.dataset.caseField)doc[el.dataset.caseField]=el.value;
 if(el.dataset.caseFlags)doc.initialFlags=[...new Set(el.value.split(',').map(value=>value.trim()).filter(Boolean))];
 if(el.dataset.op!==undefined){const o=doc.opportunities[el.dataset.op];o[el.dataset.field]=el.type==='checkbox'?el.checked:el.value}
 if(el.dataset.outcome!==undefined)doc.opportunities[el.dataset.outcome][el.dataset.result][el.dataset.field]=el.value;
 if(el.dataset.ending!==undefined)doc.endings[el.dataset.ending][el.dataset.field]=el.value;
 // Keep invalid JSON drafts intact; structured changes cannot silently discard them.
 changed();
 }));
}
// Editor interaction design is persisted independently until the runtime mechanics overhaul.
function interactionDesign(o){return o.interactionDesign??{skillCheck:!!o.check,mode:'all',checks:o.check?o.check.skills.map(attribute=>({attribute,difficulty:o.check.target})):[{attribute:STATS[0],difficulty:5}],requiredInvestigators:1,optionalInvestigators:0}}
function editInteraction(o){return o.interactionDesign??=structuredClone(interactionDesign(o))}
function skillCheckEditor(o,i){const design=interactionDesign(o);return '<section class="interaction-skill-checks">'+(design.checks.length>1?'<label>Pass condition<select data-interaction="'+i+'" data-interaction-field="mode"><option value="any" '+(design.mode==='any'?'selected':'')+'>Any check passes</option><option value="all" '+(design.mode==='all'?'selected':'')+'>All checks pass</option></select></label>':'')+design.checks.map((check,index)=>'<div class="skill-check-editor-row"><label>Attribute<select data-check-design="'+i+'" data-check-index="'+index+'" data-check-field="attribute">'+STATS.map(attribute=>'<option '+(attribute===check.attribute?'selected':'')+'>'+attribute+'</option>').join('')+'</select></label><label>Difficulty<input type="number" min="1" step="1" value="'+check.difficulty+'" data-check-design="'+i+'" data-check-index="'+index+'" data-check-field="difficulty"></label>'+(design.checks.length>1?'<button type="button" data-remove-check="'+i+'" data-check-index="'+index+'" aria-label="Remove check '+(index+1)+'">Remove</button>':'')+'</div>').join('')+'<button type="button" data-add-check="'+i+'">Add attribute</button><div class="investigator-counts"><label>Required investigators<input type="number" min="1" step="1" value="'+design.requiredInvestigators+'" data-interaction="'+i+'" data-interaction-field="requiredInvestigators"></label><label>Optional investigators<input type="number" min="0" step="1" value="'+design.optionalInvestigators+'" data-interaction="'+i+'" data-interaction-field="optionalInvestigators"></label></div></section>'}
function devOpportunityEditor(o,i){const design=interactionDesign(o);return '<article class="card"><div class="row"><button data-remove="'+i+'">Remove opportunity</button></div>'+field('Title',o.title,'data-op="'+i+'" data-field="title"')+field('Body',o.body,'data-op="'+i+'" data-field="body"',true)+'<section class="interaction-editor"><div class="row interaction-options"><label><input type="checkbox" data-deadline-toggle="'+i+'" '+(o.deadline!==null?'checked':'')+'> Deadline</label><label><input type="checkbox" data-interaction="'+i+'" data-interaction-field="skillCheck" '+(design.skillCheck?'checked':'')+'> Skill check</label></div><div data-deadline-fields="'+i+'" '+(o.deadline===null?'hidden':'')+'><label>Number of turns<input type="number" min="1" step="1" value="'+(o.deadline??1)+'" data-deadline-turns="'+i+'"></label>'+field('Deadline failed text',o.expired.text,'data-outcome="'+i+'" data-result="expired" data-field="text"',true)+'</div>'+(design.skillCheck?skillCheckEditor(o,i)+['success','failure'].map(k=>field(k==='success'?'Success text':'Failure text',o[k].text,'data-outcome="'+i+'" data-result="'+k+'" data-field="text"',true)).join(''):'')+'<label><input type="checkbox" data-op="'+i+'" data-field="repeatable" '+(o.repeatable?'checked':'')+'> Repeatable</label></section><label><input type="checkbox" data-op="'+i+'" data-field="initial" '+(o.initial?'checked':'')+'> Starts on board</label></article>'}
// Collapse cycles before ranking so retries never push a graph endlessly right.
function dependencyGraph(definition){
 const nodes=definition.opportunities.map((opportunity,index)=>({id:opportunity.id,title:opportunity.title,opportunity,index}));
 for(const ending of definition.endings)nodes.push({id:'ending:'+ending.id,title:ending.title,ending});
 const ids=new Set(nodes.map(node=>node.id)),edges=[];
 for(const node of nodes)if(node.opportunity)for(const outcome of ['success','failure','expired']){
  const effect=node.opportunity[outcome];for(const to of effect.unlock)if(ids.has(to))edges.push({from:node.id,to,outcome});
  if(effect.ending&&ids.has('ending:'+effect.ending))edges.push({from:node.id,to:'ending:'+effect.ending,outcome});
 }
 const outgoing=new Map(nodes.map(node=>[node.id,edges.filter(edge=>edge.from===node.id).map(edge=>edge.to)]));
 const index=new Map(),low=new Map(),stack=[],onStack=new Set(),component=new Map();let cursor=0,groups=0;
 function visit(id){index.set(id,cursor);low.set(id,cursor++);stack.push(id);onStack.add(id);
  for(const next of outgoing.get(id)){if(!index.has(next)){visit(next);low.set(id,Math.min(low.get(id),low.get(next)))}else if(onStack.has(next))low.set(id,Math.min(low.get(id),index.get(next)))}
  if(low.get(id)===index.get(id)){let next;do{next=stack.pop();onStack.delete(next);component.set(next,groups)}while(next!==id);groups++}
 }
 for(const node of nodes)if(!index.has(node.id))visit(node.id);
 const parents=Array.from({length:groups},()=>new Set());for(const edge of edges){const a=component.get(edge.from),b=component.get(edge.to);if(a!==b)parents[b].add(a)}
 const depths=new Map();function depth(group){if(!depths.has(group))depths.set(group,parents[group].size?1+Math.max(...[...parents[group]].map(depth)):0);return depths.get(group)}
 return {nodes:nodes.map(node=>({...node,depth:depth(component.get(node.id))})),edges};
}

function devPreviewSlot(slot){
 if(slot.type==='investigator')return '<div class="portrait-card empty-investigator-slot">Investigator</div>';
 return '<div class="item-slot empty-item-slot">'+(slot.type==='ready'?'Prepare action':esc(slot.name)+' x'+slot.quantity)+'</div>';
}
function renderDevGraph(){
 const stage=$('.dev-graph-scroll'),canvas=$('#dev-graph');if(!stage||!canvas)return;
 const left=stage.scrollLeft,top=stage.scrollTop,{nodes}=dependencyGraph(doc),depths=[...new Set(nodes.map(node=>node.depth))].sort((a,b)=>a-b);
 canvas.innerHTML='<svg id="dev-lines" class="flow-lines" aria-label="Case outcome dependencies"></svg>'+depths.map(depth=>'<div class="flow-column">'+nodes.filter(node=>node.depth===depth).map(node=>{
  if(node.ending)return '<section class="flow-node dev-ending" data-flow-id="'+esc(node.id)+'"><div class="flow-toggle"><span class="action-title">'+esc(node.title)+'</span></div><div class="opening-copy"><span class="mechanic-label">Ending</span><p class="scene-copy">'+esc(node.ending.body)+'</p></div></section>';
  const selected=node.id===selectedDevOpportunity;
  return '<section class="flow-node dev-opportunity '+(selected?'is-editing':'')+'" data-flow-id="'+esc(node.id)+'" '+(selected?'data-dev-editor="'+esc(node.id)+'"':'data-dev-select="'+esc(node.id)+'"')+'>'+(selected?devOpportunityEditor(node.opportunity,node.index):'<button type="button" class="flow-toggle" aria-label="Edit '+esc(node.title)+'"><span class="action-title">'+esc(node.title)+'</span></button>'+opportunityMarkup(node.opportunity,undefined,null,{definition:doc}))+'</section>';
 }).join('')+'</div>').join('');
 stage.scrollLeft=left;stage.scrollTop=top;bindStagePan(stage);bindDevInputs(canvas);
 canvas.querySelectorAll('[data-dev-select]').forEach(card=>card.onclick=event=>{event.stopPropagation();selectedDevOpportunity=card.dataset.devSelect;renderDevGraph()});
 canvas.querySelectorAll('[data-remove]').forEach(button=>button.onclick=event=>{
  event.stopPropagation();const index=Number(button.dataset.remove),id=doc.opportunities[index].id;
  if(doc.opportunities.some(o=>['success','failure','expired'].some(k=>o[k].unlock.includes(id)||o[k].remove.includes(id)))){problem='Remove references to '+id+' in outcomes first';showStatus();return}
  if(Object.keys(raw).length){problem='Resolve JSON drafts before deleting';showStatus();return}
  doc.opportunities.splice(index,1);selectedDevOpportunity=null;changed();renderDevGraph();
 });
 devObserver?.disconnect();devObserver=new ResizeObserver(drawDevLines);devObserver.observe(canvas);canvas.querySelectorAll('.flow-node').forEach(node=>devObserver.observe(node));requestAnimationFrame(drawDevLines);
}
function drawDevLines(){
 const canvas=$('#dev-graph'),svg=$('#dev-lines');if(!canvas||!svg)return;
 const bounds=canvas.getBoundingClientRect(),elements=new Map([...canvas.querySelectorAll('[data-flow-id]')].map(el=>[el.dataset.flowId,el]));
 svg.setAttribute('viewBox','0 0 '+canvas.clientWidth+' '+canvas.clientHeight);
 svg.innerHTML=dependencyGraph(doc).edges.map(edge=>{
  const from=elements.get(edge.from),to=elements.get(edge.to);if(!from||!to)return '';
  const a=from.getBoundingClientRect(),b=to.getBoundingClientRect(),x1=a.right-bounds.left,y1=a.top-bounds.top+26,x2=b.left-bounds.left,y2=b.top-bounds.top+26,bend=Math.max(45,Math.abs(x2-x1)/2);
  const path=x2<=x1?'M '+x1+' '+y1+' C '+(x1+bend)+' '+(y1-60)+', '+(x2-bend)+' '+(y2-60)+', '+x2+' '+y2:'M '+x1+' '+y1+' C '+(x1+bend)+' '+y1+', '+(x2-bend)+' '+y2+', '+x2+' '+y2;
  return '<path class="flow-link '+edge.outcome+'" d="'+path+'"><title>'+esc(edge.outcome+': '+edge.from+' → '+edge.to)+'</title></path>';
 }).join('');
}
document.addEventListener('click',event=>{
 if(mode!=='dev'||!selectedDevOpportunity||event.target.closest('[data-dev-editor],[data-dev-select],#add-op'))return;
 selectedDevOpportunity=null;renderDevGraph();
});
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
 const selected=new Set(selectedInvestigatorIds(doc.testSetup,investigatorCatalog));
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
 $('#apply-overrides').onclick=()=>{try{const team=JSON.parse($('[data-json="override-team"]').value),inventory=JSON.parse($('[data-json="override-inventory"]').value),resources=JSON.parse($('[data-json="override-resources"]').value);if(!Array.isArray(team)||team.some(i=>!Number.isFinite(i.health)||i.health<0))throw Error('Health must be nonnegative');validateRuntime({investigators:team,inventory,resources});runtime={...runtime,investigators:team,inventory,resources,log:[...runtime.log,'Test-only overrides applied.']};prepared={};pickedCard=null;problem='';renderSetup()}catch(e){problem='Overrides not applied: '+e.message}showStatus()};
}
function opportunityMarkup(o,result,historyEntry=null,context=null){
 const d=context?.definition||runDefinition,s=context?{investigators:[]}:runtime,interactive=result===undefined,entry=context?null:prepared[o.id],reason=interactive?(context?'Not prepared':preparationReason(d,s,o,entry)):'',slots=requiredSlots(o);
 const slotted=interactive?slots.map((slot,index)=>context?devPreviewSlot(slot):slotMarkup(o,slot,index)).join(''):historyEntry?slots.map(slot=>historicalSlotMarkup(slot,historyEntry)).join(''):'';
 const showCheck=o.check&&(interactive||Number.isFinite(result?.total));
 const requirements=[...Object.entries(o.requires.skills).map(([name,value])=>name+' '+value+'+'),...o.requires.traits];
 const mechanics=(showCheck?'<div class="mechanic-check"><span class="mechanic-label">'+(interactive?'Skill check':'Check result')+'</span>'+checkDisplay(o,interactive?s.investigators.find(i=>i.id===entry?.investigator):null,GAME_RULES,result?.total??null)+'</div>':'')
  +(requirements.length?'<div class="mechanic-detail"><span class="mechanic-label">Requires</span><span>'+esc(requirements.join(' · '))+'</span></div>':'')
  +(o.deadline!==null?'<div class="mechanic-detail"><span class="mechanic-label">Deadline</span><span>Through turn '+o.deadline+'</span></div>':'')
  +(slotted?'<div class="assignment-group"><span class="mechanic-label">'+(interactive?'Assign':'Assigned')+'</span><div class="opportunity-slots '+(interactive?'':'history-slots')+'">'+slotted+'</div></div>':'');
 return '<article class="card opportunity-card '+(interactive?(!reason?'is-ready':''):result?(result.passed?'is-passed':'is-failed'):'is-unused')+'" data-opportunity="'+o.id+'" aria-label="'+esc(o.title)+': '+esc(interactive?(reason||'Ready'):result?(result.passed?'Passed':'Failed'):'Not chosen')+'">'
  +'<div class="opportunity-scene"><p class="scene-copy">'+esc(o.body)+'</p></div>'
  +(mechanics?'<div class="opportunity-footer">'+mechanics+'</div>':'')
  +(result?'<section class="outcome-section" aria-label="Outcome"><span class="mechanic-label">Outcome</span><p class="scene-copy opportunity-outcome">'+esc(result.text)+'</p></section>':'')
  +(historyEntry?inventoryChangeMarkup(historyEntry.events):'')+'</article>';
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
 return '<section class="inventory-change-section" aria-label="Inventory changes"><span class="mechanic-label">Inventory changes</span><div class="inventory-changes">'+changes.map(change=>'<div class="item-card inventory-change '+(change.delta>0?'item-gained':'item-spent')+'" aria-label="'+esc(change.name+': '+change.before+' to '+change.after)+'"><span class="item-name">'+esc(change.name)+'</span><strong>'+(change.delta>0?'+':'')+change.delta+'</strong><span class="item-quantity">x'+change.before+' → x'+change.after+'</span></div>').join('')+'</div></section>';
}
function flowGraph(){return turnFlow(runDefinition,turnHistory,runtime)}
function actionHeadingMarkup(title){return '<span class="action-title">'+esc(title)+'</span><svg class="disclosure-icon" viewBox="0 0 16 16" aria-hidden="true"><path d="m5 3 5 5-5 5"/></svg>'}
function flowNodeMarkup(node){
 const {opportunity:o,entry,current}=node,events=entry.events||[],outcome=events.find(event=>event.outcome!=='expired')||events[0];
 const result=outcome?{...outcome,text:events.map(event=>event.text).filter(Boolean).join('\n')}:null;
 const expanded=expandedFlowNodes.has(node.id),state=current?(preparationReason(runDefinition,runtime,o,prepared[o.id])?'is-active':'is-ready'):result?(result.passed?'is-passed':'is-failed'):'is-unused';
 const panel=expanded?opportunityMarkup(o,current?undefined:result,current?null:entry):'';
 return '<div class="flow-node '+state+'" data-flow-id="'+esc(node.id)+'"><button type="button" class="flow-toggle" data-flow-toggle="'+esc(node.id)+'" aria-expanded="'+expanded+'" aria-controls="flow-panel-'+esc(node.id)+'">'+actionHeadingMarkup(o.title)+'</button><div id="flow-panel-'+esc(node.id)+'" class="flow-panel" '+(expanded?'':'hidden')+'>'+panel+'</div></div>';
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
 return '<section class="flow-node opening-card"><button type="button" class="flow-toggle" data-flow-toggle="case-opening" aria-expanded="'+expanded+'" aria-controls="flow-panel-case-opening" aria-label="'+esc(runDefinition.title)+'"><span class="briefing-heading"><span class="mechanic-label" aria-hidden="true">Case briefing</span><span class="briefing-title">'+esc(runDefinition.title)+'</span></span><svg class="disclosure-icon" viewBox="0 0 16 16" aria-hidden="true"><path d="m5 3 5 5-5 5"/></svg></button><div id="flow-panel-case-opening" class="flow-panel opening-copy" '+(expanded?'':'hidden')+'><p class="scene-copy">'+esc(runDefinition.opening)+'</p></div></section>';
}
function flowMarkup(){
 const {columns}=flowGraph(),ending=runDefinition.endings.find(ending=>ending.id===runtime.ending);
 return '<div class="case-flow-scroll" tabindex="0" aria-label="Case stage. Drag to pan."><div id="case-flow" class="case-flow"><svg id="flow-lines" class="flow-lines" aria-label="Opportunity unlock history"></svg>'+columns.map((column,index)=>'<section class="flow-column '+(column.current?'current-turn':'past-turn')+'" aria-label="Turn '+column.number+'" '+(column.current?'data-current-turn':'')+'>'+(index===0?openingCardMarkup():'')+column.entries.map(entry=>flowNodeMarkup({id:index+':'+entry.opportunity,opportunity:runDefinition.opportunities.find(o=>o.id===entry.opportunity),entry,current:!!column.current})).join('')+(column.current?(ending?'<article class="card ending-card"><h3>'+esc(ending.title)+'</h3><p>'+esc(ending.body)+'</p></article>':!column.entries.length?'<p class="muted">No opportunities. Complete a turn to wait.</p>':''):'')+(column.current?'<button id="complete-turn" type="button">Complete turn</button>':'')+'</section>').join('')+'</div></div>';
}
function bindStagePan(stage){
 let gesture=null,suppressClick=false;
 stage.onpointerdown=event=>{
  if(!event.isPrimary||event.button!==0||event.target.closest('[data-card],[data-drop-op],[data-dev-editor],input,textarea,select,a'))return;
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
 if(!runtime){try{validate(doc)}catch(e){$('#preview').innerHTML='<p>Fix the definition before testing: '+esc(e.message)+'</p>';return}runDefinition=structuredClone(doc);runtime=start(runDefinition,{...doc.testSetup,investigators:testRoster(doc.testSetup,investigatorCatalog)});prepared={};pickedCard=null;boardMessage='';expandedFlowNodes=new Set(['case-opening']);turnHistory=[];runSignature=JSON.stringify(doc)}
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
async function load(id){
 selectedDevOpportunity=null;const r=await fetch('/api/cases/'+id);if(!r.ok)throw Error('Cannot load case');const body=await r.json();doc=body.case;doc.testSetup??=defaultTestSetup();revision=body.revision;raw={};dirty=false;conflict=false;problem='';try{const text=localStorage.getItem(key(id));if(text){const draft=JSON.parse(text);if(draft.doc.id!==id)throw Error('Draft ID mismatch');doc=draft.doc;raw=draft.raw||{};dirty=true;conflict=draft.revision!==revision;revision=conflict?draft.revision:revision;problem=conflict?'Recovered browser draft conflicts with the disk file. Compare in Dev before replacing.':'Recovered unsaved browser draft.'}}catch(e){problem='Draft recovery: '+e.message}runtime=null;prepared={};pickedCard=null;generation++;$('#case-select').value=id;renderDev();if(mode==='preview')renderPreview();if(mode==='setup')renderSetup();showStatus();if(dirty&&!conflict&&!Object.keys(raw).length)save()}
$('#case-select').onchange=async()=>{const next=$('#case-select').value;await save();if(dirty||saving){$('#case-select').value=doc.id;problem='Save or resolve this draft before switching cases';showStatus();return}try{await load(next)}catch(e){problem=e.message;showStatus()}};
for(const m of ['dev','preview','setup'])$('#'+m+'-mode').onclick=()=>{resetTrayDragVisual();mode=m;if(m!=='dev')selectedDevOpportunity=null;document.body.classList.toggle('preview-active',m==='preview');document.body.classList.toggle('dev-active',m==='dev');$('#investigator-tray').hidden=m!=='preview';$('#restart').hidden=m!=='preview';$('#dev').hidden=m!=='dev';$('#preview').hidden=m!=='preview';$('#test-setup').hidden=m!=='setup';for(const n of ['dev','preview','setup']){const tab=$('#'+n+'-mode');tab.setAttribute('aria-selected',String(n===m));tab.tabIndex=n===m?0:-1;}if(m==='preview')renderPreview();else if(m==='setup')renderSetup();else renderDev()};
$('#retry').onclick=()=>{problem='';if(dirty)save();else checkServer()};$('#download').onclick=()=>{const a=document.createElement('a');a.href=URL.createObjectURL(new Blob([JSON.stringify({doc,revision,raw},null,2)],{type:'application/json'}));a.download=(doc?.id||'case')+'-draft.json';a.click();setTimeout(()=>URL.revokeObjectURL(a.href),1000)};
window.addEventListener('beforeunload',e=>{if(dirty){e.preventDefault();e.returnValue=''}});window.addEventListener('online',()=>{if(dirty&&!conflict)save()});
setInterval(checkServer,10000);
wireTabKeys($('.primary-tabs'));
document.body.classList.add('dev-active');
try{await loadCatalog();const cases=await list();if(cases.length)await load(cases[0].id);else{problem='No cases found. Add a case JSON file in cases/.';showStatus()}}catch(e){serverProblem='Server unavailable — changes are not saving.';problem='Cannot open Case Builder: '+e.message;showStatus()}

const trayLayoutObserver=new ResizeObserver(()=>{document.documentElement.style.setProperty('--inventory-tray-height',$('#investigator-tray').getBoundingClientRect().height+'px')});
trayLayoutObserver.observe($('#investigator-tray'));
