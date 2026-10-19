import {GAME_RULES} from './engine.mjs';
// A per-case testing preset. Investigator baselines come only from the catalog.
export function defaultTestSetup(){return {investigatorIds:[],skillOverrides:{},inventory:{},resources:{}}}
export function selectedInvestigatorIds(setup,catalog){return setup.investigatorIds.filter(id=>catalog.some(p=>p.id===id&&p.statsSpecified!==false))}
export function baseInvestigator(portrait){return {id:portrait.id,name:portrait.name,health:GAME_RULES.startingHealth,traits:structuredClone(portrait.traits||[]),skills:structuredClone(portrait.skills),notes:''}}
export function testInvestigator(setup,portrait){const base=baseInvestigator(portrait);if(portrait.statsSpecified===false)return base;return {...base,...structuredClone(setup.investigatorOverrides?.[portrait.id]||{}),skills:{...base.skills,...setup.skillOverrides[portrait.id]}}}
export function testRoster(setup,catalog){const selected=new Set(selectedInvestigatorIds(setup,catalog));return catalog.filter(p=>selected.has(p.id)).map(p=>testInvestigator(setup,p))}
