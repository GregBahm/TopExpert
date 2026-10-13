const STATUSES = ['unreviewed', 'rejected', 'accepted'];
function validateRecord(record) {
  if (!record || typeof record.id !== 'string' || !record.id || typeof record.notes !== 'string' || record.notes.length > 10000) throw Error('Invalid review record.');
  if (!STATUSES.includes(record.status) || Object.hasOwn(record, 'selected')) throw Error('Invalid review status.');
  return record;
}
function validateDocument(data) {
  if (!data || data.format !== 'top-expert-reviews' || data.version !== 2 || !Array.isArray(data.records)) throw Error('Invalid reviews.json.');
  const records = data.records.map(validateRecord);
  if (new Set(records.map(r => r.id)).size !== records.length) throw Error('Duplicate review IDs; original data was not changed.');
  return data;
}
module.exports = {STATUSES, validateRecord, validateDocument};
