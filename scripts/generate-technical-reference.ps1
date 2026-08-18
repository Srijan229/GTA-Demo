$ErrorActionPreference = 'Stop'

$workspace = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$outputDir = Join-Path $workspace 'docs\deliverables'
$outputPath = Join-Path $outputDir 'GTA-Application-Technical-Reference.docx'
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

$pages = @(
  @('/', 'Public', 'Redirects visitors to the development sign-in page.'),
  @('/login', 'Anonymous (development)', 'Lists seeded local identities and creates a development session.'),
  @('/applicant', 'Applicant', 'Dashboard showing identity, profile completion, document readiness, available sections, and recent applications.'),
  @('/applicant/profile', 'Applicant', 'Edits personal and academic profile data plus repeatable education and experience records; shows completion.'),
  @('/applicant/documents', 'Applicant', 'Uploads, versions, and opens the applicant resume and unofficial transcript.'),
  @('/applicant/sections', 'Applicant', 'Shows active, eligible course sections and whether the applicant already applied.'),
  @('/applicant/applications/new', 'Applicant', 'Selects employment basis and ranked section choices, validates readiness, and submits an application.'),
  @('/applicant/applications', 'Applicant', 'Lists the signed-in applicant''s submissions, current states, terms, and selected sections.'),
  @('/applicant/applications/:id', 'Owning applicant', 'Shows one application, choices, status history, and controlled withdrawal when policy allows.'),
  @('/faculty', 'Faculty', 'Summarizes assigned sections, authorized application workload, interview work, and hire recommendations.'),
  @('/faculty/sections', 'Faculty', 'Lists only sections assigned to the signed-in faculty member and their application counts.'),
  @('/faculty/applications', 'Faculty', 'Lists applicants reachable through active faculty-to-section assignments.'),
  @('/faculty/applications/:choiceId', 'Authorized faculty', 'Shows applicant profile and documents for one selected section; records interview and hire actions.'),
  @('/faculty/interviews', 'Faculty', 'Filters interview candidates and shows decision and placement workload for assigned sections.'),
  @('/admin', 'Administrator', 'Operational dashboard with application, applicant, section, review, and warning metrics.'),
  @('/admin/applications', 'Administrator', 'Searchable cross-system application list with applicant, phase, state, and choices.'),
  @('/admin/applicants', 'Administrator', 'Searchable applicant directory with profile completion and identity status.'),
  @('/admin/sections', 'Administrator', 'Edits section capacity/activity and assigns or removes faculty reviewers.'),
  @('/admin/sections/import', 'Administrator', 'Downloads the CSV template, validates imports, commits accepted rows, and displays import history.'),
  @('/admin/placements', 'Administrator', 'Activates or removes placements for hire-recommended choices while enforcing capacity and workload rules.'),
  @('/admin/phases', 'Administrator', 'Edits phase names, program scope, open/close windows, and activation state.'),
  @('/admin/users', 'Administrator', 'Activates/deactivates users and manages application roles.'),
  @('/admin/settings', 'Administrator', 'Presents validated controls for non-secret system settings, including section-choice and upload limits.'),
  @('/admin/audit', 'Administrator', 'Displays administrative and workflow audit events with correlation identifiers.'),
  @('/admin/email-deliveries', 'Administrator', 'Displays queued, sent, and failed email outbox deliveries and attempt counts.'),
  @('/access-denied', 'Any', 'Explains that the current identity lacks permission.'),
  @('*', 'Any', 'Displays the safe page-not-found state; protected route failures use a dedicated retry/error page.')
)

$apis = @(
  @('GET','/health/live','Public','Process liveness check without dependency checks.'),
  @('GET','/health/ready','Public','Readiness check including registered infrastructure dependencies.'),
  @('GET','/openapi/v1.json','Development','Generated OpenAPI description (development only).'),
  @('GET','/api/v1/system/info','Public','Returns service name, environment, and current UTC time.'),
  @('GET','/api/v1/development/users','Anonymous / development','Lists seeded identities available to the local login selector.'),
  @('POST','/api/v1/development/session/{userId}','Anonymous / development','Creates the strict, HTTP-only development authentication cookie.'),
  @('DELETE','/api/v1/development/session','Authenticated / development','Signs out and removes the development session.'),
  @('GET','/api/v1/auth/me','Authenticated','Returns current user id, display name, email, and roles.'),
  @('GET','/api/v1/applicant/access','Applicant','Role-policy probe used by protected applicant navigation.'),
  @('GET','/api/v1/faculty/access','Faculty','Role-policy probe used by protected faculty navigation.'),
  @('GET','/api/v1/admin/access','Administrator','Role-policy probe used by protected administration navigation.'),
  @('GET','/api/v1/profile/me/','Applicant','Returns the current applicant profile, education, and experience.'),
  @('GET','/api/v1/profile/me/completion','Applicant','Calculates completed/incomplete profile sections and percentage.'),
  @('PUT','/api/v1/profile/me/','Applicant','Updates the applicant profile and replaces repeatable education/experience values.'),
  @('GET','/api/v1/documents/','Applicant','Lists the applicant''s current active resume/transcript versions.'),
  @('POST','/api/v1/documents/{type}','Applicant','Validates, stores, hashes, versions, and activates an uploaded document.'),
  @('GET','/api/v1/documents/{documentId}/content','Owning applicant','Streams an owned document with range processing.'),
  @('GET','/api/v1/applications/available-sections','Applicant','Returns eligible active sections for open phases and flags prior choices.'),
  @('GET','/api/v1/applications/configuration','Applicant','Returns submission rules such as maximum section choices.'),
  @('GET','/api/v1/applications/mine','Applicant','Lists applications owned by the signed-in applicant.'),
  @('GET','/api/v1/applications/mine/{applicationId}','Owning applicant','Returns application detail, choice list, status history, and withdrawal eligibility.'),
  @('POST','/api/v1/applications/mine/{applicationId}/withdraw','Owning applicant','Applies withdrawal policy, changes state, writes history/audit, and queues email.'),
  @('POST','/api/v1/applications/','Applicant','Validates readiness, phase, selections, and duplicates; creates application and choices.'),
  @('GET','/api/v1/faculty/sections','Faculty','Returns sections connected through active faculty assignments.'),
  @('GET','/api/v1/faculty/applications','Faculty','Returns choices/applicants authorized through assigned sections.'),
  @('GET','/api/v1/faculty/interviews','Faculty','Returns authorized interview queue and placement workload.'),
  @('GET','/api/v1/faculty/applications/{choiceId}','Authorized faculty','Returns one applicant review package, profile, current documents, and action state.'),
  @('POST','/api/v1/faculty/applications/{choiceId}/actions','Authorized faculty','Activates/deactivates interview or hire-recommendation actions and records audit/email work.'),
  @('GET','/api/v1/faculty/documents/{documentId}/content','Authorized faculty','Streams applicant content only when the faculty member is assigned to a chosen section.'),
  @('GET','/api/v1/admin/dashboard','Administrator','Returns system counts and operational warnings.'),
  @('GET','/api/v1/admin/applications','Administrator','Returns the administrative application inventory.'),
  @('GET','/api/v1/admin/applicants','Administrator','Returns applicant identities and profile status.'),
  @('GET','/api/v1/admin/sections','Administrator','Returns section, term, capacity, activity, and faculty assignment data.'),
  @('GET','/api/v1/admin/phases','Administrator','Returns application phase configuration.'),
  @('GET','/api/v1/admin/users','Administrator','Returns users, activation state, and roles.'),
  @('GET','/api/v1/admin/settings','Administrator','Returns non-secret settings and descriptions.'),
  @('GET','/api/v1/admin/audit','Administrator','Returns recent audit events.'),
  @('GET','/api/v1/admin/email-deliveries','Administrator','Returns email outbox state and delivery metadata.'),
  @('GET','/api/v1/admin/placements','Administrator','Returns placement candidates, decisions, capacity, and workload.'),
  @('GET','/api/v1/admin/section-imports','Administrator','Returns committed section-import batch history.'),
  @('GET','/api/v1/admin/section-imports/template','Administrator','Downloads the required course-section CSV template.'),
  @('POST','/api/v1/admin/section-imports/preview','Administrator','Parses and validates a CSV without persisting changes.'),
  @('POST','/api/v1/admin/section-imports','Administrator','Imports accepted CSV rows and records batch/audit results.'),
  @('PUT','/api/v1/admin/placements/{choiceId}','Administrator','Activates/removes a placement and synchronizes selected state under policy constraints.'),
  @('PUT','/api/v1/admin/sections/{id}/faculty','Administrator','Adds or removes a faculty-section assignment.'),
  @('PUT','/api/v1/admin/sections/{id}','Administrator','Updates section capacity and active state.'),
  @('PUT','/api/v1/admin/phases/{id}','Administrator','Updates phase configuration.'),
  @('PUT','/api/v1/admin/users/{id}','Administrator','Updates user activation and role assignments.'),
  @('PUT','/api/v1/admin/settings/{key}','Administrator','Validates and updates one system setting.')
)

