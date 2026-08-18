export function AccessDeniedPage() {
  return (
    <main className="system-page">
      <div>
        <p className="eyebrow">Access denied</p>
        <h1>You do not have permission to view this page</h1>
        <p>Return to your dashboard or choose another local development user.</p>
        <a className="button" href="/login">Return to sign in</a>
      </div>
    </main>
  );
}
