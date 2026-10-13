const {test}=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs/promises'),path=require('node:path');
const {createReviewServer}=require('./review-server.cjs');
test('current statuses and notes save and reload without touching real reviews',async t=>{
 const base=path.join(__dirname,'.test-tmp');await fs.mkdir(base,{recursive:true});const root=await fs.mkdtemp(path.join(base,'local-smoke-'));
 for(const name of ['ranked_pitches.json','report.html','report.md','review.js','review.css'])await fs.copyFile(path.join(__dirname,name),path.join(root,name));
 await fs.writeFile(path.join(root,'reviews.json'),JSON.stringify({format:'top-expert-reviews',version:2,records:[]}));
 const {server}=createReviewServer(root);await new Promise(resolve=>server.listen(0,'127.0.0.1',resolve));
 t.after(async()=>{await new Promise(resolve=>server.close(resolve));assert.ok(path.resolve(root).startsWith(path.resolve(base)+path.sep));await fs.rm(root,{recursive:true})});
 const url='http://127.0.0.1:'+server.address().port;
 assert.equal((await fetch(url)).status,200);
 for(const status of ['unreviewed','rejected','accepted']){
  const record={id:'C01-P01',status,notes:'Fixture note — preserved'};
  assert.equal((await fetch(url+'/api/reviews',{method:'PUT',headers:{'Content-Type':'application/json'},body:JSON.stringify(record)})).status,200);
  assert.deepEqual((await (await fetch(url+'/api/reviews')).json()).records,[record]);
  assert.deepEqual(JSON.parse(await fs.readFile(path.join(root,'reviews.json'),'utf8')).records,[record]);
 }
});
