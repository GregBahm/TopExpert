CREATE TABLE `pitch_reviews` (
	`user_id` text NOT NULL,
	`pitch_id` text NOT NULL,
	`selected` integer DEFAULT false NOT NULL,
	`notes` text DEFAULT '' NOT NULL,
	`updated_at` text NOT NULL,
	PRIMARY KEY(`user_id`, `pitch_id`)
);
