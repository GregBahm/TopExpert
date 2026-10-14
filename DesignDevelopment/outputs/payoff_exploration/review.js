(async()=>{
const response=await fetch('/pitches.json');if(!response.ok)throw Error('Could not load pitches');const data=await response.json();
const esc=s=>String(s).replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const options='<option value="unreviewed">Unreviewed</option><option value="rejected">Reviewed and rejected</option><option value="accepted">Reviewed and accepted</option>';
function pitch(p,c){return '<article class="pitch" data-round="'+c.round+'" data-id="'+p.id+'"><p class="premise">'+c.number+'. '+esc(c.title)+' — '+esc(c.description)+'</p><div class="pitch-head"><h3>'+esc(p.title)+'</h3><select data-status="'+p.id+'" aria-label="Review status for '+p.id+'" disabled>'+options+'</select></div><p>'+esc(p.text)+'</p>'+(p.adjustment==='None'?'':'<p class="adjustment">Premise adjustment: '+esc(p.adjustment)+'</p>')+'<div class="note-box" hidden><label>Notes<textarea data-notes="'+p.id+'" aria-label="Notes for '+p.id+'" rows="3" maxlength="10000" placeholder="Your notes…" disabled></textarea></label></div><span class="pitch-id">'+p.id+'</span></article>'}
document.querySelector('#cases').innerHTML=[...data.concepts].sort((a,b)=>a.number-b.number).flatMap(c=>[...c.pitches].sort((a,b)=>a.id.localeCompare(b.id)).map(p=>pitch(p,c))).join('');

const byId=new Map(data.concepts.flatMap(c=>c.pitches.map(p=>[p.id,{...p,conceptNumber:c.number,conceptTitle:c.title,originalPrompt:c.description,round:c.round}])));
const rows=[...document.querySelectorAll('.pitch')],nodes=new Map();rows.forEach(el=>nodes.set(el.dataset.id,[el]));

const status=document.querySelector('#save-status'),retry=document.querySelector('#retry'),filters=[...document.querySelectorAll('[data-filter]')];

let activeRound=2;
let reviews={},dirty={},loaded=false,saving=false,saveTimer,lastError='';
const emptyReview=()=>({status:'unreviewed',notes:''});
const current=id=>reviews[id]||emptyReview();
function validRecord(id,r){return byId.has(id)&&r&&['unreviewed','rejected','accepted'].includes(r.status)&&!Object.hasOwn(r,'selected')&&typeof r.notes==='string'&&r.notes.length<=10000}
function showStatus(){retry.hidden=!lastError;retry.textContent=loaded?'Retry saving':'Retry loading';status.textContent=lastError||(!loaded?'Loading saved reviews…':saving||Object.keys(dirty).length?'Saving to reviews.json…':'Saved to reviews.json')}
function sync(id){const r=current(id);for(const el of nodes.get(id)){el.querySelector('[data-status]').value=r.status;const ta=el.querySelector('[data-notes]');if(ta.value!==r.notes)ta.value=r.notes;el.querySelector('.note-box').hidden=r.status==='unreviewed'}}
function syncAll(){for(const id of byId.keys())sync(id)}
function updateCounts(){const totals={unreviewed:0,rejected:0,accepted:0};for(const [id,p] of byId)if(p.round===activeRound)totals[current(id).status]++;for(const el of filters)el.parentElement.querySelector('[data-status-count]').textContent='('+totals[el.dataset.filter]+')'}
function filter(){updateCounts();const included=new Set(filters.filter(el=>el.checked).map(el=>el.dataset.filter));let visible=0;for(const row of rows){row.hidden=Number(row.dataset.round)!==activeRound||!included.has(current(row.dataset.id).status);if(!row.hidden)visible++}document.querySelector('#count').textContent=visible+' of '+[...byId.values()].filter(p=>p.round===activeRound).length+' ideas';document.querySelector('#empty').hidden=visible!==0}

function update(id,patch){reviews[id]={...current(id),...patch};dirty[id]={...reviews[id]};sync(id);showStatus();if('status' in patch){updateCounts();const row=nodes.get(id)[0];if(document.activeElement&&row.contains(document.activeElement)){row.addEventListener('focusout',()=>setTimeout(()=>{if(!row.contains(document.activeElement))filter()},0),)}else filter()};clearTimeout(saveTimer);saveTimer=setTimeout(flush,350)}
async function flush(){if(!loaded||saving||!Object.keys(dirty).length)return;saving=true;lastError='';showStatus();try{while(Object.keys(dirty).length){const id=Object.keys(dirty)[0],snapshot={...dirty[id]};const response=await fetch('/api/reviews',{method:'PUT',headers:{'Content-Type':'application/json'},body:JSON.stringify({id,...snapshot})});if(!response.ok)throw Error('Not saved to reviews.json. Check the server and retry');if(JSON.stringify(dirty[id])===JSON.stringify(snapshot))delete dirty[id];showStatus()}}catch(error){lastError=error.message||'Save failed'}finally{saving=false;showStatus();if(!lastError&&Object.keys(dirty).length)flush()}}
async function load(){lastError='';showStatus();try{const response=await fetch('/api/reviews',{cache:'no-store'});if(!response.ok)throw Error('Could not load reviews.json');const body=await response.json();reviews=Object.fromEntries(body.records.filter(r=>validRecord(r.id,r)).map(r=>[r.id,r]));loaded=true;document.querySelectorAll('[data-status],[data-notes]').forEach(el=>el.disabled=false);syncAll();filter();showStatus()}catch{lastError='Open http://127.0.0.1:8765 and check the local server or reviews.json, then retry.';showStatus()}}
document.querySelectorAll('[data-status]').forEach(el=>el.addEventListener('change',()=>update(el.dataset.status,{status:el.value})));
document.querySelectorAll('[data-notes]').forEach(el=>el.addEventListener('input',()=>update(el.dataset.notes,{notes:el.value})));
filters.forEach(el=>el.addEventListener('change',filter));
document.querySelectorAll('[data-round-tab]').forEach(el=>el.addEventListener('click',()=>{activeRound=Number(el.dataset.roundTab);document.querySelectorAll('[data-round-tab]').forEach(tab=>tab.setAttribute('aria-selected',String(Number(tab.dataset.roundTab)===activeRound)));filter()}));

retry.addEventListener('click',()=>{if(loaded){lastError='';flush()}else load()});
const toolbar=document.querySelector('.toolbar');new ResizeObserver(()=>document.documentElement.style.setProperty('--toolbar-height',toolbar.getBoundingClientRect().height+'px')).observe(toolbar);
window.addEventListener('beforeunload',e=>{if(Object.keys(dirty).length){e.preventDefault();e.returnValue=''}});
load();

})().catch(error=>document.querySelector('#save-status').textContent=error.message);
