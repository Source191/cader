# ReadmeScene.Tests

Tests for the [ReadmeScene](../ReadmeScene/README.md) tool, kept beside it rather than in the application suites: the scene file, the scripted Claude Code, gh and az answers, the scene's process table, the start-up keys, the key-script format and the frame schedule. They read the shipped `scene.json` and `new-session.keys`, so a change to either is checked here.

- **`InvariantGlobalization`:** the tests run in the invariant globalisation mode the shipped tool runs in.
- **Reference to TestDoubles:** `FakeProcessRunner` and `FakeProcessRegistry` stand in for the real programs and process table the scene hands everything else to.
