import { env } from 'cloudflare:workers';
import { getChatGPTUser } from '../../chatgpt-auth';

export const dynamic = 'force-dynamic';
const reply = (body: unknown, status = 200) => Response.json(body, { status, headers: { 'Cache-Control': 'no-store' } });
const knownId = (id: unknown) => typeof id === 'string' && /^C(0[1-9]|[1-4][0-9]|50)-P(0[1-9]|10)$/.test(id);
async function identity() { const user = await getChatGPTUser(); return user?.userId; }

export async function GET() {
  const userId = await identity();
  if (!userId) return reply({ error: 'Sign in through this private Site to load reviews.' }, 401);
  if (!env.DB) return reply({ error: 'Review storage is unavailable.' }, 503);
  try {
    const result = await env.DB.prepare('SELECT pitch_id AS id, selected, notes, updated_at AS updatedAt FROM pitch_reviews WHERE user_id = ?').bind(userId).all();
    return reply({ userKey: userId, records: result.results.map((r: any) => ({ ...r, selected: Boolean(r.selected) })) });
  } catch { return reply({ error: 'Could not load saved reviews. Please retry.' }, 503); }
}

export async function PUT(request: Request) {
  const userId = await identity();
  if (!userId) return reply({ error: 'Sign in through this private Site to save reviews.' }, 401);
  const origin = request.headers.get('origin');
  if (origin && origin !== new URL(request.url).origin) return reply({ error: 'Invalid request origin.' }, 403);
  if (!env.DB) return reply({ error: 'Review storage is unavailable.' }, 503);
  if (Number(request.headers.get('content-length') || 0) > 100000) return reply({ error: 'Review is too large.' }, 413);
  let record: any;
  try { record = await request.json(); } catch { return reply({ error: 'Invalid review.' }, 400); }
  if (!knownId(record.id) || typeof record.selected !== 'boolean' || typeof record.notes !== 'string' || record.notes.length > 10000) return reply({ error: 'Invalid pitch ID or review fields.' }, 400);
  try {
    const updatedAt = new Date().toISOString();
    await env.DB.prepare('INSERT INTO pitch_reviews (user_id, pitch_id, selected, notes, updated_at) VALUES (?, ?, ?, ?, ?) ON CONFLICT(user_id, pitch_id) DO UPDATE SET selected = excluded.selected, notes = excluded.notes, updated_at = excluded.updated_at').bind(userId, record.id, record.selected ? 1 : 0, record.notes, updatedAt).run();
    return reply({ ok: true, id: record.id, updatedAt });
  } catch { return reply({ error: 'Could not save this review. Your draft remains in this browser.' }, 503); }
}
