import {start as startRuntime} from './engine.mjs';
import {testRoster} from './test-setup.mjs';
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
 version:2,id:'fixture-case',title:'Bookkeeping fixture',opening:'Fixture opening',notes:'Fixture notes',
 initialFlags:[],testSetup:{investigatorIds:['analyst','helper'],skillOverrides:{},inventory:{},resources:{money:4}},
 opportunities:[opportunity('inspect',{check:{skills:['Focus'],target:3,itemBonuses:{},notes:''},success:effect({flagsAdd:['found']})}),opportunity('assist')],endings:[],
}}

export function startFixture(d){return startRuntime(d,{...d.testSetup,investigators:testRoster(d.testSetup,fixtureCatalog)})}
