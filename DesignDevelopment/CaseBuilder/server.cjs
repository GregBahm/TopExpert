const http=require('node:http'),fs=require('node:fs/promises'),path=require('node:path'),{randomUUID,createHash}=require('node:crypto');
const root=__dirname,caseDir=path.join(root,'cases');
const hash=s=>createHash('sha256').update(s).digest('hex');
async function createServer(base=root,reviewRoot=path.resolve(root,'../outputs/payoff_exploration')){
 const {validate}=await import('./engine.mjs');const dir=path.join(base,'cases');await fs.mkdir(dir,{recursive:true});let writes=Promise.resolve();
 const server=http.createServer(async(req,res)=>{
 const reply=(code,data)=>{res.writeHead(code,{'Content-Type':'application/json','Cache-Control':'no-store'});res.end(JSON.stringify(data))};
 const authority='127.0.0.1:'+server.address().port;if(req.headers.host!==authority||(req.headers.origin&&req.headers.origin!=='http://'+authority))return reply(403,{error:'Use the local Case Builder URL'});
 try{const url=new URL(req.url,'http://'+authority);if(url.pathname==='/api/cases'&&req.method==='GET'){await writes;const list=[];for(const name of (await fs.readdir(dir)).filter(n=>/^[a-z0-9-]+\.json$/.test(n))){const d=JSON.parse(await fs.readFile(path.join(dir,name),'utf8'));list.push({id:d.id,title:d.title})}return reply(200,list)}
 const sourceMatch=url.pathname.match(/^\/api\/source-pitches\/([A-Za-z0-9-]+)$/);
 if(req.method==='GET'&&url.pathname.startsWith('/assets/investigators/')){
   const name=decodeURIComponent(url.pathname.slice('/assets/investigators/'.length));
   const assetDir=path.join(base,'assets','investigators');
   const portraits=JSON.parse(await fs.readFile(path.join(assetDir,'portraits.json'),'utf8'));
   if(!['portraits.json','catalog.json'].includes(name)&&!portraits.some(p=>p.file===name))return reply(404,{error:'Not found'});
   if(path.basename(name)!==name)return reply(404,{error:'Not found'});
   const body=await fs.readFile(path.join(assetDir,name));res.writeHead(200,{'Content-Type':name.endsWith('.json')?'application/json':'image/png','Cache-Control':'no-store'});res.end(body);return;
 }
 if(sourceMatch&&req.method==='GET'){
   const [pitches,reviews]=await Promise.all(['pitches.json','reviews.json'].map(async name=>JSON.parse(await fs.readFile(path.join(reviewRoot,name),'utf8'))));
   const concept=pitches.concepts.find(c=>c.pitches.some(p=>p.id===sourceMatch[1]));
   if(!concept)return reply(404,{error:'Source pitch not found'});
   const pitch=concept.pitches.find(p=>p.id===sourceMatch[1]),review=reviews.records.find(r=>r.id===pitch.id);
   return reply(200,{id:pitch.id,premise:{number:concept.number,title:concept.title,description:concept.description},pitch:{title:pitch.title,text:pitch.text,adjustment:pitch.adjustment},notes:review?.notes??'',status:review?.status??'unreviewed'});
 }
 const match=url.pathname.match(/^\/api\/cases\/([a-z0-9-]+)$/);if(match){const file=path.join(dir,match[1]+'.json');if(req.method==='GET'){await writes;const text=await fs.readFile(file,'utf8');return reply(200,{case:JSON.parse(text),revision:hash(text)})}if(req.method==='PUT'){if(req.headers['content-type']!=='application/json')return reply(415,{error:'Expected JSON'});let body='';for await(const c of req){body+=c;if(Buffer.byteLength(body)>1000000)return reply(413,{error:'Case too large'})}let payload;try{payload=JSON.parse(body);validate(payload.case);if(payload.case.id!==match[1])throw Error('ID must match filename')}catch(e){return reply(400,{error:e.message})}
 const write=writes.then(async()=>{let old=null;try{old=await fs.readFile(file,'utf8')}catch(e){if(e.code!=='ENOENT')throw e}if((old===null?null:hash(old))!==payload.revision)return {conflict:true};const text=JSON.stringify(payload.case,null,2)+'\n',tmp=file+'.'+randomUUID()+'.tmp';try{const f=await fs.open(tmp,'wx');try{await f.writeFile(text);await f.sync()}finally{await f.close()}await fs.rename(tmp,file)}finally{await fs.rm(tmp,{force:true})}return {revision:hash(text)}});writes=write.catch(()=>{});const result=await write;return result.conflict?reply(409,{error:'File changed elsewhere. Draft kept; compare before replacing.'}):reply(200,result)}return reply(405,{error:'Method not allowed'})}
 const files={'/':'index.html','/app.mjs':'app.mjs','/engine.mjs':'engine.mjs','/test-setup.mjs':'test-setup.mjs','/turn-board.mjs':'turn-board.mjs','/turn-history.mjs':'turn-history.mjs','/style.css':'style.css'};if(req.method==='GET'&&files[url.pathname]){const name=files[url.pathname];res.writeHead(200,{'Content-Type':name.endsWith('.mjs')?'text/javascript':name.endsWith('.css')?'text/css':'text/html','Cache-Control':'no-store'});res.end(await fs.readFile(path.join(base,name)));return}reply(404,{error:'Not found'})
 }catch(e){reply(e.code==='ENOENT'?404:500,{error:e.message})}});return server;
}
if(require.main===module)createServer().then(server=>server.listen(8766,'127.0.0.1',()=>console.log('Case Builder: http://127.0.0.1:8766/\nCase files: '+caseDir))).catch(console.error);
module.exports={createServer};
