# Design

## Context

- **The images today:** `tools\New-ReadmeScreenshot.ps1` builds a made-up scene, runs `tools\ReadmeScene` (the real console over scripted Claude, `gh` and `az` answers) in its own Windows Terminal window, plays `new-session.keys` through `KeyPlayer`, and `FrameRecorder` captures a frame every 100 ms into `assets\readme\new-session.gif` (ffmpeg). Steps are `type`, `press`, `focus` and `mark`.
- **What the scene cannot do:** run a real `winget install`, show Windows Terminal's own new-tab menu (it is not Cader's window content, though it is inside the captured window), open a real browser, or close and reopen a terminal.
- **Pull request key:** Enter on a Sessions row with a found pull request raises `OpenPullRequest`, handled by `OpenPullRequestHandler`, which calls `IDesktop.OpenAsync` with the URL. The scene must not launch a browser.
- **Privacy rule:** README images show only Northwind.* names and made-up paths (`readme-front-page`).

## Goals / Non-Goals

**Goals:** a hero GIF that carries a viewer from "new session" to "my sessions are still here after a restart" in under 20 seconds and about 1.5 MB; a made-up still of the profile menu; an owner-recorded walkthrough video with a checklist that keeps it free of personal data.

**Non-Goals:** recording a real `winget install` or a real browser in the GIF; changing Cader's behaviour; hosting the video outside GitHub; a spec change.

## Decisions

- **Install and setup are text, not frames.** Two commands read faster as code blocks than as animation and can be copied. The README already carries them; the change only verifies them against the live package. Alternative rejected: typing them in the GIF, which adds 10 seconds of waiting for no information.
- **The profiles are a still, not an animation.** `profiles.png` is a screenshot of the new-tab menu in the scene's Windows Terminal window, whose settings come from the scene folder's fragment copy, never the owner's fragments folder (`windows.md`: the fragment is read, never written, by the scene). If the menu cannot be captured inside the window's client area, the fallback is a cropped screenshot taken by hand from a clean Windows Terminal profile; it is then checked for personal profile names before commit.
- **The pull request step uses a scene `IDesktop`.** `SceneHost` already swaps the machine's parts for the scene's, so it gets a recording `IDesktop` that opens nothing; the status line's "Opened #12 in the browser." is what the GIF shows. The scene's session gets a pull request in `scene.json` so the PR column is filled in. A fake in the scene is untested wiring unless it decides something, as `README.md` in `tools\ReadmeScene` already says.
- **"Restart" is a console restart, not a terminal restart.** The recording stops the console, starts it again over the same scene state and shows the Sessions tab with the same rows and Enter resuming one. This is honest: Cader's state and Claude's supervisor outlive the console, and it is exactly what a reopened terminal does. A new `restart` key-script step ends the console and starts it again; the scene host owns that loop. Alternative rejected: really closing and reopening the Windows Terminal window, which loses the capture target (`WindowCapture` holds its handle) and shows nothing more.
- **The video is owner-recorded.** Claude cannot record a real desktop. The change delivers a shot list and a pre-flight checklist (clean `%LOCALAPPDATA%\Cader`, throwaway Northwind repository, made-up profile list, no browser tabs or notifications visible, 1080p, no audio), and the owner uploads it as a GitHub attachment, linking it from the user guide. The video is optional; the change is complete without it.
- **Size budget.** More seconds means a bigger GIF. If 15 to 20 seconds exceeds 1.5 MB at the current palette, lower the frame rate for the idle stretches (`FrameSchedule`) and crop to the console's content before dropping frames from the action.

## Risks / Trade-offs

- **The winget package is not live yet**, so the install text cannot be verified until it is; the apply step waits for it (see Open Questions).
- **README conflict:** `readme-front-page` edits the same sections and is still in progress; apply after it archives, or rebase.
- **The restart step is more wiring in a tool that is mostly untested**; keep it to the loop and put every decision (what state survives, what the script step is called) in `KeyScript`, which is tested.
- **A GIF that is too fast to read.** Hold each result 1.5 seconds and check the frames by eye before commit.

## Open Questions

- **Does winget-pkgs PR 479528 merge before this is applied?** Apply waits for the package to resolve with `winget search Source191.Cader`.
- **Where should the video be uploaded?** Assumed a GitHub release or issue attachment, so the owner chooses at upload time; the change only needs the link.
