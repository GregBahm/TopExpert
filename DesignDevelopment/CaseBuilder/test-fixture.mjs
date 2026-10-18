// Synthetic bookkeeping data, independent of authored cases and portrait assets.
export const fixtureCatalog=[
 {id:'analyst',name:'Analyst',skills:{Focus:1},statsSpecified:true},
 {id:'helper',name:'Helper',skills:{Focus:0},statsSpecified:true},
 {id:'unspecified',name:'Unspecified',skills:{},statsSpecified:false},
];
export function effect(patch={}){return {text:'Fixture result',notes:'',inventory:{},resources:{},skills:{},health:0,flagsAdd:[],flagsRemove:[],unlock:[],remove:[],ending:null,...patch}}
export function opportunity(id,patch={}){return {
 id,title:id,body:'Fixture action',notes:'',kind:'investigator',initial:true,repeatable:false,duration:1,deadline:null,
 requires:{flags:[],notFlags:[],inventory:{},resources:{},skills:{},traits:[]},cost:{inventory:{},resources:{}},
 check:null,success:effect(),failure:effect(),expired:effect(),...patch,
}}
export function makeFixture(){return {
 version:1,id:'fixture-case',title:'Bookkeeping fixture',opening:'Fixture opening',notes:'Fixture notes',
 rule:{sides:6,multiplier:1,notes:'Test inputs, not balance specifications.'},
 investigators:fixtureCatalog.filter(i=>i.statsSpecified).map(i=>({id:i.id,name:i.name,skills:{...i.skills},health:3,traits:[],notes:''})),
 initial:{inventory:{},resources:{money:4},flags:[]},
 opportunities:[opportunity('inspect',{check:{skills:['Focus'],target:3,itemBonuses:{},notes:''},success:effect({flagsAdd:['found']})}),opportunity('assist')],endings:[],
}}
