import { NavLink, Outlet, useLocation } from 'react-router-dom';

type ApplicationShellProps = { area: 'Applicant' | 'Faculty' | 'Administration' };
type NavigationLink = { to: string; label: string; shortLabel?: string; end: boolean };

const navigation: Record<ApplicationShellProps['area'], NavigationLink[]> = {
  Applicant: [
    { to: '.', label: 'GTA Home', end: true },
    { to: 'profile', label: 'GTA Profile', end: false },
    { to: 'documents', label: 'Documents', end: false },
    { to: 'sections', label: 'Apply to Course', end: false },
    { to: 'applications', label: 'My Applications', end: false },
  ],
  Faculty: [
    { to: '.', label: 'Dashboard', end: true },
    { to: 'sections', label: 'Assigned Sections', shortLabel: 'Sections', end: false },
    { to: 'applications', label: 'Applicant Reviews', shortLabel: 'Applications', end: false },
    { to: 'interviews', label: 'Interviews & Decisions', shortLabel: 'Interviews', end: false },
  ],
  Administration: [
    { to: '.', label: 'Dashboard', end: true },
    { to: 'applications', label: 'Applications', end: false },
    { to: 'applicants', label: 'Applicants', end: false },
    { to: 'sections', label: 'Sections', end: true },
    { to: 'sections/import', label: 'Import Sections', end: false },
    { to: 'placements', label: 'Placements', end: false },
    { to: 'phases', label: 'Application Phases', shortLabel: 'Phases', end: false },
    { to: 'users', label: 'Users & Roles', shortLabel: 'Users', end: false },
    { to: 'settings', label: 'Settings', end: false },
    { to: 'email-deliveries', label: 'Email Deliveries', shortLabel: 'Email', end: false },
    { to: 'audit', label: 'Audit Log', end: false },
  ],
};

function pageTitle(area: ApplicationShellProps['area'], pathname: string) {
  if (pathname.match(/\/applications\/[0-9a-f-]+$/i)) return area === 'Applicant' ? 'Application Details' : 'Applicant Review';
  const segment = pathname.split('/').filter(Boolean).slice(1).join('/');
  if (!segment) return area === 'Applicant' ? 'GTA Home' : `${area} Dashboard`;
  const link = navigation[area]
    .filter((item) => item.to !== '.')
    .sort((a, b) => b.to.length - a.to.length)
    .find((item) => segment === item.to || segment.startsWith(`${item.to}/`));
  return link?.label ?? area;
}

export function ApplicationShell({ area }: ApplicationShellProps) {
  const location = useLocation();
  const links = navigation[area];
  const title = pageTitle(area, location.pathname);
  const homePath = area === 'Administration' ? '/admin' : `/${area.toLowerCase()}`;

  return (
    <div className="app-layout">
      <a className="skip-link" href="#main-content">Skip to main content</a>
      <header className="site-header">
        <div className="university-bar">
          <div className="header-container university-brand">
            <span className="mason-mark" aria-hidden="true">M</span>
            <span>George Mason University</span>
            <span className="university-unit">College of Engineering and Computing</span>
          </div>
        </div>
        <div className="product-bar">
          <div className="header-container product-bar-content">
            <NavLink className="product-brand" to={homePath}>
              <span className="product-symbol" aria-hidden="true">GTA</span>
              <span><strong>Graduate Teaching Assistant</strong><small>Application Portal</small></span>
            </NavLink>
            <div className="header-actions">
              <span className="area-chip">{area} portal</span>
              <NavLink className="switch-user" to="/login">Switch demo user</NavLink>
            </div>
          </div>
        </div>
        <div className="navigation-bar">
          <nav className="header-container top-navigation" aria-label={`${area} navigation`}>
            {links.map((link) => (
              <NavLink key={link.to} to={link.to} end={link.end}>
                <span className="full-nav-label">{link.label}</span>
                {link.shortLabel && <span className="short-nav-label">{link.shortLabel}</span>}
              </NavLink>
            ))}
          </nav>
        </div>
      </header>
      <main className="page-content" id="main-content" tabIndex={-1}>
        <header className="page-header"><p className="eyebrow">{area} portal</p><h1>{title}</h1></header>
        <section className="content-card"><Outlet /></section>
      </main>
      <footer className="site-footer">
        <div className="footer-container">
          <div><strong>GTA Application Portal</strong><span>George Mason University · College of Engineering and Computing</span></div>
          <div className="footer-meta"><span className="demo-indicator">Demo environment</span><span>Do not enter real student information.</span></div>
        </div>
      </footer>
    </div>
  );
}