$workflows = @(
  @('Sign in and authorization','/login; protected shells','development session; auth/me; role access probes','Users, UserRoles, Roles'),
  @('Maintain applicant profile','/applicant/profile','GET/PUT profile/me; completion','Users, ApplicantProfiles, EducationRecords, ExperienceRecords'),
  @('Manage documents','/applicant/documents','GET/POST documents; content download','Users, Documents; local document storage'),
  @('Submit application','sections; applications/new','available-sections; configuration; POST applications','SystemSettings, ApplicationPhases, AcademicTerms, Courses, CourseSections, Applications, ApplicationChoices'),
  @('Review or withdraw','applications; applications/:id','mine; detail; withdraw','Applications, ApplicationChoices, ApplicationStatusHistory, AuditLogs, EmailOutboxMessages'),
  @('Faculty review','faculty sections/applications/detail/interviews','faculty sections, applications, actions, documents','FacultySectionAssignments, ApplicationChoices, FacultyReviewActions, Documents, Placements'),
  @('Placement management','/admin/placements','GET placements; PUT placement','Placements, ApplicationChoices, CourseSections, Applications, AuditLogs, EmailOutboxMessages'),
  @('Section administration','/admin/sections; sections/import','section update/assignment; import preview/commit/history','Courses, AcademicTerms, CourseSections, FacultySectionAssignments, SectionImportBatches, AuditLogs'),
  @('Operations and configuration','admin dashboard/settings/audit/email','admin read APIs; setting update','SystemSettings, AuditLogs, EmailOutboxMessages and aggregate reads across domain tables')
)

function Invoke-SqlMetadata([string]$query) {
  $password = docker exec gta-application-sqlserver-1 printenv MSSQL_SA_PASSWORD
  if (-not $password) { throw 'SQL Server container password was not available.' }
  $output = docker exec gta-application-sqlserver-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $password -C -d GtaApplication -h -1 -W -s '|' -Q $query
  if ($LASTEXITCODE -ne 0) { throw 'SQL metadata query failed.' }
  return @($output | Where-Object { $_ -and $_ -notmatch '^\(' })
}

$columnSql = @"
SET NOCOUNT ON;
SELECT t.name,c.column_id,c.name,TYPE_NAME(c.user_type_id),CASE WHEN TYPE_NAME(c.user_type_id) IN ('nvarchar','nchar') AND c.max_length>0 THEN c.max_length/2 ELSE c.max_length END,c.precision,c.scale,c.is_nullable,CASE WHEN pk.column_id IS NULL THEN 0 ELSE 1 END
FROM sys.tables t JOIN sys.columns c ON c.object_id=t.object_id
LEFT JOIN (SELECT ic.object_id,ic.column_id FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id WHERE i.is_primary_key=1) pk ON pk.object_id=t.object_id AND pk.column_id=c.column_id
WHERE t.is_ms_shipped=0 AND t.name<>'sysdiagrams' ORDER BY CASE WHEN t.name LIKE '__%' THEN 1 ELSE 0 END,t.name,c.column_id;
"@
$fkSql = @"
SET NOCOUNT ON;
SELECT pt.name,pc.name,rt.name,rc.name,fk.delete_referential_action_desc
FROM sys.foreign_keys fk JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id=fk.object_id
JOIN sys.tables pt ON pt.object_id=fkc.parent_object_id JOIN sys.columns pc ON pc.object_id=fkc.parent_object_id AND pc.column_id=fkc.parent_column_id
JOIN sys.tables rt ON rt.object_id=fkc.referenced_object_id JOIN sys.columns rc ON rc.object_id=fkc.referenced_object_id AND rc.column_id=fkc.referenced_column_id
ORDER BY pt.name,fk.name,fkc.constraint_column_id;
"@
$columns = Invoke-SqlMetadata $columnSql | ForEach-Object { $p=$_ -split '\|'; [pscustomobject]@{Table=$p[0];Ordinal=[int]$p[1];Column=$p[2];Type=$p[3];Length=[int]$p[4];Precision=[int]$p[5];Scale=[int]$p[6];Nullable=([int]$p[7]-eq 1);PrimaryKey=([int]$p[8]-eq 1)} }
$foreignKeys = Invoke-SqlMetadata $fkSql | ForEach-Object { $p=$_ -split '\|'; [pscustomobject]@{ChildTable=$p[0];ChildColumn=$p[1];ParentTable=$p[2];ParentColumn=$p[3];DeleteRule=$p[4]} }

