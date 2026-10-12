import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Top Expert · Pitch Review",
  description: "Review, select and annotate Top Expert case pitches.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body className="antialiased">{children}</body>
    </html>
  );
}
