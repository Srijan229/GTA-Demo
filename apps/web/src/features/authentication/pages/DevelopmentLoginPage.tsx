import { useMutation, useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { createDevelopmentSession, getDevelopmentUsers, type DevelopmentUser } from '../api/authenticationApi';

function destinationFor(user: DevelopmentUser) {
  if (user.roles.includes('Administrator')) return '/admin';
  if (user.roles.includes('Faculty')) return '/faculty';
  return '/applicant';
}

export function DevelopmentLoginPage() {
  const navigate = useNavigate();
  const users = useQuery({ queryKey: ['development-users'], queryFn: getDevelopmentUsers });
  const session = useMutation({
    mutationFn: createDevelopmentSession,
    onSuccess: (_, userId) => {
      const user = users.data?.find((candidate) => candidate.id === userId);
      if (user) void navigate(destinationFor(user));
    },
  });

  return (
    <main className="login-page">
      <section className="login-panel" aria-labelledby="login-title">
        <p className="environment-banner">Local Development</p>
        <h1 id="login-title">GTA Application</h1>
        <p className="lede">Choose an anonymized identity for local development and demonstration.</p>
        <div className="notice" role="note">
          This local authentication interface is for development and demonstration only. Production authentication will use GMU-approved Microsoft Entra ID configuration.
        </div>
        {users.isPending && <p role="status">Loading development users…</p>}
        {users.isError && <div className="error-banner" role="alert">Development users could not be loaded. Confirm that the local API and SQL Server are running.</div>}
        <div className="identity-grid">
          {users.data?.map((user) => (
            <article className="identity-card" key={user.id}>
              <div>
                <p className="role-label">{user.roles.join(', ')}</p>
                <h2>{user.displayName}</h2>
                <p>{user.description}</p>
              </div>
              <button
                className="button"
                disabled={session.isPending}
                onClick={() => session.mutate(user.id)}
                type="button"
              >
                {session.isPending && session.variables === user.id ? 'Starting session…' : 'Continue as this user'}
              </button>
            </article>
          ))}
        </div>
        {session.isError && <div className="error-banner" role="alert">The development session could not be started. Please try again.</div>}
      </section>
    </main>
  );
}
