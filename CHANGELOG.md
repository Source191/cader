# Changelog

What changes in each release of Cader, written for the people who use it. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and versions follow [Semantic Versioning](https://semver.org/). Internal lists changes nobody using Cader can see; Cader never shows it, or the Unreleased section, in its What's new dialog.

## Unreleased

### Added

- Origin rows for branches origin has deleted now leave the Worktrees tab by themselves: after a scan that shows a ○ origin row, Cader prunes that repository's records of origin's branches in the background, at most once an hour and never prompting, so a local branch whose upstream was deleted also reads ✗ gone without a manual git fetch --prune (#365)
- Both tabs now refresh within about two seconds of a change instead of waiting for their timers: the Sessions tab when Claude Code records a session starting, ending or changing state (so the bell for a session that starts waiting rings promptly), and the Worktrees tab, even while hidden so it is current when shown, when a worktree is added or removed, a branch is checked out, or a folder appears in or leaves a repository root; Cader only notices which files changed and never reads them, the timers stay as a safety net, and a burst of changes starts at most one refresh a second (#347)
- Sessions refresh and Worktrees refresh settings on the Config tab choose how often the sessions are listed (every 5 seconds until you change it) and the shown Worktrees tab rescans (every 15 seconds), in whole seconds from 0 to 3600, where 0 turns that timer off; a saved interval applies at once, and F5 refreshes the shown tab now on every tab (#346)
- The Sessions tab shows which Claude account each session belongs to, so you know which one to log in as before resuming it: a new ACCOUNT column gives the account's label, with ● for the one logged in now and ? for one Cader has not yet seen logged in, and the detail pane's new Account line gives its email and organisation and says whether to log in as another; Cader remembers each account it sees logged in as 1, 2 and so on, the Config tab lists them under an Accounts heading, and Enter on one renames its label, for example to work or personal. Sessions from before Claude Code 2.1.325 show no account, and two logins in one Team or Enterprise organisation share a label (#337)
- The Config tab marks a repository root whose folder is not found as (not found), and its tooltip says to create the folder or remove the root, so a root the new-session wizard leaves out no longer just disappears (#336)
- The new-session wizard lists each repository root under Folders (no repository), so a session for work across repositories can be started from the console, not only with cader here; the list is grouped under Recent, Folders (no repository) and Repositories, the selection steps over the headings, and a folder choice starts the session in the folder whatever Where says, fetching nothing (#335)
- The diagnostic log, the crash file and a problem report now end their counts with a process now line giving how many handles and how much private memory Cader holds at that moment, so a console that slowly grows shows it between two 10-minute marks; a figure Windows will not give reads as unknown (#330)
- A BRANCH column on the Worktrees tab flags each row's branch: ▲ left behind when it has commits after its pull request merged (squash merges included), ✗ gone when origin no longer has it as of your last fetch, ✗ closed when its pull request was closed without merging, and · no PR when its host has none; the detail pane gives the reason in full, a branch or origin row's pane shows its repository and the commits d would lose rather than the main checkout's changes, and the table's title counts worktrees and branches apart (#313)
- The Worktrees tab now lists each local branch checked out nowhere as a ⑂ branch row, and a branch of yours only on origin as a ○ origin row once its pull request shows it left behind or closed; on a branch row d asks before deleting the local branch and refuses when it holds commits on no remote branch and outside its merged pull request, D deletes it after you type its name, and an origin row is never deleted; n, o, f, s and Enter say the branch has no worktree, while p, c and P work as on any row (#312)
- d on the Config tab's Diagnostic log file row clears the diagnostic log: it asks first, naming cader.log and cader.log.1 with their sizes, then deletes both, keeping the events a problem report carries; each log row now shows its file's size, and a Diagnostic log file (older) row shows cader.log.1 while it exists (#304)
- The diagnostic log records how many repositories the new-session wizard offered and when its filter first changed and with what kind of character, never the character itself, so a problem report can show why the list was empty (#298)
- A Before branching setting on the Config tab, and a Before branching choice in the new-session wizard: Fetch origin, Pull default branch or Nothing, for what happens before a new worktree branches from origin's default branch (#273)
- A native build for Arm Windows 11: each release carries cader_<version>_win-arm64.zip beside the x64 one, and winget installs the one that matches your machine (#93, #253)
- A Diagnostic log file row on the Config tab shows the full path of cader.log, which you can select and copy, and f on it or on Settings file opens the file in your editor; the help and user guide now say what a problem report holds and that cader.log is never attached to it (#274)
- A Diagnostic log setting on the Config tab: Enter turns the log file on or off at once, and its tooltip names %LOCALAPPDATA%\Cader\cader.log, which becomes cader.log.1 at 10 MB so at most about 20 MB is kept (#254)

### Changed

- Starting a session from the new-session wizard now says which step it is on, such as Starting fix-123: starting Claude Code (3 of 5)..., so a slow step shows as itself rather than looking stuck; Cader's own timed refreshes and the refreshes for changes it notices wait until the launch ends and then run once, so they no longer slow it down (F5 still refreshes at once); and once you press Enter on a repository, or move on from the list, with Fetch origin or Pull default branch chosen for a new worktree, its default branch is fetched while you type the name, so F5 usually finds it already fetched (Pull's fast-forward still waits for F5) (#366)
- Both tables now group their rows by repository: an orange heading with a faint rule names each repository (or, on the Sessions tab, the folder outside any repository, after folder gone: when it no longer exists), groups run A to Z ignoring case with folders last, and the arrow keys step over the headings. On the Sessions tab the REPOSITORY column has gone and NAME takes the spare width; within a group needs input still comes first, but a session needing input is no longer always at the top of the table, so check the needs input count beside the tabs. On the Worktrees tab the WORKTREE cell no longer repeats the repository (#348)
- A session's tab now keeps the session's name as its title while the session works, instead of showing a spinning glyph before it: sessions that Cader starts or resumes from now on are told to turn Claude Code's own terminal title updates off, which also stops it asking a small model for a title; sessions started earlier keep their glyph until you resume them, and Enter still finds their tabs (#331)
- The Worktrees tab now reads several repositories and worktrees at once, at most eight git commands at a time, so a scan that took 7 to 12 seconds over thirty-odd repositories takes a fraction of that; the rows and their order are unchanged (#308)
- The diagnostic log now names the Cader process on every line, so two consoles writing to cader.log can be told apart; records a program that cannot start once per folder, with Windows' error number, instead of on every refresh; names the Terminal.Gui method behind a warning and the work behind a frozen screen; and its counts say they are since start (#303)
- The Sessions tab's detail pane sets a session's facts in two columns of five rows, so related facts line up: Waiting, Folder, Worktree, Branch and Changes on the left, Started, Prompted, Session, Tab and Memory on the right; the pane is one row taller (#311)
- The Config tab's Enter and f tooltips are shorter, one sentence each like every other key's: Enter edits the setting (all roots, one per line) or says how to change the setup, and f opens the folder, or the settings or log file, in your editor (#307)
- Cader now runs on .NET 11, which it carries itself: the download grows from 33.6 to 36.0 MiB zipped (the program from 38.5 to 41.2 MiB) and start-up is unchanged at about 550 ms; on x64 it needs a processor with the x86-64-v2 instructions, which Windows 11 already requires (#280)
- A new worktree from origin's default branch now fetches that branch first, so its session starts on an up-to-date base; when the fetch fails or the remote needs signing in, the session still starts and the status line says why. Set Before branching to Nothing for the old behaviour, or to Pull default branch to fast-forward your local default branch as well (#270, #273)
- CADER_LOG is no longer read and the Diagnostic log setup row has gone; turn on the Diagnostic log setting instead, and the log is written to %LOCALAPPDATA%\Cader\cader.log (#254)
- A state file holding null where a value must be, leaving a value out or naming a key twice is now read as corrupt, as an unreadable file already is (#248)

### Fixed

- Removing a merged worktree whose branch origin has deleted no longer reports a failure and keeps the branch: Cader deletes the branch it created once the worktree is gone, and if git still refuses, the status line says the worktree was removed and the branch kept, with git's reason; a session that has started is never reported as a failed launch when the listing after it fails or Cader's state folder refuses the recent-repository record (#379)
- Saving a setting on the Config tab no longer crashes Cader, or is refused, because a repository root's folder or a custom editor's program has gone; each is checked only in the dialog that edits it (#358)
- A refresh no longer throws a long table back to its top: the Sessions, Worktrees and Config tables keep the rows you were looking at on screen when they refresh, and moving the selection past the last row on screen now scrolls the table to keep it in view (#351)
- A stray space typed along with n no longer empties the new-session wizard's list: the wizard's filter and the tables' / filter ignore spaces and invisible characters before the text, and a new session's name loses spaces at either end (#339, #345)
- Renaming a session just moved to the background with /bg no longer loses it: Claude Code gives such a session a transcript with no messages until its next turn, so rename now says it has no messages yet and to send it something first, instead of removing it and starting a copy that fails with "source session not found" (#327)
- Enter on a working session whose tab is open switches to that tab again, instead of saying the tab could not be found: Claude Code puts a spinning glyph before a working session's tab title, and Cader now finds the tab with or without it (#326)
- A branch whose pull request was rebased before it merged, or that was moved forward to main and used again after its merge, no longer reads ▲ left behind on the Worktrees tab: only commits after the merge whose changes main lacks now count, and merge commits never do (#325)
- A console left open no longer grows in handles and memory: each git and claude command Cader ran left its two output pipes open, so one open for 16 hours held over 100,000 handles and twice its usual memory (#318)
- A session whose folder has been deleted now reads folder gone: in its REPOSITORY cell and detail pane, with a tooltip saying d deletes it; Enter, rename and resume-all leave it alone, and Cader no longer starts git in that folder every 30 seconds, which filled the diagnostic log (#301)
- cader setup-terminal now changes Windows Terminal's settings file more carefully: it keeps a copy of the file as it was beside it and names it, keeps the file's permissions, writes nothing when Terminal saved the file meanwhile, when the file is read-only (giving the entries to add by hand) or when the new file would not hold exactly the profile list it should; and --remove now also takes out the entries setup added there, leaving any you customised (#275, #294)
- Cader now follows CLAUDE_CONFIG_DIR: when it is set to a full path, adopt, rename, resume and Enter look for transcripts there, the Claude bell and cader setup-terminal use that folder's settings.json, and a new Claude folder row on the Config tab names the folder (a relative or ~ value is not used, and the row says so) (#276)
- A Cader profile deleted in Windows Terminal's settings no longer stays hidden after cader setup-terminal: setup now shows the one entry Terminal needs to show it again and, if you agree, adds it to Terminal's settings file and changes nothing else there; a settings file that is a link, as a dotfiles set-up makes it, is never written and you are given the entry to add by hand; the Config tab's Windows Terminal profiles row says when Terminal is hiding one (#218, #275)
- Opening a folder in VS Code with f no longer writes VS Code's start-up messages, such as a Node.js warning, over the screen (#252)
- Removing a worktree whose folder a tab or program still had open no longer leaves an empty folder Cader cannot see: Cader waits a few seconds for it, finishes the removal and its branch either way, and the Worktrees tab lists any empty folder under the worktree root as empty, which d deletes (#261)

### Internal

- The origin pruner no longer passes an unread true or false through its failure handling, which a mutation run flagged as untested (#368)
- Each refresh allocates less: the process table names a process with one string and reads a parent's image name into a stack buffer, the worktree listing is parsed as spans, transcripts are read as bytes with Utf8JsonReader so a long message line is never decoded, the table paints and colour schemes are shared, and a table works each cell out once; allocation-budget facts hold each path (#369)
- The nightly Claude Code contract check expects claude auth status to exit 1 when no one is logged in, as it does on the runner, instead of failing; the tab title rule and the new-session wizard's list drop a guard and a duplicate walk that mutation testing could not tell apart, and two new facts pin a glyph straight before a session's name and a heading clicked with nothing selected (#340)
- The process table's handles are now SafeHandles closed as each read ends, the tab switch keeps one UI Automation client and releases the rest of each search instead of leaving them to the garbage collector, the tests dispose every process they read, and the build fails on a disposable left undisposed outside the Terminal.Gui layer (#320)
- The first part of tracking branches with no worktree, which nothing shows yet: the pull request lookups now record each pull request's head commit (gh's headRefOid, az's last merge source commit), git can list every branch with its upstream and whether its tip is yours, and count a branch's commits after a merged head or on no remote, and deleting a branch passes its name after --, so one named like a switch is still a name (#309)
- A release's mutation run leaves static mutants, which Stryker's test runner cannot judge in a short run, to the weekly full run and says how many it set aside, so the mutation issue lists only real gaps; three values no test read are now pinned, and the wizard filter's diagnostic reads a broken first character instead of tripping a mutant (#305)
- The mutation testing marks that called a mutant equivalent are mostly gone: the code they pointed at is simpler or a test now tells the mutant apart, and only true equivalents keep a mark (#296)
- Cader is built for .NET 11 and its SDK 11.0.100: every project targets net11.0, the CI jobs install that SDK, and the packages and breaking changes are checked, with the Microsoft.Extensions assemblies .NET 11 now carries left out of the build (#277)
- .NET 11's process APIs are checked against how Cader starts, captures and ends programs: ending a session's or tab's process now looks it up with .NET 11's TryGetProcessById, and the rest are declined, with facts that show why, so nothing a user can see changes (#278)
- C# 15 outside Core: every type hierarchy in the console's screen and the tools is a closed record, so a switch over one needs no fallback and a new case fails the build; Core stays on C# 14 until the mutation testing tool can compile C# 15, and the rest of the C# 15 and .NET 11 features are declined with the reasons recorded (#279)
- No parameter has a default value any more, so every call states the values it passes, and a convention check fails for a new one (#267)
- Mutation testing of Domain and Ports: facts now pin the mutants the weekly run left alive (equality of settings and editors, the wording of exceptions, the edges of fits and orderings), and the ones no test can tell apart carry a Stryker comment with the reason, so nothing a user can see changes (#282)
- Mutation testing of the diagnostics and the worktree scan: facts now pin every word a child command may name, the edges of crash lines, heartbeat gaps, code names and process counts, and the scan of registered worktrees, leftovers and gitdir pointers, and the mutants no test can tell apart carry a Stryker comment with the reason, so nothing a user can see changes (#283)
- Mutation testing of the rest of Application: facts now pin the help text and refusal layout of every command, the wording of the detail pane, the setup rows, the outcomes and the release notes, the edges of the key bar, the location cache and the memory figures, and the mutants no test can tell apart carry a Stryker comment with the reason, so nothing a user can see changes (#284)
- Mutation testing of the console's views, the detail pane, the problem report and the actions: facts now pin the edges of lists, rows and ages, the wizard's opening state, the detail pane's columns and spans, the order a problem report leaves lines out, and the folder, tab and session actions' guards, and the mutants no test can tell apart carry a Stryker comment with the reason, so nothing a user can see changes (#285)
- Strict mutation runs after a release: a release whose run counts 20 or more mutants and leaves one alive now fails that run and says so on the mutation issue, and the static word lists of the diagnostics, which the mutation tool cannot test, carry a Stryker comment with the reason, so nothing a user can see changes (#286)
- The keys in the help dialog are laid out two to a line without a separate case for a last key on its own, which no help has and which the mutation run could not reach; the help reads the same (#315)

## 0.21.1 - 2026-11-13

### Internal

- System.CommandLine 3.0.0, Microsoft.Extensions.Logging.Abstractions and Microsoft.Extensions.FileSystemGlobbing 11.0.0, SonarAnalyzer.CSharp 10.36.0.15337 and Microsoft.Extensions.TimeProvider.Testing 11.0.0 (#246)

## 0.21.0 - 2026-11-13

### Changed

- Vantage is now Cader: run cader, cader here and cader setup-terminal; install with winget install Source191.Cader; settings and state live in %LOCALAPPDATA%\Cader, the Windows Terminal profiles are Cader and Cader session, the variables are CADER_LOG and CADER_NO_UPDATE_CHECK, and the icon is a C with a star. Nothing is carried over from %LOCALAPPDATA%\Vantage (#219)
- U upgrades with winget, and the daily update check asks the winget community repository, so a release is offered only once winget can install it (#206)
- The Worktrees table's LAST COMMIT column is right-aligned, as AGE is on the Sessions tab, so the durations line up (#190)
- Removing a worktree on a detached head, or whose head git could not name, deletes no branch and asks about none; the folder's name no longer stands in for a branch (#186)
- The update notice in the settings row is amber (#180)
- The header is one grey settings row with the console's version and memory and Claude Code's version; the repository roots and worktree root moved to the Config tab, so every table gains rows (#117)
- A status line appears under the settings row only when there is something to say, marked by its tone (✓ done, ! refused, ✗ failed, a spinner while working), wraps up to three rows and can be closed with ✕ (#129)
- Enter on a session opens it in its own Windows Terminal tab, or switches to that tab; the console never attaches in its own tab, and F5 in the wizard starts the session and opens its tab (#128)
- cader here says "Ctrl+Z detaches; the session keeps running." before it attaches (#128)
- f opens a row's folder and s its solution on every tab; on Worktrees n starts a session in the worktree and Enter opens the session already running there (#161)
- The Sessions table reads NAME, TAB, STATE, REPOSITORY, PR, AGE and the Worktrees table WORKTREE, HEALTH, LAST COMMIT, SESSION; the other facts moved to the detail pane (#119)
- The new-session wizard is one column read top to bottom and fits a 120 x 36 terminal (#122)
- The wizard's Tab and Enter follow the form's order, Enter never closes it, repository roots are no longer offered as choices, and the wizard for a worktree is compact; the solution chooser is a list (#105)
- R resumes every stopped or failed session the table shows, each in its own tab, without a dialog; a / filter narrows it (#163)
- A session's tab closes when its attach ends, and stop, delete, rename and adopt close or move the tab; opening a tab checks the old one is still alive (#102)
- Pull requests are looked up per branch for the rows on screen, refresh within about 75 seconds, run at most four lookups at once, and the Azure DevOps link is built from the remote (#99)
- Session names may hold any character, including ; and " (#84)
- The tab palette is reordered so the console's tab and the first session tabs stay apart under red-green colour blindness, and dialog fields are easier to read (#165)
- Grey text is lighter, for contrast (#164)
- The TAB swatch is drawn in the shade Windows Terminal shows for that tab while another is selected (#124)
- The Visual Studio preset opens the newest install found by vswhere (#125)
- A missing editor or solution application is reported in plain words and points to the Config tab (#178)
- cader --version prints exactly the package version, without the commit (#86)
- The package holds only Windows native assets and English resources (#92)

### Added

- Installs with winget install Source191.Cader as a self-contained program, with no .NET to install first; winget brings Windows Terminal, PowerShell 7 and git when they are missing, and every release carries the zip for a machine without winget (#206)
- U opens a What's new dialog listing the changes in every release since the running one, with Update now, Release page and Later; a newly found release is announced once in the status line, and the first start after an update shows what changed, once (#180)
- Sessions, Worktrees and Config are tabs of one screen, each title carrying its count with the state counts and glyphs beside it, and a two-line key bar at the bottom (#114)
- A Config tab lists every setting and the setup it reads (settings file, terminal profiles, Claude's bell, diagnostic log, last update check); Enter edits a setting in a small dialog and saves at once, and setup that needs attention says what to run (#117)
- Left and Right, or a click on a title, move between the tabs; Esc on Worktrees or Config with no filter returns to Sessions (#117, #183)
- A detail pane under the Sessions and Worktrees tables shows the selected row's facts: what a session is waiting for, when it started and was last prompted, folder, session id, worktree, branch, tab, changed files, unpushed commits, last commit, memory and health with its reason (#119)
- The Config tab describes the selected setting, and a Setup heading separates the settings from the setup rows (#165)
- Browse (F3) in the repository roots and worktree root dialogs opens a folder picker inside the terminal (#125)
- A Default place setting, New worktree or Current branch, chooses where the new-session wizard starts (#125)
- m on a running session opens a breakdown of its memory: Claude Code, then each program it started (MCP servers, shells), largest first (#106)
- ! opens Report a problem: a preview of privacy-safe diagnostics that can be opened as a pre-filled GitHub issue in the browser or copied; nothing is sent automatically (#101)
- Recent diagnostic events are kept in memory and written to last-crash.log in %LOCALAPPDATA%\Cader after a crash (#100)
- Automated Claude runs (claude -p, the Agent SDK) are kept out of the Sessions table and shown in grey in its counts with their memory (#121)
- Every key and column has a tooltip, the help dialog scrolls, and < and > scroll a wide table sideways so every column can be reached (#98)
- The STATE column shows a glyph beside each state, and the TAB column a swatch in the tab's colour (solid for a session tab, hollow for a plain claude) (#113)
- The Worktrees table gains a HEALTH column with a glyph and word per health, and an unknown session state reads ? so it differs from stopped by shape (#164)
- The Worktrees tab is scanned as the console opens and rescanned quietly each time it is shown, so its rows are usually ready at once (#139)
- cader --help carries a support line (#88)
- Rider installed by JetBrains Toolbox 2.x is found; presets that cannot be found are marked (not found) in the Open folder with dialog; F3 there browses for a Custom editor executable (#178)
- With CADER_LOG set, the log records refresh timing by part, UI stall counts, and typical durations and counts every 10 minutes (#179)

### Fixed

- A stop, removal or listing Claude Code refuses is reported in its own words: x keeps the tab, r, R and d resume, close and remove nothing after a refused removal, and cader here erases its detach hint and restores the terminal when the attach fails (#187)
- Deleting a session whose worktree's repository has gone, or that git cannot inspect, keeps the worktree and says why; a leftover folder a tab or program holds open is reported as in use; a junction or an unreadable subfolder inside a leftover no longer fails the Worktrees scan; a new worktree is recorded before its include files are copied, so a failed copy leaves it known to Cader; adopt and close-empty finish when part of a process tree has already gone, and close-empty closes a tab whose root process is claude (#186)
- State files are written beside and moved over and created worktrees are one file each, so an interrupted save keeps the previous settings and two running copies cannot lose each other's records; a config.json that cannot be read, or one holding a number for the default base or place, starts with the defaults instead of ending it (#185)
- A GitHub remote with a user name before github.com, as Git Credential Manager writes it, shows its branches' pull requests (#188)
- A session just stopped or deleted no longer comes back for a few seconds, rows that arrive while a dialog is open are kept when it closes, the spinner keeps turning until every action has reported and stays through a filter typed before the first listing, Esc clears the status message with the filter, and a child program that has not answered after 60 seconds is ended and reported instead of running on (#184)
- R continues each session in its own row instead of leaving a duplicate, and a session just started or resumed shows as starting rather than /blocked (#162)
- Times in the detail pane and the Config tab are shown in the local time zone; Rider's newest install is chosen by version number; changed files with non-ASCII names are listed as typed (#159)
- Ignored files with non-ASCII names, spaces or apostrophes are copied into a new worktree (#82)
- A folder holding a semicolon opens its tab correctly (#84)
- Quitting restores the tab's own colour, and the return hint before an attach is erased afterwards (#95)
- cader here -d x names -d as the unknown option (#95)
- A NUL character in a state file is read as a bad value rather than crashing at start-up (#166)
- Several status messages that named retired keys now name the current ones (#174)

### Removed

- No longer a dotnet tool on nuget.org: install with winget, and try a preview from the zip on its GitHub prerelease, which tools\install-local.ps1 -FromArtifact extracts (#206)
- The a key (native agent view); run claude agents in any shell instead (#95)
- The w and , keys and the settings dialog, replaced by the Config tab (#117)
- o on the Sessions tab, F6 in the wizard and starting a session without a tab (#128)
- The e and v keys, replaced by f and s (#161)
- The Cursor editor preset; a config.json naming Cursor reads as VS Code (#125)

### Security

- git, gh, wt.exe and pwsh.exe are found once on the PATH and run by full path, never from the current directory; the console exits when git is missing (#110)
- Claude Code must be claude.exe (a claude.cmd or claude.bat alone is refused), a custom editor must be a full path, and an opening prompt starting with - is refused (#110)
- Only safe branch names reach az, pull request links open only over https, close-empty and adopt check a process is still the session's, and the update tab installs from nuget.org only (#110)
- Deleting a leftover folder refuses a relative path, a drive root or a share root (#83)
- Leftover folders are deleted only inside a repository root or the worktree root, a prunable worktree that still holds files is refused, and a .git file pointing at a live checkout is never treated as a leftover (#166)
- Relative PATH entries are never searched, the tab shell is found only through real ancestors, and session ids from outside the program must have the CLI's shape (#166)
- Diagnostics never record paths, names, prompts or program output, and the CADER_LOG file no longer carries free text (#100)

### Internal

- Guard rule: no null guards inside the solution, since nullable reference types carry null, and ArgumentNullException banned in src so the build keeps it that way (#207)
- Small repetition removed: one home each for the crash-line test, tab ownership, palette colours, the pull-request parse and the escape scanner; failed scans recorded once per folder (#199)
- RemovalDecision, WorktreeObservation, Session and RemoteRepository are closed hierarchies with their cases nested, held by an Architecture fact for Domain and Ports (#198)
- Ports take Domain values where one exists, a folder is deleted only through IFolderDeleter held by WorktreeRemover, and IDesktop.OpenAsync names what it throws, each guarded by a fact (#197)
- Every process read opens one limited handle, a process is ended only as a ConfirmedProcess its three checks mint, and a command line never leaves ProcessRegistry.UsageOf (#196)
- A session's listed name is a typed value, named or unnamed, decided once as the listing is parsed, and the Claude CLI port has one resume method (#195)
- TableBody's column sizing, tooltips and detail reading each in a class of their own, one middle ellipsis in TextFit, IScreenDefinition slimmed to a Tab and a TabRefresh, and the key bar's removed labels disposed (#194)
- Each view state holds its ListSelection directly; SelectionKey and the With copies are gone (#193)
- Every dialog title and button, the waiting words, the STATE words and the wizard's preview are declared in Wording and pinned by a fact, and a source fact keeps sentences out of the rest of Application (#191)
- Convention nets: a banned-symbols list for src, layering, reducer, handler, diagnostic-line, key-parity and source-text facts, and UnreachableException in the closed discards (#189)
- Smoke test of the packed tool in CI, its script built from Wording (#75)
- Prerelease versions are published as GitHub prereleases (#78)
- Repository guards for going public: code owner and approval for nuget.org publishing (#80)
- Faster CI checks and a documentation-only fast path (#96)
- Dependabot pull requests raise Version automatically (#107)
- CONTRIBUTING.md added as the contributor's guide (#115)
- C# 14 idioms held in .editorconfig (#116)
- The null programme: no null in Domain, Ports and Application, guarded by NullFreeModelsShould (#148)
- One MaybeNullWhen(false) shape for every Try method's out parameter (#175)
- Every class that can be sealed is sealed, guarded by SealedTypesShould (#158)
- One top-level type per file, guarded by OneTypePerFileShould (#167)
- Program.cs's decisions moved into tested Application classes (#177)
- CLAUDE.md split into path-scoped rule files, and comments citing external APIs link to their documentation (#152)
- README, CONTRIBUTING and CLAUDE.md corrected and trimmed before going public (#174)
- System.CommandLine 2.0.14 (#108)
- One ViewUpdate record and one RowAction for every tab reducer, and the console keys q, ?, U and ! answered once in SharedKeys (#192)

## 0.16.1 - 2026-10-27

Release notes start with the next release; earlier releases keep their notes on their GitHub Releases.
