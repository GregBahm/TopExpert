// Test configuration is saved per case; runtime overrides stay disposable.
export function selectedInvestigatorIds(definition,catalog){
 const ids=definition.testSetup?.investigatorIds??catalog.filter(p=>definition.investigators.some(i=>i.id===p.id||i.name===p.name)).map(p=>p.id);
 // Preserve stored configuration, but exclude explicitly unspecified baselines
 // from the effective tray and new-run roster.
 return ids.filter(id=>catalog.some(p=>p.id===id&&p.statsSpecified!==false));
}
export function baseInvestigatorForCase(definition,portrait){
 const existing=definition.investigators.find(i=>i.id===portrait.id||i.name===portrait.name);
 return {id:portrait.id,name:portrait.name,health:existing?.health??3,traits:structuredClone(existing?.traits??[]),skills:structuredClone(existing?.skills??portrait.skills),notes:existing?.notes??(portrait.statsSpecified?'Design document baseline. Health 3 is provisional.':'Stats not specified in design document; zero defaults for testing. Health 3 is provisional.')};
}
export function investigatorForCase(definition,portrait){const base=baseInvestigatorForCase(definition,portrait);if(portrait.statsSpecified===false)return base;return {...base,skills:{...base.skills,...definition.testSetup?.skillOverrides?.[portrait.id]}}}
export function testRoster(definition,catalog){const selected=new Set(selectedInvestigatorIds(definition,catalog));return catalog.filter(p=>selected.has(p.id)).map(p=>investigatorForCase(definition,p))}
