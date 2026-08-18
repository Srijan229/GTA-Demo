import { createBrowserRouter, Navigate, RouterProvider } from 'react-router-dom';
import { DevelopmentLoginPage } from '../../features/authentication/pages/DevelopmentLoginPage';
import { ProtectedRoute } from '../../features/authentication/components/ProtectedRoute';
import { AccessDeniedPage } from '../../shared/components/AccessDeniedPage';
import { ApplicantProfilePage } from '../../features/profiles/pages/ApplicantProfilePage';
import { ApplicantDocumentsPage } from '../../features/documents/pages/ApplicantDocumentsPage';
import { AvailableSectionsPage } from '../../features/applications/pages/AvailableSectionsPage';
import { NewApplicationPage } from '../../features/applications/pages/NewApplicationPage';
import { MyApplicationsPage } from '../../features/applications/pages/MyApplicationsPage';
import { ApplicationDetailPage } from '../../features/applications/pages/ApplicationDetailPage';
import { ApplicantDashboardPage } from '../../features/applications/pages/ApplicantDashboardPage';
import { FacultyDashboardPage } from '../../features/faculty-review/pages/FacultyDashboardPage';
import { FacultySectionsPage } from '../../features/faculty-review/pages/FacultySectionsPage';
import { FacultyApplicationsPage } from '../../features/faculty-review/pages/FacultyApplicationsPage';
import { FacultyReviewPage } from '../../features/faculty-review/pages/FacultyReviewPage';
import { FacultyInterviewsPage } from '../../features/faculty-review/pages/FacultyInterviewsPage';
import { ApplicationShell } from '../../shared/components/ApplicationShell';
import { NotFoundPage } from '../../shared/components/NotFoundPage';
import { AdminDashboardPage } from '../../features/administration/pages/AdminDashboardPage';
import { AdminApplicantsPage, AdminApplicationsPage, AdminAuditPage, AdminPhasesPage, AdminSectionsPage, AdminUsersPage } from '../../features/administration/pages/AdminDataPages';
import { AdminPlacementsPage } from '../../features/administration/pages/AdminPlacementsPage';
import { AdminSectionImportPage } from '../../features/administration/pages/AdminSectionImportPage';
import { AdminEmailDeliveriesPage } from '../../features/administration/pages/AdminEmailDeliveriesPage';
import { RouteErrorPage } from '../../shared/components/RouteErrorPage';
import { AdminSettingsFormPage } from '../../features/administration/pages/AdminSettingsFormPage';

const router = createBrowserRouter([
  { path: '/', element: <Navigate to="/login" replace /> },
  { path: '/login', element: <DevelopmentLoginPage /> },
  {
    path: '/applicant',
    element: <ProtectedRoute allowedRole="Applicant"><ApplicationShell area="Applicant" /></ProtectedRoute>,
    errorElement: <RouteErrorPage />,
    children: [
      { index: true, element: <ApplicantDashboardPage /> },
      { path: 'profile', element: <ApplicantProfilePage /> },
      { path: 'documents', element: <ApplicantDocumentsPage /> },
      { path: 'sections', element: <AvailableSectionsPage /> },
      { path: 'applications/new', element: <NewApplicationPage /> },
      { path: 'applications', element: <MyApplicationsPage /> },
      { path: 'applications/:id', element: <ApplicationDetailPage /> },
    ],
  },
  {
    path: '/faculty',
    element: <ProtectedRoute allowedRole="Faculty"><ApplicationShell area="Faculty" /></ProtectedRoute>,
    errorElement: <RouteErrorPage />,
    children: [
      { index: true, element: <FacultyDashboardPage /> },
      { path: 'sections', element: <FacultySectionsPage /> },
      { path: 'applications', element: <FacultyApplicationsPage /> },
      { path: 'applications/:choiceId', element: <FacultyReviewPage /> },
      { path: 'interviews', element: <FacultyInterviewsPage /> },
    ],
  },
  {
    path: '/admin',
    element: <ProtectedRoute allowedRole="Administrator"><ApplicationShell area="Administration" /></ProtectedRoute>,
    errorElement: <RouteErrorPage />,
    children: [
      { index: true, element: <AdminDashboardPage /> },
      { path: 'applications', element: <AdminApplicationsPage /> },
      { path: 'applicants', element: <AdminApplicantsPage /> },
      { path: 'sections', element: <AdminSectionsPage /> },
      { path: 'sections/import', element: <AdminSectionImportPage /> },
      { path: 'placements', element: <AdminPlacementsPage /> },
      { path: 'phases', element: <AdminPhasesPage /> },
      { path: 'users', element: <AdminUsersPage /> },
      { path: 'settings', element: <AdminSettingsFormPage /> },
      { path: 'audit', element: <AdminAuditPage /> },
      { path: 'email-deliveries', element: <AdminEmailDeliveriesPage /> },
    ],
  },
  { path: '/access-denied', element: <AccessDeniedPage /> },
  { path: '*', element: <NotFoundPage /> },
]);

export function AppRouter() {
  return <RouterProvider router={router} />;
}
