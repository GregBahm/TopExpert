const {test} = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs/promises');
const os = require('node:os');
const path = require('node:path');
const http = require('node:http');
const {createReviewServer} = require('./review-server.cjs');

async function fixture(t) {
  const root = await fs.mkdtemp(path.join(os.tmpdir(), 'top-expert-review-test-'));
  for (const name of ['ranked_pitches.json', 'report.html', 'report.md']) {
    await fs.copyFile(path.join(__dirname, name), path.join(root, name));
  }
  // Test data must never inherit someone's real reviews.
  await fs.writeFile(path.join(root, 'reviews.json'), JSON.stringify({format: 'top-expert-reviews', version: 1, records: []}));
  const servers = [];
  t.after(async () => {
    for (const server of servers) await new Promise(resolve => server.close(resolve));
    await fs.rm(root, {recursive: true});
  });
  async function start() {
    const {server} = createReviewServer(root);
    await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
    servers.push(server);
    return {server, url: `http://127.0.0.1:${server.address().port}`};
  }
  return {root, start, ...await start()};
}

const put = (url, record, headers = {}) => fetch(url + '/api/reviews', {
  method: 'PUT', headers: {'Content-Type': 'application/json', ...headers}, body: JSON.stringify(record),
});
const read = async url => (await fetch(url + '/api/reviews')).json();

test('saves selections and unselected notes to disk, reloads after restart, and clears records', async t => {
  const {root, url, server, start} = await fixture(t);
  const records = [
    {id: 'C01-P05', selected: true, notes: 'Chosen pitch'},
    {id: 'C02-P04', selected: false, notes: 'Notes without selecting \u2014 \ud83d\udca1'},
  ];
  for (const record of records) assert.equal((await put(url, record)).status, 200);
  const disk = JSON.parse(await fs.readFile(path.join(root, 'reviews.json'), 'utf8'));
  assert.deepEqual(disk.records, records);
  assert.deepEqual((await read(url)).records, records);
  assert.equal((await read(url)).storage, 'file');
  await new Promise(resolve => server.close(resolve));
  const restarted = await start();
  assert.deepEqual((await read(restarted.url)).records, records);
  assert.equal((await put(restarted.url, {id: 'C01-P05', selected: false, notes: ''})).status, 200);
  assert.deepEqual((await read(restarted.url)).records, [records[1]]);
});

test('serializes concurrent writes, retains unrelated records and reads external file updates', async t => {
  const {root, url} = await fixture(t);
  const records = Array.from({length: 10}, (_, i) => ({
    id: `C01-P${String(i + 1).padStart(2, '0')}`, selected: i % 2 === 0, notes: `Note ${i}`,
  }));
  const responses = await Promise.all(records.map(record => put(url, record)));
  assert.ok(responses.every(response => response.status === 200));
  assert.deepEqual((await read(url)).records, records);
  const external = {id: 'C02-P01', selected: true, notes: 'Edited on disk'};
  await fs.writeFile(path.join(root, 'reviews.json'), JSON.stringify({format: 'top-expert-reviews', version: 1, records: [external]}));
  await put(url, records[0]);
  assert.deepEqual((await read(url)).records, [records[0], external]);
  assert.ok(!(await fs.readdir(root)).some(name => name.endsWith('.tmp')));
});

test('rejects invalid inputs without modifying the file', async t => {
  const {root, url} = await fixture(t);
  const before = await fs.readFile(path.join(root, 'reviews.json'), 'utf8');
  for (const record of [
    null,
    {id: 'C99-P01', selected: true, notes: ''},
    {id: 'C01-P01', selected: 1, notes: ''},
    {id: 'C01-P01', selected: false, notes: 'x'.repeat(10001)},
  ]) {
    assert.equal((await put(url, record)).status, 400);
  }
  assert.equal((await put(url, {id: 'C01-P01', selected: false, notes: 'x'.repeat(10000)})).status, 200);
  await fs.writeFile(path.join(root, 'reviews.json'), before);
  const malformed = await fetch(url + '/api/reviews', {method: 'PUT', headers: {'Content-Type': 'application/json'}, body: '{'});
  assert.equal(malformed.status, 400);
  assert.equal((await put(url, {}, {'Content-Type': 'text/plain'})).status, 415);
  assert.equal((await put(url, {notes: 'x'.repeat(100001)})).status, 413);
  assert.equal(await fs.readFile(path.join(root, 'reviews.json'), 'utf8'), before);
});

test('corrupt storage and write failures are explicit, preserve the original and allow retry', async t => {
  const {root, url} = await fixture(t);
  const filename = path.join(root, 'reviews.json');
  await fs.writeFile(filename, 'invalid json');
  assert.equal((await fetch(url + '/api/reviews')).status, 500);
  const record = {id: 'C01-P01', selected: true, notes: 'Preserve this'};
  assert.equal((await put(url, record)).status, 500);
  assert.equal(await fs.readFile(filename, 'utf8'), 'invalid json');
  await fs.writeFile(filename, JSON.stringify({format: 'top-expert-reviews', version: 1, records: [record, record]}));
  assert.equal((await fetch(url + '/api/reviews')).status, 500);
  await fs.writeFile(filename, JSON.stringify({format: 'top-expert-reviews', version: 1, records: []}));
  await fs.rename(filename, filename + '.original');
  await fs.mkdir(filename);
  assert.equal((await put(url, record)).status, 500);
  await fs.rmdir(filename);
  await fs.rename(filename + '.original', filename);
  assert.equal((await put(url, record)).status, 200);
  assert.deepEqual((await read(url)).records, [record]);
  const saved = await fs.readFile(filename, 'utf8');
  const rename = t.mock.method(fs, 'rename', async () => {throw Error('Simulated atomic replacement failure');});
  assert.equal((await put(url, {...record, notes: 'Not yet saved'})).status, 500);
  rename.mock.restore();
  assert.equal(await fs.readFile(filename, 'utf8'), saved);
  assert.ok(!(await fs.readdir(root)).some(name => name.endsWith('.tmp')));
  assert.equal((await put(url, {...record, notes: 'Retried successfully'})).status, 200);
  assert.equal((await read(url)).records[0].notes, 'Retried successfully');
});

test('serves only report assets and refuses foreign origins and hosts', async t => {
  const {url} = await fixture(t);
  const page = await fetch(url);
  assert.equal(page.status, 200);
  assert.match(await page.text(), /Pitch Review/);
  assert.equal((await fetch(url + '/reviews.json')).status, 404);
  assert.equal((await fetch(url + '/review-server.cjs')).status, 404);
  assert.equal((await fetch(url + '/api/reviews', {headers: {Origin: 'https://example.com'}})).status, 403);
  const status = await new Promise((resolve, reject) => {
    http.get(url + '/api/reviews', {headers: {Host: 'example.com'}}, response => {
      response.resume();
      resolve(response.statusCode);
    }).on('error', reject);
  });
  assert.equal(status, 403);
});
