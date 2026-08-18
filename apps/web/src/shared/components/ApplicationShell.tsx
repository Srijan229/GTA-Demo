import { NavLink, Outlet, useLocation } from 'react-router-dom';

type ApplicationShellProps = { area: 'Applicant' | 'Faculty' | 'Administration' };

export function ApplicationShell({ area }: ApplicationShellProps) {
  const location = useLocation();
  const applicantLinks = area === 'Applicant'
    ? [{ to: '.', label: 'GTA Home', end: true }, { to: 'profile', label: 'View GTA Profile', end: false }, { to: 'sections', label: 'Apply to Course', end: false }, { to: 'applications', label: 'My Applications', end: false }]
    : area === 'Faculty'
      ? [{ to: '.', label: 'Dashboard', end: true }, { to: 'sections', label: 'My sections', end: false }, { to: 'applications', label: 'Applications', end: false }, { to: 'interviews', label: 'Interviews & decisions', end: false }]
      : [{ to: '.', label: 'Dashboard', end: true }, { to: 'applications', label: 'Applications', end: false }, { to: 'applicants', label: 'Applicants', end: false }, { to: 'sections', label: 'Sections', end: true }, { to: 'sections/import', label: 'Import sections', end: false }, { to: 'placements', label: 'Placements', end: false }, { to: 'phases', label: 'Application phases', end: false }, { to: 'users', label: 'Users & roles', end: false }, { to: 'settings', label: 'Settings', end: false }, { to: 'email-deliveries', label: 'Email deliveries', end: false }, { to: 'audit', label: 'Audit log', end: false }];
  const applicantTitle = location.pathname.includes('/profile') ? 'View GTA Profile' : location.pathname.includes('/documents') ? 'GTA Documents' : location.pathname.includes('/applications/new') ? 'Apply to Course' : location.pathname.match(/\/applications\/[0-9a-f-]+$/i) ? 'Application Details' : location.pathname.endsWith('/applications') ? 'My Applications' : location.pathname.endsWith('/sections') ? 'Apply to Course' : 'GTA Home';
  const title = area === 'Applicant' ? applicantTitle : area;
  return (
    <div className="app-layout">
      <a className="skip-link" href="#main-content">Skip to main content</a>
      <header className="top-header">
        <span className="product-name">GTA Application</span>
        <span className="development-chip">Development</span>
      </header>
      <aside className="side-navigation" aria-label={`${area} navigation`}>
        <p className="area-label">{area}</p>
        <nav>{applicantLinks.map((link) => <NavLink key={link.to} to={link.to} end={link.end}>{link.label}</NavLink>)}</nav>
        <NavLink className="switch-user" to="/login">Switch demo user</NavLink>
      </aside>
      <main className="page-content" id="main-content" tabIndex={-1}>
        <header className="page-header"><p className="eyebrow">{area}</p><h1>{title}</h1></header>
        <section className="content-card"><Outlet /></section>
      </main>
    </div>
  );
}
