import { reportHtml } from './report';
export const dynamic = 'force-dynamic';
export function GET() {
  return new Response(reportHtml, { headers: { 'Content-Type': 'text/html; charset=utf-8', 'Cache-Control': 'no-store' } });
}
