const {test}=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs/promises'),path=require('node:path');
const {createReviewServer}=require('./review-server.cjs');
test('current statuses and notes save and reload without touching real reviews',async t=>{
 const base=path.join(__dirname,'.test-tmp');await fs.mkdir(base,{recursive:true});const root=await fs.mkdtemp(path.join(base,'local-smoke-'));
 for(const name of ['pitches.json','report.html','report.md','review.js','review.css'])await fs.copyFile(path.join(__dirname,name),path.join(root,name));
 await fs.writeFile(path.join(root,'reviews.json'),JSON.stringify({format:'top-expert-reviews',version:2,records:[]}));
 const {server}=createReviewServer(root);await new Promise(resolve=>server.listen(0,'127.0.0.1',resolve));
 t.after(async()=>{await new Promise(resolve=>server.close(resolve));assert.ok(path.resolve(root).startsWith(path.resolve(base)+path.sep));await fs.rm(root,{recursive:true})});
 const url='http://127.0.0.1:'+server.address().port;
 assert.equal((await fetch(url)).status,200);
 const pitches=JSON.parse(await fs.readFile(path.join(root,'pitches.json'),'utf8'));
 const round2=pitches.concepts.filter(c=>c.round===2);assert.equal(round2.length,10);assert.ok(round2.every(c=>c.pitches.length===5));
 assert.equal(new Set(pitches.concepts.flatMap(c=>c.pitches.map(p=>p.id))).size,pitches.concepts.reduce((n,c)=>n+c.pitches.length,0));
 for(const id of ['C01-P01','R2-C06-P01'])for(const status of ['unreviewed','rejected','accepted']){
  const record={id,status,notes:'Fixture note — preserved'};
  assert.equal((await fetch(url+'/api/reviews',{method:'PUT',headers:{'Content-Type':'application/json'},body:JSON.stringify(record)})).status,200);
  assert.deepEqual((await (await fetch(url+'/api/reviews')).json()).records.find(r=>r.id===id),record);
  assert.deepEqual(JSON.parse(await fs.readFile(path.join(root,'reviews.json'),'utf8')).records.find(r=>r.id===id),record);
 }
});