$tablePurposes = @{
  '__EFMigrationsHistory'='EF Core migration ledger used to track applied schema versions.'; 'AcademicTerms'='Academic term calendar shared by sections and application phases.';
  'ApplicantProfiles'='One-to-one applicant-specific extension of a user identity.'; 'ApplicationChoices'='Join entity connecting an application to each requested course section.';
  'ApplicationPhases'='Program and term application windows.'; 'Applications'='Top-level applicant submission and workflow state.'; 'ApplicationStatusHistory'='Append-only record of application state transitions.';
  'AuditLogs'='Security-conscious operational event log with correlation identifiers.'; 'Courses'='Reusable course catalog identity.'; 'CourseSections'='Term-specific GTA opportunities and capacity.';
  'Documents'='Versioned metadata for resumes and transcripts; binary content remains in document storage.'; 'EducationRecords'='Repeatable education rows owned by an applicant profile.';
  'EmailOutboxMessages'='Reliable asynchronous email queue and delivery history.'; 'ExperienceRecords'='Repeatable employment/GTA experience rows owned by an applicant profile.';
  'FacultyReviewActions'='Faculty interview and hire-recommendation decisions for an application choice.'; 'FacultySectionAssignments'='Authorization bridge between faculty users and course sections.';
  'Placements'='Active/inactive placement of a selected application choice into a course section.'; 'Roles'='Named application roles.'; 'SectionImportBatches'='History and summary of committed section CSV imports.';
  'SystemSettings'='Validated, non-secret runtime configuration.'; 'UserRoles'='Many-to-many bridge assigning roles to users.'; 'Users'='Canonical identities for applicants, faculty, and administrators.'
}

