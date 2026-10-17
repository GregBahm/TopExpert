const fs = require('node:fs/promises');
const http = require('node:http');
const path = require('node:path');
const {randomUUID} = require('node:crypto');
const {validateRecord, validateDocument} = require('./review-schema.cjs');

function createReviewServer(root = __dirname) {
  const reviewPath = path.join(root, 'reviews.json');
  let writes = Promise.resolve();
  async function validRecord(record) {
    let id;
    try { id = validateRecord(record).id; } catch { return false; }
    const pitches = JSON.parse(await fs.readFile(path.join(root, 'pitches.json'), 'utf8'));
    return pitches.concepts.some(c => c.pitches.some(p => p.id === id));
  }

  async function readReviews() {
    return validateDocument(JSON.parse(await fs.readFile(reviewPath, 'utf8')));
  }

  function saveRecord(record) {
    const write = writes.then(async () => {
      const data = await readReviews();
      const records = new Map(data.records.map(r => [r.id, r]));
      const previous = records.get(record.id);
      const merged = {...previous, ...record};
      const hasExtraData = Object.keys(merged).some(key => !['id', 'status', 'notes'].includes(key));
      if (merged.status !== 'unreviewed' || merged.notes || hasExtraData) records.set(record.id, merged);
      else records.delete(record.id);
      const payload = {...data, records: [...records.values()].sort((a, b) => a.id.localeCompare(b.id))};
      const temporary = reviewPath + '.' + randomUUID() + '.tmp';
      try {
        const file = await fs.open(temporary, 'wx');
        try {
          await file.writeFile(JSON.stringify(payload, null, 2) + '\n');
          await file.sync();
        } finally {
          await file.close();
        }
        await fs.rename(temporary, reviewPath);
      } finally {
        await fs.rm(temporary, {force: true});
      }
    });
    writes = write.catch(() => {});
    return write;
  }

  const files = {
    '/': ['report.html', 'text/html; charset=utf-8'],
    '/report.html': ['report.html', 'text/html; charset=utf-8'],
    '/pitches.json': ['pitches.json', 'application/json'],
    '/report.md': ['report.md', 'text/markdown; charset=utf-8'],
    '/review.js': ['review.js', 'text/javascript; charset=utf-8'],
    '/review.css': ['review.css', 'text/css; charset=utf-8'],
  };
  const server = http.createServer(async (req, res) => {
    const reply = (code, data) => {
      res.writeHead(code, {'Content-Type': 'application/json', 'Cache-Control': 'no-store'});
      res.end(JSON.stringify(data));
    };
    const authority = `127.0.0.1:${server.address().port}`;
    if (req.headers.host !== authority || (req.headers.origin && req.headers.origin !== `http://${authority}`)) {
      reply(403, {error: 'Use the local review server URL.'});
      return;
    }
    try {
      const url = new URL(req.url, `http://${authority}`);
      if (url.pathname === '/api/reviews') {
        if (req.method === 'GET') {
          await writes;
          const data = await readReviews();
          reply(200, {records: data.records});
        } else if (req.method === 'PUT') {
          if (req.headers['content-type'] !== 'application/json') {
            reply(415, {error: 'Expected application/json.'});
            return;
          }
          const chunks = [];
          let size = 0;
          for await (const chunk of req) {
            size += chunk.length;
            if (size > 100000) {
              reply(413, {error: 'Review entry is too large.'});
              return;
            }
            chunks.push(chunk);
          }
          let record;
          try {
            record = JSON.parse(Buffer.concat(chunks).toString('utf8'));
          } catch {
            reply(400, {error: 'Invalid JSON.'});
            return;
          }
          if (!await validRecord(record)) {
            reply(400, {error: 'Invalid pitch ID, status, or notes (maximum 10000 characters).'});
            return;
          }
          await saveRecord(record);
          reply(200, {saved: true});
        } else {
          reply(405, {error: 'Method not allowed.'});
        }
      } else if (req.method === 'GET' && files[url.pathname]) {
        const [name, type] = files[url.pathname];
        const body = await fs.readFile(path.join(root, name));
        res.writeHead(200, {'Content-Type': type, 'Cache-Control': 'no-store'});
        res.end(body);
      } else {
        reply(404, {error: 'Not found.'});
      }
    } catch (error) {
      console.error('Review server:', error.message);
      reply(500, {error: 'Could not read or save reviews.json. Check the server terminal; your edit is not saved.'});
    }
  });
  return {server, readReviews};
}

if (require.main === module) {
  const {server, readReviews} = createReviewServer();
  readReviews().then(() => {
    server.on('error', error => {
      console.error('Could not start review server:', error.message);
      process.exitCode = 1;
    });
    server.listen(8765, '127.0.0.1', () => {
      console.log('Pitch review: http://127.0.0.1:8765');
      console.log(`Review statuses and notes automatically save to ${path.join(__dirname, 'reviews.json')}`);
      console.log('Wait for "Saved to reviews.json" before closing the page. Press Ctrl+C to stop.');
    });
  }).catch(error => {
    console.error('Could not load reviews.json:', error.message);
    process.exitCode = 1;
  });
}

module.exports = {createReviewServer};
