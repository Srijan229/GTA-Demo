# UI design system

## Direction

The interface should feel like a restrained university operations product: content-first, accessible, calm, and dense enough for real administrative work. It must not imply official GMU approval.

## Tokens

| Token | Value | Use |
|---|---:|---|
| `--color-brand-700` | `#005239` | Primary navigation and strong actions |
| `--color-brand-600` | `#006633` | Primary controls |
| `--color-brand-100` | `#dff2e8` | Selected and informational surfaces |
| `--color-gold-500` | `#ffbf3f` | Restrained accent and warning highlight |
| `--color-text` | `#17211d` | Primary text |
| `--color-text-muted` | `#52615a` | Secondary text |
| `--color-border` | `#cbd5cf` | Control and surface borders |
| `--color-surface` | `#ffffff` | Cards and forms |
| `--color-canvas` | `#f5f7f6` | Page background |
| `--color-danger` | `#b42318` | Destructive actions and errors |
| `--color-success` | `#137044` | Success state |

Colors are provisional and will be contrast-tested. Status meaning always includes text or an icon, never color alone.

Typography uses a system sans-serif stack. Body text defaults to 16px with a 1.5 line height. The spacing scale is based on 4px with common steps of 8, 12, 16, 24, 32, and 48px. Focus rings are visible, high contrast, and offset from controls.

## Components

Shared components will include the application shell, role navigation, page header, breadcrumbs, data table, filters, pagination, status badge, summary card, form field, confirmation dialog, document upload/card, completion indicator, activity timeline, and standard loading/empty/error states.

Components remain composable and semantic. Tables retain accessible headers and captions; responsive layouts may render structured cards without changing information or actions.

## Responsive model

- Mobile around 375px: navigation drawer, single-column forms, card-style collection fallback.
- Tablet around 768px: compact navigation and one/two-column layout as content allows.
- Laptop around 1280px: persistent sidebar and practical data tables.
- Large desktop 1440px+: bounded content widths; forms do not stretch excessively.