$mdPath=Join-Path $outputDir 'GTA-Application-Technical-Reference.md'
$md=[Text.StringBuilder]::new()
function MdRow($values){'| '+(($values|ForEach-Object{([string]$_)-replace'\|','\|'})-join' | ')+' |'}
[void]$md.AppendLine('# GTA Application Technical Reference').AppendLine().AppendLine('Pages, APIs, SQL schema, relationships, and end-to-end workflows').AppendLine().AppendLine('Implementation snapshot: August 15, 2026').AppendLine()
[void]$md.AppendLine('## Overview').AppendLine().AppendLine("- $($pages.Count) routed page states").AppendLine("- $($apis.Count) HTTP endpoints").AppendLine("- $((($columns|Select-Object -ExpandProperty Table -Unique).Count)) application/support tables").AppendLine("- $($foreignKeys.Count) enforced foreign keys").AppendLine().AppendLine('Architecture: React/Vite -> ASP.NET Core minimal APIs -> application services -> EF Core -> SQL Server. Document binaries live outside SQL; SQL stores document metadata and storage keys. Email uses a durable outbox.').AppendLine()
[void]$md.AppendLine('## 1. Page inventory').AppendLine().AppendLine((MdRow @('Route','Role / access','Purpose'))).AppendLine((MdRow @('---','---','---')))
foreach($x in $pages){[void]$md.AppendLine((MdRow @("``$($x[0])``",$x[1],$x[2])))}
[void]$md.AppendLine().AppendLine('## 2. API inventory').AppendLine().AppendLine((MdRow @('Method','Route','Authorization','Responsibility'))).AppendLine((MdRow @('---','---','---','---')))
foreach($x in $apis){[void]$md.AppendLine((MdRow @($x[0],"``$($x[1])``",$x[2],$x[3])))}
[void]$md.AppendLine().AppendLine('## 3. SQL relationship diagram').AppendLine().AppendLine('```mermaid').AppendLine('erDiagram')
foreach($fk in $foreignKeys){[void]$md.AppendLine("  $($fk.ParentTable) ||--o{ $($fk.ChildTable) : `"$($fk.ChildColumn)`"")}
[void]$md.AppendLine('```').AppendLine().AppendLine('Each arrow is backed by a SQL foreign key. All configured deletes use `NO_ACTION`/restricted behavior. Audit actor identifiers without foreign keys are documented below.').AppendLine()
[void]$md.AppendLine('## 4. SQL table dictionary').AppendLine().AppendLine('The definitions below were read from the live `GtaApplication` database. `PK` marks primary keys.').AppendLine()
foreach($tableName in @($columns|Select-Object -ExpandProperty Table -Unique)){
 $purpose=$tablePurposes[$tableName];if(-not $purpose){$purpose='Database support table.'};[void]$md.AppendLine("### $tableName").AppendLine().AppendLine($purpose).AppendLine().AppendLine((MdRow @('Column','SQL type','Key / nullability'))).AppendLine((MdRow @('---','---','---')))
 foreach($c in @($columns|Where-Object Table -eq $tableName)){$type=$c.Type;if($c.Type-in@('nvarchar','nchar')-and$c.Length-gt 0){$type+="($($c.Length))"}elseif($c.Type-in@('nvarchar','varbinary')-and$c.Length-eq -1){$type+='(max)'}elseif($c.Type-eq'decimal'){$type+="($($c.Precision),$($c.Scale))"};$flags=@();if($c.PrimaryKey){$flags+='PK'};if($c.Nullable){$flags+='Nullable'}else{$flags+='Required'};[void]$md.AppendLine((MdRow @("``$($c.Column)``","``$type``",($flags-join', '))))}
 $refs=@($foreignKeys|Where-Object ChildTable -eq $tableName);if($refs.Count){[void]$md.AppendLine().AppendLine('Relationships: '+(($refs|ForEach-Object{"``$($_.ChildColumn)`` -> ``$($_.ParentTable).$($_.ParentColumn)`` ($($_.DeleteRule))"})-join'; ')+'.')};[void]$md.AppendLine()
}
[void]$md.AppendLine('## 5. Foreign-key relationships').AppendLine().AppendLine((MdRow @('Child column','Parent key','Cardinality','Delete behavior'))).AppendLine((MdRow @('---','---','---','---')))
foreach($fk in $foreignKeys){[void]$md.AppendLine((MdRow @("``$($fk.ChildTable).$($fk.ChildColumn)``","``$($fk.ParentTable).$($fk.ParentColumn)``",'Many-to-one',($fk.DeleteRule-replace'_',' '))))}
[void]$md.AppendLine().AppendLine('### Important relationship rules beyond foreign keys').AppendLine().AppendLine('- `ApplicantProfiles.UserId` is unique, forming a one-to-one user/profile relationship.').AppendLine('- Duplicate application/section choices and duplicate applicant/phase applications are blocked by unique indexes.').AppendLine('- Only one active document exists per owner and document type; prior versions are superseded.').AppendLine('- Only one active placement exists per application choice; capacity and workload are enforced transactionally.').AppendLine('- Faculty access requires an active `FacultySectionAssignments` row for the target section.').AppendLine('- Audit, status-history, and import actor identifiers intentionally have no database foreign keys so evidence can survive identity lifecycle changes.').AppendLine()
[void]$md.AppendLine('## 6. End-to-end traceability').AppendLine().AppendLine((MdRow @('Capability','Pages','Primary APIs','Main storage'))).AppendLine((MdRow @('---','---','---','---')))
foreach($x in $workflows){[void]$md.AppendLine((MdRow @($x[0],$x[1],$x[2],$x[3])))}
[void]$md.AppendLine().AppendLine('### Application lifecycle').AppendLine().AppendLine('Profile + required documents -> open application phase -> application submission -> section choices -> faculty interview/hire actions -> administrator placement -> application state synchronized to Selected. Controlled withdrawal writes status history and audit evidence.').AppendLine().AppendLine('### Authorization lifecycle').AppendLine().AppendLine('Users receive roles through `UserRoles`. Applicant operations require the Applicant role plus ownership. Faculty operations require the Faculty role plus an active section assignment. Administration operations require the Administrator role.').AppendLine().AppendLine('### Operational side effects').AppendLine().AppendLine('Important mutations write `AuditLogs` with correlation identifiers. Notifications enter `EmailOutboxMessages`; a background dispatcher records sent or failed state.').AppendLine()
[void]$md.AppendLine('## 7. Conventions and operational notes').AppendLine().AppendLine('- Identifiers use `uniqueidentifier`; mutable records commonly use `rowversion` for optimistic concurrency.').AppendLine('- Enums persist as integers: ApplicationState Draft=1, Submitted=2, UnderReview=3, Interview=4, Selected=5, NotSelected=6, Withdrawn=7; EmploymentBasis PartTime10Hours=1, FullTime20Hours=2; ReviewActionType Interview=1, HireRecommendation=2.').AppendLine('- Document binaries are external. `Documents.StorageKey` locates content and `Sha256` supports integrity checks.').AppendLine('- `SystemSettings` contains non-secret values only; credentials remain in environment configuration.').AppendLine('- `__EFMigrationsHistory` is EF Core infrastructure. SQL Server `sysdiagrams` is not part of the application domain.').AppendLine('- Regenerate this reference whenever routes, endpoint mappings, migrations, or relationships change.')
[IO.File]::WriteAllText($mdPath,$md.ToString(),[Text.UTF8Encoding]::new($false));Write-Output $mdPath;exit 0

function Encode-Html([string]$value) { return [Net.WebUtility]::HtmlEncode($value) }
function Html-Table($headers,$rows,[string]$class='') {
  $b=[Text.StringBuilder]::new(); [void]$b.Append("<table class='$class'><thead><tr>")
  foreach($x in $headers){[void]$b.Append('<th>'+ (Encode-Html $x) +'</th>')}; [void]$b.Append('</tr></thead><tbody>')
  foreach($row in $rows){[void]$b.Append('<tr>');foreach($x in $row){[void]$b.Append('<td>'+ (Encode-Html ([string]$x)) +'</td>')};[void]$b.Append('</tr>')};[void]$b.Append('</tbody></table>');return $b.ToString()
}

# Word lays out a single standards-based source far faster and more reliably than hundreds of COM cell calls.
$runId=[Guid]::NewGuid().ToString('N')
$htmlPath=Join-Path $outputDir ".GTA-Application-Technical-Reference-$runId.html"
$diagramSvg=Join-Path $workspace 'docs\assets\gta-table-relationships.svg'
$diagramPng=Join-Path $outputDir ".gta-table-relationships-$runId.png"
$edgeProfile=Join-Path $outputDir ".edge-$runId"
$edge='C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'
if(Test-Path $edge){$edgeArgs=@('--headless','--disable-gpu','--hide-scrollbars','--window-size=1000,930',"--user-data-dir=$edgeProfile", "--screenshot=$diagramPng",('file:///'+($diagramSvg -replace '\\','/')));Start-Process -FilePath $edge -ArgumentList $edgeArgs -Wait -WindowStyle Hidden}
$b=[Text.StringBuilder]::new()
[void]$b.Append(@"
<!doctype html><html><head><meta charset="utf-8"><style>
@page{size:letter;margin:.72in .78in}body{font-family:Aptos,Arial,sans-serif;color:#243447;font-size:9.7pt;line-height:1.25}h1{font-size:19pt;color:#1f4e79;margin:18pt 0 8pt;page-break-after:avoid}h2{font-size:14pt;color:#2f5597;margin:13pt 0 6pt;page-break-after:avoid}h3{font-size:11.5pt;color:#44546a;margin:10pt 0 4pt;page-break-after:avoid}p{margin:0 0 6pt}.cover{text-align:center;padding-top:1.1in;page-break-after:always}.cover .k{font-size:11pt;letter-spacing:1px;color:#2f5597}.cover h1{font-size:30pt;margin:12pt 0 6pt}.cover .sub{font-size:16pt;color:#526575}.muted{color:#687782}.break{page-break-before:always}table{width:100%;border-collapse:collapse;margin:5pt 0 10pt;page-break-inside:auto}tr{page-break-inside:avoid}th{background:#e8eef5;color:#17365d;font-weight:700;text-align:left}th,td{border:1px solid #bcc9d4;padding:4pt 5pt;vertical-align:middle;font-size:8.3pt}.api td:nth-child(1){width:7%}.api td:nth-child(2){width:33%}.api td:nth-child(3){width:18%}.pages td:nth-child(1){width:25%}.pages td:nth-child(2){width:19%}.cols td:nth-child(1){width:38%}.cols td:nth-child(2){width:25%}.note{background:#f4f7fa;border-left:4px solid #2f5597;padding:9pt;margin:8pt 0}img{display:block;max-width:100%;height:auto;margin:10pt auto}.small{font-size:8.5pt}ul{margin:4pt 0 8pt;padding-left:20pt}li{margin-bottom:3pt}
</style></head><body>
<div class="cover"><div class="k">CEC IST GTA APPLICATION</div><h1>Technical Reference</h1><div class="sub">Pages, APIs, SQL schema, relationships, and end-to-end workflows</div><p style="margin-top:40pt" class="muted">Implementation snapshot - August 15, 2026</p><p class="muted">Built from current React routes, ASP.NET Core endpoint mappings, and the live GtaApplication SQL Server database.</p></div>
<h1>How to use this reference</h1><p>This document is the implementation map for the GTA Application migration. It distinguishes browser pages, HTTP endpoints, persistent data, authorization boundaries, and their workflow connections.</p>
<div class="note"><b>Inventory:</b> $($pages.Count) routed page states; $($apis.Count) HTTP endpoints; $((($columns|Select-Object -ExpandProperty Table -Unique).Count)) application/support tables; $($foreignKeys.Count) enforced foreign keys.<br><b>Architecture:</b> React/Vite client -&gt; ASP.NET Core minimal APIs -&gt; application services -&gt; EF Core -&gt; SQL Server. Document binaries are external; SQL stores metadata. Email is dispatched from a durable outbox.<br><b>Security:</b> API policies are authoritative. Applicant ownership and faculty-to-section assignment are checked by services before sensitive data is returned.</div>
<h1>Contents</h1><ol><li>Page inventory</li><li>API inventory</li><li>Data model overview</li><li>SQL table dictionary</li><li>Foreign-key relationships</li><li>End-to-end traceability</li><li>Conventions and operational notes</li></ol>
<h1 class="break">1. Page inventory</h1><p>Routes are implemented in <b>AppRouter.tsx</b>. Role-specific shells enforce navigation access and shared failure handling.</p>
"@)
[void]$b.Append((Html-Table @('Route','Role / access','Purpose') (@($pages|ForEach-Object{,@($_[0],$_[1],$_[2])})) 'pages'))
[void]$b.Append('<h1 class="break">2. API inventory</h1><p>Protected endpoints require the authorization shown below. Errors use Problem Details with correlation identifiers. Development authentication and OpenAPI are local-development facilities.</p>')
[void]$b.Append((Html-Table @('Method','Route','Authorization','Responsibility') (@($apis|ForEach-Object{,@($_[0],$_[1],$_[2],$_[3])})) 'api'))
[void]$b.Append('<h1 class="break">3. Data model overview</h1><p>The diagram separates identity/profile, application/catalog, faculty/placement, and operations concerns. Every connector corresponds to an enforced SQL foreign key; the exact column mappings follow.</p>')
if(Test-Path $diagramPng){[void]$b.Append('<img src="'+(Encode-Html $diagramPng)+'" alt="GTA Application SQL relationship diagram">')}
[void]$b.Append('<p><b>Reading direction:</b> a User owns at most one ApplicantProfile and many Applications/Documents. An Application belongs to one ApplicationPhase and contains ApplicationChoices, each targeting a CourseSection. Faculty access comes through FacultySectionAssignments; decisions use FacultyReviewActions; final assignments use Placements.</p>')
[void]$b.Append('<h1 class="break">4. SQL table dictionary</h1><p>Types and nullability below come from the live database. PK marks primary keys. Mutable domain records commonly share CreatedAtUtc, CreatedByUserId, UpdatedAtUtc, UpdatedByUserId, and RowVersion.</p>')
foreach($tableName in @($columns|Select-Object -ExpandProperty Table -Unique)){
 $purpose=$tablePurposes[$tableName];if(-not $purpose){$purpose='Database support table.'};[void]$b.Append('<h2>'+ (Encode-Html $tableName) +'</h2><p>'+ (Encode-Html $purpose) +'</p>')
 $rows=@($columns|Where-Object Table -eq $tableName|ForEach-Object{$type=$_.Type;if($_.Type-in@('nvarchar','nchar')-and$_.Length-gt 0){$type+="($($_.Length))"}elseif($_.Type-in@('nvarchar','varbinary')-and$_.Length-eq -1){$type+='(max)'}elseif($_.Type-eq'decimal'){$type+="($($_.Precision),$($_.Scale))"};$flags=@();if($_.PrimaryKey){$flags+='PK'};if($_.Nullable){$flags+='Nullable'}else{$flags+='Required'};,@($_.Column,$type,($flags-join', '))})
 [void]$b.Append((Html-Table @('Column','SQL type','Key / nullability') $rows 'cols'))
 $refs=@($foreignKeys|Where-Object ChildTable -eq $tableName);if($refs.Count){[void]$b.Append('<p class="small"><b>Relationships:</b> '+(Encode-Html (($refs|ForEach-Object{"$($_.ChildColumn) -> $($_.ParentTable).$($_.ParentColumn) ($($_.DeleteRule))"})-join'; '))+'.</p>')}
}
[void]$b.Append('<h1 class="break">5. Foreign-key relationships</h1><p>All configured relationships use restricted/no-action deletion, so business records are retired or dependents handled explicitly.</p>')
[void]$b.Append((Html-Table @('Child column','Parent key','Cardinality','Delete behavior') (@($foreignKeys|ForEach-Object{,@("$($_.ChildTable).$($_.ChildColumn)","$($_.ParentTable).$($_.ParentColumn)",'Many-to-one',($_.DeleteRule-replace'_',' '))})) ''))
[void]$b.Append('<h2>Relationship rules beyond foreign keys</h2><ul>')
foreach($rule in @('ApplicantProfiles.UserId is unique, forming a one-to-one user/profile relationship.','ApplicationChoices blocks duplicate ApplicationId + CourseSectionId pairs.','Applications blocks duplicate ApplicantUserId + ApplicationPhaseId pairs and uses a unique Reference.','Only one active document exists per owner and type; prior versions are superseded.','Only one active placement exists per application choice; capacity and workload are enforced transactionally.','Faculty visibility requires an active FacultySectionAssignment for the target section.','Historical actor identifiers in audit, status-history, and import tables intentionally survive identity lifecycle changes without database foreign keys.')){[void]$b.Append('<li>'+ (Encode-Html $rule) +'</li>')};[void]$b.Append('</ul>')
[void]$b.Append('<h1 class="break">6. End-to-end traceability</h1><p>Services between the API and EF Core enforce readiness, ownership, faculty authorization, capacity, concurrency, auditing, and email side effects.</p>')
[void]$b.Append((Html-Table @('Capability','Pages','Primary APIs','Main storage') (@($workflows|ForEach-Object{,@($_[0],$_[1],$_[2],$_[3])})) ''))
[void]$b.Append('<h2>Application lifecycle</h2><p>Profile + required documents -&gt; open ApplicationPhase -&gt; Applications submission -&gt; ApplicationChoices for selected CourseSections -&gt; faculty interview/hire actions -&gt; administrator Placement -&gt; application state synchronized to Selected. Controlled withdrawal writes status history and audit evidence.</p><h2>Authorization lifecycle</h2><p>Users receive Roles through UserRoles. Applicant operations require role plus ownership. Faculty operations require role plus an active section assignment. Administration operations require the Administrator role.</p><h2>Operational side effects</h2><p>Important mutations write AuditLogs with correlation ids. Notifications enter EmailOutboxMessages, and a background dispatcher records sent or failed state.</p>')
[void]$b.Append('<h1>7. Conventions and operational notes</h1><ul>')
foreach($note in @('Identifiers are uniqueidentifier values. Mutable records use rowversion for optimistic concurrency.','Enums persist as integers. ApplicationState: Draft=1, Submitted=2, UnderReview=3, Interview=4, Selected=5, NotSelected=6, Withdrawn=7. EmploymentBasis: PartTime10Hours=1, FullTime20Hours=2. ReviewActionType: Interview=1, HireRecommendation=2.','Document binaries are external. Documents.StorageKey identifies stored content; Sha256 supports integrity checking.','SystemSettings contains non-secret values only. Credentials stay in environment configuration.','__EFMigrationsHistory is EF Core infrastructure. SQL Server sysdiagrams is not part of the application domain.','Regenerate this reference whenever routes, endpoint mappings, migrations, or relationships change.')){[void]$b.Append('<li>'+ (Encode-Html $note) +'</li>')};[void]$b.Append('</ul></body></html>')
[IO.File]::WriteAllText($htmlPath,$b.ToString(),[Text.UTF8Encoding]::new($true))
Write-Output $htmlPath
exit 0
$word=New-Object -ComObject Word.Application;$word.Visible=$false;$word.DisplayAlerts=0
try{$doc=$word.Documents.Open($htmlPath,$false,$false);foreach($section in $doc.Sections){$footer=$section.Footers.Item(1).Range;$footer.ParagraphFormat.Alignment=2;$footer.Font.Name='Aptos';$footer.Font.Size=8;$footer.Text='GTA Application Technical Reference  |  Page ';$footer.Collapse(0);$footer.Fields.Add($footer,-1,'PAGE')|Out-Null};$doc.SaveAs2($outputPath,16);$doc.Close()}finally{$word.Quit();[Runtime.InteropServices.Marshal]::ReleaseComObject($word)|Out-Null}
Remove-Item -LiteralPath $htmlPath,$diagramPng,$edgeProfile -Recurse -Force -ErrorAction SilentlyContinue
Write-Output $outputPath
exit 0

function Set-CellText($cell, [string]$text, [bool]$bold=$false) {
  $cell.Range.Text = $text
  $cell.Range.Font.Name = 'Aptos'
  $cell.Range.Font.Size = 8.5
  $cell.Range.Font.Bold = [int]$bold
  $cell.VerticalAlignment = 1
}
function Add-Table($doc, $headers, $rows, $widths) {
  $start = $doc.Content.End - 1
  $lines = [Collections.Generic.List[string]]::new()
  $lines.Add(($headers -join "`t"))
  foreach ($row in $rows) { $lines.Add((@($row | ForEach-Object { ([string]$_) -replace "[`t`r`n]", ' ' }) -join "`t")) }
  $text = ($lines -join "`r")
  $range = $doc.Range($start,$start)
  $range.InsertAfter($text)
  $tableRange = $doc.Range($start,$start+$text.Length)
  $table = $tableRange.ConvertToTable(1,$rows.Count+1,$headers.Count)
  $table.Borders.Enable = 1
  $table.AllowAutoFit = $false
  $table.Rows.Item(1).HeadingFormat = -1
  $table.Rows.Item(1).Shading.BackgroundPatternColor = 15132390
  $table.Range.Font.Name = 'Aptos'
  $table.Range.Font.Size = 8.5
  $table.Rows.Item(1).Range.Font.Bold = 1
  for ($c=1; $c -le $headers.Count; $c++) { $table.Columns.Item($c).Width = $widths[$c-1] }
  $after = $doc.Content; $after.Collapse(0); $after.InsertParagraphAfter() | Out-Null
  return $table
}
function Add-Heading($selection, [string]$text, [int]$level) {
  $selection.Style = "Heading $level"
  $selection.TypeText($text)
  $selection.TypeParagraph()
}
function Add-Body($selection, [string]$text) {
  $selection.Style = 'Normal'
  $selection.TypeText($text)
  $selection.TypeParagraph()
}
function Add-PageBreak($selection) { $selection.InsertBreak(7) | Out-Null }
function Type-LabelValue($selection, [string]$label, [string]$value) {
  $selection.Font.Bold = 1; $selection.TypeText("${label}: "); $selection.Font.Bold = 0; $selection.TypeText($value); $selection.TypeParagraph()
}

$word = New-Object -ComObject Word.Application
$word.Visible = $false
try {
  $doc = $word.Documents.Add()
  $doc.PageSetup.TopMargin = $word.InchesToPoints(0.75)
  $doc.PageSetup.BottomMargin = $word.InchesToPoints(0.75)
  $doc.PageSetup.LeftMargin = $word.InchesToPoints(0.8)
  $doc.PageSetup.RightMargin = $word.InchesToPoints(0.8)

  $normal = $doc.Styles.Item('Normal'); $normal.Font.Name='Aptos'; $normal.Font.Size=10; $normal.ParagraphFormat.SpaceAfter=5; $normal.ParagraphFormat.LineSpacingRule=0
  $colors = @(0, 0x1F4E79, 0x2F5597, 0x44546A)
  foreach ($level in 1..3) { $style=$doc.Styles.Item("Heading $level"); $style.Font.Name='Aptos Display'; $style.Font.Color=$colors[$level]; $style.Font.Bold=1; $style.ParagraphFormat.KeepWithNext=-1 }
  $doc.Styles.Item('Heading 1').Font.Size=17; $doc.Styles.Item('Heading 1').ParagraphFormat.SpaceBefore=14; $doc.Styles.Item('Heading 1').ParagraphFormat.SpaceAfter=7
  $doc.Styles.Item('Heading 2').Font.Size=13; $doc.Styles.Item('Heading 2').ParagraphFormat.SpaceBefore=10; $doc.Styles.Item('Heading 2').ParagraphFormat.SpaceAfter=5
  $doc.Styles.Item('Heading 3').Font.Size=11; $doc.Styles.Item('Heading 3').ParagraphFormat.SpaceBefore=8; $doc.Styles.Item('Heading 3').ParagraphFormat.SpaceAfter=4
  $selection = $word.Selection

  $selection.ParagraphFormat.Alignment=1; $selection.Font.Name='Aptos Display'; $selection.Font.Size=11; $selection.Font.Color=0x2F5597; $selection.TypeText('CEC IST GTA APPLICATION'); $selection.TypeParagraph()
  $selection.Font.Size=28; $selection.Font.Bold=1; $selection.Font.Color=0x1F4E79; $selection.TypeText('Technical Reference'); $selection.TypeParagraph()
  $selection.Font.Size=15; $selection.Font.Bold=0; $selection.Font.Color=0x44546A; $selection.TypeText('Pages, APIs, SQL schema, relationships, and end-to-end workflows'); $selection.TypeParagraph(); $selection.TypeParagraph()
  $selection.Font.Size=11; $selection.Font.Color=0x666666; $selection.TypeText('Implementation snapshot - August 15, 2026'); $selection.TypeParagraph(); $selection.TypeText('Source: current React routes, ASP.NET Core endpoint mappings, and the live GtaApplication SQL Server database.'); $selection.TypeParagraph()
  $selection.ParagraphFormat.Alignment=0
  Add-PageBreak $selection

  Add-Heading $selection 'How to use this reference' 1
  Add-Body $selection 'This document is the implementation map for the GTA Application migration. It distinguishes browser pages, HTTP endpoints, persistent data, and authorization boundaries, then connects them through workflow traceability.'
  Type-LabelValue $selection 'Inventory' "$($pages.Count) routed page states; $($apis.Count) HTTP endpoints; $((($columns | Select-Object -ExpandProperty Table -Unique).Count)) application/support tables; $($foreignKeys.Count) enforced foreign keys."
  Type-LabelValue $selection 'Architecture' 'React/Vite client -> ASP.NET Core minimal APIs -> application services -> EF Core -> SQL Server. Document binaries are stored outside SQL; SQL stores their metadata and storage keys. Email is written to an outbox and delivered asynchronously.'
  Type-LabelValue $selection 'Security' 'The UI hides inaccessible areas, but API policies are authoritative. Applicant ownership and faculty-to-section assignment are checked again inside services before sensitive data or documents are returned.'

  Add-Heading $selection 'Contents' 1
  foreach ($item in @('1. Page inventory','2. API inventory','3. Data model overview','4. SQL table dictionary','5. Foreign-key relationships','6. End-to-end traceability','7. Conventions and operational notes')) { Add-Body $selection $item }
  Add-PageBreak $selection

  Add-Heading $selection '1. Page inventory' 1
  Add-Body $selection 'Routes are implemented in AppRouter.tsx. Role-specific route shells enforce navigation access and provide shared loading/error behavior.'
  $pageRows = @($pages | ForEach-Object { ,@($_[0],$_[1],$_[2]) })
  Add-Table $doc @('Route','Role / access','Purpose') $pageRows @($word.InchesToPoints(1.65),$word.InchesToPoints(1.25),$word.InchesToPoints(3.95)) | Out-Null
  Add-PageBreak $selection

  Add-Heading $selection '2. API inventory' 1
  Add-Body $selection 'All protected endpoints require the role shown below. Error responses use Problem Details and include a correlation identifier. Development authentication and OpenAPI are not production identity solutions.'
  $apiRows = @($apis | ForEach-Object { ,@($_[0],$_[1],$_[2],$_[3]) })
  Add-Table $doc @('Method','Route','Authorization','Responsibility') $apiRows @($word.InchesToPoints(0.55),$word.InchesToPoints(2.25),$word.InchesToPoints(1.15),$word.InchesToPoints(2.9)) | Out-Null
  Add-PageBreak $selection

  Add-Heading $selection '3. Data model overview' 1
  Add-Body $selection 'The following diagram separates identity/profile, application/catalog, faculty/placement, and operations concerns. Every connector shown corresponds to a SQL Server foreign key; the complete column-level list follows the diagram.'
  $diagram = Join-Path $workspace 'docs\assets\gta-table-relationships.svg'
  if (Test-Path $diagram) { $shape=$selection.InlineShapes.AddPicture($diagram); $shape.LockAspectRatio=-1; $shape.Width=$word.InchesToPoints(6.8); $selection.TypeParagraph() }
  Add-Body $selection 'Reading direction: a User owns at most one ApplicantProfile and many Applications/Documents. Each Application belongs to one ApplicationPhase and contains one or more ApplicationChoices. Each choice targets a CourseSection. Faculty access is granted by FacultySectionAssignments; decisions are stored as FacultyReviewActions; finalized assignments are stored as Placements.'
  Add-PageBreak $selection

  Add-Heading $selection '4. SQL table dictionary' 1
  Add-Body $selection 'Types and nullability below are read from the live GtaApplication database. PK marks primary-key columns; FK references are stated beneath the applicable table. Auditable tables share CreatedAtUtc, CreatedByUserId, UpdatedAtUtc, UpdatedByUserId, and RowVersion. Those audit user-id fields are intentional actor identifiers but are not database foreign keys.'
  $tableNames = @($columns | Select-Object -ExpandProperty Table -Unique)
  foreach ($tableName in $tableNames) {
    Add-Heading $selection $tableName 2
    $purpose = $tablePurposes[$tableName]
    if (-not $purpose) { $purpose = 'Database support table.' }
    Add-Body $selection $purpose
    $rows = @($columns | Where-Object Table -eq $tableName | ForEach-Object {
      $type = $_.Type
      if ($_.Type -in @('nvarchar','nchar') -and $_.Length -gt 0) { $type += "($($_.Length))" }
      elseif ($_.Type -in @('nvarchar','varbinary') -and $_.Length -eq -1) { $type += '(max)' }
      elseif ($_.Type -eq 'decimal') { $type += "($($_.Precision),$($_.Scale))" }
      $flags=@(); if ($_.PrimaryKey) {$flags+='PK'}; if (-not $_.Nullable) {$flags+='Required'} else {$flags+='Nullable'}
      ,@($_.Column,$type,($flags -join ', '))
    })
    Add-Table $doc @('Column','SQL type','Key / nullability') $rows @($word.InchesToPoints(2.55),$word.InchesToPoints(1.7),$word.InchesToPoints(2.6)) | Out-Null
    $refs=@($foreignKeys | Where-Object ChildTable -eq $tableName)
    if ($refs.Count) { Add-Body $selection ('Relationships: ' + (($refs | ForEach-Object { "$($_.ChildColumn) -> $($_.ParentTable).$($_.ParentColumn) ($($_.DeleteRule))" }) -join '; ') + '.') }
  }
  Add-PageBreak $selection

  Add-Heading $selection '5. Foreign-key relationships' 1
  Add-Body $selection 'All configured relationships use restricted/no-action deletion. Records must be retired or dependent rows handled explicitly; deletes do not cascade through the business graph.'
  $fkRows=@($foreignKeys | ForEach-Object { ,@("$($_.ChildTable).$($_.ChildColumn)","$($_.ParentTable).$($_.ParentColumn)",'Many-to-one',($_.DeleteRule -replace '_',' ')) })
  Add-Table $doc @('Child column','Parent key','Cardinality','Delete behavior') $fkRows @($word.InchesToPoints(2.45),$word.InchesToPoints(2.25),$word.InchesToPoints(1.15),$word.InchesToPoints(1.0)) | Out-Null

  Add-Heading $selection 'Relationship rules not expressed as foreign keys' 2
  foreach ($rule in @(
    'ApplicantProfiles.UserId is unique, creating a one-to-one User-to-profile relationship.',
    'ApplicationChoices prevents duplicate ApplicationId + CourseSectionId pairs.',
    'Applications prevents duplicate ApplicantUserId + ApplicationPhaseId pairs and uses a unique human-readable Reference.',
    'Only one active document is allowed per OwnerUserId + Type; earlier uploads are superseded.',
    'Only one active placement is allowed per ApplicationChoice and placement capacity/workload is enforced transactionally.',
    'Faculty visibility depends on an active FacultySectionAssignment matching the choice''s CourseSectionId.',
    'AuditLogs.ActorUserId, ApplicationStatusHistory.ChangedByUserId, and SectionImportBatches.ImportedByUserId are recorded identifiers without database FKs so historical evidence can survive identity lifecycle changes.'
  )) { Add-Body $selection "- $rule" }
  Add-PageBreak $selection

  Add-Heading $selection '6. End-to-end traceability' 1
  Add-Body $selection 'This matrix shows how user-visible work travels from pages to APIs and persistent data. Services between the API and EF Core enforce readiness, ownership, faculty authorization, capacity, concurrency, auditing, and email side effects.'
  $workflowRows=@($workflows | ForEach-Object { ,@($_[0],$_[1],$_[2],$_[3]) })
  Add-Table $doc @('Capability','Pages','Primary APIs','Main storage') $workflowRows @($word.InchesToPoints(1.2),$word.InchesToPoints(1.55),$word.InchesToPoints(2.0),$word.InchesToPoints(2.1)) | Out-Null

  Add-Heading $selection 'Application lifecycle' 2
  Add-Body $selection 'Applicant profile + required documents -> open ApplicationPhase -> submitted Applications row -> one ApplicationChoices row per selected CourseSection -> faculty Interview/HireRecommendation actions -> administrator Placement -> Applications state synchronized to Selected. Withdrawal creates a status-history entry and deactivates conflicting workflow state when policy permits.'
  Add-Heading $selection 'Authorization lifecycle' 2
  Add-Body $selection 'Users receive Roles through UserRoles. Applicant operations require the Applicant role plus record ownership. Faculty operations require the Faculty role plus an active FacultySectionAssignment connecting that user to the target CourseSection. Administration operations require the Administrator role.'
  Add-Heading $selection 'Operational side effects' 2
  Add-Body $selection 'Important mutations write AuditLogs with the request correlation id. User-facing notifications are inserted into EmailOutboxMessages in the same application workflow, then a background dispatcher attempts delivery and records sent/failed state.'

  Add-Heading $selection '7. Conventions and operational notes' 1
  foreach ($note in @(
    'Identifiers are uniqueidentifier values generated by the application. Most mutable domain records use SQL rowversion for optimistic concurrency.',
    'Enum values are persisted as integers. ApplicationState: Draft=1, Submitted=2, UnderReview=3, Interview=4, Selected=5, NotSelected=6, Withdrawn=7. EmploymentBasis: PartTime10Hours=1, FullTime20Hours=2. ReviewActionType: Interview=1, HireRecommendation=2.',
    'Document binaries are not database columns. Documents.StorageKey points to infrastructure/document-storage; Sha256 supports integrity checks and duplicate awareness.',
    'SystemSettings contains non-secret settings only. Secret connection strings and credentials belong in environment configuration, never in this table.',
    '__EFMigrationsHistory is an EF Core support table. SQL Server sysdiagrams may exist when database diagrams are created, but it is not part of the application domain model.',
    'This reference describes the implementation present on August 15, 2026. Regenerate it after routes, endpoint mappings, migrations, or entity relationships change.'
  )) { Add-Body $selection "- $note" }

  foreach ($section in $doc.Sections) {
    $header=$section.Headers.Item(1).Range; $header.Text='GTA Application | Technical Reference'; $header.Font.Name='Aptos'; $header.Font.Size=8; $header.Font.Color=0x777777
    $footer=$section.Footers.Item(1).Range; $footer.ParagraphFormat.Alignment=2; $footer.Font.Name='Aptos'; $footer.Font.Size=8; $footer.Text='Implementation snapshot | August 15, 2026  |  Page '
    $footer.Collapse(0); $footer.Fields.Add($footer,-1,'PAGE') | Out-Null
  }
  $doc.SaveAs2($outputPath,16)
  $doc.Close()
} finally {
  $word.Quit()
  [Runtime.InteropServices.Marshal]::ReleaseComObject($word) | Out-Null
}
Write-Output $outputPath
