import { sqliteTable, text, integer, primaryKey } from 'drizzle-orm/sqlite-core';

export const pitchReviews = sqliteTable('pitch_reviews', {
  userId: text('user_id').notNull(),
  pitchId: text('pitch_id').notNull(),
  selected: integer('selected', { mode: 'boolean' }).notNull().default(false),
  notes: text('notes').notNull().default(''),
  updatedAt: text('updated_at').notNull(),
}, table => [primaryKey({ columns: [table.userId, table.pitchId] })]);
