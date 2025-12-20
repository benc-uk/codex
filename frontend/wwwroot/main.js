// ================================================================================
// main.js
// The main entry point for the Codex WebAssembly application.
// Sets up the .NET runtime and connects JS imports/exports.
// ================================================================================

import { dotnet } from "./_framework/dotnet.js";
import { renderSection, notify } from "./codex.js";

const { setModuleImports, getAssemblyExports, getConfig, runMain } =
  await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArguments(window.location.href + "stories/cave.yaml")
    .create();

// These will be imported on the dotnet side, see Interop.cs
setModuleImports("codex", {
  renderSection,
  notify,
});

// Expose WASM exports globally so codex.js can call them
globalThis.wasmExports = await getAssemblyExports(getConfig().mainAssemblyName);

await runMain();
